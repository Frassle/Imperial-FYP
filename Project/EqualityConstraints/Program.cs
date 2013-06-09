using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqualityConstraints
{
    class Program
    {
        static void Main(string[] args)
        {
            var ass = Mono.Cecil.AssemblyDefinition.ReadAssembly(
@"C:\Users\Fraser\Documents\Important dont delete\Imperial\4th Year\ValueDependent\Project\EqualityConstraints\bin\Debug\EqualityConstraints.exe");
            Console.WriteLine(ConstraintChecker.Check(ass) ? "pass" : "fail");
        }
    }
}
