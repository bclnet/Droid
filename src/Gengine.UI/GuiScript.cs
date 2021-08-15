using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.Core;
using static Gengine.Lib;
using static System.NumericsX.Lib;

namespace Gengine.UI
{
    internal class Script
    {
        public static void Set(Window window, List<GSWinVar> src)
        {
            string val;
            var dest = (WinStr)src[0].var;
            if (dest != null)
                if (string.Equals(dest, "cmd", StringComparison.OrdinalIgnoreCase))
                {
                    dest = (WinStr)src[1].var;
                    var parmCount = src.Count;
                    if (parmCount > 2)
                    {
                        val = dest.ToString();
                        var i = 2;
                        while (i < parmCount) { val += $" \"{src[i].var}\""; i++; }
                        window.AddCommand(val);
                    }
                    else window.AddCommand(dest);
                    return;
                }
            src[0].var.Set(src[1].var.ToString());
            src[0].var.Eval = false;
        }

        static void SetFocus(Window window, List<GSWinVar> src)
        {
            var parm = (WinStr)src[0].var;
            if (parm != null)
            {
                var win = window.Gui.Desktop.FindChildByName(parm);
                if (win != null && win.win != null)
                    window.SetFocus(win.win);
            }
        }

        static void ShowCursor(Window window, List<GSWinVar> src)
        {
            var parm = (WinStr)src[0].var;
            if (parm != null)
                if (int.Parse(parm) != 0) window.Gui.Desktop.ClearFlag(WIN_NOCURSOR);
                else window.Gui.Desktop.SetFlag(WIN_NOCURSOR);
        }

        // run scripts must come after any set cmd set's in the script
        static void RunScript(Window window, List<GSWinVar> src)
        {
            var parm = (WinStr)src[0].var;
            if (parm != null)
                window.cmd = $"{window.cmd} ; runScript {parm}";
        }

        static void LocalSound(Window window, List<GSWinVar> src)
        {
            var parm = (WinStr)src[0].var;
            if (parm != null)
                session.sw.PlayShaderDirectly(parm);
        }

        static void EvalRegs(Window window, List<GSWinVar> src)
           => window.EvalRegs(-1, true);

        static void EndGame(Window window, List<GSWinVar> src)
        {
            cvarSystem.SetCVarBool("g_nightmare", true);
            cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "disconnect\n");
        }

        static void ResetTime(Window window, List<GSWinVar> src)
        {
            var parm = (WinStr)src[0].var;
            DrawWin win = null;
            if (parm != null && src.Count > 1)
            {
                win = window.Gui.Desktop.FindChildByName(parm);
                parm = (WinStr)src[1].var;
            }
            if (win != null && win.win != null)
            {
                win.win.ResetTime(int.Parse(parm));
                win.win.EvalRegs(-1, true);
            }
            else
            {
                window.ResetTime(int.Parse(parm));
                window.EvalRegs(-1, true);
            }
        }

        static void ResetCinematics(Window window, List<GSWinVar> src)
           => window.ResetCinematics();

        public static void Transition(Window window, List<GSWinVar> src)
        {
            // transitions always affect rect or vec4 vars
            if (src.Count >= 4)
            {
                WinRectangle rect = null;
                var vec4 = (WinVec4)src[0].var;

                WinFloat val = null;
                if (vec4 == null)
                {
                    rect = (WinRectangle)src[0].var;
                    if (rect == null)
                        val = (WinFloat)src[0].var;
                }
                var from = (WinVec4)src[1].var;
                var to = (WinVec4)src[2].var;
                var timeStr = (WinStr)src[3].var;

                if (!((vec4 != null || rect != null || val != null) && from != null && to != null && timeStr != null))
                {
                    common.Warning($"Bad transition in gui {window.Gui.SourceFile} in window {window.Name}\n");
                    return;
                }
                var time = int.Parse(timeStr);
                float ac = 0f, dc = 0f;
                if (src.Count > 4)
                {
                    var acv = (WinStr)src[4].var; var dcv = (WinStr)src[5].var;
                    Debug.Assert(acv != null && dcv != null);
                    ac = float.Parse(acv); dc = float.Parse(dcv);
                }

                if (vec4 != null) { vec4.Eval = false; window.AddTransition(vec4, from, to, time, ac, dc); }
                else if (val != null) { val.Eval = false; window.AddTransition(val, from, to, time, ac, dc); }
                else { rect.Eval = false; window.AddTransition(rect, from, to, time, ac, dc); }
                window.StartTransition();
            }
        }

        public struct GuiCommandDef
        {
            public GuiCommandDef(string name, Action<Window, List<GSWinVar>> handler, int mMinParms, int mMaxParms)
            {
                this.name = name;
                this.handler = handler;
                this.mMinParms = mMinParms;
                this.mMaxParms = mMaxParms;
            }
            public string name;
            public Action<Window, List<GSWinVar>> handler;
            public int mMinParms;
            public int mMaxParms;
        }

        public static readonly GuiCommandDef[] commandList = {
            new GuiCommandDef("set", Set, 2, 999),
            new GuiCommandDef("setFocus", SetFocus, 1, 1),
            new GuiCommandDef("endGame", EndGame, 0, 0),
            new GuiCommandDef("resetTime", ResetTime, 0, 2),
            new GuiCommandDef("showCursor", ShowCursor, 1, 1),
            new GuiCommandDef("resetCinematics", ResetCinematics, 0, 2),
            new GuiCommandDef("transition", Transition, 4, 6),
            new GuiCommandDef("localSound", LocalSound, 1, 1),
            new GuiCommandDef("runScript", RunScript, 1, 1),
            new GuiCommandDef("evalRegs", EvalRegs, 0, 0)
        };
    }

    public struct GSWinVar
    {
        public WinVar var;
        public bool own;
    }

    public class GuiScript
    {
        protected internal int conditionReg;
        protected internal GuiScriptList ifList;
        protected internal GuiScriptList elseList;
        protected List<GSWinVar> parms = new();
        protected Action<Window, List<GSWinVar>> handler;

        public GuiScript()
        {
            ifList = null;
            elseList = null;
            conditionReg = -1;
            handler = null;
            parms.SetGranularity(2);
        }

        public void WriteToSaveGame(VFile savefile)
        {
            if (ifList != null) ifList.WriteToSaveGame(savefile);
            if (elseList != null) elseList.WriteToSaveGame(savefile);

            savefile.Write(conditionReg);
            for (var i = 0; i < parms.Count; i++)
                if (parms[i].own)
                    parms[i].var.WriteToSaveGame(savefile);
        }

        public void ReadFromSaveGame(VFile savefile)
        {
            if (ifList != null) ifList.ReadFromSaveGame(savefile);
            if (elseList != null) elseList.ReadFromSaveGame(savefile);

            savefile.Read(out conditionReg);
            for (var i = 0; i < parms.Count; i++)
                if (parms[i].own)
                    parms[i].var.ReadFromSaveGame(savefile);
        }

        public bool Parse(Parser src)
        {
            int i;
            // first token should be function call then a potentially variable set of parms ended with a ;
            if (!src.ReadToken(out var token))
            {
                src.Error("Unexpected end of file");
                return false;
            }

            handler = null;

            for (i = 0; i < Script.commandList.Length; i++)
                if (string.Equals(token, Script.commandList[i].name, StringComparison.OrdinalIgnoreCase))
                {
                    handler = Script.commandList[i].handler;
                    break;
                }

            if (handler == null)
                src.Error($"Unknown script call {token}");
            // now read parms til ; all parms are read as idWinStr's but will be fixed up later to be proper types
            while (true)
            {
                if (!src.ReadToken(out token))
                {
                    src.Error("Unexpected end of file");
                    return false;
                }

                if (token == ";") break;
                if (token == "}") { src.UnreadToken(token); break; }

                var wv = new GSWinVar
                {
                    own = true,
                    var = new WinStr(token)
                };
                parms.Add(wv);
            }

            //  verify min/max params
            if (handler != null && (parms.Count < Script.commandList[i].mMinParms || parms.Count > Script.commandList[i].mMaxParms))
                src.Error($"incorrect number of parameters for script {Script.commandList[i].name}");

            return true;
        }

        public void Execute(Window win)
            => handler?.Invoke(win, parms);

        public void FixupParms(Window win)
        {
            if (handler == Script.Set)
            {
                var precacheBackground = false;
                var precacheSounds = false;
                var str = (WinStr)parms[0].var; Debug.Assert(str != null);
                var dest = win.GetWinVarByName(str, true);
                if (dest != null)
                {
                    parms[0].var = dest;
                    parms[0].own = false;

                    if ((WinBackground)dest != null)
                        precacheBackground = true;
                }
                else if (string.Equals(str, "cmd", StringComparison.OrdinalIgnoreCase))
                    precacheSounds = true;
                var parmCount = parms.Count;
                for (var i = 1; i < parmCount; i++)
                {
                    str = (WinStr)parms[i].var;
                    if (((string)str).StartsWith("gui::", StringComparison.OrdinalIgnoreCase))
                    {
                        //  always use a string here, no point using a float if it is one
                        //  FIXME: This creates duplicate variables, while not technically a problem since they are all bound to the same guiDict, it does consume extra memory and is generally a bad thing
                        var defvar = new WinStr();
                        defvar.Init(str, win);
                        win.AddDefinedVar(defvar);
                        parms[i].var = defvar;
                        parms[i].own = false;

                        //dest = win.GetWinVarByName(*str, true);
                        //if (dest) {
                        //	parms[i].var = dest;
                        //	parms[i].own = false;
                        //}
                    }
                    else if (((string)str)[0] == '$')
                    {
                        //  dont include the $ when asking for variable
                        dest = win.Gui.Desktop.GetWinVarByName((str) + 1, true);
                        if (dest != null)
                        {
                            parms[i].var = dest;
                            parms[i].own = false;
                        }
                    }
                    else if (((string)str).StartsWith(STRTABLE_ID))
                        str.Set(common.LanguageDictGetString(str));
                    else if (precacheBackground)
                    {
                        var mat = declManager.FindMaterial(str);
                        mat.Sort = SS_GUI;
                    }
                    else if (precacheSounds)
                    {
                        // Search for "play <...>"

                        var parser = new Parser(LEXFL.NOSTRINGCONCAT | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT);
                        parser.LoadMemory(str, str.Length, "command");

                        while (parser.ReadToken(out var token))
                            if (string.Equals(token, "play", StringComparison.OrdinalIgnoreCase))
                                if (parser.ReadToken(out token) && token != "")
                                    declManager.FindSound(token);
                    }
                }
            }
            else if (handler == Script.Transition)
            {
                if (parms.Count < 4)
                    common.Warning($"Window {win.Name} in gui {win.Gui.SourceFile} has a bad transition definition");
                var str = (WinStr)parms[0].var;
                Debug.Assert(str != null);

                //
                DrawWin destowner;
                var dest = win.GetWinVarByName(str, true, destowner);
                if (dest != null)
                {
                    parms[0].var = dest;
                    parms[0].own = false;
                }
                else common.Warning($"Window {win.Name} in gui {win.Gui.SourceFile}: a transition does not have a valid destination var {str}");

                //  support variables as parameters
                for (var c = 1; c < 3; c++)
                {
                    str = (WinStr)parms[c].var;

                    var v4 = new WinVec4();
                    parms[c].var = v4;
                    parms[c].own = true;

                    DrawWin owner;

                    dest = str[0] == '$' ? win.GetWinVarByName(str + 1, true, owner) : null;
                    if (dest != null)
                    {
                        Window ownerparent;
                        Window destparent;
                        if (owner != null)
                        {
                            ownerparent = owner.simp ? owner.simp.Parent : owner.win.Parent;
                            destparent = destowner.simp ? destowner.simp.Parent : destowner.win.Parent;

                            // If its the rectangle they are referencing then adjust it
                            if (ownerparent != null && destparent != null && (dest == owner.simp ? owner.simp.GetWinVarByName("rect") : owner.win.GetWinVarByName("rect")))
                            {
                                Rectangle rect = (WinRectangle)dest;
                                ownerparent.ClientToScreen(rect);
                                destparent.ScreenToClient(rect);
                                v4 = rect.ToVec4();
                            }
                            else v4.Set(dest.ToString());
                        }
                        else v4.Set(dest.ToString());
                    }
                    else v4.Set(str.ToString());
                }
            }
            else
            {
                for (var i = 0; i < parms.Count; i++)
                    parms[i].var.Init(parms[i].var.ToString(), win);
            }
        }

        public int Size => 0;
    }

    public class GuiScriptList
    {
        List<GuiScript> list = new();

        public GuiScriptList() => list.SetGranularity(4);

        public void Execute(Window win)
        {
            var c = list.Count;
            for (int i = 0; i < c; i++)
            {
                var gs = list[i];
                Debug.Assert(gs != null);
                if (gs.conditionReg >= 0)
                    if (win.HasOps)
                    {
                        var f = win.EvalRegs(gs.conditionReg);
                        if (f != 0f)
                            if (gs.ifList != null) win.RunScriptList(gs.ifList);
                            else if (gs.elseList != null) win.RunScriptList(gs.elseList);
                    }
                gs.Execute(win);
            }
        }

        public void Add(GuiScript gs)
            => list.Add(gs);

        public int Size => 0;

        void FixupParms(Window win)
        {
            var c = list.Count;
            for (var i = 0; i < c; i++)
            {
                var gs = list[i];
                gs.FixupParms(win);
                gs.ifList?.FixupParms(win);
                gs.elseList?.FixupParms(win);
            }
        }

        void ReadFromDemoFile(VFileDemo f) { }

        void WriteToDemoFile(VFileDemo f) { }

        internal void WriteToSaveGame(VFile savefile)
        {
            for (var i = 0; i < list.Count; i++)
                list[i].WriteToSaveGame(savefile);
        }

        internal void ReadFromSaveGame(VFile savefile)
        {
            for (var i = 0; i < list.Count; i++)
                list[i].ReadFromSaveGame(savefile);
        }
    }

}