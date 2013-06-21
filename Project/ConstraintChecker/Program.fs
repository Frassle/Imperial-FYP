// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

type Term =
    | Type of string
    | Parameter of string
    | Generic of string * Term list

let rec equalTerms a b = 
    match (a, b) with
    | (Type a, Type b) -> a = b
    | (Parameter a, Parameter b) -> a = b
    | (Generic (a, argsa), Generic (b, argsb)) ->
        a = b && (argsa,argsb) ||> List.forall2 equalTerms
    | _ -> false

let rec replaceTerm a b term =
    match term with
    | _ when equalTerms a term -> b
    | (Generic (t, args)) -> Generic (t, List.map (replaceTerm a b) args)
    | _ -> term

let substitute substitution term =
    let mutable result = term
    for (x, t) in substitution do
        result <- replaceTerm x t result
    result 

module Metadata =        

    let rec canonicalize (typeref : Mono.Cecil.TypeReference) =
        match typeref with 
        | :? Mono.Cecil.GenericInstanceType as generic ->
            Generic (generic.Name, generic.GenericArguments |> Seq.map canonicalize |> Seq.toList)
        | :? Mono.Cecil.GenericParameter as parameter -> Parameter (parameter.Name)
        | _ -> Type (typeref.Name) 
        
    let canonicalizeConstraint (a, b) = (canonicalize a, canonicalize b)       

    let rec getConstraints (methref : Mono.Cecil.MethodReference) =
        let constraints =
            match methref with
            | :? Mono.Cecil.MethodDefinition as methdef -> 
                if methdef.HasBody then
                    methdef.Body.Instructions
                    |> Seq.filter (fun inst -> inst.OpCode = Mono.Cecil.Cil.OpCodes.Call)
                    |> Seq.map (fun inst -> inst.Operand :?> Mono.Cecil.MethodReference)
                    |> Seq.filter (fun methref -> methref.Name = "EqualTypes")
                    |> Seq.map (fun methref -> methref :?> Mono.Cecil.GenericInstanceMethod)
                    |> Seq.map (fun methref -> (methref.GenericArguments.[0], methref.GenericArguments.[1]))
                    |> Seq.map canonicalizeConstraint
                    |> Seq.toList
                else 
                    List.empty
            | :? Mono.Cecil.GenericInstanceMethod as generic ->
                let element = generic.GetElementMethod().Resolve()
                let constraints = getConstraints element

                let instantiate = 
                    Seq.zip (element.GenericParameters) (generic.GenericArguments)
                    |> Seq.map canonicalizeConstraint
                    |> Seq.toList

                List.map (fun (a,b) -> (substitute instantiate a, substitute instantiate b)) constraints
            | _ -> List.empty

        match methref.DeclaringType with
        | :? Mono.Cecil.GenericInstanceType as generic -> 
            let element = generic.GetElementType().Resolve()

            let instantiate = 
                Seq.zip (element.GenericParameters) (generic.GenericArguments)
                |> Seq.map canonicalizeConstraint
                |> Seq.toList

            List.map (fun (a,b) -> (substitute instantiate a, substitute instantiate b)) constraints
        | _ -> constraints


    let getCasts (methdef : Mono.Cecil.MethodDefinition) =
        if methdef.HasBody then
            methdef.Body.Instructions
            |> Seq.filter (fun inst -> inst.OpCode = Mono.Cecil.Cil.OpCodes.Call)
            |> Seq.map (fun inst -> inst.Operand :?> Mono.Cecil.MethodReference)
            |> Seq.filter (fun methref -> methref.Name = "Cast")
            |> Seq.map (fun methref -> methref :?> Mono.Cecil.GenericInstanceMethod)
            |> Seq.map (fun methref -> (methref.GenericArguments.[0], methref.GenericArguments.[1]))
            |> Seq.map canonicalizeConstraint
            |> Seq.toList
        else 
            List.empty

    let getConstrainedCalls (methdef : Mono.Cecil.MethodDefinition) =  
        let get (instruction : Mono.Cecil.Cil.Instruction) =
            if instruction.OpCode = Mono.Cecil.Cil.OpCodes.Call  || instruction.OpCode = Mono.Cecil.Cil.OpCodes.Callvirt then
                match instruction.Operand with
                | :? Mono.Cecil.MethodReference as meth -> 
                    let constraints = getConstraints meth
                    if List.isEmpty constraints then None else Some constraints
                | _ -> None
            else
                None
      
        if methdef.HasBody then
            methdef.Body.Instructions
            |> Seq.map get
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
            |> Seq.toList
        else 
            List.empty

module Unification = 
    let rec replaceTerm x t term = 
        match term with
        | Parameter y when x = y -> t
        | Generic (a, args) -> Generic (a, List.map (replaceTerm x t) args)
        | term -> term

    let replaceTerms x t terms =
        terms |> List.map(fun (a,b) -> (replaceTerm x t a, replaceTerm x t b))

    let rec unify terms =
        let rec vars t =
            match t with
            | Type s -> []
            | Parameter s -> [s]
            | Generic (s, l) -> List.concat <| List.map vars l

        match terms with
        | [] -> []
        | (Type a, Type b)::rest ->
            if a <> b then
                failwith (sprintf "Could not unify %A and %A" (Type a) (Type b))
            else
                rest
        | (Generic (a, argsa), Generic (b, argsb))::rest ->
            if a <> b then
                failwith (sprintf "Could not unify %A and %A" (Generic (a, argsa)) (Generic (b, argsb)))
            else
                (List.zip argsa argsb) @ rest
        | (Parameter x, Parameter y)::rest ->
            rest @ [(Parameter x, Parameter y)]
        | (Parameter x, t)::rest ->
            if List.exists (fun y -> y=x) (vars t) then
                failwith (sprintf "Could not unify %A and %A" (Parameter x) t)
            else
                (replaceTerms x t rest) @ [(Parameter x, t)]            
        | (term, Parameter x)::rest ->
            (Parameter x, term) :: rest
        | (terma, termb)::reset ->
            failwith (sprintf "Could not unify %A and %A" terma termb)
    
    let unification terms =
        (Some terms)
        |> Seq.unfold (function
            | Some input ->
                let output = unify input

                let isDistinct = Seq.length (Seq.distinctBy fst output) = Seq.length output
                let isNormal = Seq.forall (fun sub -> sub |> fst |> function Parameter x -> true | _ -> false) output

                if isNormal && isDistinct then
                    Some (output, None)
                else
                    Some (output, Some output)
            | None -> None)
        |> Seq.last

    let areEqualTerms substitution a b =
        let x = substitute substitution a
        let t = substitute substitution b
        equalTerms x t

module Program = 
    let getAllMethods (assembly : Mono.Cecil.AssemblyDefinition) =    
        let rec getMethods (t : Mono.Cecil.TypeDefinition) =
            Seq.append t.Methods (Seq.concat (Seq.map getMethods t.NestedTypes))

        assembly.Modules
        |> Seq.map (fun m -> m.Types) 
        |> Seq.concat
        |> Seq.map getMethods
        |> Seq.concat

    let getAll getter assembly =
        assembly
        |> getAllMethods
        |> Seq.map (fun m -> (m, getter m))

    let getAllTypes (assembly : Mono.Cecil.AssemblyDefinition) =    
        let defs =
            assembly.Modules
            |> Seq.map (fun m -> m.GetTypes())
            |> Seq.concat
            |> Seq.map (fun def -> def :> Mono.Cecil.TypeReference)

        let refs =
            assembly.Modules
            |> Seq.map (fun m -> m.GetTypeReferences())
            |> Seq.concat

        Seq.append defs refs

    [<EntryPoint>]
    let main argv = 
        let assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(argv.[0])
        let methods = getAllMethods assembly

        for constraints in Seq.map Metadata.getConstraints methods do
            if not (Seq.isEmpty constraints) then
                try
                    let sub = Unification.unification constraints
            
                    printfn "Terms:"
                    printfn "%A" constraints
                    printfn "Unification:"
                    printfn "%A" sub
                    printfn "---"
                with
                | Failure msg -> printfn "%A" msg
            
        let castsites = 
            methods 
            |> Seq.map (fun m -> (m, Metadata.getCasts m)) 
            |> Seq.filter (snd >> Seq.isEmpty >> not)
            |> Seq.toList

        let callsites = 
            methods 
            |> Seq.map (fun m -> (m, Metadata.getConstrainedCalls m))
            |> Seq.filter (snd >> Seq.isEmpty >> not)
            |> Seq.toList

        printfn "Casts"
        for (meth, sites) in castsites do
            try
                let constraints = Metadata.getConstraints meth
                let sub = Unification.unification constraints
            
                for (a,b) in sites do
            
                    printf "Are %A and %A equal in %A? " a b meth
                    printfn "%s" (if Unification.areEqualTerms sub a b then "yes" else "no")
            with
            | Failure msg -> printfn "%A in %A" msg meth

        printfn "Callsites"
        for (meth, sites) in callsites do
            try
                let constraints = Metadata.getConstraints meth
                let sub = Unification.unification constraints
            
                for context in sites do
                    for (a,b) in context do
                        printf "Are %A and %A equal in %A? " a b meth
                        printfn "%s" (if Unification.areEqualTerms sub a b then "yes" else "no")
            with
            | Failure msg -> printfn "%A in %A" msg meth


        printfn "%A" argv
        0 // return an integer exit code
