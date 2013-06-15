using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public abstract class Exp<T> 
    { 
        public virtual bool Eq(Exp<T> that)
        { return false; }
        public virtual bool PairEq<C, D>(Pair<C, D> that)
        {
            EqualityConstraints.TypeEquality.EqualTypes<T, Tuple<C, D>>();
            return false;
        }
        public virtual bool LitEq(Lit that)
        { return false; }
    }

    public class Lit : Exp<int> { 
        int value;

        public override bool Eq(Exp<int> that)
        { return that.LitEq(this); }
        public override bool LitEq(Lit that)
        { return value == that.value; }
    }

    public class Pair<A,B> : Exp<Tuple<A,B>> {

        Exp<A> e1;
        Exp<B> e2;

        public override bool Eq(Exp<Tuple<A, B>> that)
        {
            return that.PairEq<A, B>(this); 
        }

        public override bool PairEq<C, D>(Pair<C, D> that)
        {
            EqualityConstraints.TypeEquality.EqualTypes<Tuple<A, B>, Tuple<C, D>>();
            Pair<A, B> That = EqualityConstraints.TypeEquality.Cast<Pair<C, D>, Pair<A, B>>(that);
            return That.e1.Eq(e1) && That.e2.Eq(e2); 
        }
    }

    public static class ExpTest
    {
        public static void Main()
        {
            Pass();
            Fail();
        }

        static void Pass()
        {

        }

        static void Fail()
        {
        }
    }
}