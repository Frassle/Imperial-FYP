using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqualityConstraints
{
    class ConstraintChecker
    {
        public static bool Check(Mono.Cecil.AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (!Check(type))
                        return false;
                }
            }

            return true;
        }

        static bool Check(Mono.Cecil.TypeDefinition type)
        {
            foreach (var inner_type in type.NestedTypes)
            {
                if (!Check(inner_type))
                    return false;
            }

            foreach (var method in type.Methods)
            {
                if (!Check(method))
                    return false;
            }

            return true;
        }

        static bool Check(Mono.Cecil.MethodDefinition method)
        {
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == Mono.Cecil.Cil.OpCodes.Call
                    || instr.OpCode == Mono.Cecil.Cil.OpCodes.Calli
                    || instr.OpCode == Mono.Cecil.Cil.OpCodes.Callvirt)
                {
                    if (!CheckCall(method, instr))
                        return false;
                }
            }

            return true;
        }

        static bool CheckCall(Mono.Cecil.MethodDefinition method, Mono.Cecil.Cil.Instruction instruction)
        {
            foreach (var attr in method.CustomAttributes)
            {
                if (attr.AttributeType.Name == "EqualityConstraint")
                {
                    Console.WriteLine(attr.Fields[0].Argument.Value);
                    Console.WriteLine(attr.Fields[1].Argument.Value);
                }
            }

            //Mono.Cecil.TypeReference t;
            //module.TryGetTypeReference("System.Collections.Generic.List`1", out t);


            return true;
        }
    }
}
