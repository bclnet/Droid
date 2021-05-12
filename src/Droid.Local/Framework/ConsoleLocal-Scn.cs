using System;

namespace Droid.Framework
{
    partial class ConsoleLocal
    {
        /*
        ==================
        SCR_DrawTextLeftAlign
        ==================
        */
        void SCR_DrawTextLeftAlign(ref float y, string text, params string[] args)
        {
            char string[MAX_STRING_CHARS];
            va_list argptr;
            va_start(argptr, text);
            idStr::vsnPrintf(string, sizeof(string), text, argptr);
            va_end(argptr);
            renderSystem.DrawSmallStringExt(0, y + 2, string, colorWhite, true, localConsole.charSetShader);
            y += SMALLCHAR_HEIGHT + 4;
        }

        /*
        ==================
        SCR_DrawTextRightAlign
        ==================
        */
        void SCR_DrawTextRightAlign(ref float y, string text, params string[] args)
        {
            char string[MAX_STRING_CHARS];
            va_list argptr;
            va_start(argptr, text);
            int i = idStr::vsnPrintf(string, sizeof(string), text, argptr);
            va_end(argptr);
            renderSystem.DrawSmallStringExt(635 - i * SMALLCHAR_WIDTH, y + 2, string, colorWhite, true, localConsole.charSetShader);
            y += SMALLCHAR_HEIGHT + 4;
        }

        /*
        ==================
        SCR_DrawFPS
        ==================
        */
        const int FPS_FRAMES = 5;
        float SCR_DrawFPS(float y)
        {
            char* s;
            int w;
            static int previousTimes[FPS_FRAMES];
            static int index;
            int i, total;
            static int fps = 0;
            static int previous;
            int t, frameTime;

            int new_y = idMath::FtoiFast(y) + 300;

            // don't use serverTime, because that will be drifting to
            // correct for internet lag changes, timescales, timedemos, etc
            t = Sys_Milliseconds();
            frameTime = t - previous;
            previous = t;

            previousTimes[index % FPS_FRAMES] = frameTime;
            index++;
            if (index > FPS_FRAMES)
            {
                // average multiple frames together to smooth changes out a bit
                total = 0;
                for (i = 0; i < FPS_FRAMES; i++)
                {
                    total += previousTimes[i];
                }
                if (!total)
                {
                    total = 1;
                }
                fps = 10000 * FPS_FRAMES / total;
                fps = (fps + 5) / 10;

                s = va("%ifps", fps);
                w = strlen(s) * SMALLCHAR_WIDTH;

                renderSystem.DrawSmallStringExt((634 / 2) - w, new_y, s, colorWhite, true,
                                               localConsole.charSetShader);
            }

            return y + BIGCHAR_HEIGHT + 4;
        }

        /*
        ==================
        SCR_DrawMemoryUsage
        ==================
        */
        float SCR_DrawMemoryUsage(float y)
        {
            memoryStats_t allocs, frees;

            Mem_GetStats(allocs);
            SCR_DrawTextRightAlign(y, "total allocated memory: %4d, %4dkB", allocs.num, allocs.totalSize >> 10);

            Mem_GetFrameStats(allocs, frees);
            SCR_DrawTextRightAlign(y, "frame alloc: %4d, %4dkB  frame free: %4d, %4dkB", allocs.num, allocs.totalSize >> 10, frees.num, frees.totalSize >> 10);

            Mem_ClearFrameStats();

            return y;
        }

        /*
        ==================
        SCR_DrawAsyncStats
        ==================
        */
        float SCR_DrawAsyncStats(float y)
        {
            int i, outgoingRate, incomingRate;
            float outgoingCompression, incomingCompression;

            if (idAsyncNetwork::server.IsActive())
            {

                SCR_DrawTextRightAlign(y, "server delay = %d msec", idAsyncNetwork::server.GetDelay());
                SCR_DrawTextRightAlign(y, "total outgoing rate = %d KB/s", idAsyncNetwork::server.GetOutgoingRate() >> 10);
                SCR_DrawTextRightAlign(y, "total incoming rate = %d KB/s", idAsyncNetwork::server.GetIncomingRate() >> 10);

                for (i = 0; i < MAX_ASYNC_CLIENTS; i++)
                {

                    outgoingRate = idAsyncNetwork::server.GetClientOutgoingRate(i);
                    incomingRate = idAsyncNetwork::server.GetClientIncomingRate(i);
                    outgoingCompression = idAsyncNetwork::server.GetClientOutgoingCompression(i);
                    incomingCompression = idAsyncNetwork::server.GetClientIncomingCompression(i);

                    if (outgoingRate != -1 && incomingRate != -1)
                    {
                        SCR_DrawTextRightAlign(y, "client %d: out rate = %d B/s (% -2.1f%%), in rate = %d B/s (% -2.1f%%)",
                                                    i, outgoingRate, outgoingCompression, incomingRate, incomingCompression);
                    }
                }

                idStr msg;
                idAsyncNetwork::server.GetAsyncStatsAvgMsg(msg);
                SCR_DrawTextRightAlign(y, msg.c_str());

            }
            else if (idAsyncNetwork::client.IsActive())
            {

                outgoingRate = idAsyncNetwork::client.GetOutgoingRate();
                incomingRate = idAsyncNetwork::client.GetIncomingRate();
                outgoingCompression = idAsyncNetwork::client.GetOutgoingCompression();
                incomingCompression = idAsyncNetwork::client.GetIncomingCompression();

                if (outgoingRate != -1 && incomingRate != -1)
                {
                    SCR_DrawTextRightAlign(y, "out rate = %d B/s (% -2.1f%%), in rate = %d B/s (% -2.1f%%)",
                                                outgoingRate, outgoingCompression, incomingRate, incomingCompression);
                }

                SCR_DrawTextRightAlign(y, "packet loss = %d%%, client prediction = %d",
                                            (int)idAsyncNetwork::client.GetIncomingPacketLoss(), idAsyncNetwork::client.GetPrediction());

                SCR_DrawTextRightAlign(y, "predicted frames: %d", idAsyncNetwork::client.GetPredictedFrames());

            }

            return y;
        }

        /*
        ==================
        SCR_DrawSoundDecoders
        ==================
        */
        float SCR_DrawSoundDecoders(float y)
        {
            int index, numActiveDecoders;
            soundDecoderInfo_t decoderInfo;

            index = -1;
            numActiveDecoders = 0;
            while ((index = soundSystem.GetSoundDecoderInfo(index, decoderInfo)) != -1)
            {
                int localTime = decoderInfo.current44kHzTime - decoderInfo.start44kHzTime;
                int sampleTime = decoderInfo.num44kHzSamples / decoderInfo.numChannels;
                int percent;
                if (localTime > sampleTime)
                {
                    if (decoderInfo.looping)
                    {
                        percent = (localTime % sampleTime) * 100 / sampleTime;
                    }
                    else
                    {
                        percent = 100;
                    }
                }
                else
                {
                    percent = localTime * 100 / sampleTime;
                }
                SCR_DrawTextLeftAlign(y, "%3d: %3d%% (%1.2f) %s: %s (%dkB)", numActiveDecoders, percent, decoderInfo.lastVolume, decoderInfo.format.c_str(), decoderInfo.name.c_str(), decoderInfo.numBytes >> 10);
                numActiveDecoders++;
            }
            return y;
        }

        /*
        ==============
        Con_Clear_f
        ==============
        */
        static void Con_Clear_f(CmdArgs args) => G.localConsole.Clear();

        /*
        ==============
        Con_Dump_f
        ==============
        */
        static void Con_Dump_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                G.common.Printf("usage: conDump <filename>\n");
                return;
            }

            var fileName = args[1];
            fileName.DefaultFileExtension(".txt");

            G.common.Printf($"Dumped console text to {fileName}.\n");

            G.localConsole.Dump(fileName);
        }
    }
}
