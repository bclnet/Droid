namespace System.NumericsX.OpenStack
{
    [Flags]
    public enum CPUID
    {
        NONE = 0x00000,
        UNSUPPORTED = 0x00001,  // unsupported (386/486)
        GENERIC = 0x00002,      // unrecognized processor
        MMX = 0x00010,          // Multi Media Extensions
        _3DNOW = 0x00020,       // 3DNow!
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
        //public const int SizeOf = sizeof(SysEvent);
        public static readonly SysEvent None = new() { evType = SE.NONE, evValue = 0, evValue2 = 0, evPtrLength = 0, evPtr = IntPtr.Zero };
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
        public void memset()
        {
            type = 0;
            Array.Clear(ip, 0, 4);
            port = 0;
        }
        public override string ToString()
            => ISystem.NetAdrToString(this);
    }

    public class NetPort
    {
        public const short PORT_ANY = -1;

        //public NetPort();               // this just zeros netSocket and port

        // if the InitForPort fails, the idPort.port field will remain 0
        public bool InitForPort(int portNumber) => throw new NotImplementedException();
        public int Port => bound_to.port;
        public Netadr Adr => bound_to;
        public void Close() => throw new NotImplementedException();

        public bool GetPacket(out Netadr from, byte[] data, out int size, int maxSize) => throw new NotImplementedException();
        public bool GetPacketBlocking(out Netadr from, byte[] data, out int size, int maxSize, int timeout) => throw new NotImplementedException();
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

    public interface ISystem
    {
        // parses the port number, can also do DNS resolve if you ask for it.
        // NOTE: DNS resolve is a slow/blocking call, think before you use (could be exploited for server DoS)
        public static bool StringToNetAdr(string s, out Netadr a, bool doDNSResolve) => throw new NotImplementedException();
        public static string NetAdrToString(Netadr a) => throw new NotImplementedException();
        public static bool IsLANAddress(Netadr a) => throw new NotImplementedException();
        public static bool CompareNetAdrBase(Netadr a, Netadr b) => throw new NotImplementedException();

        public static void InitNetworking() => throw new NotImplementedException();
        public static void ShutdownNetworking() => throw new NotImplementedException();

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

        void DebugPrintf(string fmt, params object[] args);

        uint Milliseconds { get; }
        CPUID ProcessorId { get; }
        void FPU_SetFTZ(bool enable);
        void FPU_SetDAZ(bool enable);

        unsafe bool LockMemory(void* ptr, int bytes);
        unsafe bool UnlockMemory(void* ptr, int bytes);

        IntPtr DLL_Load(string dllName);
        IntPtr DLL_GetProcAddress(IntPtr dllHandle, string procName);
        void DLL_Unload(IntPtr dllHandle);
        string DLL_GetFileName(string baseName);

        SysEvent GenerateMouseButtonEvent(int button, bool down);
        SysEvent GenerateMouseMoveEvent(int deltax, int deltay);

        void OpenURL(string url, bool quit);
        void StartProcess(string exePath, bool quit);
    }
}