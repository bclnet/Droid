using Droid.Framework;
using Droid.Sys;
using System.Collections.Generic;
using System.IO;

namespace Droid.UI
{
    public interface UserInterface
    {
        // Returns the name of the gui.
        string Name();

        // Returns a comment on the gui.
        string Comment();

        // Returns true if the gui is interactive.
        bool IsInteractive { get; }

        bool Uniqued { get; set; }

        // returns false if it failed to load
        bool InitFromFile(string qpath, bool rebuild = true, bool cache = true);

        // handles an event, can return an action string, the caller interprets any return and acts accordingly
        string HandleEvent(sysEvent e, int time, out bool updateVisuals);

        // handles a named event
        void HandleNamedEvent(string eventName);

        // repaints the ui
        void Redraw(int time);

        // repaints the cursor
        void DrawCursor();

        // Provides read access to the idDict that holds this gui's state.
        Dictionary<string, object> State { get; }

        // Removes a gui state variable
        void DeleteStateVar(string varName);

        // Sets a gui state variable.
        void SetStateString(string varName, string value);
        void SetStateBool(string varName, bool value);
        void SetStateInt(string varName, int value);
        void SetStateFloat(string varName, float value);

        // Gets a gui state variable
        string GetStateString(string varName, string defaultString = "");
        bool GetStateBool(string varName, string defaultString = "0");
        int GetStateInt(string varName, string defaultString = "0");
        float GetStateFloat(string varName, string defaultString = "0");

        // The state has changed and the gui needs to update from the state idDict.
        void StateChanged(int time, bool redraw = false);

        // Activated the gui.
        string Activate(bool activate, int time);

        // Triggers the gui and runs the onTrigger scripts.
        void Trigger(int time);

        void ReadFromDemoFile(DemoFile f);
        void WriteToDemoFile(DemoFile f);

        bool WriteToSaveGame(VFile savefile);
        bool ReadFromSaveGame(VFile savefile);
        void SetKeyBindingNames();

        void SetCursor(float x, float y);
        float CursorX();
        float CursorY();
    }

    public interface UserInterfaceManager
    {
        void Init();
        void Shutdown();
        void Touch(string name);
        void WritePrecacheCommands(VFile f);

        // Sets the size for 640x480 adjustment.
        void SetSize(float width, float height);

        void BeginLevelLoad();
        void EndLevelLoad();

        // Reloads changed guis, or all guis.
        void Reload(bool all);

        // lists all guis
        void ListGuis();

        // Returns true if gui exists.
        bool CheckGui(string qpath);

        // Allocates a new gui.
        UserInterface Alloc();

        // De-allocates a gui.. ONLY USE FOR PRECACHING
        void DeAlloc(UserInterface gui);

        // Returns NULL if gui by that name does not exist.
        UserInterface FindGui(string qpath, bool autoLoad = false, bool needUnique = false, bool forceUnique = false);

        // Returns NULL if gui by that name does not exist.
        UserInterface FindDemoGui(string qpath);

        // Allocates a new GUI list handler
        ListGUI AllocListGUI();

        // De-allocates a list gui
        void FreeListGUI(ListGUI listgui);
    }
}