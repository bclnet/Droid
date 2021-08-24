using System;
using System.Collections.Generic;
using Gengine.NumericsX.Core;
using Gengine.NumericsX.Sys;
using static Gengine.NumericsX.Core.Key;
using static Gengine.NumericsX.Lib;

namespace Gengine.UI
{
    //string R_GetVidModeListString(bool addCustom);
    //string R_GetVidModeValsString(bool addCustom);

    public class ChoiceWindow : Window
    {
        int currentChoice;
        int choiceType;
        string latchedChoices;
        WinStr choicesStr;
        string latchedVals;
        WinStr choiceVals;
        List<string> choices = new();
        List<string> values = new();

        WinStr guiStr;
        WinStr cvarStr;
        CVar cvar;
        MultiWinVar updateStr;

        WinBool liveUpdate;
        WinStr updateGroup;

        void CommonInit()
        {
            currentChoice = 0;
            choiceType = 0;
            cvar = null;
            liveUpdate = true;
            choices.Clear();
        }
        public ChoiceWindow(UserInterfaceLocal gui) : base(gui)
        {
            this.gui = gui;
            CommonInit();
        }
        public ChoiceWindow(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public override string HandleEvent(SysEvent ev, bool updateVisuals)
        {
            Key key; bool runAction = false, runAction2 = false;

            if (ev.evType == SE.KEY)
            {
                key = (Key)ev.evValue;

                if (key == K_RIGHTARROW || key == K_KP_RIGHTARROW || key == K_MOUSE1)
                {
                    // never affects the state, but we want to execute script handlers anyway
                    if (ev.evValue2 == 0) { RunScript(ON_ACTIONRELEASE); return cmd; }
                    currentChoice++;
                    if (currentChoice >= choices.Count)
                        currentChoice = 0;
                    runAction = true;
                }

                if (key == K_LEFTARROW || key == K_KP_LEFTARROW || key == K_MOUSE2)
                {
                    // never affects the state, but we want to execute script handlers anyway
                    if (ev.evValue2 == 0) { RunScript(ON_ACTIONRELEASE); return cmd; }
                    currentChoice--;
                    if (currentChoice < 0)
                        currentChoice = choices.Count - 1;
                    runAction = true;
                }

                if (ev.evValue2 == 0)
                    // is a key release with no action catch
                    return "";
            }
            else if (ev.evType == SE.CHAR)
            {
                key = (Key)ev.evValue;

                var potentialChoice = -1;
                for (var i = 0; i < choices.Count; i++)
                    if (char.ToUpperInvariant((char)key) == char.ToUpperInvariant(choices[i][0]))
                        if (i < currentChoice && potentialChoice < 0) potentialChoice = i;
                        else if (i > currentChoice) { potentialChoice = -1; currentChoice = i; break; }
                if (potentialChoice >= 0)
                    currentChoice = potentialChoice;

                runAction = true;
                runAction2 = true;
            }
            else return "";

            if (runAction)
                RunScript(ON_ACTION);

            if (choiceType == 0) cvarStr.Set($"{currentChoice}");
            else if (values.Count != 0) cvarStr.Set(values[currentChoice]);
            else cvarStr.Set(choices[currentChoice]);

            UpdateVars(false);

            if (runAction2)
                RunScript(ON_ACTIONRELEASE);

            return cmd;
        }

        public override void PostParse()
        {
            base.PostParse();

            // DG: HACKHACKFUCKINGUGLYHACK: overwrite resolution list to support more resolutions and widescreen and stuff.
            var injectResolutions = false;
            var injectCustomMode = true;

            /*
             * Mods that have their own video settings menu can tell dhewm3 to replace the "choices" and "values" entries in their choiceDef with the resolutions supported by
             * dhewm3 (and corresponding modes). So if we add new video modes to dhewm3, they'll automatically appear in the menu without changing the .gui
             * To enable this, the mod authors only need to add an "injectResolutions 1" entry to their resolution choiceDef. By default, the first entry will be "r_custom*"
             * for r_mode -1, which means "custom resolution, use r_customWidth and r_customHeight". If that entry shoud be disabled for the mod, just add another entry:
             * "injectCustomResolutionMode 0"
             */
            var wv = GetWinVarByName("injectResolutions");
            if (wv != null)
            {
                var val = wv.ToString();
                if (val != "0")
                {
                    injectResolutions = true;
                    wv = GetWinVarByName("injectCustomResolutionMode");
                    if (wv != null)
                    {
                        val = wv.ToString();
                        if (val == "0")
                            injectCustomMode = false;
                    }
                }
            }
            else if (Name == "OS2Primary" && cvarStr == "r_mode" && (string.Equals(Gui.SourceFile, "guis/demo_mainmenu.gui", StringComparison.OrdinalIgnoreCase) || string.Equals(Gui.SourceFile, "guis/mainmenu.gui", StringComparison.OrdinalIgnoreCase)))
                // always enable this for base/ and d3xp/ mainmenu.gui (like we did before)
                injectResolutions = true;

            if (injectResolutions)
            {
                choicesStr.Set(R_GetVidModeListString(injectCustomMode));
                choiceVals.Set(R_GetVidModeValsString(injectCustomMode));
            }
            // DG end

            UpdateChoicesAndVals();

            InitVars();
            UpdateChoice();
            UpdateVars(false);

            flags |= WIN_CANFOCUS;
        }

        public override void Draw(int time, float x, float y)
        {
            var color = foreColor;

            UpdateChoicesAndVals();
            UpdateChoice();

            // FIXME: It'd be really cool if textAlign worked, but a lot of the guis have it set wrong because it used to not work
            textAlign = (char)0;

            if (textShadow!=0)
            {
                var shadowText = choices[currentChoice];
                var shadowRect = new Rectangle(textRect);

                shadowText.RemoveColors();
                shadowRect.x += textShadow;
                shadowRect.y += textShadow;

                dc.DrawText(shadowText, textScale, textAlign, colorBlack, shadowRect, false, -1);
            }

            if (hover && !noEvents && Contains(gui.CursorX, gui.CursorY)) color = hoverColor;
            else hover = false;
            if ((flags & WIN_FOCUS) != 0)
                color = hoverColor;

            dc.DrawText(choices[currentChoice], textScale, textAlign, color, textRect, false, -1);
        }

        public override void Activate(bool activate, string act)
        {
            base.Activate(activate, act);
            if (activate)
                // sets the gui state based on the current choice the window contains
                UpdateChoice();
        }

        public override int Allocated => base.Allocated;

        public override WinVar GetWinVarByName(string name, bool winLookup = false, DrawWin owner = null)
        {
            if (string.Equals(name, "choices", StringComparison.OrdinalIgnoreCase)) return choicesStr;
            if (string.Equals(name, "values", StringComparison.OrdinalIgnoreCase)) return choiceVals;
            if (string.Equals(name, "cvar", StringComparison.OrdinalIgnoreCase)) return cvarStr;
            if (string.Equals(name, "gui", StringComparison.OrdinalIgnoreCase)) return guiStr;
            if (string.Equals(name, "liveUpdate", StringComparison.OrdinalIgnoreCase)) return liveUpdate;
            if (string.Equals(name, "updateGroup", StringComparison.OrdinalIgnoreCase)) return updateGroup;
            return base.GetWinVarByName(name, winLookup, owner);
        }

        public void RunNamedEvent(string eventName)
        {
            string event_, group;

            if (eventName.StartsWith("cvar read "))
            {
                event_ = eventName;
                group = event_.Mid(10, event_.Length - 10);
                if (group == updateGroup)
                    UpdateVars(true, true);
            }
            else if (eventName.StartsWith("cvar write "))
            {
                event_ = eventName;
                group = event_.Mid(11, event_.Length - 11);
                if (group == updateGroup)
                    UpdateVars(false, true);
            }
        }

        override bool ParseInternalVar(string name, Parser src)
        {
            if (string.Equals(name, "choicetype", StringComparison.OrdinalIgnoreCase)) { choiceType = src.ParseInt(); return true; }
            if (string.Equals(name, "currentchoice", StringComparison.OrdinalIgnoreCase)) { currentChoice = src.ParseInt(); return true; }
            return base.ParseInternalVar(name, src);
        }

        void UpdateChoice()
        {
            if (updateStr.Count == 0)
                return;
            UpdateVars(true);
            updateStr.Update();
            if (choiceType == 0)
            {
                // ChoiceType 0 stores current as an integer in either cvar or gui If both cvar and gui are defined then cvar wins, but they are both updated
                if (updateStr[0].NeedsUpdate)
                    currentChoice = int.Parse(updateStr[0].ToString());
                ValidateChoice();
            }
            else
            {
                // ChoiceType 1 stores current as a cvar string
                var c = values.Count != 0 ? values.Count : choices.Count;
                int i;
                for (i = 0; i < c; i++)
                    if (string.Equals(cvarStr.ToString(), values.Count != 0 ? values[i] : choices[i], StringComparison.OrdinalIgnoreCase))
                        break;
                if (i == c)
                    i = 0;
                currentChoice = i;
                ValidateChoice();
            }
        }

        void ValidateChoice()
        {
            if (currentChoice < 0 || currentChoice >= choices.Num())
                currentChoice = 0;
            if (choices.Num() == 0)
                choices.Append("No Choices Defined");
        }

        void InitVars()
        {
            if (cvarStr.Length!=0)
            {
                cvar = cvarSystem.Find(cvarStr);
                if (cvar == null)
                {
                    if (cvarStr.ToString() == "s_driver" && cvarStr.ToString() == "net_serverAllowServerMod")
                        common.Warning($"ChoiceWindow::InitVars: gui '{gui.SourceFile}' window '{name}' references undefined cvar '{cvarStr}'");
                    return;
                }
                updateStr.Add(cvarStr);
            }
            if (guiStr.Length != 0)
                updateStr.Add(guiStr);
            updateStr.SetGuiInfo(gui.GetStateDict());
            updateStr.Update();
        }
        // true: read the updated cvar from cvar system, gui from dict
        // false: write to the cvar system, to the gui dict
        // force == true overrides liveUpdate 0
        void UpdateVars(bool read, bool force = false)
        {
            if (force || liveUpdate)
            {
                if (cvar && cvarStr.NeedsUpdate())
                {
                    if (read)
                    {
                        cvarStr.Set(cvar.GetString());
                    }
                    else
                    {
                        cvar.SetString(cvarStr.c_str());
                    }
                }
                if (!read && guiStr.NeedsUpdate())
                {
                    guiStr.Set(va("%i", currentChoice));
                }
            }
        }

        // update the lists whenever the WinVar have changed
        void UpdateChoicesAndVals()
        {
            idToken token;
            idStr str2, str3;
            idLexer src;

            if (latchedChoices.Icmp(choicesStr))
            {
                choices.Clear();
                src.FreeSource();
                src.SetFlags(LEXFL_NOFATALERRORS | LEXFL_ALLOWPATHNAMES | LEXFL_ALLOWMULTICHARLITERALS | LEXFL_ALLOWBACKSLASHSTRINGCONCAT);
                src.LoadMemory(choicesStr, choicesStr.Length(), "<ChoiceList>");
                if (src.IsLoaded())
                {
                    while (src.ReadToken(&token))
                    {
                        if (token == ";")
                        {
                            if (str2.Length())
                            {
                                str2.StripTrailingWhitespace();
                                str2 = common.GetLanguageDict().GetString(str2);
                                choices.Append(str2);
                                str2 = "";
                            }
                            continue;
                        }
                        str2 += token;
                        str2 += " ";
                    }
                    if (str2.Length())
                    {
                        str2.StripTrailingWhitespace();
                        choices.Append(str2);
                    }
                }
                latchedChoices = choicesStr.c_str();
            }
            if (choiceVals.Length() && latchedVals.Icmp(choiceVals))
            {
                values.Clear();
                src.FreeSource();
                src.SetFlags(LEXFL_ALLOWPATHNAMES | LEXFL_ALLOWMULTICHARLITERALS | LEXFL_ALLOWBACKSLASHSTRINGCONCAT);
                src.LoadMemory(choiceVals, choiceVals.Length(), "<ChoiceVals>");
                str2 = "";
                bool negNum = false;
                if (src.IsLoaded())
                {
                    while (src.ReadToken(&token))
                    {
                        if (token == "-")
                        {
                            negNum = true;
                            continue;
                        }
                        if (token == ";")
                        {
                            if (str2.Length())
                            {
                                str2.StripTrailingWhitespace();
                                values.Append(str2);
                                str2 = "";
                            }
                            continue;
                        }
                        if (negNum)
                        {
                            str2 += "-";
                            negNum = false;
                        }
                        str2 += token;
                        str2 += " ";
                    }
                    if (str2.Length())
                    {
                        str2.StripTrailingWhitespace();
                        values.Append(str2);
                    }
                }
                if (choices.Num() != values.Num())
                {
                    common.Warning("idChoiceWindow:: gui '%s' window '%s' has value count unequal to choices count", gui.GetSourceFile(), name.c_str());
                }
                latchedVals = choiceVals.c_str();
            }
        }
    }
}
