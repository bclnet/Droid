using Gengine.Framework.Async;

namespace Gengine.Framework
{
    static class Lib
    {
        public static SessionLocal sessLocal = new();
        public static INetworkSystem networkSystem;
        public static DeclManagerLocal declManagerLocal = new();
    }
}