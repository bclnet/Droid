namespace Droid.Framework
{
    partial class SessionLocal
    {
        // these must be kept up to date with window Levelshot in guis/mainmenu.gui
        const int PREVIEW_X = 211;
        const int PREVIEW_Y = 31;
        const int PREVIEW_WIDTH = 398;
        const int PREVIEW_HEIGHT = 298;

        void RandomizeStack()
        {
            // attempt to force uninitialized stack memory bugs
            int bytes = 4000000;
            byte* buf = (byte*)_alloca(bytes);

            int fill = rand() & 255;
            for (int i = 0; i < bytes; i++)
            {
                buf[i] = fill;
            }
        }

        extern "C" void Doom3Quest_setUseScreenLayer(int use);

        void setupScreenLayer()
        {
            int inMenu = (((idSessionLocal*)session)->guiActive != 0);
            int inGameGui = (game && game->InGameGuiActive());
            int objectiveActive = (game && game->ObjectiveSystemActive());
            int cinematic = (game && game->InCinematic());
            bool loading = (((idSessionLocal*)session)->insideExecuteMapChange);

            Doom3Quest_setUseScreenLayer(inMenu ? 1 : 0 + inGameGui ? 2 : 0 + objectiveActive ? 4 : 0 + cinematic ? 8 : 0 + loading ? 16 : 0);
        }


        const int FPS_FRAMES = 5;
        int calcFPS()
        {
            static int previousTimes[FPS_FRAMES];
            static int index;
            int i, total;
            static int fps = 0;
            static int previous;
            int t, frameTime;

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

                //common->Printf( " FPS: %i ", fps );
            }

            return fps;
        }
    }
}