using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project10
{
    static class ListExtensions
    {
        //List extension:
        public static T popFront<T>(this List<T> list)
        {
            var ret = list.First();
            list.RemoveAt(0);
            return ret;
        }

        public static void rmFront<T>(this List<T> list)
        {
            list.RemoveAt(0);
        }

        public static void pushFront<T>(this List<T> list, T val)
        {
            list.Insert(0,val);
        }
    }
}
