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
        int Value;

        public Lit(int value)
        {
            Value = value;
        }

        public override bool Eq(Exp<int> that)
        { return that.LitEq(this); }
        public override bool LitEq(Lit that)
        { return Value == that.Value; }
    }

    public class Pair<A,B> : Exp<Tuple<A,B>> {

        Exp<A> ExpA;
        Exp<B> ExpB;

        public Pair(Exp<A> a, Exp<B> b)
        {
            ExpA = a;
            ExpB = b;
        }

        public override bool Eq(Exp<Tuple<A, B>> that)
        {
            return that.PairEq<A, B>(this); 
        }

        public override bool PairEq<C, D>(Pair<C, D> that)
        {
            EqualityConstraints.TypeEquality.EqualTypes<Tuple<A, B>, Tuple<C, D>>();
            Pair<A, B> That = EqualityConstraints.TypeEquality.Cast<Pair<C, D>, Pair<A, B>>(that);
            return That.ExpA.Eq(ExpA) && That.ExpB.Eq(ExpB); 
        }
    }

    public static class ExpTest
    {
        public static void Main()
        {
            Pass();
        }

        static void Pass()
        {
            Pair<int, int> p1 = new Pair<int, int>(new Lit(1), new Lit(2));
            Pair<int, int> p2 = new Pair<int, int>(new Lit(3), new Lit(4));

            bool result = p1.Eq(p2);
        }
    }
}