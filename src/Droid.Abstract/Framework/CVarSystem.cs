using System;
using System.Collections.Generic;
using System.IO;

namespace Droid.Framework
{
    [Flags]
    public enum CVAR
    {
        ALL = -1,       // all flags
        BOOL = 1 << 0,  // variable is a boolean
        INTEGER = 1 << 1,   // variable is an integer
        FLOAT = 1 << 2, // variable is a float
        SYSTEM = 1 << 3,    // system variable
        RENDERER = 1 << 4,  // renderer variable
        SOUND = 1 << 5, // sound variable
        GUI = 1 << 6,   // gui variable
        GAME = 1 << 7,  // game variable
        TOOL = 1 << 8,  // tool variable
        USERINFO = 1 << 9,  // sent to servers, available to menu
        SERVERINFO = 1 << 10,   // sent from servers, available to menu
        NETWORKSYNC = 1 << 11,  // cvar is synced from the server to clients
        STATIC = 1 << 12,   // statically declared, not user created
        CHEAT = 1 << 13,    // variable is considered a cheat
        NOCHEAT = 1 << 14,  // variable is not considered a cheat
        INIT = 1 << 15, // can only be set from the command-line
        ROM = 1 << 16,  // display only, cannot be set by user at all
        ARCHIVE = 1 << 17,  // set to cause it to be saved to a config file
        MODIFIED = 1 << 18  // set when the variable is modified
    }

    public class CVar
    {
        static readonly CVar Empty = new();

        // Never use the default constructor.
        protected CVar() { } // Debug.Assert(GetType() != typeof(CVar)); }

        // Always use one of the following constructors.
        public CVar(string name, string value, CVAR flags, string description, ArgCompletion valueCompletion = null)
            : this(name, value, flags, description, 1, -1, null, valueCompletion) { }
        public CVar(string name, string value, CVAR flags, string description, float valueMin, float valueMax, ArgCompletion valueCompletion = null)
            : this(name, value, flags, description, valueMin, valueMax, null, valueCompletion) { }
        public CVar(string name, string value, CVAR flags, string description, string[] valueStrings, ArgCompletion valueCompletion = null)
            : this(name, value, flags, description, 1, -1, valueStrings, valueCompletion) { }
        CVar(string name, string value, CVAR flags, string description, float valueMin, float valueMax, string[] valueStrings, ArgCompletion valueCompletion)
        {
            if (valueCompletion == null && (flags & CVAR.BOOL) != 0)
                valueCompletion = CmdSystemX.ArgCompletion_Boolean;
            this.name = name;
            this.value = value;
            this.flags = flags | CVAR.STATIC;
            this.description = description;
            this.valueMin = valueMin;
            this.valueMax = valueMax;
            this.valueStrings = valueStrings;
            this.valueCompletion = valueCompletion;
            this.integerValue = 0;
            this.floatValue = 0.0f;
            this.internalVar = this;
            if (staticVars != Empty)
            {
                this.next = staticVars;
                staticVars = this;
            }
            else
                G.cvarSystem.Register(this);
        }

        public string Name => internalVar.name;
        public CVAR Flags => internalVar.flags;
        public string Description => internalVar.description;
        public float MinValue => internalVar.valueMin;
        public float MaxValue => internalVar.valueMax;
        public string[] ValueStrings => valueStrings;
        public ArgCompletion GetValueCompletion() => valueCompletion;

        public bool Modified
        {
            get => (internalVar.flags & CVAR.MODIFIED) != 0;
            set => internalVar.flags |= CVAR.MODIFIED;
        }
        public void ClearModified() => internalVar.flags &= ~CVAR.MODIFIED;

        public string String
        {
            get => internalVar.value;
            set => internalVar.InternalSetString(value);
        }
        public bool Bool
        {
            get => internalVar.integerValue != 0;
            set => internalVar.InternalSetBool(value);
        }
        public int Integer
        {
            get => internalVar.integerValue;
            set => internalVar.InternalSetInteger(value);
        }
        public float Float
        {
            get => internalVar.floatValue;
            set => internalVar.InternalSetFloat(value);
        }

        public CVar InternalVar
        {
            get => internalVar;
            set => internalVar = value;
        }

        public void RegisterStaticVars()
        {
            if (staticVars != Empty)
            {
                for (var cvar = staticVars; cvar != null; cvar = cvar.next)
                    G.cvarSystem.Register(cvar);
                staticVars = Empty;
            }
        }

        protected string name;                      // name
        protected string value;                     // value
        protected string description;               // description
        protected internal CVAR flags;              // CVAR_? flags
        protected float valueMin;                   // minimum value
        protected float valueMax;                   // maximum value
        protected string[] valueStrings;            // valid value strings
        protected internal ArgCompletion valueCompletion;    // value auto-completion function
        protected int integerValue;                 // atoi( string )
        protected float floatValue;                 // atof( value )
        protected CVar internalVar;                 // internal cvar
        protected CVar next;                        // next statically declared cvar

        protected internal virtual void InternalSetString(string newValue) { }
        protected internal virtual void InternalSetBool(bool newValue) { }
        protected internal virtual void InternalSetInteger(int newValue) { }
        protected internal virtual void InternalSetFloat(float newValue) { }

        static CVar staticVars;
    }

    public interface CVarSystem
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