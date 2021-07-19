using Droid.Core;
using Droid.Sys;

namespace Droid.Framework.Async
{
    // unreliable server -> client messages
    public enum SERVER_UNRELIABLE_MESSAGE
    {
        SERVER_UNRELIABLE_MESSAGE_EMPTY = 0,
        SERVER_UNRELIABLE_MESSAGE_PING,
        SERVER_UNRELIABLE_MESSAGE_GAMEINIT,
        SERVER_UNRELIABLE_MESSAGE_SNAPSHOT
    }

    // reliable server -> client messages
    public enum SERVER_RELIABLE_MESSAGE
    {
        SERVER_RELIABLE_MESSAGE_PURE = 0,
        SERVER_RELIABLE_MESSAGE_RELOAD,
        SERVER_RELIABLE_MESSAGE_CLIENTINFO,
        SERVER_RELIABLE_MESSAGE_SYNCEDCVARS,
        SERVER_RELIABLE_MESSAGE_PRINT,
        SERVER_RELIABLE_MESSAGE_DISCONNECT,
        SERVER_RELIABLE_MESSAGE_APPLYSNAPSHOT,
        SERVER_RELIABLE_MESSAGE_GAME,
        SERVER_RELIABLE_MESSAGE_ENTERGAME
    }

    // unreliable client -> server messages
    public enum CLIENT_UNRELIABLE_MESSAGE
    {
        CLIENT_UNRELIABLE_MESSAGE_EMPTY = 0,
        CLIENT_UNRELIABLE_MESSAGE_PINGRESPONSE,
        CLIENT_UNRELIABLE_MESSAGE_USERCMD
    }

    // reliable client -> server messages
    public enum CLIENT_RELIABLE_MESSAGE
    {
        CLIENT_RELIABLE_MESSAGE_PURE = 0,
        CLIENT_RELIABLE_MESSAGE_CLIENTINFO,
        CLIENT_RELIABLE_MESSAGE_PRINT,
        CLIENT_RELIABLE_MESSAGE_DISCONNECT,
        CLIENT_RELIABLE_MESSAGE_GAME
    }

    // server print messages
    public enum SERVER_PRINT
    {
        SERVER_PRINT_MISC = 0,
        SERVER_PRINT_BADPROTOCOL,
        SERVER_PRINT_RCON,
        SERVER_PRINT_GAMEDENY,
        SERVER_PRINT_BADCHALLENGE
    }

    public enum SERVER_DL
    {
        SERVER_DL_REDIRECT = 1,
        SERVER_DL_LIST,
        SERVER_DL_NONE
    }

    public enum SERVER_PAK
    {
        SERVER_PAK_NO = 0,
        SERVER_PAK_YES,
        SERVER_PAK_END
    }

    public struct Master
    {
        public CVar var;
        public Netadr address;
        public bool resolved;
    }

    public class AsyncNetwork
    {
        public AsyncNetwork();

        public static void Init();
        public static void Shutdown();
        public static bool IsActive => server.IsActive || client.IsActive;
        public static void RunFrame();

        public static void WriteUserCmdDelta(BitMsg msg, Usercmd cmd, Usercmd base_);
        public static void ReadUserCmdDelta(BitMsg msg, Usercmd cmd, Usercmd base_);

        public static bool DuplicateUsercmd(usercmd previousUserCmd, Usercmd currentUserCmd, int frame, int time);
        public static bool UsercmdInputChanged(Usercmd previousUserCmd, Usercmd currentUserCmd);

        // returns true if the corresponding master is set to something (and could be resolved)
        public static bool GetMasterAddress(int index, Netadr adr);
        // get the hardcoded idnet master, equivalent to GetMasterAddress( 0, .. )
        public static Netadr GetMasterAddress();

        public static void GetNETServers();

        public static void ExecuteSessionCommand(string sessCmd);

        public static AsyncServer server;
        public static AsyncClient client;

        public static CVar verbose;                     // verbose output
        public static CVar allowCheats;                 // allow cheats
        public static CVar serverDedicated;             // if set run a dedicated server
        public static CVar serverSnapshotDelay;         // number of milliseconds between snapshots
        public static CVar serverMaxClientRate;         // maximum outgoing rate to clients
        public static CVar clientMaxRate;                   // maximum rate from server requested by client
        public static CVar serverMaxUsercmdRelay;           // maximum number of usercmds relayed to other clients
        public static CVar serverZombieTimeout;         // time out in seconds for zombie clients
        public static CVar serverClientTimeout;         // time out in seconds for connected clients
        public static CVar clientServerTimeout;         // time out in seconds for server
        public static CVar serverDrawClient;                // the server draws the view of this client
        public static CVar serverRemoteConsolePassword; // remote console password
        public static CVar clientPrediction;                // how many additional milliseconds the clients runs ahead
        public static CVar clientMaxPrediction;         // max milliseconds into the future a client can run prediction
        public static CVar clientUsercmdBackup;         // how many usercmds the client sends from previous frames
        public static CVar clientRemoteConsoleAddress;      // remote console address
        public static CVar clientRemoteConsolePassword; // remote console password
        public static CVar master0;                     // idnet master server
        public static CVar master1;                     // 1st master server
        public static CVar master2;                     // 2nd master server
        public static CVar master3;                     // 3rd master server
        public static CVar master4;                     // 4th master server
        public static CVar LANServer;                       // LAN mode
        public static CVar serverReloadEngine;              // reload engine on map change instead of growing the referenced paks
        public static CVar serverAllowServerMod;            // let a pure server start with a different game code than what is referenced in game code
        public static CVar idleServer;                      // serverinfo reply, indicates all clients are idle
        public static CVar clientDownload;                  // preferred download policy

        // same message used for offline check and network reply
        public static void BuildInvalidKeyMsg(string msg, bool[] valid);

        static int realTime;
        static Master[] masters = new Master[MAX_MASTER_SERVERS];    // master1 etc.

        static void SpawnServer_f(CmdArgs args);
        static void NextMap_f(CmdArgs args);
        static void Connect_f(CmdArgs args);
        static void Reconnect_f(CmdArgs args);
        static void GetServerInfo_f(CmdArgs args);
        static void GetLANServers_f(CmdArgs args);
        static void ListServers_f(CmdArgs args);
        static void RemoteConsole_f(CmdArgs args);
        static void Heartbeat_f(CmdArgs args);
        static void Kick_f(CmdArgs args);
        static void CheckNewVersion_f(CmdArgs args);
        static void UpdateUI_f(CmdArgs args);
    }
}