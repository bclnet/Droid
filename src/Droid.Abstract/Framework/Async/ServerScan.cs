using Droid.Sys;
using Droid.UI;
using System.Collections.Generic;

namespace Droid.Framework.Async
{
    // storage for incoming servers / server scan
    public struct InServer_t
    {
        public Netadr adr;
        public int id;
        public int time;
    }

    // the menu gui uses a hard-coded control type to display a list of network games
    public class NetworkServer
    {
        public Netadr adr;
        public Dictionary<string, string> serverInfo;
        public int ping;
        public int id;          // idnet mode sends an id for each server in list
        public int clients;
        public string[] nickname = new string[MAX_ASYNC_CLIENTS];
        public short[] pings = new short[MAX_ASYNC_CLIENTS];
        public int[] rate = new int[MAX_ASYNC_CLIENTS];
        public int challenge;
    }

    public enum ServerSort
    {
        SORT_PING,
        SORT_SERVERNAME,
        SORT_PLAYERS,
        SORT_GAMETYPE,
        SORT_MAP,
        SORT_GAME
    }

    public class ServerScan : List<NetworkServer>
    {
        public ServerScan();

        public int InfoResponse(out NetworkServer server);

        // add an internet server - ( store a numeric id along with it )
        public void AddServer(int id, string srv);

        // we are going to feed server entries to be pinged. if timeout is true, use a timeout once we start AddServer to trigger EndServers and decide the scan is done
        public void StartServers(bool timeout);
        // we are done filling up the list of server entries
        public void EndServers();

        // scan the current list of servers - used for refreshes and while receiving a fresh list
        public void NetScan();

        // clear
        public void Clear();

        // called each game frame. Updates the scanner state, takes care of ongoing scans
        public void RunFrame();

        public enum ScanState
        {
            IDLE = 0,
            WAIT_ON_INIT,
            LAN_SCAN,
            NET_SCAN
        }

        public ScanState State
        {
            get => scan_state;
            set
            {
                //void SetState(scan_state a);
            }
        }

        public bool GetBestPing(NetworkServer serv);

        // prepare for a LAN scan. idAsyncClient does the network job (UDP broadcast), we do the storage
        public void SetupLANScan();

        public void GUIConfig(IUserInterface pGUI, string name);
        // update the GUI fields with information about the currently selected server
        public void GUIUpdateSelected();

        public void Shutdown();

        public void ApplyFilter();

        // there is an internal toggle, call twice with same sort to switch
        public void SetSorting(ServerSort sort);

        public int GetChallenge();

        static const int MAX_PINGREQUESTS = 32;     // how many servers to query at once
        static const int REPLY_TIMEOUT = 999;       // how long should we wait for a reply from a game server
        static const int INCOMING_TIMEOUT = 1500;       // when we got an incoming server list, how long till we decide the list is done
        static const int REFRESH_START = 10000; // how long to wait when sending the initial refresh request

        ScanState scan_state;

        bool incoming_net;  // set to true while new servers are fed through AddServer
        bool incoming_useTimeout;
        int incoming_lastTime;

        int lan_pingtime;   // holds the time of LAN scan

        // servers we're waiting for a reply from won't exceed MAX_PINGREQUESTS elements
        // holds index of net_servers elements, indexed by 'from' string
        Dictionary<string, string> net_info;

        List<InServer> net_servers = new();
        // where we are in net_servers list for getInfo emissions ( NET_SCAN only )
        // we may either be waiting on MAX_PINGREQUESTS, or for net_servers to grow some more ( through AddServer )
        int cur_info;

        IUserInterface m_pGUI;
        ListGUI listGUI;

        ServerSort m_sort;
        bool m_sortAscending;
        List<int> m_sortedServers;  // use ascending for the walking order

        string screenshot;
        int challenge;          // challenge for current scan

        int endWaitTime;        // when to stop waiting on a port init

        void LocalClear();      // we need to clear some internal data as well

        void EmitGetInfo(Netadr serv);
        void GUIAdd(int id, NetworkServer server);
        bool IsFiltered(NetworkServer server);

        static int Cmp(ref int a, ref int b);
    }
}