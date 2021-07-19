using Droid.Core;
using Droid.Sys;
using System.Collections.Generic;

namespace Droid.Framework.Async
{
    // states for the server's authorization process
    public enum AuthState
    {
        CDK_WAIT = 0,   // we are waiting for a confirm/deny from auth this is subject to timeout if we don't hear from auth or a permanent wait if auth said so
        CDK_OK,
        CDK_ONLYLAN,
        CDK_PUREWAIT,
        CDK_PUREOK,
        CDK_MAXSTATES
    }

    // states from the auth server, while the client is in CDK_WAIT
    public enum AuthReply
    {
        AUTH_NONE = 0,  // no reply yet
        AUTH_OK,        // this client is good
        AUTH_WAIT,      // wait - keep sending me srvAuth though
        AUTH_DENY,      // denied - don't send me anything about this client anymore
        AUTH_MAXSTATES
    }

    // message from auth to be forwarded back to the client some are locally hardcoded to save space, auth has the possibility to send a custom reply
    public enum AuthReplyMsg
    {
        AUTH_REPLY_WAITING = 0, // waiting on an initial reply from auth
        AUTH_REPLY_UNKNOWN,     // client unknown to auth
        AUTH_REPLY_DENIED,      // access denied
        AUTH_REPLY_PRINT,       // custom message
        AUTH_REPLY_SRVWAIT,     // auth server replied and tells us he's working on it
        AUTH_REPLY_MAXSTATES
    }

    public class Challenge
    {
        public Netadr address;      // client address
        public int clientId;        // client identification
        public int challenge;       // challenge code
        public int time;            // time the challenge was created
        public int pingTime;        // time the challenge response was sent to client
        public bool connected;      // true if the client is connected
        public AuthState authState;     // local state regarding the client
        public AuthReply authReply;     // cd key check replies
        public AuthReplyMsg authReplyMsg;   // default auth messages
        public string authReplyPrint;   // custom msg
        public string guid;     // guid
    }

    public enum ServerClientState
    {
        SCS_FREE,           // can be reused for a new connection
        SCS_ZOMBIE,         // client has been disconnected, but don't reuse connection for a couple seconds
        SCS_PUREWAIT,       // client needs to update it's pure checksums before we can go further
        SCS_CONNECTED,      // client is connected
        SCS_INGAME          // client is in the game
    }

    public class ServerClient
    {
        public int clientId;
        public ServerClientState clientState;
        public int clientPrediction;
        public int clientAheadTime;
        public int clientRate;
        public int clientPing;

        public int gameInitSequence;
        public int gameFrame;
        public int gameTime;

        public MsgChannel channel;
        public int lastConnectTime;
        public int lastEmptyTime;
        public int lastPingTime;
        public int lastSnapshotTime;
        public int lastPacketTime;
        public int lastInputTime;
        public int snapshotSequence;
        public int acknowledgeSnapshotSequence;
        public int numDuplicatedUsercmds;

        public string guid;  // Even Balance - M. Quinn
    }

    public class AsyncServer
    {
        // MAX_CHALLENGES is made large to prevent a denial of service attack that could cycle all of them out before legitimate users connected
        public const int MAX_CHALLENGES = 1024;

        // if we don't hear from authorize server, assume it is down
        public const int AUTHORIZE_TIMEOUT = 5000;

        public AsyncServer();

        public bool InitPort();
        public void ClosePort();
        public void Spawn();
        public void Kill();
        public void ExecuteMapChange();

        public int GetPort();
        public Netadr GetBoundAdr();
        public bool IsActive() => active;
        public int GetDelay() => gameTimeResidual;
        public int GetOutgoingRate();
        public int GetIncomingRate();
        public bool IsClientInGame(int clientNum);
        public int GetClientPing(int clientNum);
        public int GetClientPrediction(int clientNum);
        public int GetClientTimeSinceLastPacket(int clientNum);
        public int GetClientTimeSinceLastInput(int clientNum);
        public int GetClientOutgoingRate(int clientNum);
        public int GetClientIncomingRate(int clientNum);
        public float GetClientOutgoingCompression(int clientNum);
        public float GetClientIncomingCompression(int clientNum);
        public float GetClientIncomingPacketLoss(int clientNum);
        public int GetNumClients();
        public int GetNumIdleClients();
        public int GetLocalClientNum() => localClientNum;

        public void RunFrame();
        public void ProcessConnectionLessMessages();
        public void RemoteConsoleOutput(string s);
        public void SendReliableGameMessage(int clientNum, BitMsg msg);
        public void SendReliableGameMessageExcluding(int clientNum, BitMsg msg);
        public void LocalClientSendReliableMessage(BitMsg msg);

        public void MasterHeartbeat(bool force = false);
        public void DropClient(int clientNum, string reason);

        public void PacifierUpdate();

        public void UpdateUI(int clientNum);

        public void UpdateAsyncStatsAvg();
        public void GetAsyncStatsAvgMsg(string msg);

        public void PrintLocalServerInfo();

        bool active;                        // true if server is active
        int realTime;                   // absolute time

        int serverTime;                 // local server time
        Port serverPort;                  // UDP port
        int serverId;                   // server identification
        int serverDataChecksum;         // checksum of the data used by the server
        int localClientNum;             // local client on listen server

        Challenge[] challenges = new Challenge[MAX_CHALLENGES]; // to prevent invalid IPs from connecting
        ServerClient[] clients = new ServerClient[MAX_ASYNC_CLIENTS];   // clients
        Usercmd[][] userCmds = new Usercmd[MAX_USERCMD_BACKUP][MAX_ASYNC_CLIENTS];

        int gameInitId;                 // game initialization identification
        int gameFrame;                  // local game frame
        int gameTime;                   // local game time
        int gameTimeResidual;           // left over time from previous frame

        Netadr rconAddress;

        int nextHeartbeatTime;
        int nextAsyncStatsTime;

        bool serverReloadingEngine;     // flip-flop to not loop over when net_serverReloadEngine is on

        bool noRconOutput;              // for default rcon response when command is silent

        int lastAuthTime;               // global for auth server timeout

        // track the max outgoing rate over the last few secs to watch for spikes dependent on net_serverSnapshotDelay. 50ms, for a 3 seconds backlog -> 60 samples
        static int stats_numsamples = 60;
        int[] stats_outrate = new int[stats_numsamples];
        int stats_current;
        int stats_average_sum;
        int stats_max;
        int stats_max_index;

        void PrintOOB(Netadr to, int opcode, string s);
        void DuplicateUsercmds(int frame, int time);
        void ClearClient(int clientNum);
        void InitClient(int clientNum, int clientId, int clientRate);
        void InitLocalClient(int clientNum);
        void BeginLocalClient();
        void LocalClientInput();
        void CheckClientTimeouts();
        void SendPrintBroadcast(string s);
        void SendPrintToClient(int clientNum, string s);
        void SendUserInfoBroadcast(int userInfoNum, Dictionary<string, string> info, bool sendToAll = false);
        void SendUserInfoToClient(int clientNum, int userInfoNum, Dictionary<string, string> info);
        void SendSyncedCvarsBroadcast(Dictionary<string, string> cvars);
        void SendSyncedCvarsToClient(int clientNum, Dictionary<string, string> cvars);
        void SendApplySnapshotToClient(int clientNum, int sequence);
        bool SendEmptyToClient(int clientNum, bool force = false);
        bool SendPingToClient(int clientNum);
        void SendGameInitToClient(int clientNum);
        bool SendSnapshotToClient(int clientNum);
        void ProcessUnreliableClientMessage(int clientNum, BitMsg msg);
        void ProcessReliableClientMessages(int clientNum);
        void ProcessChallengeMessage(Netadr from, BitMsg msg);
        void ProcessConnectMessage(Netadr from, BitMsg msg);
        void ProcessRemoteConsoleMessage(Netadr from, BitMsg msg);
        void ProcessGetInfoMessage(Netadr from, BitMsg msg);
        bool ConnectionlessMessage(Netadr from, BitMsg msg);
        bool ProcessMessage(Netadr from, BitMsg msg);
        void ProcessAuthMessage(BitMsg msg);
        bool SendPureServerMessage(Netadr to);                                      // returns false if no pure paks on the list
        void ProcessPureMessage(Netadr from, BitMsg msg);
        int ValidateChallenge(Netadr from, int challenge, int clientId);    // returns -1 if validate failed
        bool SendReliablePureToClient(int clientNum);
        void ProcessReliablePure(int clientNum, BitMsg msg);
        bool VerifyChecksumMessage(int clientNum, Netadr rom, BitMsg msg, string reply); // if from is NULL, clientNum is used for error messages
        void SendReliableMessage(int clientNum, BitMsg msg);                // checks for overflow and disconnects the faulty client
        int UpdateTime(int clamp);
        void SendEnterGameToClient(int clientNum);
        void ProcessDownloadRequestMessage(Netadr from, BitMsg msg);
    }
}