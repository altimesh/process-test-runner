using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitSample
{
    public class Sequence : ValuesAttribute
    {
        public Sequence() : base(1, 2, 3)
        { }
    }

    [TestFixture]
    public class Class1
    {
        [Test, Sequential]
        public void test([Sequence] int s) 
        {
            Console.WriteLine("sequence value: " + s);
        }

        [Test, Sequential]
        public void test([Sequence] int s1, [Sequence] int s2)
        {
            Console.WriteLine("sequence value: " + s1 + " : " + s2);
        }

        [Test, Sequential]
        public void test()
        {
            Console.WriteLine("toto");
        }
    }
}
