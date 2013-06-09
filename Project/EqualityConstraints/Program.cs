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
            var ass = Mono.Cecil.AssemblyDefinition.ReadAssembly(@"D:\VisualStudio\Projects\Ibasa\Ibasa\bin\Debug\Ibasa.dll");
            Console.WriteLine(ConstraintChecker.Check(ass) ? "pass" : "fail");
        }
    }
}
