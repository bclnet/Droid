//#define OV_EXCLUDE_STATIC_CALLBACKS
//#include <vorbis/codec.h>
//#include <vorbis/vorbisfile.h>

using Gengine.Framework;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.NumericsX;
using System.NumericsX.Core;
using static Gengine.Lib;
using static System.NumericsX.Lib;

namespace Gengine.Sound
{
    // Thread safe decoder memory allocator.
    // Each OggVorbis decoder consumes about 150kB of memory.
    static class _decoder
    {
        static DynamicBlockAlloc<byte> decoderMemoryAllocator = new(1 << 20, 128);
        const int MIN_OGGVORBIS_MEMORY = 768 * 1024;

        static byte[] malloc(int size)
        {
            var ptr = decoderMemoryAllocator.Alloc(size);
            Debug.Assert(size == 0 || ptr != null);
            return ptr;
        }

        static byte[] calloc(int num, int size)
        {
            var ptr = decoderMemoryAllocator.Alloc(num * size);
            Debug.Assert((num * size) == 0 || ptr != null);
            //memset(ptr, 0, num * size);
            return ptr;
        }

        static void* realloc(void* memblock, size_t size)
        {
            void* ptr = decoderMemoryAllocator.Resize((byte*)memblock, size);
            assert(size == 0 || ptr != NULL);
            return ptr;
        }

        static void free(ref object memblock)
        {
            decoderMemoryAllocator.Free(ref memblock);
        }
    }

    // OggVorbis file loading/decoding.
    public static class OggVorbis
    {
        static int FS_ReadOGG(void* dest, size_t size1, size_t size2, void* fh)
        {
            idFile* f = reinterpret_cast<idFile*>(fh);
            return f.Read(dest, size1 * size2);
        }

        static int FS_SeekOGG(void* fh, ogg_int64_t to, int type)
        {
            fsOrigin_t retype = FS_SEEK_SET;

            if (type == SEEK_CUR)
            {
                retype = FS_SEEK_CUR;
            }
            else if (type == SEEK_END)
            {
                retype = FS_SEEK_END;
            }
            else if (type == SEEK_SET)
            {
                retype = FS_SEEK_SET;
            }
            else
            {
                common.FatalError("fs_seekOGG: seek without type\n");
            }
            idFile* f = reinterpret_cast<idFile*>(fh);
            return f.Seek(to, retype);
        }


        static int FS_CloseOGG(void* fh)
        {
            return 0;
        }


        static long FS_TellOGG(void* fh)
        {
            idFile* f = reinterpret_cast<idFile*>(fh);
            return f.Tell();
        }


        static int ov_openFile(idFile* f, OggVorbis_File* vf)
        {
            ov_callbacks callbacks;

            memset(vf, 0, sizeof(OggVorbis_File));

            callbacks.read_func = FS_ReadOGG;
            callbacks.seek_func = FS_SeekOGG;
            callbacks.close_func = FS_CloseOGG;
            callbacks.tell_func = FS_TellOGG;
            return ov_open_callbacks((void*)f, vf, NULL, -1, callbacks);
        }
    }

    partial class WaveFile
    {
        int OpenOGG( const char* strFileName, waveformatex_t *pwfx ) {
	OggVorbis_File* ov;

        memset(pwfx, 0, sizeof(waveformatex_t ) );

        mhmmio = fileSystem.OpenFileRead(strFileName );
	if ( !mhmmio ) {
		return -1;
	}

	Sys_EnterCriticalSection(CRITICAL_SECTION_ONE );

        ov = new OggVorbis_File;

	if(ov_openFile(mhmmio, ov) < 0 ) {
		delete ov;
        Sys_LeaveCriticalSection(CRITICAL_SECTION_ONE );
        fileSystem.CloseFile(mhmmio );
		mhmmio = NULL;
		return -1;
	}

    mfileTime = mhmmio.Timestamp();

vorbis_info* vi = ov_info(ov, -1);

    mpwfx.Format.nSamplesPerSec = vi.rate;
mpwfx.Format.nChannels = vi.channels;
mpwfx.Format.wBitsPerSample = sizeof(short)* 8;
mdwSize = ov_pcm_total(ov, -1) * vi.channels;   // pcm samples * num channels
    mbIsReadingFromMemory = false;

if (idSoundSystemLocal::s_realTimeDecoding.GetBool())
{

    ov_clear(ov);
    fileSystem.CloseFile(mhmmio);
    mhmmio = NULL;
    delete ov;

    mpwfx.Format.wFormatTag = WAVE_FORMAT_TAG_OGG;
    mhmmio = fileSystem.OpenFileRead(strFileName);
    mMemSize = mhmmio.Length();

}
else
{

    ogg = ov;

    mpwfx.Format.wFormatTag = WAVE_FORMAT_TAG_PCM;
    mMemSize = mdwSize * sizeof(short);
}

memcpy(pwfx, &mpwfx, sizeof(waveformatex_t));

Sys_LeaveCriticalSection(CRITICAL_SECTION_ONE);

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
        int ret = ov_read(ov, bufferPtr, total >= 4096 ? 4096 : total, Swap_IsBigEndian(), 2, 1, NULL);
        if (ret == 0)
        {
            break;
        }
        if (ret < 0)
        {
            return -1;
        }
        bufferPtr += ret;
        total -= ret;
    } while (total > 0);

    dwSizeToRead = (byte*)bufferPtr - pBuffer;

    if (pdwSizeRead != NULL)
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
        int readSamples44k;

        if (sample.objectInfo.wFormatTag != lastFormat || sample != lastSample)
        {
            ClearDecoder();
        }

        lastDecodeTime = soundSystemLocal.CurrentSoundTime;

        if (failed)
        {
            memset(dest, 0, sampleCount44k * sizeof(dest[0]));
            return;
        }

        // samples can be decoded both from the sound thread and the main thread for shakes
        Sys_EnterCriticalSection(CRITICAL_SECTION_ONE);

        switch (sample.objectInfo.wFormatTag)
        {
            case WAVE_FORMAT_TAG_PCM:
                {
                    readSamples44k = DecodePCM(sample, sampleOffset44k, sampleCount44k, dest);
                    break;
                }
            case WAVE_FORMAT_TAG_OGG:
                {
                    readSamples44k = DecodeOGG(sample, sampleOffset44k, sampleCount44k, dest);
                    break;
                }
            default:
                {
                    readSamples44k = 0;
                    break;
                }
        }

        Sys_LeaveCriticalSection(CRITICAL_SECTION_ONE);

        if (readSamples44k < sampleCount44k)
        {
            memset(dest + readSamples44k, 0, (sampleCount44k - readSamples44k) * sizeof(dest[0]));
        }
    }

    public virtual void ClearDecoder()
    {
        Sys_EnterCriticalSection(CRITICAL_SECTION_ONE);

        switch (lastFormat)
        {
            case WAVE_FORMAT_TAG_PCM:
                {
                    break;
                }
            case WAVE_FORMAT_TAG_OGG:
                {
                    ov_clear(&ogg);
                    memset(&ogg, 0, sizeof(ogg));
                    break;
                }
        }

        Clear();

        Sys_LeaveCriticalSection(CRITICAL_SECTION_ONE);
    }
    public virtual ISoundSample Sample => lastSample;
    public virtual int LastDecodeTime => lastDecodeTime;

    public void Clear()
    {
        failed = false;
        lastFormat = WAVE_FORMAT_TAG_PCM;
        lastSample = NULL;
        lastSampleOffset = 0;
        lastDecodeTime = 0;
    }

    public int DecodePCM(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest)
    {
        const byte* first;
        int pos, size, readSamples;

        lastFormat = WAVE_FORMAT_TAG_PCM;
        lastSample = sample;

        int shift = 22050 / sample.objectInfo.nSamplesPerSec;
        int sampleOffset = sampleOffset44k >> shift;
        int sampleCount = sampleCount44k >> shift;

        if (sample.nonCacheData == NULL)
        {
            assert(false);  // this should never happen ( note: I've seen that happen with the main thread down in idGameLocal::MapClear clearing entities - TTimo )
            failed = true;
            return 0;
        }

        if (!sample.FetchFromCache(sampleOffset * sizeof(short), &first, &pos, &size, false))
        {
            failed = true;
            return 0;
        }

        if (size - pos < sampleCount * sizeof(short))
        {
            readSamples = (size - pos) / sizeof(short);
        }
        else
        {
            readSamples = sampleCount;
        }

        // duplicate samples for 44kHz output
        SIMDProcessor.UpSamplePCMTo44kHz(dest, (const short*)(first + pos), readSamples, sample.objectInfo.nSamplesPerSec, sample.objectInfo.nChannels );

        return (readSamples << shift);
    }

    public int DecodeOGG(SoundSample sample, int sampleOffset44k, int sampleCount44k, float[] dest)
    {
        int readSamples, totalSamples;

        int shift = 22050 / sample.objectInfo.nSamplesPerSec;
        int sampleOffset = sampleOffset44k >> shift;
        int sampleCount = sampleCount44k >> shift;

        // open OGG file if not yet opened
        if (lastSample == NULL)
        {
            // make sure there is enough space for another decoder
            if (decoderMemoryAllocator.GetFreeBlockMemory() < MIN_OGGVORBIS_MEMORY)
            {
                return 0;
            }
            if (sample.nonCacheData == NULL)
            {
                assert(false);  // this should never happen
                failed = true;
                return 0;
            }
            file.SetData((const char*)sample.nonCacheData, sample.objectMemSize );
            if (ov_openFile(&file, &ogg) < 0)
            {
                failed = true;
                return 0;
            }
            lastFormat = WAVE_FORMAT_TAG_OGG;
            lastSample = sample;
        }

        // seek to the right offset if necessary
        if (sampleOffset != lastSampleOffset)
        {
            if (ov_pcm_seek(&ogg, sampleOffset / sample.objectInfo.nChannels) != 0)
            {
                failed = true;
                return 0;
            }
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