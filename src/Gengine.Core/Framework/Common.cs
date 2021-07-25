namespace Gengine.Framework
{
    //#define STRTABLE_ID				"#str_"
    //#define STRTABLE_ID_LENGTH		5

    //extern idCVar vr_refresh;
    //extern idCVar vr_supersampling;
    //extern idCVar vr_msaa;


    //extern idCVar		com_version;
    //extern idCVar		com_skipRenderer;
    //extern idCVar		com_asyncInput;
    //extern idCVar		com_asyncSound;
    //extern idCVar		com_purgeAll;
    //extern idCVar		com_developer;
    //extern idCVar		com_allowConsole;
    //extern idCVar		com_speeds;
    //extern idCVar		com_showFPS;
    //extern idCVar		com_showMemoryUsage;
    //extern idCVar		com_showAsyncStats;
    //extern idCVar		com_showSoundDecoders;
    //extern idCVar		com_makingBuild;
    //extern idCVar		com_updateLoadSize;

    //extern int			time_gameFrame;			// game logic time
    //extern int			time_gameDraw;			// game present time
    //extern int			time_frontend;			// renderer frontend time
    //extern int			time_backend;			// renderer backend time

    //extern int			com_frameTime;			// time for the current frame in milliseconds
    //extern volatile int	com_ticNumber;			// 60 hz tics, incremented by async function
    //extern int			com_editors;			// current active editor(s)
    //extern bool			com_editorActive;		// true if an editor has focus

    //#ifdef _WIN32
    //const char			DMAP_MSGID[] = "DMAPOutput";
    //const char			DMAP_DONE[] = "DMAPDone";
    //extern HWND			com_hwndMsg;
    //extern bool			com_outputMsg;
    //#endif

    public struct MemInfo
    {
        public string filebase;

        public int total;
        public int assetTotals;

        // memory manager totals
        public int memoryManagerTotal;

        // subsystem totals
        public int gameSubsystemTotal;
        public int renderSubsystemTotal;

        // asset totals
        public int imageAssetsTotal;
        public int modelAssetsTotal;
        public int soundAssetsTotal;
    }
}