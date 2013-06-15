using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public abstract class List<T>
    {
        public abstract List<T> Append(List<T> list);

        public virtual List<U> Flatten<U>()
        {
            EqualityConstraints.TypeEquality.EqualTypes<T, List<U>>();
            return null;
        }
    }

    public class Nil<T> : List<T>
    {
        public override List<T> Append(List<T> list)
        {
            return list;
        }

        public override List<U> Flatten<U>()
        {
            EqualityConstraints.TypeEquality.EqualTypes<T, List<U>>();
            return new Nil<U>();
        }
    }

    public class Cons<T> : List<T>
    {
        T Head;
        List<T> Tail;

        public Cons(T head, List<T> tail)
        {
            Head = head;
            Tail = tail;
        }

        public override List<T> Append(List<T> list)
        {
            return new Cons<T>(Head, Tail.Append(list));
        }

        public override List<U> Flatten<U>()
        {
            EqualityConstraints.TypeEquality.EqualTypes<T, List<U>>();
            return EqualityConstraints.TypeEquality.Cast<T, List<U>>(Head).Append(Tail.Flatten<U>());
        }
    }

    public static class ListTest
    {
        public static void Main()
        {
            Pass();
            Fail();
        }

        static void Pass()
        {
            var ilist1 = new Cons<int>(1, new Nil<int>());
            var ilist2 = ilist1.Append(new Cons<int>(2, new Cons<int>(3, new Nil<int>())));

            var list1 = new Cons<List<int>>(ilist2, new Nil<List<int>>());
            var list2 = list1.Append(new Cons<List<int>>(ilist1, new Nil<List<int>>()));

            var flist = list2.Flatten<int>();
        }

        static void Fail()
        {
            var ilist = new Cons<int>(1, new Cons<int>(2, new Cons<int>(3, new Nil<int>())));

            var flist = ilist.Flatten<int>();
        }
    }
}
