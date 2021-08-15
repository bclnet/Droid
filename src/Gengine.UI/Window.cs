using Gengine.Render;
using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;
using System.NumericsX.Sys;
using static System.NumericsX.Lib;
using static System.NumericsX.Core.Key;

namespace Gengine.UI
{
    public enum WexpOpType
    {
        WOP_TYPE_ADD,
        WOP_TYPE_SUBTRACT,
        WOP_TYPE_MULTIPLY,
        WOP_TYPE_DIVIDE,
        WOP_TYPE_MOD,
        WOP_TYPE_TABLE,
        WOP_TYPE_GT,
        WOP_TYPE_GE,
        WOP_TYPE_LT,
        WOP_TYPE_LE,
        WOP_TYPE_EQ,
        WOP_TYPE_NE,
        WOP_TYPE_AND,
        WOP_TYPE_OR,
        WOP_TYPE_VAR,
        WOP_TYPE_VARS,
        WOP_TYPE_VARF,
        WOP_TYPE_VARI,
        WOP_TYPE_VARB,
        WOP_TYPE_COND
    }

    public enum WexpRegister
    {
        WEXP_REG_TIME,
        WEXP_REG_NUM_PREDEFINED
    }

    public struct WexpOp
    {
        public WexpOpType opType;
        public int a, b, c, d;
    }

    public struct RegEntry
    {
        public RegEntry(string name, Register.REGTYPE type)
        {
            this.name = name;
            this.type = type;
            index = 0;
        }
        public string name;
        public Register.REGTYPE type;
        public int index;
    }

    public class TimeLineEvent
    {
        public TimeLineEvent()
        {
            event_ = new GuiScriptList();
        }
        public int time;
        public GuiScriptList event_;
        public bool pending;
        public int Size => 0 + event_.Size;
    }

    public class RVNamedEvent
    {
        public RVNamedEvent(string name)
        {
            mEvent = new GuiScriptList();
            mName = name;
        }
        public int Size => 0 + mEvent.Size;

        public string mName;
        public GuiScriptList mEvent;
    }

    public struct TransitionData
    {
        public WinVar data;
        public int offset;
        public InterpolateAccelDecelLinear_Vector4 interp;
    }

    public class Window
    {
        //extern CVar r_skipGuiShaders;		// 1 = don't render any gui elements on surfaces
        //extern CVar r_scaleMenusTo43;


        public const int WIN_CHILD = 0x00000001;
        public const int WIN_CAPTION = 0x00000002;
        public const int WIN_BORDER = 0x00000004;
        public const int WIN_SIZABLE = 0x00000008;
        public const int WIN_MOVABLE = 0x00000010;
        public const int WIN_FOCUS = 0x00000020;
        public const int WIN_CAPTURE = 0x00000040;
        public const int WIN_HCENTER = 0x00000080;
        public const int WIN_VCENTER = 0x00000100;
        public const int WIN_MODAL = 0x00000200;
        public const int WIN_INTRANSITION = 0x00000400;
        public const int WIN_CANFOCUS = 0x00000800;
        public const int WIN_SELECTED = 0x00001000;
        public const int WIN_TRANSFORM = 0x00002000;
        public const int WIN_HOLDCAPTURE = 0x00004000;
        public const int WIN_NOWRAP = 0x00008000;
        public const int WIN_NOCLIP = 0x00010000;
        public const int WIN_INVERTRECT = 0x00020000;
        public const int WIN_NATURALMAT = 0x00040000;
        public const int WIN_NOCURSOR = 0x00080000;
        public const int WIN_MENUGUI = 0x00100000;
        public const int WIN_ACTIVE = 0x00200000;
        public const int WIN_SHOWCOORDS = 0x00400000;
        public const int WIN_SHOWTIME = 0x00800000;
        public const int WIN_WANTENTER = 0x01000000;

        public const int WIN_DESKTOP = 0x10000000;

        public const int WIN_SCALETO43 = 0x20000000; // DG: for the "scaleto43" window flag (=> scale window to 4:3 with "empty" bars left/right or above/below)

        public const string CAPTION_HEIGHT = "16.0";
        public const string SCROLLER_SIZE = "16.0";
        public const int SCROLLBAR_SIZE = 16;

        public const int MAX_WINDOW_NAME = 32;
        public const int MAX_LIST_ITEMS = 1024;

        public const int MAX_EXPRESSION_OPS = 4096;
        public const int MAX_EXPRESSION_REGISTERS = 4096;

        public const string DEFAULT_BACKCOLOR = "1 1 1 1";
        public const string DEFAULT_FORECOLOR = "0 0 0 1";
        public const string DEFAULT_BORDERCOLOR = "0 0 0 1";
        public const string DEFAULT_TEXTSCALE = "0.4";

        public enum SCRIPT
        {
            ON_MOUSEENTER = 0,
            ON_MOUSEEXIT,
            ON_ACTION,
            ON_ACTIVATE,
            ON_DEACTIVATE,
            ON_ESC,
            ON_FRAME,
            ON_TRIGGER,
            ON_ACTIONRELEASE,
            ON_ENTER,
            ON_ENTERRELEASE,
            SCRIPT_COUNT
        }

        public enum ADJUST
        {
            MOVE = 0,
            TOP,
            RIGHT,
            BOTTOM,
            LEFT,
            TOPLEFT,
            BOTTOMRIGHT,
            TOPRIGHT,
            BOTTOMLEFT
        }

        public static readonly string[] ScriptNames = {
            "onMouseEnter",
            "onMouseExit",
            "onAction",
            "onActivate",
            "onDeactivate",
            "onESC",
            "onEvent",
            "onTrigger",
            "onActionRelease",
            "onEnter",
            "onEnterRelease"
        };

        // made RegisterVars a member of idWindow
        public static readonly RegEntry[] RegisterVars =
        {
            new RegEntry( "forecolor", Register.REGTYPE.VEC4),
            new RegEntry( "hovercolor", Register.REGTYPE.VEC4),
            new RegEntry( "backcolor", Register.REGTYPE.VEC4),
            new RegEntry( "bordercolor", Register.REGTYPE.VEC4),
            new RegEntry( "rect", Register.REGTYPE.RECTANGLE),
            new RegEntry( "matcolor", Register.REGTYPE.VEC4),
            new RegEntry("scale", Register.REGTYPE.VEC2),
            new RegEntry( "translate", Register.REGTYPE.VEC2),
            new RegEntry("rotate", Register.REGTYPE.FLOAT),
            new RegEntry("textscale", Register.REGTYPE.FLOAT),
            new RegEntry("visible", Register.REGTYPE.BOOL),
            new RegEntry("noevents", Register.REGTYPE.BOOL),
            new RegEntry("text", Register.REGTYPE.STRING),
            new RegEntry( "background", Register.REGTYPE.STRING),
            new RegEntry( "runscript", Register.REGTYPE.STRING),
            new RegEntry( "varbackground", Register.REGTYPE.STRING),
            new RegEntry( "cvar", Register.REGTYPE.STRING),
            new RegEntry( "choices", Register.REGTYPE.STRING),
            new RegEntry( "choiceVar", Register.REGTYPE.STRING),
            new RegEntry( "bind", Register.REGTYPE.STRING),
            new RegEntry( "modelRotate", Register.REGTYPE.VEC4),
            new RegEntry("modelOrigin", Register.REGTYPE.VEC4),
            new RegEntry("lightOrigin", Register.REGTYPE.VEC4),
            new RegEntry("lightColor", Register.REGTYPE.VEC4),
            new RegEntry("viewOffset", Register.REGTYPE.VEC4),
            new RegEntry("hideCursor", Register.REGTYPE.BOOL)
        };

        protected float actualX;                    // physical coords
        protected float actualY;                    // ''
        protected int childID;                  // this childs id
        protected internal uint flags;             // visible, focus, mouseover, cursor, border, etc..
        protected int lastTimeRun;              //
        protected internal Rectangle drawRect;           // overall rect
        protected internal Rectangle clientRect;         // client area
        protected internal Vector2 origin;

        protected int timeLine;                 // time stamp used for various fx
        protected float xOffset;
        protected float yOffset;
        protected float forceAspectWidth;
        protected float forceAspectHeight;
        protected internal float matScalex;
        protected internal float matScaley;
        protected internal float borderSize;
        protected internal float textAlignx;
        protected internal float textAligny;
        protected internal string name;
        protected string comment;
        protected internal Vector2 shear;

        protected internal byte textShadow;
        protected internal byte fontNum;
        protected byte cursor;                  //
        protected internal DeviceContext.ALIGN textAlign;

        protected WinBool noTime;                   //
        protected internal WinBool visible;              //
        protected WinBool noEvents;
        protected internal WinRectangle rect;                // overall rect
        protected internal WinVec4 backColor;
        protected internal WinVec4 matColor;
        protected internal WinVec4 foreColor;
        protected WinVec4 hoverColor;
        protected internal WinVec4 borderColor;
        protected internal WinFloat textScale;
        protected internal WinFloat rotate;
        protected internal WinStr text;
        protected internal WinBackground backGroundName;         //

        protected List<WinVar> definedVars = new();
        protected List<WinVar> updateVars = new();

        protected internal Rectangle textRect;           // text extented rect
        protected internal Material background;         // background asset

        protected Window parent;                // parent window
        protected List<Window> children = new();        // child windows
        protected List<DrawWin> drawWindows = new();

        protected Window focusedChild;          // if a child window has the focus
        protected Window captureChild;          // if a child window has mouse capture
        protected Window overChild;         // if a child window has mouse capture
        protected bool hover;

        protected internal DeviceContext dc;

        protected UserInterfaceLocal gui;

        protected static readonly CVar gui_debug = new("gui_debug", "0", CVAR.GUI | CVAR.BOOL, "");
        protected static readonly CVar gui_edit = new("gui_edit", "0", CVAR.GUI | CVAR.BOOL, "");

        protected GuiScriptList[] scripts = new GuiScriptList[(int)SCRIPT.SCRIPT_COUNT];
        protected bool[] saveTemps;

        protected List<TimeLineEvent> timeLineEvents = new();
        protected List<TransitionData> transitions = new();

        protected static bool[] registerIsTemporary = new bool[MAX_EXPRESSION_REGISTERS]; // statics to assist during parsing

        protected List<WexpOp> ops = new();             // evaluate to make expressionRegisters
        protected List<float> expressionRegisters = new();
        protected List<WexpOp> saveOps = new();             // evaluate to make expressionRegisters
        protected List<RVNamedEvent> namedEvents = new();       //  added named events
        protected List<float> saveRegs = new();

        protected RegisterList regList;

        protected internal WinBool hideCursor;

        public void CommonInit()
        {
            childID = 0;
            flags = 0;
            lastTimeRun = 0;
            origin.Zero();
            fontNum = 0;
            timeLine = -1;
            xOffset = yOffset = 0f;
            cursor = 0;
            forceAspectWidth = 640f;
            forceAspectHeight = 480f;
            matScalex = 1f;
            matScaley = 1f;
            borderSize = 0f;
            noTime = false;
            visible = true;
            textAlign = 0;
            textAlignx = 0f;
            textAligny = 0f;
            noEvents = false;
            rotate = 0;
            shear.Zero();
            textScale = 0.35f;
            backColor.Zero();
            foreColor = new Vector4(1f, 1f, 1f, 1f);
            hoverColor = new Vector4(1f, 1f, 1f, 1f);
            matColor = new Vector4(1f, 1f, 1f, 1f);
            borderColor.Zero();
            background = null;
            backGroundName = "";
            focusedChild = null;
            captureChild = null;
            overChild = null;
            parent = null;
            saveOps = null;
            saveRegs = null;
            timeLine = -1;
            textShadow = 0;
            hover = false;

            for (var i = 0; i < scripts.Length; i++)
                scripts[i] = null;

            hideCursor = false;
        }

        public Window(UserInterfaceLocal gui)
        {
            this.dc = null;
            this.gui = gui;
            CommonInit();
        }
        public Window(DeviceContext dc, UserInterfaceLocal gui)
        {
            this.dc = dc;
            this.gui = gui;
            CommonInit();
        }

        public int Size => 0;

        public virtual int Allocated => 0;

        public DeviceContext DC
        {
            get => dc;
            set => dc = value;
        }

        public void CleanUp()
        {
            // ensure the register list gets cleaned up
            regList.Reset();

            // Cleanup the named events
            namedEvents.Clear();

            // Cleanup the operations and update vars (if it is not fixed, orphane register references are possible)
            ops.Clear();
            updateVars.Clear();

            drawWindows.Clear();
            children.Clear();
            definedVars.Clear();
            timeLineEvents.Clear();
            for (var i = 0; i < scripts.Length; i++)
                scripts[i] = null;
            CommonInit();
        }

        public Window SetFocus(Window w, bool scripts = true);

        public Window SetCapture(Window w)
        {
            // only one child can have the focus
            Window last = null;
            var c = children.Count;
            for (var i = 0; i < c; i++)
                if ((children[i].flags & WIN_CAPTURE) != 0)
                {
                    last = children[i];
                    //last.flags &= ~WIN_CAPTURE;
                    last.LoseCapture();
                    break;
                }

            w.flags |= WIN_CAPTURE;
            w.GainCapture();
            gui.Desktop.captureChild = w;
            return last;
        }

        public void SetParent(Window w);

        public void ClearFlag(uint f);

        public uint Flags
        {
            get => flags;
            set => flags = value;
        }

        public void Move(float x, float y)
        {
            var rct = new Rectangle(rect) { x = x, y = y };
            var reg = RegList.FindReg("rect");
            reg?.Enable(false);
            rect = rct;
        }

        public void BringToTop(Window w)
        {
            if (w != null && (w.flags & WIN_MODAL) == 0)
                return;

            var c = children.Count;
            for (var i = 0; i < c; i++)
                if (children[i] == w)
                {
                    // this is it move from i - 1 to 0 to i to 1 then shove this one into 0
                    for (var j = i + 1; j < c; j++)
                        children[j - 1] = children[j];
                    children[c - 1] = w;
                    break;
                }
        }

        public void Adjust(float xd, float yd);

        public void SetAdjustMode(Window child);

        public void Size(float x, float y, float w, float h)
        {
            rect = new Rectangle(rect)
            {
                x = x,
                y = y,
                w = w,
                h = h
            };
            CalcClientRect(0, 0);
        }

        public void SetupFromState();

        public void SetupBackground();

        public DrawWin FindChildByName(string name);

        public SimpleWindow FindSimpleWinByName(string name);

        public Window Parent => parent;

        public UserInterfaceLocal Gui => gui;

        public bool Contains(float x, float y)
        {
            var r = new Rectangle(drawRect) { x = actualX, y = actualY };
            return r.Contains(x, y);
        }

        public string GetStrPtrByName(string name);

        public virtual WinVar GetWinVarByName(string name, bool winLookup = false, DrawWin owner = null);

        public int GetWinVarOffset(WinVar wv, DrawWin dw);

        public float MaxCharHeight { get { SetFont(); return dc.MaxCharHeight(textScale); } }

        public float MaxCharWidth { get { SetFont(); return dc.MaxCharWidth(textScale); } }

        public void SetFont() => dc.SetFont(fontNum);

        public void SetInitialState(string name);

        public void AddChild(Window win);

        public void DebugDraw(int time, float x, float y)
        {
            if (dc != null)
            {
                dc.EnableClipping(false);
                if (gui_debug.Integer == 1)
                    dc.DrawRect(drawRect.x, drawRect.y, drawRect.w, drawRect.h, 1, DeviceContext.colorRed);
                else if (gui_debug.Integer == 2)
                {
                    var str = (string)text;
                    var buff = (str.Length != 0 ? $"{str}\n" : "") +
                        $"Rect: {rect.x:0.1}, {rect.y:0.1}, {rect.w:0.1}, {rect.h:0.1}\n" +
                        $"Draw Rect: {drawRect.x:0.1}, {drawRect.y:0.1}, {drawRect.w:0.1}, {drawRect.h:0.1}\n" +
                        $"Client Rect: {clientRect.x:0.1}, {clientRect.y:0.1}, {clientRect.w:0.1}, {clientRect.h:0.1}\n" +
                        $"Cursor: {gui.CursorX:0.1} : {gui.CursorY:0.1}\n";
                    dc.DrawText(buff, textScale, textAlign, foreColor, textRect, true);
                }
                dc.EnableClipping(true);
            }
        }

        public void CalcClientRect(float xofs, float yofs);

        public void DrawBorderAndCaption(Rectangle drawRect);

        public void DrawCaption(int time, float x, float y);

        public void SetupTransforms(float x, float y);

        public bool Contains(Rectangle sr, float x, float y)
        {
            var r = new Rectangle(sr);
            r.x += actualX - drawRect.x; r.y += actualY - drawRect.y;
            return r.Contains(x, y);
        }

        public string Name => name;

        public virtual bool Parse(Parser src, bool rebuild = true);

        static bool HandleEvent_actionDownRun;
        static bool HandleEvent_actionUpRun;
        public virtual string HandleEvent(SysEvent ev, Action<bool> updateVisuals = null)
        {
            cmd = "";

            if ((flags & WIN_DESKTOP) != 0)
            {
                HandleEvent_actionDownRun = false;
                HandleEvent_actionUpRun = false;
                if (expressionRegisters.Count != 0 && ops.Count != 0)
                    EvalRegs();
                RunTimeEvents(gui.Time);
                CalcRects(0, 0);
                dc.SetCursor(DeviceContext.CURSOR.ARROW);
            }

            if (visible && !noEvents)
            {
                if (ev.evType == SE.KEY)
                {
                    EvalRegs(-1, true);
                    updateVisuals?.Invoke(true);

                    if (ev.evValue == (int)K_MOUSE1)
                    {
                        if (ev.evValue2 == 0 && CaptureChild != null)
                        {
                            CaptureChild.LoseCapture();
                            gui.Desktop.captureChild = null;
                            return "";
                        }

                        var c = children.Count;
                        while (--c >= 0)
                            if (children[c].visible && children[c].Contains(children[c].drawRect, gui.CursorX, gui.CursorY) && !children[c].noEvents)
                            {
                                var child = children[c];
                                if (ev.evValue2 != 0)
                                {
                                    BringToTop(child);
                                    SetFocus(child);
                                    if ((child.flags & WIN_HOLDCAPTURE) != 0)
                                        SetCapture(child);
                                }
                                if (child.Contains(child.clientRect, gui.CursorX, gui.CursorY))
                                {
                                    //if ((gui_edit.Bool && (child.flags & WIN_SELECTED) != 0) || (!gui_edit.Bool && (child.flags & WIN_MOVABLE) != 0))
                                    //    SetCapture(child);
                                    SetFocus(child);
                                    var childRet = child.HandleEvent(ev, updateVisuals);
                                    if (!string.IsNullOrEmpty(childRet))
                                        return childRet;
                                    if ((child.flags & WIN_MODAL) != 0)
                                        return "";
                                }
                                else if (ev.evValue2 != 0)
                                {
                                    SetFocus(child);
                                    var capture = true;
                                    if (capture && ((child.flags & WIN_MOVABLE) != 0 || gui_edit.Bool))
                                        SetCapture(child);
                                    return "";
                                }
                            }
                        if (ev.evValue2 != 0 && !HandleEvent_actionDownRun)
                            HandleEvent_actionDownRun = RunScript(SCRIPT.ON_ACTION);
                        else if (!HandleEvent_actionUpRun)
                            HandleEvent_actionUpRun = RunScript(SCRIPT.ON_ACTIONRELEASE);
                    }
                    else if (ev.evValue == (int)K_MOUSE2)
                    {
                        if (ev.evValue2 == 0 && CaptureChild != null)
                        {
                            CaptureChild.LoseCapture();
                            gui.Desktop.captureChild = null;
                            return "";
                        }

                        var c = children.Count;
                        while (--c >= 0)
                            if (children[c].visible && children[c].Contains(children[c].drawRect, gui.CursorX, gui.CursorY) && !children[c].noEvents)
                            {
                                var child = children[c];
                                if (ev.evValue2 != 0)
                                {
                                    BringToTop(child);
                                    SetFocus(child);
                                }
                                if (child.Contains(child.clientRect, gui.CursorX, gui.CursorY) || CaptureChild == child)
                                {
                                    if ((gui_edit.Bool && (child.flags & WIN_SELECTED) != 0) || (!gui_edit.Bool && (child.flags & WIN_MOVABLE) != 0))
                                        SetCapture(child);
                                    var childRet = child.HandleEvent(ev, updateVisuals);
                                    if (!string.IsNullOrEmpty(childRet))
                                        return childRet;
                                    if ((child.flags & WIN_MODAL) != 0)
                                        return "";
                                }
                            }
                    }
                    else if (ev.evValue == (int)K_MOUSE3)
                    {
                        if (gui_edit.Bool)
                        {
                            var c = children.Count;
                            for (var i = 0; i < c; i++)
                                if (children[i].drawRect.Contains(gui.CursorX, gui.CursorY))
                                    if (ev.evValue2 != 0)
                                    {
                                        children[i].flags ^= WIN_SELECTED;
                                        if ((children[i].flags & WIN_SELECTED) != 0)
                                        {
                                            flags &= ~WIN_SELECTED;
                                            return "childsel";
                                        }
                                    }
                        }
                    }
                    else if (ev.evValue == (int)K_TAB && ev.evValue2 != 0)
                    {
                        if (FocusedChild != null)
                        {
                            var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                            if (!string.IsNullOrEmpty(childRet))
                                return childRet;

                            // If the window didn't handle the tab, then move the focus to the next window or the previous window if shift is held down
                            var direction = KeyInput.IsDown(K_SHIFT) ? -1 : 1;

                            var currentFocus = FocusedChild;
                            var child = FocusedChild;
                            var parent = child.Parent;
                            while (parent != null)
                            {
                                var foundFocus = false;
                                var recurse = false;
                                var index = 0;
                                if (child != null) index = parent.GetChildIndex(child) + direction;
                                else if (direction < 0) index = parent.ChildCount - 1;
                                while (index < parent.ChildCount && index >= 0)
                                {
                                    var testWindow = parent.GetChild(index);
                                    if (testWindow == currentFocus)
                                    {
                                        // we managed to wrap around and get back to our starting window
                                        foundFocus = true;
                                        break;
                                    }
                                    if (testWindow != null && !testWindow.noEvents && testWindow.visible)
                                        if ((testWindow.flags & WIN_CANFOCUS) != 0)
                                        {
                                            SetFocus(testWindow);
                                            foundFocus = true;
                                            break;
                                        }
                                        else if (testWindow.ChildCount > 0)
                                        {
                                            parent = testWindow;
                                            child = null;
                                            recurse = true;
                                            break;
                                        }
                                    index += direction;
                                }
                                // We found a child to focus on
                                if (foundFocus)
                                    break;
                                // We found a child with children
                                else if (recurse)
                                    continue;
                                else
                                {
                                    // We didn't find anything, so go back up to our parent
                                    child = parent;
                                    parent = child.Parent;
                                    if (parent == gui.Desktop)
                                    {
                                        // We got back to the desktop, so wrap around but don't actually go to the desktop
                                        parent = null;
                                        child = null;
                                    }
                                }
                            }
                        }
                    }
                    else if (ev.evValue == (int)K_ESCAPE && ev.evValue2 != 0)
                    {
                        if (FocusedChild != null)
                        {
                            var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                            if (!string.IsNullOrEmpty(childRet))
                                return childRet;
                        }
                        RunScript(SCRIPT.ON_ESC);
                    }
                    else if (ev.evValue == (int)K_ENTER)
                    {
                        if (FocusedChild != null)
                        {
                            var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                            if (!string.IsNullOrEmpty(childRet))
                                return childRet;
                        }
                        if ((flags & WIN_WANTENTER) != 0)
                            RunScript(ev.evValue2 != 0 ? SCRIPT.ON_ACTION : SCRIPT.ON_ACTIONRELEASE);
                    }
                    else if (FocusedChild != null)
                    {
                        var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                        if (!string.IsNullOrEmpty(childRet))
                            return childRet;
                    }
                }
                else if (ev.evType == SE.MOUSE)
                {
                    updateVisuals?.Invoke(true);
                    var mouseRet = RouteMouseCoords(ev.evValue, ev.evValue2);
                    if (!string.IsNullOrEmpty(mouseRet))
                        return mouseRet;
                }
                else if (ev.evType == SE.NONE) { }
                else if (ev.evType == SE.CHAR)
                {
                    if (FocusedChild != null)
                    {
                        var childRet = FocusedChild.HandleEvent(ev, updateVisuals);
                        if (!string.IsNullOrEmpty(childRet))
                            return childRet;
                    }
                }
            }

            gui.ReturnCmd = cmd;
            if (gui.PendingCmd.Length != 0)
            {
                gui.ReturnCmd += " ; ";
                gui.ReturnCmd += gui.PendingCmd;
                gui.PendingCmd = string.Empty;
            }
            cmd = "";
            return gui.ReturnCmd;
        }

        public void CalcRects(float x, float y);

        public virtual void Redraw(float x, float y);

        public virtual void ArchiveToDictionary(Dictionary<string, string> dict, bool useNames = true);

        public virtual void InitFromDictionary(Dictionary<string, string> dict, bool byName = true);

        public virtual void PostParse();

        public virtual void Activate(bool activate, ref string act)
        {
            var n = activate ? SCRIPT.ON_ACTIVATE : SCRIPT.ON_DEACTIVATE;

            //  make sure win vars are updated before activation
            UpdateWinVars();

            RunScript(n);
            var c = children.Count;
            for (var i = 0; i < c; i++)
                children[i].Activate(activate, ref act);

            if (act.Length != 0)
                act += " ; ";
        }

        public virtual void Trigger()
        {
            RunScript(SCRIPT.ON_TRIGGER);
            var c = children.Count;
            for (var i = 0; i < c; i++)
                children[i].Trigger();
            StateChanged(true);
        }

        public virtual void GainFocus();

        public virtual void LoseFocus();

        public virtual void GainCapture();

        public virtual void LoseCapture();

        public virtual void Sized();

        public virtual void Moved();

        public virtual void Draw(int time, float x, float y)
        {
            if (text.Length == 0)
                return;
            if (textShadow != 0)
            {
                var shadowText = text;
                var shadowRect = new Rectangle(textRect);

                shadowText.RemoveColors();
                shadowRect.x += textShadow;
                shadowRect.y += textShadow;

                dc.DrawText(shadowText, textScale, textAlign, DeviceContext.colorBlack, shadowRect, (flags & WIN_NOWRAP) == 0, -1);
            }
            dc.DrawText(text, textScale, textAlign, foreColor, textRect, (flags & WIN_NOWRAP) == 0, -1);

            if (gui_edit.Bool)
            {
                dc.EnableClipping(false);
                dc.DrawText($"x: {(int)rect.x}  y: {(int)rect.y}", 0.25f, 0, DeviceContext.colorWhite, new Rectangle(rect.x, rect.y - 15, 100, 20), false);
                dc.DrawText($"w: {(int)rect.w}  h: {(int)rect.h}", 0.25f, 0, DeviceContext.colorWhite, new Rectangle(rect.x + rect.w, rect.w + rect.h + 5, 100, 20), false);
                dc.EnableClipping(true);
            }
        }

        public virtual void MouseExit()
        {
            if (noEvents)
                return;
            RunScript(SCRIPT.ON_MOUSEEXIT);
        }

        public virtual void MouseEnter()
        {
            if (noEvents)
                return;
            RunScript(SCRIPT.ON_MOUSEENTER);
        }

        public virtual void DrawBackground(Rectangle drawRect);

        public virtual string RouteMouseCoords(float xd, float yd)
        {
            string str;
            if (CaptureChild != null)
                //FIXME: unkludge this whole mechanism
                return CaptureChild.RouteMouseCoords(xd, yd);

            if (xd == -2000 || yd == -2000)
                return "";

            var c = children.Count;
            while (c > 0)
            {
                var child = children[--c];
                if (child.visible && !child.noEvents && child.Contains(child.drawRect, gui.CursorX, gui.CursorY))
                {
                    dc.SetCursor(child.cursor);
                    child.hover = true;

                    if (overChild != child)
                    {
                        if (overChild != null)
                        {
                            overChild.MouseExit();
                            str = overChild.cmd;
                            if (str.Length != 0)
                            {
                                gui.Desktop.AddCommand(str);
                                overChild.cmd = "";
                            }
                        }
                        overChild = child;
                        overChild.MouseEnter();
                        str = overChild.cmd;
                        if (str.Length != 0)
                        {
                            gui.Desktop.AddCommand(str);
                            overChild.cmd = "";
                        }
                    }
                    else if ((child.flags & WIN_HOLDCAPTURE) == 0)
                        child.RouteMouseCoords(xd, yd);
                    return "";
                }
            }
            if (overChild != null)
            {
                overChild.MouseExit();
                str = overChild.cmd;
                if (str.Length != 0)
                {
                    gui.Desktop.AddCommand(str);
                    overChild.cmd = "";
                }
                overChild = null;
            }
            return "";
        }

        public virtual void SetBuddy(Window buddy) { }

        public virtual void HandleBuddyUpdate(Window buddy) { }

        public virtual void StateChanged(bool redraw)
        {
            UpdateWinVars();

            if (expressionRegisters.Count != 0 && ops.Count != 0)
                EvalRegs();

            var c = drawWindows.Count;
            for (var i = 0; i < c; i++)
                if (drawWindows[i].win != null) drawWindows[i].win.StateChanged(redraw);
                else drawWindows[i].simp.StateChanged(redraw);

            if (redraw)
            {
                if ((flags & WIN_DESKTOP) != 0)
                    Redraw(0f, 0f);
                if (background != null && background.CinematicLength != 0)
                    background.UpdateCinematic(gui.Time);
            }
        }

        public virtual void ReadFromDemoFile(VFileDemo f, bool rebuild = true);

        public virtual void WriteToDemoFile(VFileDemo f);

        // SaveGame support
        public void WriteSaveGameString(string s, VFile savefile);

        public void WriteSaveGameTransition(TransitionData trans, VFile savefile);

        public virtual void WriteToSaveGame(VFile savefile);

        public void ReadSaveGameString(string s, VFile savefile);

        public void ReadSaveGameTransition(TransitionData trans, VFile savefile);

        public virtual void ReadFromSaveGame(VFile savefile);

        public void FixupTransitions();

        public virtual void HasAction() { }

        public virtual void HasScripts() { }

        public void FixupParms();

        public void GetScriptString(string name, string o);

        public void SetScriptParams();

        public bool HasOps => ops.Count > 0;

        public float EvalRegs(int test = -1, bool force = false);

        public void StartTransition();

        public void AddTransition(WinVar dest, Vector4 from, Vector4 to, int time, float accelTime, float decelTime);

        public void ResetTime(int time);

        public void ResetCinematics();

        public int NumTransitions => 0;

        public bool ParseScript(Parser src, GuiScriptList list, int? timeParm = null, bool allowIf = false);

        public bool RunScript(SCRIPT n);

        public bool RunScriptList(GuiScriptList src);

        public void SetRegs(string key, string val);

        public int ParseExpression(Parser src, WinVar var = null, int component = 0);

        public int ExpressionConstant(float f);

        public RegisterList RegList => regList;

        public void AddCommand(string cmd)
        {
            var str = this.cmd;
            if (str.Length != 0) str += $" ; {cmd}";
            else str = cmd;
            this.cmd = str;
        }

        public void AddUpdateVar(WinVar var) => updateVars.AddUnique(var);

        public bool Interactive;

        public bool ContainsStateVars();

        public void SetChildWinVarVal(string name, string var, string val);

        public Window FocusedChild => null;

        public Window CaptureChild => null;

        public string Comment
        {
            get => comment;
            set => comment = value;
        }

        public string cmd;

        public virtual void RunNamedEvent(string eventName)
        {
            int i, c;

            // Find and run the event
            c = namedEvents.Count;
            for (i = 0; i < c; i++)
            {
                if (string.Equals(namedEvents[i].mName, eventName, StringComparison.OrdinalIgnoreCase))
                    continue;

                UpdateWinVars();

                // Make sure we got all the current values for stuff
                if (expressionRegisters.Count != 0 && ops.Count != 0)
                    EvalRegs(-1, true);

                RunScriptList(namedEvents[i].mEvent);

                break;
            }

            // Run the event in all the children as well
            c = children.Count;
            for (i = 0; i < c; i++)
                children[i].RunNamedEvent(eventName);
        }

        public void AddDefinedVar(WinVar var)
            => definedVars.AddUnique(var);

        public Window FindChildByPoint(float x, float y, Window below = null);

        public int GetChildIndex(Window window);

        public int ChildCount;

        public Window GetChild(int index);

        public void RemoveChild(Window win);

        public bool InsertChild(Window win, Window before);

        public void ScreenToClient(Rectangle rect);

        public void ClientToScreen(Rectangle rect);

        public bool UpdateFromDictionary(Dictionary<string, string> dict);

        protected Window FindChildByPoint(float x, float y, out Window below);

        protected void SetDefaults();

        protected bool IsSimple;

        protected void UpdateWinVars()
        {
            var c = updateVars.Count;
            for (var i = 0; i < c; i++)
                updateVars[i].Update();
        }

        protected void DisableRegister(string name);

        protected void Transition()
        {
            int i, c = transitions.Count;
            var clear = true;

            for (i = 0; i < c; i++)
            {
                var data = transitions[i];
                var data_data = data.data;
                if (data.interp.IsDone(gui.Time) && data_data != null)
                {
                    if (data_data is WinVec4) data.data = (WinVec4)data.interp.EndValue;
                    else if (data_data is WinFloat) data.data = (WinFloat)data.interp.EndValue.x;
                    else data.data = (WinRectangle)data.interp.EndValue;
                }
                else
                {
                    clear = false;
                    if (data.data != null)
                    {
                        if (data_data is WinVec4) data.data = (WinVec4)data.interp.GetCurrentValue(gui.Time);
                        else if (data_data is WinFloat) data.data = (WinFloat)data.interp.GetCurrentValue(gui.Time).x;
                        else data.data = (WinRectangle)data.interp.GetCurrentValue(gui.Time);
                    }
                    else common.Warning($"Invalid transitional data for window {Name} in gui {gui.SourceFile}");
                }
            }

            if (clear)
            {
                transitions.SetNum(0, false);
                flags &= ~WIN_INTRANSITION;
            }
        }

        protected void Time()
        {
            if (noTime)
                return;

            if (timeLine == -1)
                timeLine = gui.Time;

            cmd = "";

            var c = timeLineEvents.Count;
            if (c > 0)
                for (int i = 0; i < c; i++)
                    if (timeLineEvents[i].pending && gui.Time - timeLine >= timeLineEvents[i].time)
                    {
                        timeLineEvents[i].pending = false;
                        RunScriptList(timeLineEvents[i].event_);
                    }
            if (gui.Active)
                gui.PendingCmd += cmd;
        }

        protected bool RunTimeEvents(int time)
        {
            if (time - lastTimeRun < Usercmd.USERCMD_MSEC)
                //common.Printf("Skipping gui time events at %i\n", time);
                return false;

            lastTimeRun = time;

            UpdateWinVars();

            if (expressionRegisters.Count != 0 && ops.Count != 0)
                EvalRegs();

            if ((flags & WIN_INTRANSITION) != 0)
                Transition();

            Time();

            // renamed ON_EVENT to ON_FRAME
            RunScript(SCRIPT.ON_FRAME);

            var c = children.Count;
            for (var i = 0; i < c; i++)
                children[i].RunTimeEvents(time);

            return true;
        }
        protected void Dump();

        protected int ExpressionTemporary();
        protected WexpOp ExpressionOp();
        protected IntPtr EmitOp(IntPtr a, IntPtr b, WexpOpType opType, WexpOp opp = null);
        protected IntPtr ParseEmitOp(Parser src, IntPtr a, WexpOpType opType, int priority, WexpOp opp = null);
        protected IntPtr ParseTerm(Parser src, WinVar var_ = null, IntPtr component = 0);
        protected IntPtr ParseExpressionPriority(Parser src, int priority, WinVar var_ = null, IntPtr component = 0);
        protected void EvaluateRegisters(float[] registers);
        protected void SaveExpressionParseState();
        protected void RestoreExpressionParseState();
        protected void ParseBracedExpression(Parser src);
        protected bool ParseScriptEntry(string name, Parser src);
        protected bool ParseRegEntry(string name, Parser src);
        protected virtual bool ParseInternalVar(string name, Parser src);
        protected void ParseString(Parser src, out string o);
        protected void ParseVec4(Parser src, out Vector4 o);
        protected void ConvertRegEntry(string name, Parser src, out string o, int tabs);
    }
}