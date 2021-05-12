using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Droid.Framework
{

    class InternalCVar : CVar
    {
        public InternalCVar() { }
        public InternalCVar(string newName, string newValue, CVAR newFlags)
        {
            nameString = newName;
            name = nameString;
            valueString = newValue;
            value = valueString;
            resetString = newValue;
            descriptionString = string.Empty;
            description = descriptionString;
            flags = (newFlags & ~CVAR.STATIC) | CVAR.MODIFIED;
            valueMin = 1;
            valueMax = -1;
            valueStrings = null;
            valueCompletion = null;
            UpdateValue();
            UpdateCheat();
            internalVar = this;
        }
        public InternalCVar(CVar cvar)
        {
            nameString = cvar.Name;
            name = nameString;
            valueString = cvar.String;
            value = valueString;
            resetString = cvar.String;
            descriptionString = cvar.Description;
            description = descriptionString;
            flags = cvar.Flags | CVAR.MODIFIED;
            valueMin = cvar.MinValue;
            valueMax = cvar.MaxValue;
            valueStrings = cvar.ValueStrings;
            valueCompletion = cvar.GetValueCompletion();
            UpdateValue();
            UpdateCheat();
            internalVar = this;
        }

        public void Update(CVar cvar)
        {
            // if this is a statically declared variable
            if ((cvar.Flags & CVAR.STATIC) != 0)
            {
                if ((flags & CVAR.STATIC) != 0)
                {
                    // the code has more than one static declaration of the same variable, make sure they have the same properties
                    if (!string.Equals(resetString, cvar.String, StringComparison.OrdinalIgnoreCase))
                        G.common.Warning($"CVar '{nameString}' declared multiple times with different initial value");
                    if ((flags & (CVAR.BOOL | CVAR.INTEGER | CVAR.FLOAT)) != (cvar.Flags & (CVAR.BOOL | CVAR.INTEGER | CVAR.FLOAT)))
                        G.common.Warning($"CVar '{nameString}' declared multiple times with different type");
                    if (valueMin != cvar.MinValue || valueMax != cvar.MaxValue)
                        G.common.Warning($"CVar '{nameString}' declared multiple times with different minimum/maximum");
                }

                // the code is now specifying a variable that the user already set a value for, take the new value as the reset value
                resetString = cvar.String;
                descriptionString = cvar.Description;
                description = descriptionString;
                valueMin = cvar.MinValue;
                valueMax = cvar.MaxValue;
                valueStrings = cvar.ValueStrings;
                valueCompletion = cvar.GetValueCompletion();
                UpdateValue();
                G.cvarSystem.SetModifiedFlags(cvar.Flags);
            }

            flags |= cvar.Flags;

            UpdateCheat();

            // only allow one non-empty reset string without a warning
            if (resetString.Length == 0)
                resetString = cvar.String;
            else if (cvar.String.Length != 0 && resetString != cvar.String)
                G.common.Warning($"cvar \"{nameString}\" given initial values: \"{resetString}\" and \"{cvar.String}\"\n");
        }
        public void UpdateValue()
        {
            var clamped = false;

            if ((flags & CVAR.BOOL) != 0)
            {
                integerValue = (int.TryParse(value, out var z) ? z : 0) != 0 ? 1 : 0;
                floatValue = integerValue;
                if (value != "0" && value != "1")
                    value = valueString = integerValue != 0 ? "true" : "false";
            }
            else if ((flags & CVAR.INTEGER) != 0)
            {
                integerValue = int.TryParse(value, out var z) ? z : 0;
                if (valueMin < valueMax)
                {
                    if (integerValue < valueMin) { integerValue = (int)valueMin; clamped = true; }
                    else if (integerValue > valueMax) { integerValue = (int)valueMax; clamped = true; }
                }
                if (clamped || !value.All(char.IsNumber) || value.IndexOf('.') != 0)
                    value = valueString = integerValue.ToString();
                floatValue = integerValue;
            }
            else if ((flags & CVAR.FLOAT) != 0)
            {
                floatValue = float.TryParse(value, out var z) ? z : 0f;
                if (valueMin < valueMax)
                {
                    if (floatValue < valueMin) { floatValue = valueMin; clamped = true; }
                    else if (floatValue > valueMax) { floatValue = valueMax; clamped = true; }
                }
                if (clamped || !value.All(char.IsNumber))
                    value = valueString = floatValue.ToString();
                integerValue = (int)floatValue;
            }
            else
            {
                if (valueStrings != null && valueStrings.Length > 0)
                {
                    integerValue = 0;
                    for (var i = 0; valueStrings.Length < i; i++)
                        if (string.Equals(valueString, valueStrings[i], StringComparison.OrdinalIgnoreCase))
                        {
                            integerValue = i;
                            break;
                        }
                    value = valueString = valueStrings[integerValue];
                    floatValue = integerValue;
                }
                else if (valueString.Length < 32)
                    integerValue = (int)(floatValue = float.TryParse(value, out var z) ? z : 0f);
                else
                    integerValue = (int)(floatValue = 0.0f);
            }
        }
        public void UpdateCheat()
        {
            // all variables are considered cheats except for a few types
            if ((flags & (CVAR.NOCHEAT | CVAR.INIT | CVAR.ROM | CVAR.ARCHIVE | CVAR.USERINFO | CVAR.SERVERINFO | CVAR.NETWORKSYNC)) != 0) flags &= ~CVAR.CHEAT;
            else flags |= CVAR.CHEAT;
        }
        public void Set(string newValue, bool force, bool fromServer)
        {
            if (G.session != null && G.session.IsMultiplayer() && !fromServer)
            {
#if !TYPEINFO
                if ((flags & CVAR.NETWORKSYNC) != 0 && AsyncNetwork.client.IsActive())
                {
                    G.common.Printf($"{nameString} is a synced over the network and cannot be changed on a multiplayer client.\n");
#if ALLOW_CHEATS
                    G.common.Printf("ALLOW_CHEATS override!\n");
#else
                    return;
#endif
                }
#endif
                if ((flags & CVAR.CHEAT) != 0 && !G.cvarSystem.GetCVarBool("net_allowCheats"))
                {
                    G.common.Printf($"{nameString} cannot be changed in multiplayer.\n");
#if ALLOW_CHEATS
                    G.common.Printf("ALLOW_CHEATS override!\n");
#else
                    return;
#endif
                }
            }

            if (newValue == null)
                newValue = resetString;

            if (!force)
            {
                if ((flags & CVAR.ROM) != 0)
                {
                    G.common.Printf($"{nameString} is read only.\n");
                    return;
                }

                if ((flags & CVAR.INIT) != 0)
                {
                    G.common.Printf($"{nameString} is write protected.\n");
                    return;
                }
            }

            if (string.Equals(valueString, newValue, StringComparison.OrdinalIgnoreCase))
                return;

            valueString = newValue;
            value = valueString;
            UpdateValue();

            Modified = true;
            G.cvarSystem.SetModifiedFlags(flags);
        }

        public void Reset()
        {
            valueString = resetString;
            value = valueString;
            UpdateValue();
        }

        internal string nameString;          // name
        internal string resetString;         // resetting will change to this value
        internal string valueString;         // value
        internal string descriptionString;   // description

        protected internal override void InternalSetString(string newValue) => Set(newValue, true, false);
        protected internal virtual void InternalServerSetString(string newValue) => Set(newValue, true, true);
        protected internal override void InternalSetBool(bool newValue) => Set(newValue.ToString(), true, false);
        protected internal override void InternalSetInteger(int newValue) => Set(newValue.ToString(), true, false);
        protected internal override void InternalSetFloat(float newValue) => Set(newValue.ToString(), true, false);
    }

    class CVarSystemLocal : CVarSystem
    {
        public CVarSystemLocal()
        {
            initialized = false;
            modifiedFlags = 0;
        }

        public override void Init()
        {
            modifiedFlags = 0;

            G.cmdSystem.AddCommand("toggle", Toggle_f, CMD_FL.SYSTEM, "toggles a cvar");
            G.cmdSystem.AddCommand("set", Set_f, CMD_FL.SYSTEM, "sets a cvar");
            G.cmdSystem.AddCommand("sets", SetS_f, CMD_FL.SYSTEM, "sets a cvar and flags it as server info");
            G.cmdSystem.AddCommand("setu", SetU_f, CMD_FL.SYSTEM, "sets a cvar and flags it as user info");
            G.cmdSystem.AddCommand("sett", SetT_f, CMD_FL.SYSTEM, "sets a cvar and flags it as tool");
            G.cmdSystem.AddCommand("seta", SetA_f, CMD_FL.SYSTEM, "sets a cvar and flags it as archive");
            G.cmdSystem.AddCommand("reset", Reset_f, CMD_FL.SYSTEM, "resets a cvar");
            G.cmdSystem.AddCommand("listCvars", List_f, CMD_FL.SYSTEM, "lists cvars");
            G.cmdSystem.AddCommand("cvar_restart", Restart_f, CMD_FL.SYSTEM, "restart the cvar system");

            initialized = true;
        }
        public override void Shutdown()
        {
            cvars.DeleteContents(true);
            cvarHash.Free();
            moveCVarsToDict.Clear();
            initialized = false;
        }

        public override bool IsInitialized() => initialized;

        public override void Register(CVar cvar)
        {
            cvar.InternalVar = cvar;

            var internal_ = FindInternal(cvar.Name);

            if (internal_ != null)
                internal_.Update(cvar);
            else
            {
                internal_ = new InternalCVar(cvar);
                var hash = cvarHash.GenerateKey(internal_.nameString, false);
                cvarHash.Add(hash, cvars.Append(internal_));
            }

            cvar.InternalVar = internal_;
        }

        public override CVar Find(string name) => FindInternal(name);
        public override void SetCVarString(string name, string value, CVAR flags = 0) => SetInternal(name, value, flags);
        public override void SetCVarBool(string name, bool value, CVAR flags = 0) => SetInternal(name, value.ToString(), flags);
        public override void SetCVarInteger(string name, int value, CVAR flags = 0) => SetInternal(name, value.ToString(), flags);
        public override void SetCVarFloat(string name, float value, CVAR flags = 0) => SetInternal(name, value.ToString(), flags);
        public override string GetCVarString(string name) => FindInternal(name)?.String ?? string.Empty;
        public override bool GetCVarBool(string name) => FindInternal(name)?.Bool ?? false;
        public override int GetCVarInteger(string name) => FindInternal(name)?.Integer ?? 0;
        public override float GetCVarFloat(string name) => FindInternal(name)?.Float ?? 0f;

        public override bool Command(CmdArgs args)
        {
            var internal_ = FindInternal(args[0]);

            if (internal_ == null)
                return false;

            if (args.Count == 1)
            {
                // print the variable
                G.common.Printf($"\"{internal_.nameString}\" is:\"{internal_.valueString}\"{G.S_COLOR_WHITE} default:\"{internal_.resetString}\"\n");
                if (internal_.Description.Length > 0)
                    G.common.Printf($"{G.S_COLOR_WHITE}{internal_.Description}\n");
            }
            // set the value
            else internal_.Set(args.Args(), false, false);
            return true;
        }

        public override void CommandCompletion(Action<string> callback)
        {
            for (var i = 0; i < cvars.Count; i++)
                callback(cvars[i].Name);
        }
        public override void ArgCompletion(string cmdString, Action<string> callback)
        {
            var args = new CmdArgs();
            args.TokenizeString(cmdString, false);

            for (var i = 0; i < cvars.Count; i++)
            {
                if (cvars[i].valueCompletion == null)
                    continue;
                if (string.Equals(args[0], cvars[i].nameString, StringComparison.OrdinalIgnoreCase))
                {
                    cvars[i].valueCompletion(args, callback);
                    break;
                }
            }
        }

        public override void SetModifiedFlags(CVAR flags) => modifiedFlags |= flags;
        public override CVAR GetModifiedFlags() => modifiedFlags;
        public override void ClearModifiedFlags(CVAR flags) => modifiedFlags &= ~flags;

        public override void ResetFlaggedVariables(CVAR flags)
        {
            for (var i = 0; i < cvars.Count; i++)
            {
                var cvar = cvars[i];
                if ((cvar.Flags & flags) != 0)
                    cvar.Set(null, true, true);
            }
        }
        public override void RemoveFlaggedAutoCompletion(CVAR flags)
        {
            for (var i = 0; i < cvars.Count; i++)
            {
                var cvar = cvars[i];
                if ((cvar.Flags & flags) != 0)
                    cvar.valueCompletion = null;
            }
        }
        public override void WriteFlaggedVariables(CVAR flags, string setCmd, VFile f)
        {
            for (var i = 0; i < cvars.Count; i++)
            {
                var cvar = cvars[i];
                if ((cvar.Flags & flags) != 0)
                    f.Printf($"{setCmd} {cvar.Name} \"{cvar.String}\"\n");
            }
        }

        public override Dictionary<string, object> MoveCVarsToDict(CVAR flags)
        {
            moveCVarsToDict.Clear();
            for (var i = 0; i < cvars.Count; i++)
            {
                var cvar = cvars[i];
                if ((cvar.Flags & flags) != 0)
                    moveCVarsToDict.Set(cvar.Name, cvar.String);
            }
            return moveCVarsToDict;
        }
        public override void SetCVarsFromDict(Dictionary<string, object> dict)
        {
            for (var i = 0; i < dict.GetNumKeyVals(); i++)
            {
                var kv = dict.GetKeyVal(i);

                var internal_ = FindInternal(kv.GetKey());
                if (internal_ != null)
                    internal_.InternalServerSetString(kv.GetValue());
            }
        }

        public void RegisterInternal(CVar cvar) { }
        public InternalCVar FindInternal(string name)
        {
            var hash = cvarHash.GenerateKey(name, false);
            for (var i = cvarHash.First(hash); i != -1; i = cvarHash.Next(i))
                if (cvars[i].nameString.Icmp(name) == 0)
                    return cvars[i];
            return null;
        }
        public void SetInternal(string name, string value, CVAR flags)
        {
            var internal_ = FindInternal(name);
            if (internal_ != null)
            {
                internal_.InternalSetString(value);
                internal_.flags |= flags & ~CVAR.STATIC;
                internal_.UpdateCheat();
            }
            else
            {
                internal_ = new InternalCVar(name, value, flags);
                var hash = cvarHash.GenerateKey(internal_.nameString, false);
                cvarHash.Add(hash, cvars.Append(internal_));
            }
        }

        bool initialized;
        List<InternalCVar> cvars;
        HashIndex cvarHash;
        CVAR modifiedFlags;
        // use a static dictionary to MoveCVarsToDict can be used from game
        static Dictionary<string, object> moveCVarsToDict;

        static void Toggle_f(CmdArgs args)
        {
            int argc, i;
            float current, set;
            string text;

            argc = args.Count;
            if (argc < 2)
            {
                G.common.Printf(@"usage:\n"
                    + "   toggle <variable>  - toggles between 0 and 1\n"
                    + "   toggle <variable> <value> - toggles between 0 and <value>\n"
                    + "   toggle <variable> [string 1] [string 2]...[string n] - cycles through all strings\n");
                return;
            }

            var cvar = G.localCVarSystem.FindInternal(args[1]);

            if (cvar == null)
            {
                G.common.Warning("Toggle_f: cvar \"{args[1]}\" not found");
                return;
            }

            if (argc > 3)
            {
                // cycle through multiple values
                text = cvar.String;
                for (i = 2; i < argc; i++)
                    if (string.Equals(text, args[i], StringComparison.OrdinalIgnoreCase)) { i++; break; } // point to next value
                if (i >= argc)
                    i = 2;

                G.common.Printf($"set {args[1]} = {args[i]}\n");
                cvar.Set(args[i], false, false);
            }
            else
            {
                // toggle between 0 and 1
                current = cvar.Float;
                set = argc == 3 ? float.TryParse(args[2], out var z) ? z : 0f : 1.0f;
                current = current == 0.0f ? set : 0.0f;
                G.common.Printf($"set {args[1]} = {current}\n");
                cvar.Set(current.ToString(), false, false);
            }
        }
        static void Set_f(CmdArgs args)
        {
            var str = args.Args(2, args.Count - 1);
            G.localCVarSystem.SetCVarString(args[1], str);
        }

        static void SetS_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = G.localCVarSystem.FindInternal(args[1]);
            if (cvar == null)
                return;
            cvar.flags |= CVAR.SERVERINFO | CVAR.ARCHIVE;
        }

        static void SetU_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = G.localCVarSystem.FindInternal(args[1]);
            if (cvar == null)
                return;
            cvar.flags |= CVAR.USERINFO | CVAR.ARCHIVE;
        }
        static void SetT_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = G.localCVarSystem.FindInternal(args[1]);
            if (cvar == null)
                return;
            cvar.flags |= CVAR.TOOL;
        }
        static void SetA_f(CmdArgs args)
        {
            Set_f(args);
            var cvar = G.localCVarSystem.FindInternal(args[1]);
            if (cvar == null)
                return;

            // FIXME: enable this for ship, so mods can store extra data but during development we don't want obsolete cvars to continue to be saved
            //	cvar.flags |= CVAR.ARCHIVE;
        }
        static void Reset_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                G.common.Printf("usage: reset <variable>\n");
                return;
            }
            var cvar = G.localCVarSystem.FindInternal(args[1]);
            if (cvar == null)
                return;

            cvar.Reset();
        }

        enum SHOW
        {
            VALUE,
            DESCRIPTION,
            TYPE,
            FLAGS
        }

        static void ListByFlags(CmdArgs args, CVAR flags)
        {
            int i;
            
            InternalCVar cvar;
            var cvarList = new List<InternalCVar>();

            var argNum = 1;
            var show = SHOW.VALUE;

            if (string.Equals(args[argNum], "-", StringComparison.OrdinalIgnoreCase) || string.Equals(args[argNum], "/", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(args[argNum + 1], "help", StringComparison.OrdinalIgnoreCase) || string.Equals(args[argNum + 1], "?", StringComparison.OrdinalIgnoreCase))
                {
                    argNum = 3;
                    show = SHOW.DESCRIPTION;
                }
                else if (string.Equals(args[argNum + 1], "type", StringComparison.OrdinalIgnoreCase) || string.Equals(args[argNum + 1], "range", StringComparison.OrdinalIgnoreCase))
                {
                    argNum = 3;
                    show = SHOW.TYPE;
                }
                else if (string.Equals(args[argNum + 1], "flags", StringComparison.OrdinalIgnoreCase))
                {
                    argNum = 3;
                    show = SHOW.FLAGS;
                }
            }

            string match;
            if (args.Count > argNum)
            {
                match = args.Args(argNum, -1);
                match = match.Replace(" ", "");
            }
            else match = string.Empty;

            for (i = 0; i < G.localCVarSystem.cvars.Count; i++)
            {
                cvar = G.localCVarSystem.cvars[i];

                if ((cvar.Flags & flags) == 0)
                    continue;

                if (match.Length != 0 && !cvar.nameString.Filter(match, false))
                    continue;

                cvarList.Add(cvar);
            }

            cvarList.Sort();

            const int NUM_COLUMNS = 77; // 78 - 1
            const int NUM_NAME_CHARS = 33;
            const int NUM_DESCRIPTION_CHARS = NUM_COLUMNS - NUM_NAME_CHARS;

            switch (show)
            {
                case SHOW.VALUE:
                    {
                        for (i = 0; i < cvarList.Count; i++)
                        {
                            cvar = cvarList[i];
                            G.common.Printf($"{cvar.nameString:-32}{G.S_COLOR_WHITE} \"{cvar.valueString}\"\n");
                        }
                        break;
                    }
                case SHOW.DESCRIPTION:
                    {
                        var indent = "\n" + new string(' ', NUM_NAME_CHARS);
                        var b = new StringBuilder();
                        for (i = 0; i < cvarList.Count; i++)
                        {
                            cvar = cvarList[i];
                            G.common.Printf($"{cvar.nameString:-32}{G.S_COLOR_WHITE}{CreateColumn(cvar.Description, NUM_DESCRIPTION_CHARS, indent, b)}\n");
                        }
                        break;
                    }
                case SHOW.TYPE:
                    {
                        for (i = 0; i < cvarList.Count; i++)
                        {
                            cvar = cvarList[i];
                            if ((cvar.Flags & CVAR.BOOL) != 0)
                                G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_CYAN}bool\n");
                            else if ((cvar.Flags & CVAR.INTEGER) != 0)
                            {
                                if (cvar.MinValue < cvar.MaxValue) G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_GREEN}int {G.S_COLOR_WHITE}[{(int)cvar.MinValue}, {(int)cvar.MaxValue}]\n");
                                else G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_GREEN}int\n");
                            }
                            else if ((cvar.Flags & CVAR.FLOAT) != 0)
                            {
                                if (cvar.MinValue < cvar.MaxValue) G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_RED}float {G.S_COLOR_WHITE}[{cvar.MinValue}, {cvar.MaxValue}]\n");
                                else G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_RED}float\n");
                            }
                            else if (cvar.ValueStrings != null)
                            {
                                G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_WHITE}string {G.S_COLOR_WHITE}[");
                                for (var j = 0; cvar.ValueStrings[j] != null; j++)
                                    if (j != 0) G.common.Printf($"{G.S_COLOR_WHITE}, {cvar.ValueStrings[j]}");
                                    else G.common.Printf($"{G.S_COLOR_WHITE}{cvar.ValueStrings[j]}");
                                G.common.Printf($"{G.S_COLOR_WHITE}]\n");
                            }
                            else G.common.Printf($"{cvar.Name:-32}{G.S_COLOR_WHITE}string\n");
                        }
                        break;
                    }
                case SHOW.FLAGS:
                    {
                        for (i = 0; i < cvarList.Count; i++)
                        {
                            cvar = cvarList[i];
                            G.common.Printf($"{cvar.Name:-32}");
                            var s = string.Empty;
                            if ((cvar.Flags & CVAR.BOOL) != 0) s += $"{G.S_COLOR_CYAN}B ";
                            else if ((cvar.Flags & CVAR.INTEGER) != 0) s += $"{G.S_COLOR_GREEN}I ";
                            else if ((cvar.Flags & CVAR.FLOAT) != 0) s += $"{G.S_COLOR_RED}F ";
                            else s += $"{G.S_COLOR_WHITE}S ";
                            if ((cvar.Flags & CVAR.SYSTEM) != 0) s += $"{G.S_COLOR_WHITE}SYS  ";
                            else if ((cvar.Flags & CVAR.RENDERER) != 0) s += $"{G.S_COLOR_WHITE}RNDR ";
                            else if ((cvar.Flags & CVAR.SOUND) != 0) s += $"{G.S_COLOR_WHITE}SND  ";
                            else if ((cvar.Flags & CVAR.GUI) != 0) s += $"{G.S_COLOR_WHITE}GUI  ";
                            else if ((cvar.Flags & CVAR.GAME) != 0) s += $"{G.S_COLOR_WHITE}GAME ";
                            else if ((cvar.Flags & CVAR.TOOL) != 0) s += $"{G.S_COLOR_WHITE}TOOL ";
                            else s += $"{G.S_COLOR_WHITE}     ";
                            s += ((cvar.Flags & CVAR.USERINFO) != 0) ? "UI " : "   ";
                            s += ((cvar.Flags & CVAR.SERVERINFO) != 0) ? "SI " : "   ";
                            s += ((cvar.Flags & CVAR.STATIC) != 0) ? "ST " : "   ";
                            s += ((cvar.Flags & CVAR.CHEAT) != 0) ? "CH " : "   ";
                            s += ((cvar.Flags & CVAR.INIT) != 0) ? "IN " : "   ";
                            s += ((cvar.Flags & CVAR.ROM) != 0) ? "RO " : "   ";
                            s += ((cvar.Flags & CVAR.ARCHIVE) != 0) ? "AR " : "   ";
                            s += ((cvar.Flags & CVAR.MODIFIED) != 0) ? "MO " : "   ";
                            s += "\n";
                            G.common.Printf(s);
                        }
                        break;
                    }
            }

            G.common.Printf($"\n{cvarList.Count} cvars listed\n\n");
            G.common.Printf("listCvar [search string]          = list cvar values\n"
                          + "listCvar -help [search string]    = list cvar descriptions\n"
                          + "listCvar -type [search string]    = list cvar types\n"
                          + "listCvar -flags [search string]   = list cvar flags\n");
        }
        static void List_f(CmdArgs args) => ListByFlags(args, CVAR.ALL);
        static void Restart_f(CmdArgs args)
        {
            for (var i = 0; i < G.localCVarSystem.cvars.Count; i++)
            {
                var cvar = G.localCVarSystem.cvars[i];

                // don't mess with rom values
                if ((cvar.flags & (CVAR.ROM | CVAR.INIT)) != 0)
                    continue;

                // throw out any variables the user created
                if ((cvar.flags & CVAR.STATIC) == 0)
                {
                    var hash = G.localCVarSystem.cvarHash.GenerateKey(cvar.nameString, false);
                    //delete cvar;
                    G.localCVarSystem.cvars.RemoveAt(i);
                    G.localCVarSystem.cvarHash.RemoveIndex(hash, i);
                    i--;
                    continue;
                }

                cvar.Reset();
            }
        }

        //Dict CVarSystemLocal.moveCVarsToDict;

        static string CreateColumn(string text, int columnWidth, string indent, StringBuilder b)
        {
            int i, lastLine;
            b.Clear();
            for (lastLine = i = 0; text[i] != '\0'; i++)
                if (i - lastLine >= columnWidth || text[i] == '\n')
                {
                    while (i > 0 && text[i] > ' ' && text[i] != '/' && text[i] != ',' && text[i] != '\\') i--;
                    while (lastLine < i) b.Append(text[lastLine++]);
                    b.Append(indent);
                    lastLine++;
                }
            while (lastLine < i) b.Append(text[lastLine++]);
            return b.ToString();
        }
    }
}
