using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqualityConstraints
{
    [Serializable]
    public class TypeEqualityException : Exception
    {
        public TypeEqualityException() { }
        public TypeEqualityException(string message) : base(message) { }
        public TypeEqualityException(string message, Exception inner) : base(message, inner) { }
        protected TypeEqualityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public static class TypeEquality
    {
        public static U Cast<T, U>(T obj)
        {
            if (typeof(T) == typeof(U))
            {
                return (U)(Object)obj;
            }
            else
            {
                throw new TypeEqualityException();
            }
        }

        public static void EqualTypes<T, U>()
        {
            if (typeof(T) != typeof(U))
            {
                throw new TypeEqualityException();
            }
        }
    }
}
