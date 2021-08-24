//#define OV_EXCLUDE_STATIC_CALLBACKS
//#include <vorbis/codec.h>
//#include <vorbis/vorbisfile.h>

using Gengine.Framework;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Gengine.NumericsX;
using Gengine.NumericsX.Core;
using static Gengine.Lib;
using static Gengine.NumericsX.Lib;
using System.NumericsX;
using Gengine.NumericsX.Sys;

namespace Gengine.Sound
{
    // Thread safe decoder memory allocator.
    // Each OggVorbis decoder consumes about 150kB of memory.
    static class _decoder
    {
        static DynamicBlockAlloc<byte> decoderMemoryAllocator = new(1 << 20, 128, 0, x => new byte[x]);
        const int MIN_OGGVORBIS_MEMORY = 768 * 1024;

        static DynamicElement<byte> malloc(int size)
        {
            var ptr = decoderMemoryAllocator.Alloc(size);
            Debug.Assert(size == 0 || ptr != null);
            return ptr;
        }

        static DynamicElement<byte> calloc(int num, int size)
        {
            var ptr = decoderMemoryAllocator.Alloc(num * size);
            Debug.Assert((num * size) == 0 || ptr != null);
            //memset(ptr, 0, num * size);
            return ptr;
        }

        static DynamicElement<byte> realloc(DynamicElement<byte> memblock, int size)
        {
            var ptr = decoderMemoryAllocator.Resize(memblock, size);
            Debug.Assert(size == 0 || ptr.Value != null);
            return ptr;
        }

        static void free(DynamicElement<byte> memblock)
            => decoderMemoryAllocator.Free(memblock);
    }

    // OggVorbis file loading/decoding.
    public static class OggVorbis
    {
        static int FS_ReadOGG(byte[] dest, int size1, int size2, object fh)
        {
            var f = (VFile)fh;
            return f.Read(dest, size1 * size2);
        }

        static int FS_SeekOGG(object fh, long to, int type)
        {
            var retype = FS_SEEK.SET;

            if (type == SEEK_CUR) retype = FS_SEEK.CUR;
            else if (type == SEEK_END) retype = FS_SEEK.END;
            else if (type == SEEK_SET) retype = FS_SEEK.SET;
            else common.FatalError("fs_seekOGG: seek without type\n");
            var f = (VFile)fh;
            return f.Seek(to, retype);
        }

        static int FS_CloseOGG(object fh)
            => 0;

        static long FS_TellOGG(object fh)
        {
            var f = (VFile)fh;
            return f.Tell();
        }

        static int ov_openFile(VFile f, OggVorbis_File vf)
        {
            ov_callbacks callbacks;

            memset(vf, 0, sizeof(OggVorbis_File));

            callbacks.read_func = FS_ReadOGG;
            callbacks.seek_func = FS_SeekOGG;
            callbacks.close_func = FS_CloseOGG;
            callbacks.tell_func = FS_TellOGG;
            return ov_open_callbacks((void*)f, vf, null, -1, callbacks);
        }
    }

    partial class WaveFile
    {
        int OpenOGG(string strFileName, WaveformatEx pwfx)
        {
            OggVorbis_File ov;

            memset(pwfx, 0, sizeof(WaveformatEx));

            mhmmio = fileSystem.OpenFileRead(strFileName);
            if (mhmmio == null)
                return -1;

            SysW.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);

            ov = new OggVorbis_File;

            if (ov_openFile(mhmmio, ov) < 0)
            {
                delete ov;
                SysW.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);
                fileSystem.CloseFile(mhmmio);
                mhmmio = null;
                return -1;
            }

            mfileTime = mhmmio.Timestamp;

            vorbis_info* vi = ov_info(ov, -1);

            mpwfx.Format.nSamplesPerSec = vi.rate;
            mpwfx.Format.nChannels = vi.channels;
            mpwfx.Format.wBitsPerSample = sizeof(short) * 8;
            mdwSize = ov_pcm_total(ov, -1) * vi.channels;   // pcm samples * num channels
            mbIsReadingFromMemory = false;

            if (SoundSystemLocal.s_realTimeDecoding.Bool)
            {
                ov_clear(ov);
                fileSystem.CloseFile(mhmmio);
                mhmmio = null;
                delete ov;

                mpwfx.Format.wFormatTag = WAVE_FORMAT_TAG.OGG;
                mhmmio = fileSystem.OpenFileRead(strFileName);
                mMemSize = mhmmio.Length;

            }
            else
            {
                ogg = ov;

                mpwfx.Format.wFormatTag = WAVE_FORMAT_TAG.PCM;
                mMemSize = mdwSize * sizeof(short);
            }

            memcpy(pwfx, &mpwfx, sizeof(waveformatex_t));

            SysW.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);
            isOgg = true;
            return 0;
        }

        int ReadOGG(byte* pBuffer, int dwSizeToRead, int* pdwSizeRead)
        {
            int total = dwSizeToRead;
            char* bufferPtr = (char*)pBuffer;
            OggVorbis_File* ov = (OggVorbis_File*)ogg;

            do
            {
                var ret = ov_read(ov, bufferPtr, total >= 4096 ? 4096 : total, Swap_IsBigEndian(), 2, 1, NULL);
                if (ret == 0)
                    break;
                if (ret < 0)
                    return -1;
                bufferPtr += ret;
                total -= ret;
            } while (total > 0);

            dwSizeToRead = (byte*)bufferPtr - pBuffer;

            if (pdwSizeRead != null)
            {
                *pdwSizeRead = dwSizeToRead;
            }

            return dwSizeToRead;
        }


        int CloseOGG()
        {
            OggVorbis_File* ov = (OggVorbis_File*)ogg;
            if (ov != NULL)
            {
                Sys_EnterCriticalSection(CRITICAL_SECTION_ONE);
                ov_clear(ov);
                delete ov;
                Sys_LeaveCriticalSection(CRITICAL_SECTION_ONE);
                fileSystem.CloseFile(mhmmio);
                mhmmio = NULL;
                ogg = NULL;
                return 0;
            }
            return -1;
        }
    }

    public interface ISampleDecoder
    {
        public static void Init()
        {
            decoderMemoryAllocator.Init();
            decoderMemoryAllocator.SetLockMemory(true);
            decoderMemoryAllocator.SetFixedBlocks(idSoundSystemLocal::s_realTimeDecoding.GetBool() ? 10 : 1);
        }
        public static void Shutdown()
        {
            decoderMemoryAllocator.Shutdown();
            sampleDecoderAllocator.Shutdown();
        }
        public static ISampleDecoder Alloc()
        {
            idSampleDecoderLocal* decoder = sampleDecoderAllocator.Alloc();
            decoder.Clear();
            return decoder;
        }
        public static void Free(ISampleDecoder decoder)
        {
            idSampleDecoderLocal* localDecoder = static_cast<idSampleDecoderLocal*>(decoder);
            localDecoder.ClearDecoder();
            sampleDecoderAllocator.Free(localDecoder);
        }

        public static int NumUsedBlocks
            => decoderMemoryAllocator.NumUsedBlocks;

        public static int UsedBlockMemory
            => decoderMemoryAllocator.UsedBlockMemory;

        void Decode(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest);
        void ClearDecoder();
        SoundSample Sample { get; }
        int LastDecodeTime { get; }
    }


    public class SampleDecoderLocal : ISampleDecoder
    {
        public static BlockAlloc<SampleDecoderLocal> sampleDecoderAllocator = new(64);

        bool failed;                // set if decoding failed
        int lastFormat;         // last format being decoded
        SoundSample lastSample;         // last sample being decoded
        int lastSampleOffset;   // last offset into the decoded sample
        int lastDecodeTime;     // last time decoding sound
        VMemoryFile file;               // encoded file in memory

        OggVorbis_File ogg;             // OggVorbis file

        public virtual void Decode(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest)
        {
            if (sample.objectInfo.wFormatTag != lastFormat || sample != lastSample)
                ClearDecoder();

            lastDecodeTime = soundSystemLocal.CurrentSoundTime;

            if (failed)
            {
                memset(dest, 0, sampleCount44k * sizeof(dest[0]));
                return;
            }

            // samples can be decoded both from the sound thread and the main thread for shakes
            SysW.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);
            var readSamples44k = (WAVE_FORMAT_TAG)sample.objectInfo.wFormatTag switch
            {
                WAVE_FORMAT_TAG.PCM => DecodePCM(sample, sampleOffset44k, sampleCount44k, dest),
                WAVE_FORMAT_TAG.OGG => DecodeOGG(sample, sampleOffset44k, sampleCount44k, dest),
                _ => 0,
            };
            SysW.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);

            if (readSamples44k < sampleCount44k)
                memset(dest + readSamples44k, 0, (sampleCount44k - readSamples44k) * sizeof(dest[0]));
        }

        public virtual void ClearDecoder()
        {
            SysW.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);
            switch ((WAVE_FORMAT_TAG)lastFormat)
            {
                case WAVE_FORMAT_TAG.PCM: break;
                case WAVE_FORMAT_TAG.OGG: ov_clear(&ogg); memset(&ogg, 0, sizeof(ogg)); break;
            }
            Clear();
            SysW.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);
        }

        public virtual SoundSample Sample => lastSample;

        public virtual int LastDecodeTime => lastDecodeTime;

        public void Clear()
        {
            failed = false;
            lastFormat = (short)WAVE_FORMAT_TAG.PCM;
            lastSample = null;
            lastSampleOffset = 0;
            lastDecodeTime = 0;
        }

        public int DecodePCM(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest)
        {
            byte[] first; int pos, size, readSamples;

            lastFormat = (short)WAVE_FORMAT_TAG.PCM;
            lastSample = sample;

            var shift = 22050 / sample.objectInfo.nSamplesPerSec;
            var sampleOffset = sampleOffset44k >> shift;
            var sampleCount = sampleCount44k >> shift;

            if (sample.nonCacheData == null)
            {
                Debug.Assert(false);  // this should never happen ( note: I've seen that happen with the main thread down in idGameLocal::MapClear clearing entities - TTimo )
                failed = true;
                return 0;
            }

            if (!sample.FetchFromCache(sampleOffset * sizeof(short), out first, out pos, out size, false))
            {
                failed = true;
                return 0;
            }

            readSamples = size - pos < sampleCount * sizeof(short) ? (size - pos) / sizeof(short) : sampleCount;

            // duplicate samples for 44kHz output
            SIMDProcessor.UpSamplePCMTo44kHz(dest, (short*)(first + pos), readSamples, sample.objectInfo.nSamplesPerSec, sample.objectInfo.nChannels);

            return (readSamples << shift);
        }

        public int DecodeOGG(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest)
        {
            int readSamples, totalSamples;

            var shift = 22050 / sample.objectInfo.nSamplesPerSec;
            var sampleOffset = sampleOffset44k >> shift;
            var sampleCount = sampleCount44k >> shift;

            // open OGG file if not yet opened
            if (lastSample == null)
            {
                // make sure there is enough space for another decoder
                if (decoderMemoryAllocator.FreeBlockMemory < MIN_OGGVORBIS_MEMORY)
                    return 0;
                if (sample.nonCacheData == null)
                {
                    Debug.Assert(false);  // this should never happen
                    failed = true;
                    return 0;
                }
                file.SetData((string)sample.nonCacheData, sample.objectMemSize );
                if (ov_openFile(file, ogg) < 0)
                {
                    failed = true;
                    return 0;
                }
                lastFormat = (short)WAVE_FORMAT_TAG.OGG;
                lastSample = sample;
            }

            // seek to the right offset if necessary
            if (sampleOffset != lastSampleOffset)
                if (ov_pcm_seek(&ogg, sampleOffset / sample.objectInfo.nChannels) != 0)
                {
                    failed = true;
                    return 0;
            }

            lastSampleOffset = sampleOffset;

            // decode OGG samples
            totalSamples = sampleCount;
            readSamples = 0;
            do
            {
                float** samples;
                int ret = ov_read_float(&ogg, &samples, totalSamples / sample.objectInfo.nChannels, NULL);
                if (ret == 0)
                {
                    failed = true;
                    break;
                }
                if (ret < 0)
                {
                    failed = true;
                    return 0;
                }
                ret *= sample.objectInfo.nChannels;

                SIMDProcessor.UpSampleOGGTo44kHz(dest + (readSamples << shift), samples, ret, sample.objectInfo.nSamplesPerSec, sample.objectInfo.nChannels);

                readSamples += ret;
                totalSamples -= ret;
            } while (totalSamples > 0);

            lastSampleOffset += readSamples;

            return (readSamples << shift);
        }
    }
}