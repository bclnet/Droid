using Droid.Framework;
using Droid.Render;
using Droid.Sound;
using Droid.UI;

namespace Droid
{
    public static class G
    {
        public static Common common;

        public static CmdSystem cmdSystem;

        public static CVarSystem cvarSystem;

        public static Session session;

        public static Console console;

        public static FileSystem fileSystem;

        public static Sys.System sys;

        public static UserInterfaceManager uiManager;

        public static UsercmdGen usercmdGen;

        public static SoundSystem soundSystem;

        public static RenderSystem renderSystem;

        public static ImageManager globalImages;     // pointer to global list for the rest of the system

        public static DeclManager declManager;

        // color escape character
        internal const int C_COLOR_ESCAPE = '^';
        internal const int C_COLOR_DEFAULT = '0';
        internal const int C_COLOR_RED = '1';
        internal const int C_COLOR_GREEN = '2';
        internal const int C_COLOR_YELLOW = '3';
        internal const int C_COLOR_BLUE = '4';
        internal const int C_COLOR_CYAN = '5';
        internal const int C_COLOR_MAGENTA = '6';
        internal const int C_COLOR_WHITE = '7';
        internal const int C_COLOR_GRAY = '8';
        internal const int C_COLOR_BLACK = '9';

        // color escape string
        internal const string S_COLOR_DEFAULT = "^0";
        internal const string S_COLOR_RED = "^1";
        internal const string S_COLOR_GREEN = "^2";
        internal const string S_COLOR_YELLOW = "^3";
        internal const string S_COLOR_BLUE = "^4";
        internal const string S_COLOR_CYAN = "^5";
        internal const string S_COLOR_MAGENTA = "^6";
        internal const string S_COLOR_WHITE = "^7";
        internal const string S_COLOR_GRAY = "^8";
        internal const string S_COLOR_BLACK = "^9";
    }
}