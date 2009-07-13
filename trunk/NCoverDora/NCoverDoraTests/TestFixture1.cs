using NCoverDora;
using NUnit.Framework;

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
