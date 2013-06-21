using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Broken<T>
    {
        public T Horribly<U>(U u)
        {
            EqualityConstraints.TypeEquality.EqualTypes<List<int>, T>();
            EqualityConstraints.TypeEquality.EqualTypes<U, List<int>>();
            EqualityConstraints.TypeEquality.EqualTypes<T, int>();

            return EqualityConstraints.TypeEquality.Cast<U, T>(u);
        }
    }
}
