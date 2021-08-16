using System.NumericsX.Sys;

namespace System.NumericsX.Core
{
    // needed by the gui system for the load game menu
    public struct LogStats
    {
        public const int MAX_LOGGED_STATS = 60 * 120;       // log every half second

        public short health;
        public short heartRate;
        public short stamina;
        public short combat;
    }

    public enum MSG
    {
        OK,
        ABORT,
        OKCANCEL,
        YESNO,
        PROMPT,
        CDKEY,
        INFO,
        WAIT
    }

    public delegate string HandleGuiCommand(string s);

    public interface ISession
    {
        //// The render world and sound world used for this session.
        //RenderWorld rw;
        //SoundWorld sw;
        //// The renderer and sound system will write changes to writeDemo.
        //// Demos can be recorded and played at the same time when splicing.
        //DemoFile readDemo;
        //DemoFile writeDemo;
        //int renderdemoVersion;

        // Called in an orderly fashion at system startup, so commands, cvars, files, etc are all available.
        void Init();

        // Shut down the session.
        void Shutdown();

        // Called on errors and game exits.
        void Stop();

        // Redraws the screen, handling games, guis, console, etc during normal once-a-frame updates, outOfSequence will be false,
        // but when the screen is updated in a modal manner, as with utility output, the mouse cursor will be released if running windowed.
        void UpdateScreen(bool outOfSequence = true);

        // Called when console prints happen, allowing the loading screen to redraw if enough time has passed.
        void PacifierUpdate();

        // Called every frame, possibly spinning in place if we are above maxFps, or we haven't advanced at least one demo frame.
        // Returns the number of milliseconds since the last frame.
        void Frame();

        // Returns true if a multiplayer game is running.
        // CVars and commands are checked differently in multiplayer mode.
        bool IsMultiplayer { get; }

        // Processes the given event.
        bool ProcessEvent(SysEvent event_);

        // Activates the main menu
        void StartMenu(bool playIntro = false);

        void SetGUI(object gui, HandleGuiCommand handle);

        // Updates gui and dispatched events to it
        void GuiFrameEvents();

        // fires up the optional GUI event, also returns them if you set wait to true
        // if MSG_PROMPT and wait, returns the prompt string or NULL if aborted
        // if MSG_CDKEY and want, returns the cd key or NULL if aborted
        // network tells wether one should still run the network loop in a wait dialog
        string MessageBox(MSG type, string message, string title = null, bool wait = false, string fire_yes = null, string fire_no = null, bool network = false);
        void StopBox();
        // monitor this download in a progress box to either abort or completion
        void DownloadProgressBox(BackgroundDownload bgl, string title, int progress_start = 0, int progress_end = 100);

        void SetPlayingSoundWorld();

        // this is used by the sound system when an OnDemand sound is loaded, so the game action doesn't advance and get things out of sync
        void TimeHitch(int msec);

        // read and write the cd key data to files
        // doesn't perform any validity checks
        void ReadCDKey();
        void WriteCDKey();

        // returns NULL for if xp is true and xp key is not valid or not present
        string GetCDKey(bool xp);

        // check keys for validity when typed in by the user ( with checksum verification ) store the new set of keys if they are found valid
        bool CheckKey(string key, bool netConnect, bool[] offline_valid);

        // verify the current set of keys for validity strict -> keys in state CDKEY_CHECKING state are not ok
        bool CDKeysAreValid(bool strict);
        // wipe the key on file if the network check finds it invalid
        void ClearCDKey(bool[] valid);

        // configure gui variables for mainmenu.gui and cd key state
        void SetCDKeyGuiVars();

        bool WaitingForGameAuth();

        // got reply from master about the keys. if !valid, auth_msg given
        void CDKeysAuthReply(bool valid, string auth_msg);

        string CurrentMapName { get; }

        int SaveGameVersion { get; }
    }
}
