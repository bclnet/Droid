using Droid.Framework;

namespace Droid
{
    public static class G2 : G
    {
        internal static CmdSystemLocal cmdSystemLocal = new();
        public static CmdSystem cmdSystem = cmdSystemLocal;

        internal static CVarSystemLocal localCVarSystem = new();
        public static CVarSystem cvarSystem = localCVarSystem;

        internal static SessionLocal sessLocal = new();
        public static Session session = sessLocal;

        internal static ConsoleLocal localConsole = new();
        public static Console console = localConsole;

        internal static UsercmdGenLocal localUsercmdGen = new();
        public static UsercmdGen usercmdGen = localUsercmdGen;
    }
}