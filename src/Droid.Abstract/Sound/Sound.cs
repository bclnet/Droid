using Droid.Core;
using Droid.Framework;
using Droid.Render;
using System;
using SoundSample = System.Object;

namespace Droid.Sound
{
    public static partial class SoundEx
    {
        // unfortunately, our minDistance / maxDistance is specified in meters, and we have far too many of them to change at this time.
        const float DOOM_TO_METERS = 0.0254f;                   // doom to meters
        const float METERS_TO_DOOM = 1.0f / DOOM_TO_METERS;   // meters to doom
    }

    // sound shader flags
    [Flags]
    public enum SSF
    {
        PRIVATE_SOUND = 1 << 0,    // only plays for the current listenerId
        ANTI_PRIVATE_SOUND = 1 << 1,   // plays for everyone but the current listenerId
        NO_OCCLUSION = 1 << 2, // don't flow through portals, only use straight line
        GLOBAL = 1 << 3,   // play full volume to all speakers and all listeners
        OMNIDIRECTIONAL = 1 << 4,  // fall off with distance, but play same volume in all speakers
        LOOPING = 1 << 5,  // repeat the sound continuously
        PLAY_ONCE = 1 << 6,    // never restart if already playing on any channel of a given emitter
        UNCLAMPED = 1 << 7,    // don't clamp calculated volumes at 1.0
        NO_FLICKER = 1 << 8,   // always return 1.0 for volume queries
        NO_DUPS = 1 << 9,  // try not to play the same sound twice in a row
    }

    // these options can be overriden from sound shader defaults on a per-emitter and per-channel basis
    public struct soundShaderParms
    {
        public float minDistance;
        public float maxDistance;
        public float volume;                    // in dB, unfortunately.  Negative values get quieter
        public float shakes;
        public int soundShaderFlags;       // SSF_* bit flags
        public int soundClass;              // for global fading of sounds
    }

    // it is somewhat tempting to make this a virtual class to hide the private
    // details here, but that doesn't fit easily with the decl manager at the moment.
    public class SoundShader : Decl
    {
        const int SOUND_MAX_LIST_WAVS = 32;

        // sound classes are used to fade most sounds down inside cinematics, leaving dialog flagged with a non-zero class full volume
        const int SOUND_MAX_CLASSES = 4;

        //idSoundShader();

        public virtual int Size() => throw new NotImplementedException();
        public virtual bool SetDefaultText() => throw new NotImplementedException();
        public virtual string DefaultDefinition() => throw new NotImplementedException();
        public virtual bool Parse(string text, int textLength) => throw new NotImplementedException();
        public virtual void FreeData() => throw new NotImplementedException();
        public virtual void List() => throw new NotImplementedException();

        public virtual string GetDescription() => throw new NotImplementedException();

        // so the editor can draw correct default sound spheres this is currently defined as meters, which sucks, IMHO.
        public virtual float GetMinDistance() => throw new NotImplementedException();       // FIXME: replace this with a GetSoundShaderParms()
        public virtual float GetMaxDistance() => throw new NotImplementedException();

        // returns NULL if an AltSound isn't defined in the shader.
        // we use this for pairing a specific broken light sound with a normal light sound
        public virtual SoundShader GetAltSound() => throw new NotImplementedException();

        public virtual bool HasDefaultSound() => throw new NotImplementedException();

        public virtual soundShaderParms GetParms() => throw new NotImplementedException();
        public virtual int GetNumSounds() => throw new NotImplementedException();
        public virtual string GetSound(int index) => throw new NotImplementedException();

        public virtual bool CheckShakesAndOgg() => throw new NotImplementedException();

        // options from sound shader text
        soundShaderParms parms;                       // can be overriden on a per-channel basis

        bool onDemand;                  // only load when played, and free when finished
        int speakerMask;
        SoundShader altSound;
        string desc;                     // description
        bool errorDuringParse;
        float leadinVolume;             // allows light breaking leadin sounds to be much louder than the broken loop

        SoundSample[] leadins = new SoundSample[SOUND_MAX_LIST_WAVS];
        int numLeadins;
        SoundSample[] entries = new SoundSample[SOUND_MAX_LIST_WAVS];
        int numEntries;

        void Init() => throw new NotImplementedException();
        bool ParseShader(Lexer src) => throw new NotImplementedException();
    }

    /*
    ===============================================================================

        SOUND EMITTER

    ===============================================================================
    */

    // sound channels
    public enum SCHANNEL {
        ANY = 0,  // used in queries and commands to effect every channel at once, in startSound to have it not override any other channel
        ONE = 1,  // any following integer can be used as a channel number
    }

    public interface SoundEmitter
    {
        // a non-immediate free will let all currently playing sounds complete soundEmitters are not actually deleted, they are just marked as
        // reusable by the soundWorld
        void Free(bool immediate);

        // the parms specified will be the default overrides for all sounds started on this emitter.
        // NULL is acceptable for parms
        void UpdateEmitter(Vector3 origin, int listenerId, soundShaderParms parms);

        // returns the length of the started sound in msec
        int StartSound(SoundShader shader, SCHANNEL channel, float diversity = 0, int shaderFlags = 0, bool allowSlow = true);

        // pass SCHANNEL_ANY to effect all channels
        void ModifySound(SCHANNEL channel, soundShaderParms parms);
        void StopSound(SCHANNEL channel);
        // to is in Db (sigh), over is in seconds
        void FadeSound(SCHANNEL channel, float to, float over);

        // returns true if there are any sounds playing from this emitter.  There is some conservative slop at the end to remove inconsistent race conditions with the sound thread updates.
        // FIXME: network game: on a dedicated server, this will always be false
        bool CurrentlyPlaying();

        // returns a 0.0 to 1.0 value based on the current sound amplitude, allowing graphic effects to be modified in time with the audio.
        // just samples the raw wav file, it doesn't account for volume overrides in the
        float CurrentAmplitude();

        // for save games.  Index will always be > 0
        int Index();
    }

    public interface SoundWorld
    {
        // call at each map start
        void ClearAllSoundEmitters();
        void StopAllSounds();

        // get a new emitter that can play sounds in this world
        SoundEmitter AllocSoundEmitter();

        // for load games, index 0 will return NULL
        SoundEmitter EmitterForIndex(int index);

        // query sound samples from all emitters reaching a given position
        float CurrentShakeAmplitudeForPosition(int time, Vector3 listenerPosition);

        // where is the camera/microphone
        // listenerId allows listener-private and antiPrivate sounds to be filtered
        // gameTime is in msec, and is used to time sound queries and removals so that they are independent
        // of any race conditions with the async update
        void PlaceListener(Vector3 origin, Matrix3x3 axis, int listenerId, int gameTime, out string areaName);

        // fade all sounds in the world with a given shader soundClass
        // to is in Db (sigh), over is in seconds
        void FadeSoundClasses(int soundClass, float to, float over);

        // background music
        void PlayShaderDirectly(string name, int channel = -1);

        // dumps the current state and begins archiving commands
        void StartWritingDemo(DemoFile demo);
        void StopWritingDemo();

        // read a sound command from a demo file
        void ProcessDemoCommand(DemoFile demo);

        // pause and unpause the sound world
        void Pause();
        void UnPause();
        bool IsPaused();

        // Write the sound output to multiple wav files.  Note that this does not use the
        // work done by AsyncUpdate, it mixes explicitly in the foreground every PlaceOrigin(),
        // under the assumption that we are rendering out screenshots and the gameTime is going
        // much slower than real time.
        // path should not include an extension, and the generated filenames will be:
        // <path>_left.raw, <path>_right.raw, or <path>_51left.raw, <path>_51right.raw,
        // <path>_51center.raw, <path>_51lfe.raw, <path>_51backleft.raw, <path>_51backright.raw,
        // If only two channel mixing is enabled, the left and right .raw files will also be
        // combined into a stereo .wav file.
        void AVIOpen(string path, string name);
        void AVIClose();

        // SaveGame / demo Support
        void WriteToSaveGame(VFile savefile);
        void ReadFromSaveGame(VFile savefile);

        void SetSlowmo(bool active);
        void SetSlowmoSpeed(float speed);
        void SetEnviroSuit(bool active);
    }

    public class soundDecoderInfo
    {
        public string name;
        public string format;
        public int numChannels;
        public int numSamplesPerSecond;
        public int num44kHzSamples;
        public int numBytes;
        public bool looping;
        public float lastVolume;
        public int start44kHzTime;
        public int current44kHzTime;
    }

    public interface SoundSystem
    {
        // all non-hardware initialization
        void Init();

        // shutdown routine
        void Shutdown();

        // sound is attached to the window, and must be recreated when the window is changed
        bool InitHW();
        bool ShutdownHW();

        // asyn loop, called at 60Hz
        int AsyncUpdate(int time);

        // async loop, when the sound driver uses a write strategy
        int AsyncUpdateWrite(int time);

        // it is a good idea to mute everything when starting a new level,
        // because sounds may be started before a valid listener origin
        // is specified
        void SetMute(bool mute);

        // for the sound level meter window
        cinData ImageForTime(int milliseconds, bool waveform);

        // get sound decoder info
        int GetSoundDecoderInfo(int index, out soundDecoderInfo decoderInfo);

        // if rw == NULL, no portal occlusion or rendered debugging is available
        SoundWorld AllocSoundWorld(RenderWorld rw);

        // specifying NULL will cause silence to be played
        void SetPlayingSoundWorld(SoundWorld soundWorld);

        // some tools, like the sound dialog, may be used in both the game and the editor
        // This can return NULL, so check!
        SoundWorld GetPlayingSoundWorld();

        // Mark all soundSamples as currently unused,
        // but don't free anything.
        void BeginLevelLoad();

        // Free all soundSamples marked as unused
        // We might want to defer the loading of new sounds to this point,
        // as we do with images, to avoid having a union in memory at one time.
        void EndLevelLoad(string mapString);

        // direct mixing for OSes that support it
        int AsyncMix(int soundTime, float[] mixBuffer);

        // prints memory info
        void PrintMemInfo(MemInfo mi);

        // is EFX support present - -1: disabled at compile time, 0: no suitable hardware, 1: ok
        int IsEFXAvailable();
    }
}