using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqualityConstraints
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    sealed class EqualityConstraint : Attribute
    {
        readonly string TypeT;
        readonly string TypeU;

        // This is a positional argument
        public EqualityConstraint(string t, string u)
        {
            TypeT = t;
            TypeU = u;
        }

        public string T
        {
            get { return TypeT; }
        }

        public string U
        {
            get { return TypeU; }
        }

        public static U Cast<T, U>(T value)
            where T : class 
            where U : class
        {
            if (typeof(T) == typeof(U))
            {
                return value as U;
            }

            throw new Exception();
        }
    }

    public class FList<T>
    {
        [EqualityConstraint("T", "List<U>")]
        public void Method<U>(T a)
        {
        }
    }
}
