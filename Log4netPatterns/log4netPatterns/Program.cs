using log4net;
using log4net.Config;

namespace log4netPatterns
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var t = new BaseClass<int>();
            t.MyMethod(3);

            var t2 = new SubClass();
            t2.MyMethod("blah");

        }
    }

    public class BaseClass<T>
    {
        private ILog _log;

        public BaseClass()
        {
            _log = LogManager.GetLogger(typeof (BaseClass<T>));
        }

        public virtual string MyMethod(T stuff)
        {
            _log.Debug("BaseClass");

            return "ha";
        }
    }

    public class SubClass : BaseClass<string>
    {
        private ILog _log;

        public SubClass()
        {
            _log = LogManager.GetLogger(typeof (SubClass));
        }

        public override string MyMethod(string stuff)
        {
            _log.Debug("SubClass");

            return "ho ho";
        }
    }
}