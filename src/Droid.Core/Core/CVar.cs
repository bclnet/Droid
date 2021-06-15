using System;

namespace Droid.Core
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
        static CVar staticVars;

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
                valueCompletion = CmdArgsX.ArgCompletion_Boolean;
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
                Lib.cvarSystem.Register(this);
        }

        public string Name => internalVar.name;
        public CVAR Flags => internalVar.flags;
        public string Description => internalVar.description;
        public float MinValue => internalVar.valueMin;
        public float MaxValue => internalVar.valueMax;
        public string[] ValueStrings => valueStrings;
        public ArgCompletion GetValueCompletion() => valueCompletion;

        public bool IsModified
            => (internalVar.flags & CVAR.MODIFIED) != 0;
        public void SetModified()
            => internalVar.flags |= CVAR.MODIFIED;
        public void ClearModified()
            => internalVar.flags &= ~CVAR.MODIFIED;

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

        public static Action<CVar> Register;

        public void RegisterStaticVars()
        {
            if (staticVars != Empty)
            {
                for (var cvar = staticVars; cvar != null; cvar = cvar.next)
                    Lib.cvarSystem.Register(cvar);
                staticVars = Empty;
            }
        }

        protected internal virtual void InternalSetString(string newValue) { }
        protected internal virtual void InternalSetBool(bool newValue) { }
        protected internal virtual void InternalSetInteger(int newValue) { }
        protected internal virtual void InternalSetFloat(float newValue) { }
    }
}