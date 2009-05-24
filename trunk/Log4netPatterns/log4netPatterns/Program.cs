using log4net;
using log4net.Config;

namespace log4netPatterns
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var t = new Thingy<int>();
            t.Doover(3);

            var t2 = new Thangy();
            t2.Doover("blah");

        }
    }

    public class Thingy<T>
    {
        private ILog _log;

        public Thingy()
        {
            _log = LogManager.GetLogger(typeof (Thingy<T>));
        }

        public virtual string Doover(T stuff)
        {
            _log.Debug("Thingy");

            return "ha";
        }
    }

    public class Thangy : Thingy<string>
    {
        private ILog _log;

        public Thangy()
        {
            _log = LogManager.GetLogger(typeof (Thangy));
        }

        public override string Doover(string stuff)
        {
            _log.Debug("Thangy");

            return "ho ho";
        }
    }
}