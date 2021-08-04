using Gengine.Render;
using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;

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
        WexpOpType opType;
        int a, b, c, d;
    }

    public struct RegEntry
    {
        string name;
        Register.REGTYPE type;
        int index;
    }

    public struct TimeLineEvent
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
        RVNamedEvent(string name)
        {
            mEvent = new GuiScriptList();
            mName = name;
        }
        public int Size => 0 + mEvent.Size;

        string mName;
        GuiScriptList mEvent;
    }

    public struct TransitionData
    {
        WinVar data;
        int offset;
        InterpolateAccelDecelLinear<Vector4> interp;
    }


    public class Window
    {
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

        public Window(UserInterfaceLocal gui);
        public Window(DeviceContext d, UserInterfaceLocal gui);

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

        public static readonly string[] ScriptNames = new string[SCRIPT_COUNT];

        public static readonly RegEntry[] RegisterVars;
        public static const int NumRegisterVars;

        public DeviceContext DC
        {
            get => dc;
            set => dc = value;
        }

        public Window SetFocus(Window w, bool scripts = true);

        public Window SetCapture(Window w);
        public void SetParent(Window w);
        public void SetFlag(uint f);
        public void ClearFlag(uint f);
        public uint Flags => flags;
        public void Move(float x, float y);
        public void BringToTop(Window w);
        public void Adjust(float xd, float yd);
        public void SetAdjustMode(Window child);
        public void Size(float x, float y, float w, float h);
        public void SetupFromState();
        public void SetupBackground();
        public DrawWin FindChildByName(string name);
        public SimpleWindow FindSimpleWinByName(string name);
        public Window Parent => parent;
        public UserInterfaceLocal Gui => gui;
        public bool Contains(float x, float y);
        public int Size => 0;
        public virtual int Allocated => 0;
        public string GetStrPtrByName(string name);

        public virtual WinVar GetWinVarByName(string name, bool winLookup = false, DrawWin owner = null);

        public int GetWinVarOffset(WinVar wv, DrawWin dw);
        public float MaxCharHeight => 0;
        public float MaxCharWidth => 0;
        public void SetFont();
        public void SetInitialState(string name);
        public void AddChild(Window win);
        public void DebugDraw(int time, float x, float y);
        public void CalcClientRect(float xofs, float yofs);
        public void CommonInit();
        public void CleanUp();
        public void DrawBorderAndCaption(Rectangle drawRect);
        public void DrawCaption(int time, float x, float y);
        public void SetupTransforms(float x, float y);
        public bool Contains(Rectangle sr, float x, float y);
        public string Name => name;

        public virtual bool Parse(Parser src, bool rebuild = true);
        public virtual string HandleEvent(SysEvent ev, out bool updateVisuals);
        public void CalcRects(float x, float y);
        public virtual void Redraw(float x, float y);

        public virtual void ArchiveToDictionary(Dictionary<string, string> dict, bool useNames = true);
        public virtual void InitFromDictionary(Dictionary<string, string> dict, bool byName = true);
        public virtual void PostParse();
        public virtual void Activate(bool activate, string act);
        public virtual void Trigger();
        public virtual void GainFocus();
        public virtual void LoseFocus();
        public virtual void GainCapture();
        public virtual void LoseCapture();
        public virtual void Sized();
        public virtual void Moved();
        public virtual void Draw(int time, float x, float y);
        public virtual void MouseExit();
        public virtual void MouseEnter();
        public virtual void DrawBackground(Rectangle drawRect);
        public virtual string RouteMouseCoords(float xd, float yd);
        public virtual void SetBuddy(Window buddy) { }
        public virtual void HandleBuddyUpdate(Window buddy) { }
        public virtual void StateChanged(bool redraw);
        public virtual void ReadFromDemoFile(VDemoFile f, bool rebuild = true);
        public virtual void WriteToDemoFile(VDemoFile f);

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
        public bool RunScript(int n);
        public bool RunScriptList(GuiScriptList src);
        public void SetRegs(string key, string val);
        public int ParseExpression(Parser src, WinVar var = null, int component = 0);
        public int ExpressionConstant(float f);
        public RegisterList RegList => regList;
        public void AddCommand(string cmd);
        public void AddUpdateVar(WinVar var);
        public bool Interactive();
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

        public virtual void RunNamedEvent(string eventName);

        public void AddDefinedVar(WinVar var)
            => definedVars.AddUnique(var);

        public Window FindChildByPoint(float x, float y, Window below = null);
        public int GetChildIndex(Window window);
        public int GetChildCount();
        public idWindow* GetChild(int index);
        public void RemoveChild(Window win);
        public bool InsertChild(Window win, Window before);

        public void ScreenToClient(Rectangle rect);
        public void ClientToScreen(Rectangle rect);

        public bool UpdateFromDictionary(Dictionary<string, string> dict);


        protected Window FindChildByPoint(float x, float y, out Window below);
        protected void SetDefaults();

        protected bool IsSimple();
        protected void UpdateWinVars();
        protected void DisableRegister(string name);
        protected void Transition();
        protected void Time();
        protected bool RunTimeEvents(int time);
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

        protected float actualX;                    // physical coords
        protected float actualY;                    // ''
        protected int childID;                  // this childs id
        protected uint flags;             // visible, focus, mouseover, cursor, border, etc..
        protected int lastTimeRun;              //
        protected Rectangle drawRect;           // overall rect
        protected Rectangle clientRect;         // client area
        protected Vector2 origin;

        protected int timeLine;                 // time stamp used for various fx
        protected float xOffset;
        protected float yOffset;
        protected float forceAspectWidth;
        protected float forceAspectHeight;
        protected float matScalex;
        protected float matScaley;
        protected float borderSize;
        protected float textAlignx;
        protected float textAligny;
        protected string name;
        protected string comment;
        protected Vector2 shear;

        protected char textShadow;
        protected byte fontNum;
        protected byte cursor;                  //
        protected char textAlign;

        protected WinBool noTime;                   //
        protected WinBool visible;              //
        protected WinBool noEvents;
        protected WinRectangle rect;                // overall rect
        protected WinVec4 backColor;
        protected WinVec4 matColor;
        protected WinVec4 foreColor;
        protected WinVec4 hoverColor;
        protected WinVec4 borderColor;
        protected WinFloat textScale;
        protected WinFloat rotate;
        protected WinStr text;
        protected WinBackground backGroundName;         //

        protected List<WinVar> definedVars = new();
        protected List<WinVar> updateVars = new();

        protected Rectangle textRect;           // text extented rect
        protected Material background;         // background asset

        protected Window parent;                // parent window
        protected List<Window> children = new();        // child windows
        protected List<DrawWin> drawWindows = new();

        protected Window focusedChild;          // if a child window has the focus
        protected Window captureChild;          // if a child window has mouse capture
        protected Window overChild;         // if a child window has mouse capture
        protected bool hover;

        protected DeviceContext dc;

        protected UserInterfaceLocal gui;

        protected static readonly CVar gui_debug;
        protected static readonly CVar gui_edit;

        protected GuiScriptList[] scripts = new GuiScriptList[SCRIPT_COUNT];
        protected bool[] saveTemps;

        protected List<TimeLineEvent> timeLineEvents = new();
        protected List<TransitionData> transitions = new();

        protected static bool registerIsTemporary[MAX_EXPRESSION_REGISTERS]; // statics to assist during parsing

        protected List<WexpOp> ops = new();             // evaluate to make expressionRegisters
        protected List<float> expressionRegisters = new();
        protected List<WexpOp> saveOps = new();             // evaluate to make expressionRegisters
        protected List<RVNamedEvent> namedEvents = new();       //  added named events
        protected List<float> saveRegs = new();

        protected RegisterList regList;

        protected WinBool hideCursor;
    }
}