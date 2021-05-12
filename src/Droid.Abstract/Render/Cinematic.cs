using System;

namespace Droid.Render
{
    // cinematic states
    public enum cinStatus
    {
        FMV_IDLE,
        FMV_PLAY,           // play
        FMV_EOF,            // all other conditions, i.e. stop/EOF/abort
        FMV_ID_BLT,
        FMV_ID_IDLE,
        FMV_LOOPED,
        FMV_ID_WAIT
    }

    // a cinematic stream generates an image buffer, which the caller will upload to a texture
    public struct cinData
    {
        public int imageWidth, imageHeight; // will be a power of 2
        public byte[] image;                        // RGBA format, alpha will be 255
        public int status;
    }

    public class Cinematic
    {
        // initialize cinematic play back data
        public static void InitCinematic() => throw new NotImplementedException();

        // shutdown cinematic play back data
        public static void ShutdownCinematic() => throw new NotImplementedException();

        // allocates and returns a private subclass that implements the methods
        // This should be used instead of new
        public static Cinematic Alloc() => throw new NotImplementedException();

        // returns false if it failed to load
        public virtual bool InitFromFile(string qpath, bool looping) => throw new NotImplementedException();

        // returns the length of the animation in milliseconds
        public virtual int AnimationLength() => throw new NotImplementedException();

        // the pointers in cinData_t will remain valid until the next UpdateForTime() call
        public virtual cinData ImageForTime(int milliseconds) => throw new NotImplementedException();

        // closes the file and frees all allocated memory
        public virtual void Close() => throw new NotImplementedException();

        // closes the file and frees all allocated memory
        public virtual void ResetTime(int time) => throw new NotImplementedException();
    }

    /*
    ===============================================

        Sound meter.

    ===============================================
    */

    public class SndWindow : Cinematic
    {
        public override bool InitFromFile(string qpath, bool looping) => throw new NotImplementedException();
        public override cinData ImageForTime(int milliseconds) => throw new NotImplementedException();
        public override int AnimationLength() => throw new NotImplementedException();

        bool showWaveform;
    }
}