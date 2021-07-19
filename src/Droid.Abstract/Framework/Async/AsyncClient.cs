using Droid.Core;
using Droid.Sys;
using System;
using System.Collections.Generic;
using System.IO;

namespace Droid.Framework.Async
{
    public enum ClientState
    {
        CS_DISCONNECTED,
        CS_PURERESTART,
        CS_CHALLENGING,
        CS_CONNECTING,
        CS_CONNECTED,
        CS_INGAME
    }

    public enum AuthKeyMsg
    {
        AUTHKEY_BADKEY,
        AUTHKEY_GUID
    }

    public enum AuthBadKeyStatus
    {
        AUTHKEY_BAD_INVALID,
        AUTHKEY_BAD_BANNED,
        AUTHKEY_BAD_INUSE,
        AUTHKEY_BAD_MSG
    }

    public enum ClientUpdateState
    {
        UPDATE_NONE,
        UPDATE_SENT,
        UPDATE_READY,
        UPDATE_DLING,
        UPDATE_DONE
    }

    public struct PakDlEntry
    {
        public string url;
        public string filename;
        public int size;
        public int checksum;
    }

    public class AsyncClient
    {
        public AsyncClient();

        public void Shutdown();
        public bool InitPort();
        public void ClosePort();
        public void ConnectToServer(Netadr adr);
        public void ConnectToServer(string address);
        public void Reconnect();
        public void DisconnectFromServer();
        public void GetServerInfo(Netadr adr);
        public void GetServerInfo(string address);
        public void GetLANServers();
        public void GetNETServers();
        public void ListServers();
        public void ClearServers();
        public void RemoteConsole(string command);
        public bool IsPortInitialized => clientPort.GetPort() != 0;

        public bool IsActive => active;
        public int GetLocalClientNum => clientNum;
        public int GetPrediction => throw new NotImplementedException();
        public int GetTimeSinceLastPacket => throw new NotImplementedException();
        public int GetOutgoingRate => throw new NotImplementedException();
        public int GetIncomingRate => throw new NotImplementedException();
        public float GetOutgoingCompression => throw new NotImplementedException();
        public float GetIncomingCompression => throw new NotImplementedException();
        public float GetIncomingPacketLoss => throw new NotImplementedException();
        public int GetPredictedFrames => lastFrameDelta;

        public void RunFrame();
        public void SendReliableGameMessage(BitMsg msg);

        public void SendVersionCheck(bool fromMenu = false);
        // pass NULL for the keys you don't care to auth for
        // returns false if internet link doesn't appear to be available
        public bool SendAuthCheck(string cdkey, string xpkey);

        public void PacifierUpdate();

        public ServerScan serverList;

        bool active;                        // true if client is active
        int realTime;                   // absolute time

        int clientTime;                 // client local time
        Port clientPort;                    // UDP port
        int clientId;                   // client identification
        int clientDataChecksum;         // checksum of the data used by the client
        int clientNum;                  // client number on server
        ClientState clientState;                // client state
        int clientPrediction;           // how far the client predicts ahead
        int clientPredictTime;          // prediction time used to send user commands

        Netadr serverAddress;               // IP address of server
        int serverId;                   // server identification
        int serverChallenge;            // challenge from server
        int serverMessageSequence;      // sequence number of last server message

        Netadr lastRconAddress;         // last rcon address we emitted to
        int lastRconTime;               // when last rcon emitted

        MsgChannel channel;                 // message channel to server
        int lastConnectTime;            // last time a connect message was sent
        int lastEmptyTime;              // last time an empty message was sent
        int lastPacketTime;             // last time a packet was received from the server
        int lastSnapshotTime;           // last time a snapshot was received

        int snapshotSequence;           // sequence number of the last received snapshot
        int snapshotGameFrame;          // game frame number of the last received snapshot
        int snapshotGameTime;           // game time of the last received snapshot

        int gameInitId;                 // game initialization identification
        int gameFrame;                  // local game frame
        int gameTime;                   // local game time
        int gameTimeResidual;           // left over time from previous frame

        Usercmd[][] userCmds = new Usercmd[MAX_USERCMD_BACKUP][MAX_ASYNC_CLIENTS];

        IUserInterface guiNetMenu;

        ClientUpdateState updateState;
        int updateSentTime;
        string updateMSG;
        string updateURL;
        bool updateDirectDownload;
        string updateFile;
        DlMime updateMime;
        string updateFallback;
        bool showUpdateMessage;

        BackgroundDownload backgroundDownload;
        int dltotal;
        int dlnow;

        int lastFrameDelta;

        int dlRequest;      // randomized number to keep track of the requests
        int dlChecksums[MAX_PURE_PAKS]; // 0-terminated, first element is the game pak checksum or 0
        int dlCount;        // total number of paks we request download for ( including the game pak )
        List<PakDlEntry> dlList;            // list of paks to download, with url and name
        int currentDlSize;
        int totalDlSize;    // for partial progress stuff

        void Clear();
        void ClearPendingPackets();
        void DuplicateUsercmds(int frame, int time);
        void SendUserInfoToServer();
        void SendEmptyToServer(bool force = false, bool mapLoad = false);
        void SendPingResponseToServer(int time);
        void SendUsercmdsToServer();
        void InitGame(int serverGameInitId, int serverGameFrame, int serverGameTime, Dictionary<string, string> serverSI);
        void ProcessUnreliableServerMessage(BitMsg msg);
        void ProcessReliableServerMessages();
        void ProcessChallengeResponseMessage(Netadr from, BitMsg msg);
        void ProcessConnectResponseMessage(Netadr from, BitMsg msg);
        void ProcessDisconnectMessage(Netadr from, BitMsg msg);
        void ProcessInfoResponseMessage(Netadr from, BitMsg msg);
        void ProcessPrintMessage(Netadr from, BitMsg msg);
        void ProcessServersListMessage(Netadr from, BitMsg msg);
        void ProcessAuthKeyMessage(Netadr from, BitMsg msg);
        void ProcessVersionMessage(Netadr from, BitMsg msg);
        void ConnectionlessMessage(Netadr from, BitMsg msg);
        void ProcessMessage(Netadr from, BitMsg msg);
        void SetupConnection();
        void ProcessPureMessage(Netadr from, BitMsg msg);
        bool ValidatePureServerChecksums(Netadr from, BitMsg msg);
        void ProcessReliableMessagePure(BitMsg msg);
        static string HandleGuiCommand(string cmd);
        string HandleGuiCommandInternal(string cmd);
        void SendVersionDLUpdate(int state);
        void HandleDownloads();
        void Idle();
        int UpdateTime(int clamp);
        void ReadLocalizedServerString(BitMsg msg, out string o, int maxLen);
        bool CheckTimeout();
        void ProcessDownloadInfoMessage(Netadr from, BitMsg msg);
        int GetDownloadRequest(int[] checksums, int count);
    }
}