namespace System.NumericsX.OpenStack
{
    public static class Config
    {
        // paths
        public const string BASE_GAMEDIR = "base";
        public const string BUILD_LIBRARY_SUFFIX = "/libdes_game.so";

        // CD Key file info
        // goes into BASE_GAMEDIR whatever the fs_game is set to two distinct files for easier win32 installer job
        public const string CDKEY_FILE = "doomkey";
        public const string XPKEY_FILE = "xpkey";
        public const string CDKEY_TEXT = "\n// Do not give this file to ANYONE.\n"
                                        + "// id Software or Zenimax will NEVER ask you to send this file to them.\n";

        public const string CONFIG_SPEC = "config.spec";

        // default idnet host address
        public const string IDNET_HOST = "idnet.ua-corp.com";

        // default idnet master port
        public const string IDNET_MASTER_PORT = "27650";

        // default network server port
        public const int PORT_SERVER = 27666;

        // broadcast scan this many ports after PORT_SERVER so a single machine can run multiple servers
        public const int NUM_SERVER_PORTS = 4;

        // use a different major for each game
        public const int ASYNC_PROTOCOL_MAJOR = 1;

        #region BUILD DEFINES

        public const int ASYNC_PROTOCOL_MINOR = 42;
        public const int ASYNC_PROTOCOL_VERSION = (ASYNC_PROTOCOL_MAJOR << 16) + ASYNC_PROTOCOL_MINOR;

        public const int MAX_ASYNC_CLIENTS = 32;

        public const int MAX_USERCMD_BACKUP = 256;
        public const int MAX_USERCMD_DUPLICATION = 25;
        public const int MAX_USERCMD_RELAY = 10;

        // index 0 is hardcoded to be the idnet master which leaves 4 to user customization
        public const int MAX_MASTER_SERVERS = 5;

        public const int MAX_NICKLEN = 32;

        // max number of servers that will be scanned for at a single IP address
        public const int MAX_SERVER_PORTS = 8;

        // special game init ids
        public const int GAME_INIT_ID_INVALID = -1;
        public const int GAME_INIT_ID_MAP_LOAD = -2;

        #endregion
    }
}