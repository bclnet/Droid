using System;
using System.Collections.Generic;

namespace Droid.Core
{
    public interface ICVarSystem
    {
        void Init();
        void Shutdown();
        bool IsInitialized();

        // Registers a CVar.
        void Register(CVar cvar);

        // Finds the CVar with the given name.
        // Returns null if there is no CVar with the given name.
        CVar Find(string name);

        // Sets the value of a CVar by name.
        void SetCVarString(string name, string value, CVAR flags = 0);
        void SetCVarBool(string name, bool value, CVAR flags = 0);
        void SetCVarInteger(string name, int value, CVAR flags = 0);
        void SetCVarFloat(string name, float value, CVAR flags = 0);

        // Gets the value of a CVar by name.
        string GetCVarString(string name);
        bool GetCVarBool(string name);
        int GetCVarInteger(string name);
        float GetCVarFloat(string name);

        // Called by the command system when argv(0) doesn't match a known command.
        // Returns true if argv(0) is a variable reference and prints or changes the CVar.
        bool Command(CmdArgs args);

        // Command and argument completion using callback for each valid string.
        void CommandCompletion(Action<string> callback);
        void ArgCompletion(string cmdString, Action<string> callback);

        // Sets/gets/clears modified flags that tell what kind of CVars have changed.
        void SetModifiedFlags(CVAR flags);
        CVAR GetModifiedFlags();
        void ClearModifiedFlags(CVAR flags);

        // Resets variables with one of the given flags set.
        void ResetFlaggedVariables(CVAR flags);

        // Removes auto-completion from the flagged variables.
        void RemoveFlaggedAutoCompletion(CVAR flags);

        // Writes variables with one of the given flags set to the given file.
        void WriteFlaggedVariables(CVAR flags, string setCmd, VFile f);

        // Moves CVars to and from dictionaries.
        Dictionary<string, object> MoveCVarsToDict(CVAR flags);
        void SetCVarsFromDict(Dictionary<string, object> dict);
    }
}