namespace Droid
{
    public static class Config
    {
        public const string GAME_NAME = "Doom3Quest";

        // paths
        public const string BASE_GAMEDIR = "base";
        public const string BUILD_LIBRARY_SUFFIX = "/libd3es_game.so";

        // CD Key file info
        // goes into BASE_GAMEDIR whatever the fs_game is set to two distinct files for easier win32 installer job
        public const string CDKEY_FILE = "doomkey";
        public const string XPKEY_FILE = "xpkey";
        public const string CDKEY_TEXT = "\n// Do not give this file to ANYONE.\n"
                                        + "// id Software or Zenimax will NEVER ask you to send this file to them.\n";

        public const string WIN32_CONSOLE_CLASS = "dhewm 3 WinConsole";
        public const string CONFIG_SPEC = "config.spec";
    }
}