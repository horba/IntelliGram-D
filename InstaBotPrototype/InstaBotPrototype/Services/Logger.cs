using log4net;

namespace InstaBotPrototype.Services
{
    public static class Logger
    {
        public static ILog GetLog<T>() => LogManager.GetLogger(typeof(T));
    }
}