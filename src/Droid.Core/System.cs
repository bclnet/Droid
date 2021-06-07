using Droid.Core;
using System;
using System.Collections.Generic;

namespace Droid
{
    [Flags]
    public enum CPUID
    {
        NONE = 0x00000,
        UNSUPPORTED = 0x00001,  // unsupported (386/486)
        GENERIC = 0x00002,      // unrecognized processor
        MMX = 0x00010,          // Multi Media Extensions
        _3DNOW = 0x00020,        // 3DNow!
        SSE = 0x00040,          // Streaming SIMD Extensions
        SSE2 = 0x00080,         // Streaming SIMD Extensions 2
        SSE3 = 0x00100,         // Streaming SIMD Extentions 3 aka Prescott's New Instructions
        ALTIVEC = 0x00200,      // AltiVec
    }

    public enum JOYSTICK_AXIS
    {
        SIDE,
        FORWARD,
        UP,
        ROLL,
        YAW,
        PITCH,
        MAX
    }

    public enum SE
    {
        NONE,                // evTime is still valid
        KEY,                 // evValue is a key code, evValue2 is the down flag
        CHAR,                // evValue is an ascii char
        MOUSE,               // evValue and evValue2 are reletive signed x / y moves
        JOYSTICK_AXIS,       // evValue is an axis number and evValue2 is the current state (-127 to 127)
        CONSOLE              // evPtr is a char*, from typing something at a non-game console
    }

    public enum M
    {
        M_ACTION1,
        M_ACTION2,
        M_ACTION3,
        M_ACTION4,
        M_ACTION5,
        M_ACTION6,
        M_ACTION7,
        M_ACTION8,
        M_DELTAX,
        M_DELTAY,
        M_DELTAZ
    }

    public struct SysEvent
    {
        public SE evType;
        public int evValue;
        public int evValue2;
        public int evPtrLength;         // bytes of data pointed to by evPtr, for journaling
        public IntPtr evPtr;            // this must be manually freed if not NULL
    }

    public enum PATH
    {
        BASE,
        CONFIG,
        SAVE,
        EXE
    }

    public static partial class SysX
    {
        public static void Init() => throw new NotImplementedException();
        public static void Shutdown() => throw new NotImplementedException();
        public static void Error(string error, params object[] args) => throw new NotImplementedException();
        public static void Quit() => throw new NotImplementedException();

        // note that this isn't journaled...
        public static string GetClipboardData() => throw new NotImplementedException();
        public static void SetClipboardData(string s) => throw new NotImplementedException();

        // will go to the various text consoles
        // NOT thread safe - never use in the async paths
        public static void Printf(string msg, params object[] args) => throw new NotImplementedException();

        // guaranteed to be thread-safe
        public static void DebugPrintf(string fmt, params object[] args) => throw new NotImplementedException();
        public static void DebugVPrintf(string fmt, object[] args) => throw new NotImplementedException();

        // allow game to yield CPU time
        // NOTE: due to SDL_TIMESLICE this is very bad portability karma, and should be completely removed
        public static void Sleep(int msec) => throw new NotImplementedException();

        // Sys_Milliseconds should only be used for profiling purposes,
        // any game related timing information should come from event timestamps
        public static uint Milliseconds() => throw new NotImplementedException();

        // returns a selection of the CPUID_* flags
        public static int GetProcessorId() => throw new NotImplementedException();

        // sets the FPU precision
        public static void FPU_SetPrecision() => throw new NotImplementedException();

        // sets Flush-To-Zero mode
        public static void FPU_SetFTZ(bool enable) => throw new NotImplementedException();

        // sets Denormals-Are-Zero mode
        public static void FPU_SetDAZ(bool enable) => throw new NotImplementedException();

        // returns amount of system ram
        public static int GetSystemRam() => throw new NotImplementedException();

        // returns amount of drive space in path
        public static int GetDriveFreeSpace(string path) => throw new NotImplementedException();

        // lock and unlock memory
        public static unsafe bool LockMemory(void* ptr, int bytes) => throw new NotImplementedException();
        public static unsafe bool UnlockMemory(void* ptr, int bytes) => throw new NotImplementedException();

        // set amount of physical work memory
        public static void SetPhysicalWorkMemory(int minBytes, int maxBytes) => throw new NotImplementedException();

        // DLL loading, the path should be a fully qualified OS path to the DLL file to be loaded
        public static IntPtr DLL_Load(string dllName) => throw new NotImplementedException();
        public static IntPtr DLL_GetProcAddress(IntPtr dllHandle, string procName) => throw new NotImplementedException();
        public static void DLL_Unload(IntPtr dllHandle) => throw new NotImplementedException();

        // event generation
        public static void GenerateEvents() => throw new NotImplementedException();
        public static SysEvent GetEvent() => throw new NotImplementedException();
        public static void ClearEvents() => throw new NotImplementedException();
        public static string ConsoleInput() => throw new NotImplementedException();

        // input is tied to windows, so it needs to be started up and shut down whenever the main window is recreated
        public static void InitInput() => throw new NotImplementedException();
        public static void ShutdownInput() => throw new NotImplementedException();
        public static void InitScanTable() => throw new NotImplementedException();
        public static sbyte GetConsoleKey(bool shifted) => throw new NotImplementedException();
        // map a scancode key to a char. does nothing on win32, as SE_KEY == SE_CHAR there on other OSes, consider the keyboard mapping
        public static char MapCharForKey(int key) => throw new NotImplementedException();

        // keyboard input polling
        public static int PollKeyboardInputEvents() => throw new NotImplementedException();
        public static int ReturnKeyboardInputEvent(int n, out int ch, out bool state) => throw new NotImplementedException();
        public static void EndKeyboardInputEvents() => throw new NotImplementedException();

        // mouse input polling
        public static int PollMouseInputEvents() => throw new NotImplementedException();
        public static int ReturnMouseInputEvent(int n, out int action, out int value) => throw new NotImplementedException();
        public static void EndMouseInputEvents() => throw new NotImplementedException();

        // when the console is down, or the game is about to perform a lengthy operation like map loading, the system can release the mouse cursor when in windowed mode
        public static void GrabMouseCursor(bool grabIt) => throw new NotImplementedException();

        public static void AddMouseMoveEvent(int dx, int dy) => throw new NotImplementedException();
        public static void AddMouseButtonEvent(int button, bool pressed) => throw new NotImplementedException();
        public static void AddKeyEvent(int key, bool pressed) => throw new NotImplementedException();

        public static void ShowWindow(bool show) => throw new NotImplementedException();
        public static bool IsWindowVisible() => throw new NotImplementedException();
        public static void ShowConsole(int visLevel, bool quitOnClose) => throw new NotImplementedException();

        public static void Mkdir(string path) => throw new NotImplementedException();
        public static DateTime FileTimeStamp(VFile fp) => throw new NotImplementedException();
        // NOTE: do we need to guarantee the same output on all platforms?
        public static string TimeStampToStr(DateTime timeStamp) => throw new NotImplementedException();

        public static bool GetPath(PATH type, out string path) => throw new NotImplementedException();

        // use fs_debug to verbose Sys_ListFiles
        // returns -1 if directory was not found (the list is cleared)
        public static int ListFiles(string directory, string extension, List<string> list) => throw new NotImplementedException();
    }

    public enum NA
    {
        BAD,                 // an address lookup failed
        LOOPBACK,
        BROADCAST,
        IP
    }

    public class Netadr
    {
        public NA type;
        public byte[] ip = new byte[4];
        public ushort port;
    }

    public class Port
    {
        const short PORT_ANY = -1;

        //public SysPort();               // this just zeros netSocket and port

        // if the InitForPort fails, the idPort.port field will remain 0
        public bool InitForPort(int portNumber) => throw new NotImplementedException();
        public int GetPort() => bound_to.port;
        public Netadr GetAdr() => bound_to;
        public void Close() => throw new NotImplementedException();

        public bool GetPacket(Netadr from, byte[] data, int size, int maxSize) => throw new NotImplementedException();
        public bool GetPacketBlocking(Netadr from, byte[] data, int size, int maxSize, int timeout) => throw new NotImplementedException();
        public void SendPacket(Netadr to, byte[] data, int size) => throw new NotImplementedException();

        public int packetsRead;
        public int bytesRead;

        public int packetsWritten;
        public int bytesWritten;

        Netadr bound_to;      // interface and port
        int netSocket;      // OS specific socket
    }

    public class TCP
    {
        //public TCP();

        // if host is host:port, the value of port is ignored
        public bool Init(string host, short port) => throw new NotImplementedException();
        public void Close() => throw new NotImplementedException();

        // returns -1 on failure (and closes socket)
        // those are non blocking, can be used for polling
        // there is no buffering, you are not guaranteed to Read or Write everything in a single call
        // (specially on win32, see recv and send documentation)
        public int Read(byte[] data, int size) => throw new NotImplementedException();
        public int Write(byte[] data, int size) => throw new NotImplementedException();

        Netadr address;       // remote address
        int fd;             // OS specific socket
    }

    static partial class SysX
    {
        // parses the port number, can also do DNS resolve if you ask for it.
        // NOTE: DNS resolve is a slow/blocking call, think before you use (could be exploited for server DoS)
        public static bool StringToNetAdr(string s, Netadr a, bool doDNSResolve) => throw new NotImplementedException();
        public static string NetAdrToString(Netadr a) => throw new NotImplementedException();
        public static bool IsLANAddress(Netadr a) => throw new NotImplementedException();
        public static bool CompareNetAdrBase(Netadr a, Netadr b) => throw new NotImplementedException();

        public static void InitNetworking() => throw new NotImplementedException();
        public static void ShutdownNetworking() => throw new NotImplementedException();
    }

    /*
    ==============================================================

        Multi-threading

    ==============================================================
    */

    public struct XThreadInfo
    {
        public string name;
        public object threadHandle;
        public int threadId;
    }

    public enum CRITICAL_SECTION
    {
        ZERO = 0,
        SECTION_ONE,
        SECTION_TWO,
        SECTION_THREE,
        SECTION_SYS
    }

    public enum TRIGGER_EVENT
    {
        ZERO = 0,
        EVENT_ONE,
        EVENT_TWO,
        EVENT_THREE,
        EVENT_RUN_BACKEND,
        EVENT_BACKEND_FINISHED,
        EVENT_IMAGES_PROCESSES
    }

    static partial class SysX
    {
        public static void CreateThread(object function, object parms, out XThreadInfo info, string name) => throw new NotImplementedException();
        public static void DestroyThread(ref XThreadInfo info) => throw new NotImplementedException(); // sets threadHandle back to 0

        // find the name of the calling thread
        // if index != NULL, set the index in threads array (use -1 for "main" thread)
        public static string GetThreadName(int index = 0) => throw new NotImplementedException();

        public static void InitThreads() => throw new NotImplementedException();
        public static void ShutdownThreads() => throw new NotImplementedException();

        const int MAX_CRITICAL_SECTIONS = 5;

        public static void EnterCriticalSection(CRITICAL_SECTION index = CRITICAL_SECTION.ZERO) => throw new NotImplementedException();
        public static void LeaveCriticalSection(CRITICAL_SECTION index = CRITICAL_SECTION.ZERO) => throw new NotImplementedException();

        const int MAX_TRIGGER_EVENTS = 7;

        public static void WaitForEvent(TRIGGER_EVENT index = TRIGGER_EVENT.ZERO) => throw new NotImplementedException();
        public static void TriggerEvent(TRIGGER_EVENT index = TRIGGER_EVENT.ZERO) => throw new NotImplementedException();
    }

    public interface ISystem
    {
        void DebugPrintf(string fmt, params object[] args);
        void DebugVPrintf(string fmt, object[] args);

        uint GetMilliseconds();
        CPUID GetProcessorId();
        void FPU_SetFTZ(bool enable);
        void FPU_SetDAZ(bool enable);

        bool LockMemory(IntPtr ptr, int bytes);
        bool UnlockMemory(IntPtr ptr, int bytes);

        IntPtr DLL_Load(string dllName);
        IntPtr DLL_GetProcAddress(IntPtr dllHandle, string procName);
        void DLL_Unload(IntPtr dllHandle);
        void DLL_GetFileName(string baseName, string dllName, int maxLength);

        SysEvent GenerateMouseButtonEvent(int button, bool down);
        SysEvent GenerateMouseMoveEvent(int deltax, int deltay);

        void OpenURL(string url, bool quit);
        void StartProcess(string exePath, bool quit);
    }
}