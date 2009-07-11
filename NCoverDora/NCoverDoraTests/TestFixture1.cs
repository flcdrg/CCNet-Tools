using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using NCoverDora;

namespace NCoverDoraTests
{
    [TestFixture]
    public class TestFixture1
    {
        [Test]
        public void Test()
        {
            var c = new SomeClass();

            c.Method1();
        }
    }
}
