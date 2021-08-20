using Gengine.Framework;
using Gengine.Render;
using System;
using System.NumericsX;
using System.NumericsX.Core;
//using ChannelType = System.Int32; // the game uses its own series of enums, and we don't want to require casts
using FourCC = System.Int32;
//using ALuint = System.UInt32;
using System.Collections.Generic;

namespace Gengine.Sound
{
    // demo sound commands
    public enum SCMD : int
    {
        STATE,             // followed by a load game state
        PLACE_LISTENER,
        ALLOC_EMITTER,
        FREE,
        UPDATE,
        START,
        MODIFY,
        STOP,
        FADE
    }

    #region General extended waveform format structure.

    public class WaveformatEx
    {
        public short wFormatTag;        // format type
        public short nChannels;         // number of channels (i.e. mono, stereo...)
        public int nSamplesPerSec;    // sample rate
        public int nAvgBytesPerSec;   // for buffer estimation
        public short nBlockAlign;       // block size of data
        public short wBitsPerSample;    // Number of bits per sample of mono data
        public short cbSize;            // The count in bytes of the size of extra information (after cbSize)

        internal void memset()
        {
            throw new NotImplementedException();
        }
    }

    // OLD general waveform format structure (information common to all formats)
    public struct Waveformat
    {
        public short wFormatTag;        // format type
        public short nChannels;         // number of channels (i.e. mono, stereo, etc.)
        public int nSamplesPerSec;    // sample rate
        public int nAvgBytesPerSec;   // for buffer estimation
        public short nBlockAlign;       // block size of data
    }

    // flags for wFormatTag field of WAVEFORMAT
    public enum WAVE_FORMAT_TAG : short
    {
        PCM = 1,
        OGG = 2
    }

    // specific waveform format structure for PCM data
    public struct Pcmwaveformat
    {
        public Waveformat wf;
        public short wBitsPerSample;
    }


    public struct WaveformatExtensible
    {
        public WaveformatEx Format;
        //      union {
        //short wValidBitsPerSample;       // bits of precision 
        //      short wSamplesPerBlock;          // valid if wBitsPerSample==0
        //      short wReserved;                 // If neither applies, set to zero
        //      } Samples;
        public int dwChannelMask;      // which channels are
        public int SubFormat; // present in stream 

        internal void memset()
        {
            throw new NotImplementedException();
        }
    }

    // RIFF chunk information data structure
    public struct Mminfo
    {
        public FourCC ckid;           // chunk ID //
        public uint cksize;         // chunk size //
        public FourCC fccType;        // form type or list type //
        public int dwDataOffset;   // offset of data portion of chunk //
    }

    #endregion

    #region WaveFile

    //public class WaveFile
    //{
    //    WaveformatExtensible mpwfx;        // Pointer to waveformatex structure
    //    VFile mhmmio;         // I/O handle for the WAVE
    //    Mminfo mck;           // Multimedia RIFF chunk
    //    Mminfo mckRiff;       // used when opening a WAVE file
    //    int mdwSize;      // size in samples
    //    int mMemSize;     // size of the wave data in memory
    //    int mseekBase;
    //    DateTime mfileTime;

    //    bool mbIsReadingFromMemory;
    //    short[] mpbData;
    //    short[] mpbDataCur;
    //    int mulDataSize;

    //    object ogg;          // only !NULL when !s_realTimeDecoding
    //    bool isOgg;

    //    public WaveFile();

    //    public int Open(string strFileName, WaveformatEx pwfx = null);
    //    public int OpenFromMemory(short[] pbData, int ulDataSize, WaveformatExtensible pwfx);
    //    public int Read(byte[] pBuffer, int dwSizeToRead, out int pdwSizeRead);
    //    public int Seek(int offset);
    //    public int Close();
    //    public int ResetFile();

    //    public int OutputSize => mdwSize;
    //    public int MemorySize => mMemSize;

    //    int ReadMMIO();

    //    int OpenOGG(string strFileName, Waveformatex pwfx = null);
    //    int ReadOGG(byte[] pBuffer, int dwSizeToRead, out int pdwSizeRead);
    //    int CloseOGG();
    //}

    #endregion

    #region Encapsulates functionality of a DirectSound buffer.

    public interface IAudioBuffer
    {
        int Play(int dwPriority = 0, int dwFlags = 0);
        int Stop();
        int Reset();
        bool IsSoundPlaying();
        void SetVolume(float x);
    }

    #endregion

    #region SoundEmitterLocal

    public enum REMOVE_STATUS // removeStatus_t;
    {
        INVALID = -1,
        ALIVE = 0,
        WAITSAMPLEFINISHED = 1,
        SAMPLEFINISHED = 2
    }

    public class SoundFade
    {
        public int fadeStart44kHz;
        public int fadeEnd44kHz;
        public float fadeStartVolume;      // in dB
        public float fadeEndVolume;            // in dB

        public void Clear();
        public float FadeDbAt44kHz(int current44kHz);
    }

    public class SoundFX
    {
        protected bool initialized;

        protected int channel;
        protected int maxlen;

        protected float[] buffer;
        protected float[] continuitySamples = new float[4];

        protected float param;

        public SoundFX() { channel = 0; buffer = null; initialized = false; maxlen = 0; Array.Clear(continuitySamples, 0, 4); }

        public virtual void Initialize() { }
        public abstract void ProcessSample(float[] i, float[] o);

        public int Channel
        {
            get => channel;
            set => channel = value;
        }

        public void SetContinuitySamples(float in1, float in2, float out1, float out2) { continuitySamples[0] = in1; continuitySamples[1] = in2; continuitySamples[2] = out1; continuitySamples[3] = out2; };      // FIXME?
        public void GetContinuitySamples(out float in1, out float in2, out float out1, out float out2) { in1 = continuitySamples[0]; in2 = continuitySamples[1]; out1 = continuitySamples[2]; out2 = continuitySamples[3]; };

        public void SetParameter(float val) => param = val;
    }

    public class SoundFX_Lowpass : SoundFX
    {
        public virtual void ProcessSample(float[] i, float[] o);
    }

    public class SoundFX_LowpassFast : SoundFX
    {
        float freq;
        float res;
        float a1, a2, a3;
        float b1, b2;

        public virtual void ProcessSample(float[] i, float[] o);
        public void SetParms(float p1 = 0, float p2 = 0, float p3 = 0);
    }

    public class SoundFX_Comb : SoundFX
    {
        int currentTime;

        public virtual void Initialize();
        public virtual void ProcessSample(float[] i, float[] o);
    }

    public class FracTime
    {
        public int time;
        public float frac;

        public void Set(int val) { time = val; frac = 0; }
        public void Increment(float val) { frac += val; while (frac >= 1f) { time++; frac--; } }
    }

    public enum PLAYBACK
    {
        RESET,
        ADVANCING
    }

    public class SlowChannel
    {
        bool active;
        SoundChannel chan;

        int playbackState;
        int triggerOffset;

        FracTime newPosition;
        int newSampleOffset;

        FracTime curPosition;
        int curSampleOffset;

        SoundFX_LowpassFast lowpass;

        // functions
        void GenerateSlowChannel(ref FracTime playPos, int sampleCount44k, float[] finalBuffer);

        float GetSlowmoSpeed();

        public void AttachSoundChannel(SoundChannel chan);
        public void Reset();

        public void GatherChannelSamples(int sampleOffset44k, int sampleCount44k, float[] dest);

        public bool IsActive => active;
        public FracTime CurrentPosition => curPosition;
    }

    public class SoundChannel
    {
        public bool triggerState;
        public int trigger44kHzTime;       // hardware time sample the channel started
        public int triggerGame44kHzTime;   // game time sample time the channel started
        public SoundShaderParms parms;                   // combines the shader parms and the per-channel overrides
        public SoundSample leadinSample;            // if not looped, this is the only sample
        public int triggerChannel;
        public SoundShader soundShader;
        public ISampleDecoder decoder;
        public float diversity;
        public float lastVolume;               // last calculated volume based on distance
        public float[] lastV = new float[6];             // last calculated volume for each speaker, so we can smoothly fade
        public SoundFade channelFade;
        public bool triggered;
        public int openalSource;
        public int openalStreamingOffset;
        public int[] openalStreamingBuffer = new int[3];
        public int[] lastopenalStreamingBuffer = new int[3];
        public bool stopped;

        public bool disallowSlow;

        public SoundChannel();

        public void Clear();
        public void Start();
        public void Stop();
        public void GatherChannelSamples(int sampleOffset44k, int sampleCount44k, float[] dest);
        public void ALStop();         // free OpenAL resources if any
    }

    public class SoundEmitterLocal : ISoundEmitter
    {
        public SoundEmitterLocal();

        //----------------------------------------------

        // the "time" parameters should be game time in msec, which is used to make queries return deterministic values regardless of async buffer scheduling
        // a non-immediate free will let all currently playing sounds complete
        public virtual void Free(bool immediate);

        // the parms specified will be the default overrides for all sounds started on this emitter. NULL is acceptable for parms
        public virtual void UpdateEmitter(Vector3 origin, int listenerId, SoundShaderParms parms);

        // returns the length of the started sound in msec
        public virtual int StartSound(ISoundShader shader, int channel, float diversity = 0, int shaderFlags = 0, bool allowSlow = true /* D3XP */ );

        // can pass SCHANNEL_ANY
        public virtual void ModifySound(int channel, SoundShaderParms parms);
        public virtual void StopSound(int channel);
        public virtual void FadeSound(int channel, float to, float over);

        public virtual bool CurrentlyPlaying();

        // can pass SCHANNEL_ANY
        public virtual float CurrentAmplitude();

        // used for save games
        public virtual int Index();

        //----------------------------------------------

        public void Clear();

        public void OverrideParms(SoundShaderParms base_, SoundShaderParms over, SoundShaderParms o);
        public void CheckForCompletion(int current44kHzTime);
        public void Spatialize(Vector3 listenerPos, int listenerArea, IRenderWorld rw);

        public SoundWorldLocal soundWorld;              // the world that holds this emitter

        public int index;                      // in world emitter list
        public RemoveStatus removeStatus;

        public Vector3 origin;
        public int listenerId;
        public SoundShaderParms parms;                       // default overrides for all channels

        // the following are calculated in UpdateEmitter, and don't need to be archived
        public float maxDistance;              // greatest of all playing channel distances
        public int lastValidPortalArea;        // so an emitter that slides out of the world continues playing
        public bool playing;                   // if false, no channel is active
        public bool hasShakes;
        public Vector3 spatializedOrigin;      // the virtual sound origin, either the real sound origin, or a point through a portal chain
        public float realDistance;             // in meters
        public float distance;                 // in meters, this may be the straight-line distance, or it may go through a chain of portals.  If there
                                               // is not an open-portal path, distance will be > maxDistance

        // a single soundEmitter can have many channels playing from the same point
        public SoundChannel[] channels = new SoundChannel[SoundSystemLocal.SOUND_MAX_CHANNELS];

        public SlowChannel[] slowChannels = new SlowChannel[SoundSystemLocal.SOUND_MAX_CHANNELS];

        public SlowChannel GetSlowChannel(SoundChannel chan);
        public void SetSlowChannel(SoundChannel chan, SlowChannel slow);
        public void ResetSlowChannel(SoundChannel chan);

        // this is just used for feedback to the game or rendering system: flashing lights and screen shakes.  Because the material expression
        // evaluation doesn't do common subexpression removal, we cache the last generated value
        public int ampTime;
        public float amplitude;
    }

    #endregion

    #region SoundWorldLocal

    public class SoundStats
    {
        public int rinuse;
        public int runs = 1;
        public int timeinprocess;
        public int missedWindow;
        public int missedUpdateWindow;
        public int activeSounds;
    }

    public class SoundPortalTrace
    {
        public int portalArea;
        public SoundPortalTrace prevStack;
    }

    //public class SoundWorldLocal : ISoundWorld
    //{
    //    // call at each map start
    //    public virtual void ClearAllSoundEmitters();
    //    public virtual void StopAllSounds();

    //    // get a new emitter that can play sounds in this world
    //    public virtual SoundEmitter AllocSoundEmitter();

    //    // for load games
    //    public virtual SoundEmitter EmitterForIndex(int index);

    //    // query data from all emitters in the world
    //    public virtual float CurrentShakeAmplitudeForPosition(int time, Vector3 listererPosition);

    //    // where is the camera/microphone listenerId allows listener-private sounds to be added
    //    public virtual void PlaceListener(Vector3 origin, Matrix3x3 axis, int listenerId, int gameTime, string areaName);

    //    // fade all sounds in the world with a given shader soundClass to is in Db (sigh), over is in seconds
    //    public virtual void FadeSoundClasses(int soundClass, float to, float over);

    //    // dumps the current state and begins archiving commands
    //    public virtual void StartWritingDemo(VFileDemo demo);
    //    public virtual void StopWritingDemo();

    //    // read a sound command from a demo file
    //    public virtual void ProcessDemoCommand(VFileDemo readDemo);

    //    // background music
    //    public virtual void PlayShaderDirectly(string name, int channel = -1);

    //    // pause and unpause the sound world
    //    public virtual void Pause();
    //    public virtual void UnPause();
    //    public virtual bool IsPaused { get; }

    //    // avidump
    //    public virtual void AVIOpen(string path, string name);
    //    public virtual void AVIClose();

    //    // SaveGame Support
    //    public virtual void WriteToSaveGame(VFile savefile);
    //    public virtual void ReadFromSaveGame(VFile savefile);

    //    public virtual void ReadFromSaveGameSoundChannel(VFile saveGame, SoundChannel ch);
    //    public virtual void ReadFromSaveGameSoundShaderParams(VFile saveGame, SoundShaderParms parms);
    //    public virtual void WriteToSaveGameSoundChannel(VFile saveGame, SoundChannel ch);
    //    public virtual void WriteToSaveGameSoundShaderParams(VFile saveGame, SoundShaderParms parms);

    //    public virtual void SetSlowmo(bool active);
    //    public virtual void SetSlowmoSpeed(float speed);
    //    public virtual void SetEnviroSuit(bool active);

    //    //=======================================

    //    public IRenderWorld rw;              // for portals and debug drawing
    //    public VFileDemo writeDemo;          // if not NULL, archive commands here

    //    public Matrix3x3 listenerAxis;
    //    public Vector3 listenerPos;     // position in meters
    //    public int listenerPrivateId;
    //    public Vector3 listenerQU;          // position in "quake units"
    //    public int listenerArea;
    //    public string listenerAreaName;
    //    public ALuint listenerEffect;
    //    public ALuint listenerSlot;
    //    public ALuint listenerFilter;

    //    public int gameMsec;
    //    public int game44kHz;
    //    public int pause44kHz;
    //    public int lastAVI44kHz;       // determine when we need to mix and write another block

    //    public List<SoundEmitterLocal> emitters = new();

    //    public SoundFade[] soundClassFade = new SoundFade[SOUND_MAX_CLASSES];  // for global sound fading

    //    // avi stuff
    //    public VFile[] fpa = new VFile[6];
    //    public string aviDemoPath;
    //    public string aviDemoName;

    //    public SoundEmitterLocal localSound;        // just for playShaderDirectly()

    //    public bool slowmoActive;
    //    public float slowmoSpeed;
    //    public bool enviroSuitActive;

    //    public SoundWorldLocal();

    //    public void Shutdown();
    //    public void Init(IRenderWorld rw);

    //    // update
    //    public void ForegroundUpdate(int currentTime);
    //    public void OffsetSoundTime(int offset44kHz);

    //    public SoundEmitterLocal AllocLocalSoundEmitter();
    //    public void CalcEars(int numSpeakers, Vector3 realOrigin, Vector3 listenerPos, Matrix3x3 listenerAxis, float[] ears, float spatialize);
    //    public void AddChannelContribution(SoundEmitterLocal sound, SoundChannel chan, int current44kHz, int numSpeakers, float[] finalMixBuffer);
    //    public void MixLoop(int current44kHz, int numSpeakers, float[] finalMixBuffer);
    //    public void AVIUpdate();
    //    public void ResolveOrigin(int stackDepth, SoundPortalTrace prevStack, int soundArea, float dist, Vector3 soundOrigin, SoundEmitterLocal def);
    //    public float FindAmplitude(SoundEmitterLocal sound, int localTime, Vector3 listenerPosition, ChannelType channel, bool shakesOnly);
    //}

    #endregion

    #region SoundSystemLocal

    public struct OpenalSource
    {
        public int handle;
        public int startTime;
        public SoundChannel chan;
        public bool inUse;
        public bool looping;
        public bool stereo;
    }

    public class SoundSystemLocal : ISoundSystem
    {
        //#define mmioFOURCC( ch0, ch1, ch2, ch3 )				 ((dword)(byte) (ch0) | ((dword)(byte) (ch1) << 8 ) | ((dword)(byte) (ch2) << 16 ) | ((dword)(byte) (ch3) << 24 ) )
        //#define fourcc_riff     mmioFOURCC('R', 'I', 'F', 'F')

        public const int SOUND_MAX_CHANNELS = 8;
        public const int SOUND_DECODER_FREE_DELAY = 1000 * SIMD.MIXBUFFER_SAMPLES / Usercmd.USERCMD_MSEC;       // four seconds
        public const int PRIMARYFREQ = 44100;          // samples per second
        public const float SND_EPSILON = 1f / 32768f;  // if volume is below this, it will always multiply to zero
        public const int ROOM_SLICES_IN_BUFFER = 10;

        public SoundSystemLocal()
            => isInitialized = false;

        // all non-hardware initialization
        public virtual void Init();

        // shutdown routine
        public virtual void Shutdown();

        // sound is attached to the window, and must be recreated when the window is changed
        public virtual bool ShutdownHW();
        public virtual bool InitHW();

        // async loop, called at 60Hz
        public virtual int AsyncUpdate(int time);
        // async loop, when the sound driver uses a write strategy
        public virtual int AsyncUpdateWrite(int time);
        // direct mixing called from the sound driver thread for OSes that support it
        public virtual int AsyncMix(int soundTime, float[] mixBuffer);

        public virtual void SetMute(bool mute);

        public virtual CinData ImageForTime(int milliseconds, bool waveform);

        public int GetSoundDecoderInfo(int index, SoundDecoderInfo decoderInfo);

        // if rw == NULL, no portal occlusion or rendered debugging is available
        public virtual ISoundWorld AllocSoundWorld(IRenderWorld rw);

        // specifying NULL will cause silence to be played
        public virtual void SetPlayingSoundWorld(ISoundWorld soundWorld);

        // some tools, like the sound dialog, may be used in both the game and the editor This can return NULL, so check!
        public virtual ISoundWorld GetPlayingSoundWorld();

        public virtual void BeginLevelLoad();
        public virtual void EndLevelLoad(string mapString);

        public virtual void PrintMemInfo(MemInfo mi);

        public virtual int IsEFXAvailable();

        //-------------------------

        public int Current44kHzTime => 0;
        public float dB2Scale(float val);
        public int SamplesToMilliseconds(int samples);
        public int MillisecondsToSamples(int ms);

        public void DoEnviroSuit(float[] samples, int numSamples, int numSpeakers);

        public int AllocOpenALSource(SoundChannel chan, bool looping, bool stereo);
        public void FreeOpenALSource(int handle);

        // returns true if openalDevice is still available, otherwise it will try to recover the device and return false while it's gone
        // (display audio sound devices sometimes disappear for a few seconds when switching resolution)
        public bool CheckDeviceAndRecoverIfNeeded();

        public SoundCache soundCache;

        public SoundWorldLocal currentSoundWorld;   // the one to mix each async tic

        public int olddwCurrentWritePos;   // statistics
        public int buffers;                // statistics
        public int CurrentSoundTime;       // set by the async thread and only used by the main thread

        public uint nextWriteBlock;

        public float[] realAccum = new float[6 * SIMD.MIXBUFFER_SAMPLES + 16];
        public float[] finalMixBuffer;          // points inside realAccum at a 16 byte aligned boundary

        public bool isInitialized;
        public bool muted;
        public bool shutdown;

        public SoundStats soundStats;             // NOTE: updated throughout the code, not displayed anywhere

        public int[] meterTops = new int[256];
        public int[] meterTopsTime = new int[256];

        public int[] graph;

        public float[] volumesDB = new float[1200];      // dB to float volume conversion

        public List<SoundFX> fxList = new();

        public ALCdevice openalDevice;
        public ALCcontext openalContext;
        public ALsizei openalSourceCount;
        public OpenalSource[] openalSources = new OpenalSource[256];

        public LPALGENEFFECTS alGenEffects;
        public LPALDELETEEFFECTS alDeleteEffects;
        public LPALISEFFECT alIsEffect;
        public LPALEFFECTI alEffecti;
        public LPALEFFECTF alEffectf;
        public LPALEFFECTFV alEffectfv;
        public LPALGENFILTERS alGenFilters;
        public LPALDELETEFILTERS alDeleteFilters;
        public LPALISFILTER alIsFilter;
        public LPALFILTERI alFilteri;
        public LPALFILTERF alFilterf;
        public LPALGENAUXILIARYEFFECTSLOTS alGenAuxiliaryEffectSlots;
        public LPALDELETEAUXILIARYEFFECTSLOTS alDeleteAuxiliaryEffectSlots;
        public LPALISAUXILIARYEFFECTSLOT alIsAuxiliaryEffectSlot;
        public LPALAUXILIARYEFFECTSLOTI alAuxiliaryEffectSloti;

        public EFXFile EFXDatabase;
        public bool efxloaded;

        // latches
        public static bool useEFXReverb;
        // mark available during initialization, or through an explicit test
        public static int EFXAvailable;

        // DG: for CheckDeviceAndRecoverIfNeeded()
#if __ANDROID__
        public ALCboolean(ALC_APIENTRY LPALCRESETDEVICESOFT)(ALCdevice device, ALCint[] attribs);
#endif
        public LPALCRESETDEVICESOFT alcResetDeviceSOFT; // needs ALC_SOFT_HRTF extension
        public int resetRetryCount;
        public uint lastCheckTime;

        public static readonly CVar s_noSound;
        public static readonly CVar s_device;
        public static readonly CVar s_quadraticFalloff;
        public static readonly CVar s_drawSounds;
        public static readonly CVar s_minVolume6;
        public static readonly CVar s_dotbias6;
        public static readonly CVar s_minVolume2;
        public static readonly CVar s_dotbias2;
        public static readonly CVar s_spatializationDecay;
        public static readonly CVar s_showStartSound;
        public static readonly CVar s_maxSoundsPerShader;
        public static readonly CVar s_reverse;
        public static readonly CVar s_showLevelMeter;
        public static readonly CVar s_meterTopTime;
        public static readonly CVar s_volume;
        public static readonly CVar s_constantAmplitude;
        public static readonly CVar s_playDefaultSound;
        public static readonly CVar s_useOcclusion;
        public static readonly CVar s_subFraction;
        public static readonly CVar s_globalFraction;
        public static readonly CVar s_doorDistanceAdd;
        public static readonly CVar s_singleEmitter;
        public static readonly CVar s_numberOfSpeakers;
        public static readonly CVar s_force22kHz;
        public static readonly CVar s_clipVolumes;
        public static readonly CVar s_realTimeDecoding;
        public static readonly CVar s_useEAXReverb;
        public static readonly CVar s_decompressionLimit;

        public static readonly CVar s_slowAttenuate;

        public static readonly CVar s_enviroSuitCutoffFreq;
        public static readonly CVar s_enviroSuitCutoffQ;
        public static readonly CVar s_enviroSuitSkipLowpass;
        public static readonly CVar s_enviroSuitSkipReverb;

        public static readonly CVar s_reverbTime;
        public static readonly CVar s_reverbFeedback;
        public static readonly CVar s_enviroSuitVolumeScale;
        public static readonly CVar s_skipHelltimeFX;
    }

    #endregion

    //extern idSoundSystemLocal soundSystemLocal;

    #region This class holds the actual wavefile bitmap, size, and info.

    //public class SoundSample
    //{
    //    public const int SCACHE_SIZE = MIXBUFFER_SAMPLES * 20; // 1/2 of a second (aroundabout)
    //    public SoundSample();

    //    public string name;                     // name of the sample file
    //    public DateTime timestamp;                    // the most recent of all images used in creation, for reloadImages command

    //    public WaveformatEx objectInfo;                  // what are we caching
    //    public int objectSize;                 // size of waveform in samples, excludes the header
    //    public int objectMemSize;              // object size in memory
    //    public byte[] nonCacheData;             // if it's not cached
    //    public byte[] amplitudeData;                // precomputed min,max amplitude pairs
    //    public ALuint openalBuffer;                // openal buffer
    //    public bool hardwareBuffer;
    //    public bool defaultSound;
    //    public bool onDemand;
    //    public bool purged;
    //    public bool levelLoadReferenced;       // so we can tell which samples aren't needed any more

    //    public int LengthIn44kHzSamples => 0;
    //    public DateTime NewTimeStamp => 0;
    //    public void MakeDefault();             // turns it into a beep
    //    public void Load();                        // loads the current sound based on name
    //    public void Reload(bool force);        // reloads if timestamp has changed, or always if force
    //    public void PurgeSoundSample();            // frees all data
    //    public void CheckForDownSample();      // down sample if required
    //    public bool FetchFromCache(int offset, byte[] output, out int position, out int size, bool allowIO);
    //}

    #endregion

    #region Sound sample decoder.

    //public interface ISampleDecoder
    //{
    //    public static void Init();
    //    public static void Shutdown();
    //    public static ISampleDecoder Alloc();
    //    public static void Free(ISampleDecoder decoder);
    //    public static int GetNumUsedBlocks();
    //    public static int GetUsedBlockMemory();

    //    void Decode(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest);
    //    void ClearDecoder();
    //    SoundSample Sample { get; }
    //    int LastDecodeTime { get; }
    //}

    #endregion

    #region The actual sound cache.

    //public class SoundCache
    //{
    //    bool insideLevelLoad;
    //    List<SoundSample> listCache = new();
    //    public SoundCache();
    //    public SoundSample FindSound(string fname, bool loadOnDemandOnly);
    //    public int NumObjects => listCache.Count;
    //    public SoundSample GetObject(int index);
    //    public void ReloadSounds(bool force);
    //    public void BeginLevelLoad();
    //    public void EndLevelLoad();
    //    public void PrintMemInfo(MemInfo mi);
    //}

    #endregion
}
