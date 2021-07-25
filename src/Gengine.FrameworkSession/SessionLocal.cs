using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;
using System.NumericsX.Sys;
using Gengine.Render;
using Gengine.Sound;
using Gengine.UI;
using static System.NumericsX.Lib;
using static Gengine.Lib;

namespace Gengine.Framework
{
    public struct LogCmd
    {
        public Usercmd cmd;
        public int consistencyHash;
    }

    //struct fileTIME_T
    //{
    //    int index;
    //    ID_TIME_T timeStamp;

    //    operator int() const { return timeStamp; }
    //}

    public class MapSpawnData
    {
        public Dictionary<string, object> serverInfo;
        public Dictionary<string, object> syncedCVars;
        public Dictionary<string, object>[] userInfo = new Dictionary<string, object>[Config.MAX_ASYNC_CLIENTS];
        public Dictionary<string, object>[] persistentPlayerInfo = new Dictionary<string, object>[Config.MAX_ASYNC_CLIENTS];
        public Usercmd[] mapSpawnUsercmd = new Usercmd[Config.MAX_ASYNC_CLIENTS];     // needed for tracking delta angles
    }

    public enum TD //: TimeDemo
    {
        NO,
        YES,
        YES_THEN_QUIT
    }

    public partial class SessionLocal : ISession
    {
        const int USERCMD_PER_DEMO_FRAME = 2;
        const int CONNECT_TRANSMIT_TIME = 1000;
        const int MAX_LOGGED_USERCMDS = 60 * 60 * 60;   // one hour of single player, 15 minutes of four player

        // The render world and sound world used for this session.
        IRenderWorld rw;
        ISoundWorld sw;
        // The renderer and sound system will write changes to writeDemo. Demos can be recorded and played at the same time when splicing.
        VFileDemo readDemo;
        VFileDemo writeDemo;
        int renderdemoVersion;

        public SessionLocal()
        {
            guiInGame = guiMainMenu = guiIntro = guiRestartMenu = guiLoading = guiGameOver = guiActive = guiTest = guiMsg = guiMsgRestore = guiTakeNotes = null;

            menuSoundWorld = null;

            demoversion = false;

            Clear();
        }

        /// <summary>
        /// Called in an orderly fashion at system startup, so commands, cvars, files, etc are all available
        /// </summary>
        public void Init()
        {
            common.Printf("----- Initializing Session -----\n");

            cmdSystem.AddCommand("writePrecache", Sess_WritePrecache_f, CMD_FL.SYSTEM | CMD_FL.CHEAT, "writes precache commands");

#if !ID_DEDICATED
            cmdSystem.AddCommand("map", Session_Map_f, CMD_FL.SYSTEM, "loads a map", CmdArgs.ArgCompletion_MapName);
            cmdSystem.AddCommand("devmap", Session_DevMap_f, CMD_FL.SYSTEM, "loads a map in developer mode", CmdArgs.ArgCompletion_MapName);
            cmdSystem.AddCommand("testmap", Session_TestMap_f, CMD_FL.SYSTEM, "tests a map", CmdArgs.ArgCompletion_MapName);

            cmdSystem.AddCommand("writeCmdDemo", Session_WriteCmdDemo_f, CMD_FL.SYSTEM, "writes a command demo");
            cmdSystem.AddCommand("playCmdDemo", Session_PlayCmdDemo_f, CMD_FL.SYSTEM, "plays back a command demo");
            cmdSystem.AddCommand("timeCmdDemo", Session_TimeCmdDemo_f, CMD_FL.SYSTEM, "times a command demo");
            cmdSystem.AddCommand("exitCmdDemo", Session_ExitCmdDemo_f, CMD_FL.SYSTEM, "exits a command demo");
            cmdSystem.AddCommand("aviCmdDemo", Session_AVICmdDemo_f, CMD_FL.SYSTEM, "writes AVIs for a command demo");
            cmdSystem.AddCommand("aviGame", Session_AVIGame_f, CMD_FL.SYSTEM, "writes AVIs for the current game");

            cmdSystem.AddCommand("recordDemo", Session_RecordDemo_f, CMD_FL.SYSTEM, "records a demo");
            cmdSystem.AddCommand("stopRecording", Session_StopRecordingDemo_f, CMD_FL.SYSTEM, "stops demo recording");
            cmdSystem.AddCommand("playDemo", Session_PlayDemo_f, CMD_FL.SYSTEM, "plays back a demo", CmdArgs.ArgCompletion_DemoName);
            cmdSystem.AddCommand("timeDemo", Session_TimeDemo_f, CMD_FL.SYSTEM, "times a demo", CmdArgs.ArgCompletion_DemoName);
            cmdSystem.AddCommand("timeDemoQuit", Session_TimeDemoQuit_f, CMD_FL.SYSTEM, "times a demo and quits", CmdArgs.ArgCompletion_DemoName);
            cmdSystem.AddCommand("aviDemo", Session_AVIDemo_f, CMD_FL.SYSTEM, "writes AVIs for a demo", CmdArgs.ArgCompletion_DemoName);
            cmdSystem.AddCommand("compressDemo", Session_CompressDemo_f, CMD_FL.SYSTEM, "compresses a demo file", CmdArgs.ArgCompletion_DemoName);
#endif

            cmdSystem.AddCommand("disconnect", Session_Disconnect_f, CMD_FL.SYSTEM, "disconnects from a game");

            cmdSystem.AddCommand("demoShot", Session_DemoShot_f, CMD_FL.SYSTEM, "writes a screenshot for a demo");
            cmdSystem.AddCommand("testGUI", Session_TestGUI_f, CMD_FL.SYSTEM, "tests a gui");

#if !ID_DEDICATED
            cmdSystem.AddCommand("saveGame", SaveGame_f, CMD_FL.SYSTEM | CMD_FL.CHEAT, "saves a game");
            cmdSystem.AddCommand("loadGame", LoadGame_f, CMD_FL.SYSTEM | CMD_FL.CHEAT, "loads a game", CmdArgs.ArgCompletion_SaveGame);
#endif

            cmdSystem.AddCommand("takeViewNotes", TakeViewNotes_f, CMD_FL.SYSTEM, "take notes about the current map from the current view");
            cmdSystem.AddCommand("takeViewNotes2", TakeViewNotes2_f, CMD_FL.SYSTEM, "extended take view notes");

            cmdSystem.AddCommand("rescanSI", Session_RescanSI_f, CMD_FL.SYSTEM, "internal - rescan serverinfo cvars and tell game");

            cmdSystem.AddCommand("promptKey", Session_PromptKey_f, CMD_FL.SYSTEM, "prompt and sets the CD Key");

            cmdSystem.AddCommand("hitch", Session_Hitch_f, CMD_FL.SYSTEM | CMD_FL.CHEAT, "hitches the game");

            // the same idRenderWorld will be used for all games and demos, insuring that level specific models will be freed
            rw = renderSystem.AllocRenderWorld();
            sw = soundSystem.AllocSoundWorld(rw);

            menuSoundWorld = soundSystem.AllocSoundWorld(rw);

            // we have a single instance of the main menu
            guiMainMenu = uiManager.FindGui("guis/mainmenu.gui", true, false, true);
            if (guiMainMenu == null)
            {
                guiMainMenu = uiManager.FindGui("guis/demo_mainmenu.gui", true, false, true);
                demoversion = guiMainMenu != null;
            }
            guiMainMenu_MapList = uiManager.AllocListGUI();
            guiMainMenu_MapList.Config(guiMainMenu, "mapList");
            idAsyncNetwork.client.serverList.GUIConfig(guiMainMenu, "serverList");
            guiRestartMenu = uiManager.FindGui("guis/restart.gui", true, false, true);
            guiGameOver = uiManager.FindGui("guis/gameover.gui", true, false, true);
            guiMsg = uiManager.FindGui("guis/msg.gui", true, false, true);
            guiTakeNotes = uiManager.FindGui("guis/takeNotes.gui", true, false, true);
            guiIntro = uiManager.FindGui("guis/intro.gui", true, false, true);

            whiteMaterial = declManager.FindMaterial("_white");

            guiInGame = null;
            guiTest = null;

            guiActive = null;
            guiHandle = null;

            ReadCDKey();
        }

        public void Shutdown()
        {
            int i;

            if (aviCaptureMode)
                EndAVICapture();

            if (timeDemo == TD.YES)
                // else the game freezes when showing the timedemo results
                timeDemo = TD.YES_THEN_QUIT;

            Stop();

            if (rw != null)
                rw = null;

            if (sw != null)
                sw = null;

            if (menuSoundWorld != null)
                menuSoundWorld = null;

            mapSpawnData.serverInfo.Clear();
            mapSpawnData.syncedCVars.Clear();
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                mapSpawnData.userInfo[i].Clear();
                mapSpawnData.persistentPlayerInfo[i].Clear();
            }

            if (guiMainMenu_MapList != null)
            {
                guiMainMenu_MapList.Shutdown();
                uiManager.FreeListGUI(guiMainMenu_MapList);
                guiMainMenu_MapList = null;
            }

            Clear();
        }

        /// <summary>
        /// called on errors and game exits
        /// </summary>
        public void Stop()
        {
            ClearWipe();

            // clear mapSpawned and demo playing flags
            UnloadMap();

            // disconnect async client
            AsyncNetwork.client.DisconnectFromServer();

            // kill async server
            AsyncNetwork.server.Kill();

            sw?.StopAllSounds();

            insideUpdateScreen = false;
            insideExecuteMapChange = false;

            // drop all guis
            SetGUI(null, null);
        }

        public void UpdateScreen(bool outOfSequence = true)
        {

#if true //_WIN32

            if (com_editors)
                if (!SysW.IsWindowVisible())
                    return;
#endif

            if (insideUpdateScreen)
            {
                return;
                //		common.FatalError("idSessionLocal.UpdateScreen: recursively called" );
            }

            insideUpdateScreen = true;

            // if this is a long-operation update and we are in windowed mode, release the mouse capture back to the desktop
            if (outOfSequence)
                SysW.GrabMouseCursor(false);

            renderSystem.BeginFrame(renderSystem.GetScreenWidth(), renderSystem.GetScreenHeight());

            // draw everything
            Draw();

            if (com_speeds.Bool) renderSystem.EndFrame(out time_frontend, out time_backend);
            else renderSystem.EndFrame(out _, out _);

            insideUpdateScreen = false;
        }

        public void PacifierUpdate()
        {
            if (!insideExecuteMapChange)
                return;

            // never do pacifier screen updates while inside the drawing code, or we can have various recursive problems
            if (insideUpdateScreen)
                return;

            int time = eventLoop.Milliseconds();

            if (time - lastPacifierTime < 100)
                return;
            lastPacifierTime = time;

            if (guiLoading && bytesNeededForMapLoad)
            {
                float n = fileSystem.GetReadCount();
                float pct = (n / bytesNeededForMapLoad);
                // pct = idMath.ClampFloat( 0.0f, 100.0f, pct );
                guiLoading.SetStateFloat("map_loading", pct);
                guiLoading.StateChanged(com_frameTime);
            }

            SysW.GenerateEvents();

            UpdateScreen();

            AsyncNetwork.client.PacifierUpdate();
            AsyncNetwork.server.PacifierUpdate();
        }

        public void Frame()
        {
            if (com_asyncSound.GetInteger() == 0)
            {
                soundSystem.AsyncUpdateWrite(Sys_Milliseconds());
            }

            // DG: periodically check if sound device is still there and try to reset it if not
            //     (calling this from idSoundSystem.AsyncUpdate(), which runs in a separate thread
            //      by default, causes a deadlock when calling idCommon.Warning())
            CheckOpenALDeviceAndRecoverIfNeeded();

            // Editors that completely take over the game
            if (com_editorActive && (com_editors & (EDITOR_RADIANT | EDITOR_GUI)))
                return;

            // if the console is down, we don't need to hold the mouse cursor
            SysW.GrabMouseCursor(console.Active || com_editorActive ? false : true);

            // save the screenshot and audio from the last draw if needed
            if (aviCaptureMode)
            {
                idStr name;

                name = va("demos/%s/%s_%05i.tga", aviDemoShortName.c_str(), aviDemoShortName.c_str(), aviTicStart);

                float ratio = 30.0f / (1000.0f / USERCMD_MSEC / com_aviDemoTics.GetInteger());
                aviDemoFrameCount += ratio;
                if (aviTicStart + 1 != (int)aviDemoFrameCount)
                {
                    // skipped frames so write them out
                    var c = aviDemoFrameCount - aviTicStart;
                    while (c--)
                    {
                        renderSystem.TakeScreenshot(com_aviDemoWidth.Integer, com_aviDemoHeight.Integer, name, com_aviDemoSamples.Integer, null);
                        name = $"demos/{aviDemoShortName}/{aviDemoShortName}_{++aviTicStart:05}.tga");
                    }
                }
                aviTicStart = aviDemoFrameCount;

                // remove any printed lines at the top before taking the screenshot
                console.ClearNotifyLines();

                // this will call Draw, possibly multiple times if com_aviDemoSamples is > 1
                renderSystem.TakeScreenshot(com_aviDemoWidth.Integer, com_aviDemoHeight.Integer, name, com_aviDemoSamples.Integer, null);
            }

            // at startup, we may be backwards
            if (latchedTicNumber > com_ticNumber)
                latchedTicNumber = com_ticNumber;

            // se how many tics we should have before continuing
            int minTic = latchedTicNumber + 1;
            if (com_minTics.GetInteger() > 1)
                minTic = lastGameTic + com_minTics.Integer;

            if (readDemo)
                minTic = !timeDemo && numDemoFrames != 1
                    ? lastDemoTic + USERCMD_PER_DEMO_FRAME
                    : latchedTicNumber; // timedemos and demoshots will run as fast as they can, other demos will not run more than 30 hz
            else if (writeDemo)
                minTic = lastGameTic + USERCMD_PER_DEMO_FRAME;      // demos are recorded at 30 hz

            // fixedTic lets us run a forced number of usercmd each frame without timing
            if (com_fixedTic.Integer != 0)
                minTic = latchedTicNumber;

            while (true)
            {
                latchedTicNumber = com_ticNumber;
                if (latchedTicNumber >= minTic)
                    break;
                SysW.WaitForEvent(TRIGGER_EVENT_ONE);
            }

            if (authEmitTimeout != 0)
            {
                // waiting for a game auth
                if (SysW.Milliseconds > authEmitTimeout)
                {
                    // expired with no reply
                    // means that if a firewall is blocking the master, we will let through
                    common.DPrintf("no reply from auth\n");
                    if (authWaitBox)
                    {
                        // close the wait box
                        StopBox();
                        authWaitBox = false;
                    }
                    if (cdkey_state == CDKEY.CHECKING)
                        cdkey_state = CDKEY.OK;
                    if (xpkey_state == CDKEY.CHECKING)
                        xpkey_state = CDKEY.OK;
                    // maintain this empty as it's set by auth denials
                    authMsg = string.Empty;
                    authEmitTimeout = 0;
                    SetCDKeyGuiVars();
                }
            }

            // send frame and mouse events to active guis
            GuiFrameEvents();

            // advance demos
            if (readDemo != null)
            {
                AdvanceRenderDemo(false);
                return;
            }

            //------------ single player game tics --------------

            if (!mapSpawned || guiActive)
                if (!com_asyncInput.GetBool())
                    // early exit, won't do RunGameTic .. but still need to update mouse position for GUIs
                    usercmdGen.GetDirectUsercmd();

            if (!mapSpawned)
                return;

            if (guiActive)
            {
                lastGameTic = latchedTicNumber;
                return;
            }

            // in message box / GUIFrame, idSessionLocal.Frame is used for GUI interactivity but we early exit to avoid running game frames
            if (AsyncNetwork.IsActive)
                return;

            // check for user info changes
            if ((cvarSystem.GetModifiedFlags() & CVAR.USERINFO) != 0)
            {
                mapSpawnData.userInfo[0] = cvarSystem.MoveCVarsToDict(CVAR.USERINFO);
                game.SetUserInfo(0, mapSpawnData.userInfo[0], false, false);
                cvarSystem.ClearModifiedFlags(CVAR.USERINFO);
            }

            // see how many usercmds we are going to run
            var numCmdsToRun = latchedTicNumber - lastGameTic;

            // don't let a long onDemand sound load unsync everything
            if (timeHitch)
            {
                var skip = timeHitch / USERCMD_MSEC;
                lastGameTic += skip;
                numCmdsToRun -= skip;
                timeHitch = 0;
            }

            // don't get too far behind after a hitch
            if (numCmdsToRun > 10)
                lastGameTic = latchedTicNumber - 10;

            // never use more than USERCMD_PER_DEMO_FRAME, which makes it go into slow motion when recording
            if (writeDemo != null)
            {
                var fixedTic = USERCMD_PER_DEMO_FRAME;
                // we should have waited long enough
                if (numCmdsToRun < fixedTic)
                    common.Error("idSessionLocal.Frame: numCmdsToRun < fixedTic");
                // we may need to dump older commands
                lastGameTic = latchedTicNumber - fixedTic;
            }
            else if (com_fixedTic.GetInteger() > 0)
                // this may cause commands run in a previous frame to be run again if we are going at above the real time rate
                lastGameTic = latchedTicNumber - com_fixedTic.GetInteger();
            else if (aviCaptureMode)
                lastGameTic = latchedTicNumber - com_aviDemoTics.GetInteger();

            // force only one game frame update this frame.  the game code requests this after skipping cinematics so we come back immediately after the cinematic is done instead of a few frames later which can
            // cause sounds played right after the cinematic to not play.
            if (syncNextGameFrame)
            {
                lastGameTic = latchedTicNumber - 1;
                syncNextGameFrame = false;
            }

            // create client commands, which will be sent directly to the game
            //if (com_showTics.GetBool())
            //    common.Printf(" Tics to run: %i ", latchedTicNumber - lastGameTic);

            var gameTicsToRun = latchedTicNumber - lastGameTic;

            // DrBeef's "smoothing out" logic, dodgy, but seems to do the trick
            //
            // This is here because, for example, if we are running at 60hz, then the game tic interval is
            // 16ms, which actually means 63 tics per second, so every half second or so we get an extra tic.
            // This extra tic results is a movement glitch (subtle, but annoying when you are aware of it)
            // because you move two tics worth of distance compared to the other frames, which is more obvious in VR.
            //
            // The solution is to just skip these extra tics, however if we skip all extra tics and only process
            // one per frame then if the fps drop due to a lot of action, the whole game slows down, which isn't desriable.
            // Therefore we only want to skip isolated instances of a single extra tic if we are maintaining almost max frame rate
            int fps = calcFPS();
            bool skipTics = false;
            if (com_skipTics.Bool && gameTicsToRun > 1)
            {
                int refresh = renderSystem.GetRefresh();

                //Skip extra tics if we are maintaining 95% of the intended refresh rate
                skipTics = (fps >= (refresh * 0.95F));
            }

            for (var i = 0; i < gameTicsToRun; i++)
            {
                RunGameTic();
                if (!mapSpawned)
                    // exited game play
                    break;

                if (syncNextGameFrame || skipTics)
                {
                    // Do this in case skipTics is true but this flag isn't, since RunGameTic will reset the syncNextGameFrame flag
                    syncNextGameFrame = true;

                    // long game frame, so break out and continue executing as if there was no hitch
                    break;
                }
            }
        }

        public bool IsMultiplayer
            => AsyncNetwork.IsActive;

        public bool ProcessEvent(SysEvent event_)
        {
            // hitting escape anywhere brings up the menu
            // DG: but shift-escape should bring up console instead so ignore that
            if (!guiActive && event_.evType == SE_KEY && event_.evValue2 == 1 && event_.evValue == K_ESCAPE && !idKeyInput.IsDown(K_SHIFT))
            {
                console.Close();
                if (game)
                {
                    IUserInterface gui = null;
                    escReply_t op;
                    op = game.HandleESC(gui);
                    if (op == ESC_IGNORE)
                        return true;
                    else if (op == ESC_GUI)
                    {
                        SetGUI(gui, null);
                        return true;
                    }
                }
                StartMenu();
                return true;
            }

            // let the pull-down console take it if desired
            if (console.ProcessEvent(event_, false))
                return true;

            // if we are testing a GUI, send all events to it
            if (guiTest)
            {
                // hitting escape exits the testgui
                if (event_.evType == SE_KEY && event_.evValue2 == 1 && event_.evValue == K_ESCAPE)
                {
                    guiTest = null;
                    return true;
                }

                static string cmd;
                cmd = guiTest.HandleEvent(event_, com_frameTime);
                if (!string.IsNullOrEmpty(cmd))
                    common.Printf($"testGui event returned: '{cmd}'\n");
                return true;
            }

            // menus / etc
            if (guiActive)
            {
                MenuEvent(event_);
                return true;
            }

            // if we aren't in a game, force the console to take it
            if (!mapSpawned)
            {
                console.ProcessEvent(event_, true);
                return true;
            }

            // in game, exec bindings for all key downs
            if (event_.evType == SE_KEY && event_.evValue2 == 1)
            {
                idKeyInput.ExecKeyBinding(event_.evValue);
                return true;
            }

            return false;
        }

        public void StartMenu(bool playIntro = false) { }
        public void ExitMenu() { }
        public void GuiFrameEvents() { }
        public void SetGUI(IUserInterface gui, HandleGuiCommand handle) { }

        public string MessageBox(msgBoxType type, string message, string title = null, bool wait = false, string fire_yes = null, string fire_no = null, bool network = false) { }
        public void StopBox() { }
        public void DownloadProgressBox(BackgroundDownload bgl, string title, int progress_start = 0, int progress_end = 100) { }
        public void SetPlayingSoundWorld()
        {
            soundSystem.SetPlayingSoundWorld(guiActive && (guiActive == guiMainMenu || guiActive == guiIntro || guiActive == guiLoading || (guiActive == guiMsg && !mapSpawned))
                ? menuSoundWorld
                : sw);
        }

        /// <summary>
        /// this is used by the sound system when an OnDemand sound is loaded, so the game action doesn't advance and get things out of sync
        /// </summary>
        /// <param name="msec">The msec.</param>
        public void TimeHitch(int msec) => timeHitch += msec;

        public int GetSaveGameVersion() => savegameVersion;

        public string GetCurrentMapName() => currentMapName;

        //=====================================

        public int GetLocalClientNum()
        {
            if (AsyncNetwork.client.IsActive)
                return AsyncNetwork.client.GetLocalClientNum();
            else if (AsyncNetwork.server.IsActive)
            {
                if (AsyncNetwork.serverDedicated.Integer == 0) return 0;
                else if (AsyncNetwork.server.IsClientInGame(AsyncNetwork.serverDrawClient.Integer)) return AsyncNetwork.serverDrawClient.Integer;
                else return -1;
            }
            else return 0;
        }

        /// <summary>
        /// Leaves the existing userinfo and serverinfo
        /// </summary>
        /// <param name="mapName">Name of the map.</param>
        public void MoveToNewMap(string mapName)
        {
            mapSpawnData.serverInfo.Set("si_map", mapName);

            ExecuteMapChange();

            if (!mapSpawnData.serverInfo.GetBool("devmap"))
            {
                // Autosave at the beginning of the level

                // DG: set an explicit savename to avoid problems with autosave names
                //     (they were translated which caused problems like all alpha labs parts
                //      getting the same filename in spanish, probably because the strings contained
                //      dots and everything behind them was cut off as "file extension".. see #305)
                var saveFileName = "Autosave_";
                saveFileName += mapName;
                SaveGame(GetAutoSaveName(mapName), true, saveFileName);
            }

            SetGUI(null, null);
        }

        // loads a map and starts a new game on it
        public void StartNewGame(string mapName, bool devmap = false)
        {
#if ID_DEDICATED
            common.Printf("Dedicated servers cannot start singleplayer games.\n");
            return;
#else
#if ID_ENFORCE_KEY
            // strict check. don't let a game start without a definitive answer
            if (!CDKeysAreValid(true))
            {
                var prompt = true;
                if (MaybeWaitOnCDKey())
                {
                    // check again, maybe we just needed more time
                    if (CDKeysAreValid(true))
                        // can continue directly
                        prompt = false;
                }
                if (prompt)
                {
                    cmdSystem.BufferCommandText(CMD_EXEC_NOW, "promptKey force");
                    cmdSystem.ExecuteCommandBuffer();
                }
            }
#endif
            if (AsyncNetwork.server.IsActive)
            {
                common.Printf("Server running, use si_map / serverMapRestart\n");
                return;
            }
            if (AsyncNetwork.client.IsActive)
            {
                common.Printf("Client running, disconnect from server first\n");
                return;
            }

            // clear the userInfo so the player starts out with the defaults
            mapSpawnData.userInfo[0].Clear();
            mapSpawnData.persistentPlayerInfo[0].Clear();
            mapSpawnData.userInfo[0] = cvarSystem.MoveCVarsToDict(CVAR.USERINFO);

            mapSpawnData.serverInfo.Clear();
            mapSpawnData.serverInfo = cvarSystem.MoveCVarsToDict(CVAR.SERVERINFO);
            mapSpawnData.serverInfo.Set("si_gameType", "singleplayer");

            // set the devmap key so any play testing items will be given at spawn time to set approximately the right weapons and ammo
            if (devmap)
                mapSpawnData.serverInfo.Set("devmap", "1");

            mapSpawnData.syncedCVars.Clear();
            mapSpawnData.syncedCVars = cvarSystem.MoveCVarsToDict(CVAR.NETWORKSYNC);

            MoveToNewMap(mapName);
#endif
        }
        public void PlayIntroGui() { }

        public void LoadSession(string name) { }
        public void SaveSession(string name) { }

        // called by Draw when the scene to scene wipe is still running
        /// <summary>
        /// Draw the fade material over everything that has been drawn
        /// </summary>
        public void DrawWipeModel()
        {
            var latchedTic = com_ticNumber;

            if (wipeStartTic >= wipeStopTic)
                return;

            if (!wipeHold && latchedTic >= wipeStopTic)
                return;

            var fade = (float)(latchedTic - wipeStartTic) / (wipeStopTic - wipeStartTic);
            renderSystem.SetColor4(1, 1, 1, fade);
            renderSystem.DrawStretchPic(0, 0, 640, 480, 0, 0, 1, 1, wipeMaterial);
        }
        /// <summary>
        /// Draws and captures the current state, then starts a wipe with that image
        /// </summary>
        /// <param name="materialName">Name of the material.</param>
        /// <param name="hold">if set to <c>true</c> [hold].</param>
        public void StartWipe(string materialName, bool hold = false)
        {
            console.Close();

            // render the current screen into a texture for the wipe model
            renderSystem.CropRenderSize(640, 480, true);

            Draw();

            renderSystem.CaptureRenderToImage("_scratch");
            renderSystem.UnCrop();

            wipeMaterial = declManager.FindMaterial(wipeMaterial, false);

            wipeStartTic = com_ticNumber;
            wipeStopTic = wipeStartTic + 1000.0f / USERCMD_MSEC * com_wipeSeconds.Float;
            wipeHold = hold;
        }
        public void CompleteWipe()
        {
            if (com_ticNumber == 0)
            {
                // if the async thread hasn't started, we would hang here
                wipeStopTic = 0;
                UpdateScreen(true);
                return;
            }
            while (com_ticNumber < wipeStopTic)
            {
#if ID_CONSOLE_LOCK
		        emptyDrawCount = 0;
#endif
                UpdateScreen(true);
            }
        }
        public void ClearWipe()
        {
            wipeHold = false;
            wipeStopTic = 0;
            wipeStartTic = wipeStopTic + 1;
        }

        public void ShowLoadingGui()
        {
            if (com_ticNumber == 0)
                return;
            console.Close();

            // introduced in D3XP code. don't think it actually fixes anything, but doesn't hurt either
#if true
            // Try and prevent the while loop from being skipped over (long hitch on the main thread?)
            var stop = SysW.Milliseconds + 1000;
            var force = 10;
            while (SysW.Milliseconds < stop || force-- > 0)
            {
                com_frameTime = com_ticNumber * USERCMD_MSEC;
                session.Frame();
                session.UpdateScreen(false);
            }
#else
            var stop = com_ticNumber + 1000.0f / USERCMD_MSEC * 1.0f;
            while (com_ticNumber < stop)
            {
                com_frameTime = com_ticNumber * USERCMD_MSEC;
                session.Frame();
                session.UpdateScreen(false);
            }
#endif
        }

        /// <summary>
        /// Turns a bad file name into a good one or your money back
        /// </summary>
        /// <param name="saveFileName">Name of the save file.</param>
        public void ScrubSaveGameFileName(string saveFileName)
        {
            int i;
            string inFileName;

            inFileName = saveFileName;
            inFileName.RemoveColors();
            inFileName.StripFileExtension();

            saveFileName = string.Empty;

            var len = inFileName.Length;
            for (i = 0; i < len; i++)
            {
                if (strchr("',.~!@#$%^&*()[]{}<>\\|/=?+;:-\'\"", inFileName[i])) saveFileName += '_'; // random junk
                else if (inFileName[i] >= 128) saveFileName += '_'; // high ascii chars
                else if (inFileName[i] == ' ') saveFileName += '_';
                else saveFileName += inFileName[i];
            }
        }
        public string GetAutoSaveName(string mapName)
        {
            var mapDecl = declManager.FindType(DECL_MAPDEF, mapName, false);
            var mapDef = (DeclEntityDef)mapDecl;
            if (mapDef)
                mapName = common.GetLanguageDict().GetString(mapDef.dict.GetString("name", mapName));
            // Fixme: Localization
            return $"^3AutoSave:^0 {mapName}";
        }
        public string GetSaveMapName(string mapName)
        {
            var mapDecl = declManager.FindType(DECL_MAPDEF, mapName, false);
            var mapDef = (DeclEntityDef)mapDecl;
            if (mapDef != null)
                mapName = common.LanguageDictGetString(mapDef.dict.GetString("name", mapName));
            // Fixme: Localization
            return mapName;
        }

        public bool LoadGame(string saveName)
        {
#if ID_DEDICATED
            common.Printf("Dedicated servers cannot load games.\n");
            return false;
#else
            int i; string in_, loadFile, saveMap, gamename;

            if (IsMultiplayer)
            {
                common.Printf("Can't load during net play.\n");
                return false;
            }

            //Hide the dialog box if it is up.
            StopBox();

            loadFile = saveName;
            ScrubSaveGameFileName(loadFile);
            loadFile.SetFileExtension(".save");

            in_ = "savegames/";
            in_ += loadFile;

            // Open savegame file. only allow loads from the game directory because we don't want a base game to load
            var game = cvarSystem.GetCVarString("fs_game");
            savegameFile = fileSystem.OpenFileRead(in_, true, game.Length != 0 ? game : null);

            if (savegameFile == null)
            {
                common.Warning($"Couldn't open savegame file {in_}");
                return false;
            }

            loadingSaveGame = true;

            // Read in save game header
            // Game Name / Version / Map Name / Persistant Player Info

            // game
            savegameFile.ReadString(gamename);

            // if this isn't a savegame for the correct game, abort loadgame
            if (!(gamename == GAME_NAME || gamename == "DOOM 3"))
            {
                common.Warning($"Attempted to load an invalid savegame: {in_}");

                loadingSaveGame = false;
                fileSystem.CloseFile(savegameFile);
                savegameFile = null;
                return false;
            }

            // version
            savegameFile.ReadInt(savegameVersion);

            // map
            savegameFile.ReadString(saveMap);

            // persistent player info
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
                mapSpawnData.persistentPlayerInfo[i].ReadFromFileHandle(savegameFile);

            // check the version, if it doesn't match, cancel the loadgame, but still load the map with the persistant playerInfo from the header
            // so that the player doesn't lose too much progress.
            if (savegameVersion <= 17)
            {   // handle savegame v16 in v17
                common.Warning("Savegame Version Too Early: aborting loadgame and starting level with persistent data");
                loadingSaveGame = false;
                fileSystem.CloseFile(savegameFile);
                savegameFile = null;
            }

            common.DPrintf("loading a v%d savegame\n", savegameVersion);

            if (saveMap.Length > 0)
            {
                // Start loading map
                mapSpawnData.serverInfo.Clear();

                mapSpawnData.serverInfo = cvarSystem.MoveCVarsToDict(CVAR.SERVERINFO);
                mapSpawnData.serverInfo.Set("si_gameType", "singleplayer");

                mapSpawnData.serverInfo.Set("si_map", saveMap);

                mapSpawnData.syncedCVars.Clear();
                mapSpawnData.syncedCVars = cvarSystem.MoveCVarsToDict(CVAR.NETWORKSYNC);

                mapSpawnData.mapSpawnUsercmd[0] = usercmdGen.TicCmd(latchedTicNumber);
                // make sure no buttons are pressed
                mapSpawnData.mapSpawnUsercmd[0].buttons = 0;

                ExecuteMapChange();

                SetGUI(null, null);
            }

            if (loadingSaveGame)
            {
                fileSystem.CloseFile(savegameFile);
                loadingSaveGame = false;
                savegameFile = null;
            }

            return true;
#endif
        }
        // DG: added saveFileName so we can set a sensible filename for autosaves (see comment in MoveToNewMap())
        public bool SaveGame(string saveName, bool autosave = false, string saveFileName = null)
        {
#if ID_DEDICATED
            common.Printf("Dedicated servers cannot save games.\n");
            return false;
#else
            int i;
            string previewFile, descriptionFile, mapName;
            // DG: support setting an explicit savename to avoid problems with autosave names
            var gameFile = saveFileName != null ? saveFileName : saveName;

            if (!mapSpawned)
            {
                common.Printf("Not playing a game.\n");
                return false;
            }

            if (IsMultiplayer)
            {
                common.Printf("Can't save during net play.\n");
                return false;
            }

            if (game.GetPersistentPlayerInfo(0).GetInt("health") <= 0)
            {
                MessageBox(MSG_OK, common.LanguageDictGetString("#str_04311"), common.LanguageDictGetString("#str_04312"), true);
                common.Printf("You must be alive to save the game\n");
                return false;
            }

            if (SysW.GetDriveFreeSpace(cvarSystem.GetCVarString("fs_savepath")) < 25)
            {
                MessageBox(MSG_OK, common.LanguageDictGetString("#str_04313"), common.LanguageDictGetString("#str_04314"), true);
                common.Printf("Not enough drive space to save the game\n");
                return false;
            }

            var pauseWorld = soundSystem.GetPlayingSoundWorld();
            if (pauseWorld != null)
            {
                pauseWorld.Pause();
                soundSystem.SetPlayingSoundWorld(null);
            }

            // setup up filenames and paths
            ScrubSaveGameFileName(gameFile);

            gameFile = "savegames/" + gameFile;
            gameFile.SetFileExtension(".save");

            previewFile = gameFile;
            previewFile.SetFileExtension(".tga");

            descriptionFile = gameFile;
            descriptionFile.SetFileExtension(".txt");

            // Open savegame file
            var fileOut = fileSystem.OpenFileWrite(gameFile);
            if (fileOut == null)
            {
                common.Warning($"Failed to open save file '{gameFile}'\n");
                if (pauseWorld != null)
                {
                    soundSystem.SetPlayingSoundWorld(pauseWorld);
                    pauseWorld.UnPause();
                }
                return false;
            }

            // Write SaveGame Header:
            // Game Name / Version / Map Name / Persistant Player Info

            // game
            var gamename = Config.GAME_NAME;
            fileOut.WriteString(gamename);

            // version
            fileOut.WriteInt(SAVEGAME_VERSION);

            // map
            mapName = mapSpawnData.serverInfo.GetString("si_map");
            fileOut.WriteString(mapName);

            // persistent player info
            for (i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                mapSpawnData.persistentPlayerInfo[i] = game.GetPersistentPlayerInfo(i);
                mapSpawnData.persistentPlayerInfo[i].WriteToFileHandle(fileOut);
            }

            // let the game save its state
            game.SaveGame(fileOut);

            // close the sava game file
            fileSystem.CloseFile(fileOut);

            // Write screenshot
            if (!autosave)
            {
                renderSystem.CropRenderSize(320, 240, false);
                game.Draw(0);
                renderSystem.CaptureRenderToFile(previewFile, true);
                renderSystem.UnCrop();
            }

            // Write description, which is just a text file with
            // the unclean save name on line 1, map name on line 2, screenshot on line 3
            var fileDesc = fileSystem.OpenFileWrite(descriptionFile);
            if (fileDesc == null)
            {
                common.Warning($"Failed to open description file '{descriptionFile}'\n");
                if (pauseWorld != null)
                {
                    soundSystem.SetPlayingSoundWorld(pauseWorld);
                    pauseWorld.UnPause();
                }
                return false;
            }

            var description = saveName;
            description.Replace("\\", "\\\\");
            description.Replace("\"", "\\\"");

            var mapDef = (DeclEntityDef)declManager.FindType(DECL_MAPDEF, mapName, false);
            if (mapDef != null)
                mapName = common.LanguageDictGetString(mapDef.dict.GetString("name", mapName));

            fileDesc.Printf($"\"{description}\"\n");
            fileDesc.Printf($"\"{mapName}\"\n");

            if (autosave)
            {
                var sshot = mapSpawnData.serverInfo.GetString("si_map");
                sshot.StripPath();
                sshot.StripFileExtension();
                fileDesc.Printf($"\"guis/assets/autosave/{sshot}\"\n");
            }
            else
                fileDesc.Printf("\"\"\n");

            fileSystem.CloseFile(fileDesc);

            if (pauseWorld != null)
            {
                soundSystem.SetPlayingSoundWorld(pauseWorld);
                pauseWorld.UnPause();
            }

            syncNextGameFrame = true;

            return true;
#endif
        }

        public string GetAuthMsg() => authMsg;

        //=====================================

        public static CVar com_showAngles = new("com_showAngles", "0", CVAR.SYSTEM | CVAR.BOOL, string.Empty);
        public static CVar com_showTics = new("com_showTics", "1", CVAR.SYSTEM | CVAR.BOOL, string.Empty);
        public static CVar com_skipTics = new("com_skipTics", "1", CVAR.SYSTEM | CVAR.BOOL | CVAR.ARCHIVE, "Skip all missed tics and only use one tick per frame, unless in a low fps situation, then process all tics");
        public static CVar com_minTics = new("com_minTics", "1", CVAR.SYSTEM, string.Empty);
        public static CVar com_fixedTic = new("com_fixedTic", "0", CVAR.SYSTEM | CVAR.INTEGER | CVAR.ARCHIVE, string.Empty, -1, 10);
        public static CVar com_showDemo = new("com_showDemo", "0", CVAR.SYSTEM | CVAR.BOOL, string.Empty);
        public static CVar com_skipGameDraw = new("com_skipGameDraw", "0", CVAR.SYSTEM | CVAR.BOOL, string.Empty);
        public static CVar com_aviDemoWidth = new("com_aviDemoWidth", "256", CVAR.SYSTEM, string.Empty);
        public static CVar com_aviDemoHeight = new("com_aviDemoHeight", "256", CVAR.SYSTEM, string.Empty);
        public static CVar com_aviDemoSamples = new("com_aviDemoSamples", "16", CVAR.SYSTEM, string.Empty);
        public static CVar com_aviDemoTics = new("com_aviDemoTics", "2", CVAR.SYSTEM | CVAR.INTEGER, string.Empty, 1, 60);
        public static CVar com_wipeSeconds = new("com_wipeSeconds", "1", CVAR.SYSTEM, string.Empty);
        public static CVar com_guid = new("com_guid", "", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.ROM, string.Empty);

        public static CVar gui_configServerRate;

        public int timeHitch;

        public bool menuActive;
        public ISoundWorld menuSoundWorld;           // so the game soundWorld can be muted

        public bool insideExecuteMapChange;    // draw loading screen and update
                                               // screen on prints
        public int bytesNeededForMapLoad;  //

        // we don't want to redraw the loading screen for every single console print that happens
        public int lastPacifierTime;

        // this is the information required to be set before ExecuteMapChange() is called, which can be saved off at any time with the following commands so it can all be played back
        public MapSpawnData mapSpawnData;
        public string currentMapName;           // for checking reload on same level
        public bool mapSpawned;                // cleared on Stop()

        public int numClients;             // from serverInfo

        public int logIndex;
        public LogCmd[] loggedUsercmds = new LogCmd[MAX_LOGGED_USERCMDS];
        public int statIndex;
        public LogStats[] loggedStats = new LogStats[MAX_LOGGED_STATS];
        public int lastSaveIndex;
        // each game tic, numClients usercmds will be added, until full

        public bool insideUpdateScreen;    // true while inside .UpdateScreen()

        public bool loadingSaveGame;   // currently loading map from a SaveGame
        public VFile savegameFile;       // this is the savegame file to load from
        public int savegameVersion;

        public VFile cmdDemoFile;        // if non-zero, we are reading commands from a file

        public int latchedTicNumber;   // set to com_ticNumber each frame
        public int lastGameTic;        // while latchedTicNumber > lastGameTic, run game frames
        public int lastDemoTic;
        public bool syncNextGameFrame;


        public bool aviCaptureMode;        // if true, screenshots will be taken and sound captured
        public string aviDemoShortName; //
        public float aviDemoFrameCount;
        public int aviTicStart;

        public TimeDemo timeDemo;
        public int timeDemoStartTime;
        public int numDemoFrames;      // for timeDemo and demoShot
        public int demoTimeOffset;
        public RenderView currentDemoRenderView;
        // the next one will be read when com_frameTime + demoTimeOffset > currentDemoRenderView.

        // TODO: make this private (after sync networking removal and idnet tweaks)
        public IUserInterface guiActive;
        public HandleGuiCommand guiHandle;

        public IUserInterface guiInGame;
        public IUserInterface guiMainMenu;
        public IListGUI guiMainMenu_MapList;     // easy map list handling
        public IUserInterface guiRestartMenu;
        public IUserInterface guiLoading;
        public IUserInterface guiIntro;
        public IUserInterface guiGameOver;
        public IUserInterface guiTest;
        public IUserInterface guiTakeNotes;

        public IUserInterface guiMsg;
        public IUserInterface guiMsgRestore;             // store the calling GUI for restore
        public string[] msgFireBack = new string[2];
        public bool msgRunning;
        public int msgRetIndex;
        public bool msgIgnoreButtons;

        public bool waitingOnBind;

        public Material whiteMaterial;

        public Material wipeMaterial;
        public int wipeStartTic;
        public int wipeStopTic;
        public bool wipeHold;

#if ID_CONSOLE_LOCK
        int emptyDrawCount;				// watchdog to force the main menu to restart
#endif

        //=====================================
        public void Clear()
        {
            insideUpdateScreen = false;
            insideExecuteMapChange = false;

            loadingSaveGame = false;
            savegameFile = null;
            savegameVersion = 0;

            currentMapName = string.Empty;
            aviDemoShortName = string.Empty;
            msgFireBack[0] = string.Empty;
            msgFireBack[1] = string.Empty;

            timeHitch = 0;

            rw = null;
            sw = null;
            menuSoundWorld = null;
            readDemo = null;
            writeDemo = null;
            renderdemoVersion = 0;
            cmdDemoFile = null;

            syncNextGameFrame = false;
            mapSpawned = false;
            guiActive = null;
            aviCaptureMode = false;
            timeDemo = TD.NO;
            waitingOnBind = false;
            lastPacifierTime = 0;

            msgRunning = false;
            guiMsgRestore = null;
            msgIgnoreButtons = false;

            bytesNeededForMapLoad = 0;

#if ID_CONSOLE_LOCK
            emptyDrawCount = 0;
#endif
            ClearWipe();

            loadGameList.Clear();
            modsList.Clear();

            authEmitTimeout = 0;
            authWaitBox = false;

            authMsg = string.Empty;
        }

        const int ANGLE_GRAPH_HEIGHT = 128;
        const int ANGLE_GRAPH_STRETCH = 3;
        /// <summary>
        /// Graphs yaw angle for testing smoothness
        /// </summary>
        public void DrawCmdGraph()
        {
            if (!com_showAngles.Bool)
                return;
            renderSystem.SetColor4(0.1f, 0.1f, 0.1f, 1.0f);
            renderSystem.DrawStretchPic(0, 480 - ANGLE_GRAPH_HEIGHT, MAX_BUFFERED_USERCMD * ANGLE_GRAPH_STRETCH, ANGLE_GRAPH_HEIGHT, 0, 0, 1, 1, whiteMaterial);
            renderSystem.SetColor4(0.9f, 0.9f, 0.9f, 1.0f);
            for (var i = 0; i < MAX_BUFFERED_USERCMD - 4; i++)
            {
                var cmd = usercmdGen.TicCmd(latchedTicNumber - (MAX_BUFFERED_USERCMD - 4) + i);
                var h = cmd.angles[1];
                h >>= 8;
                h &= (ANGLE_GRAPH_HEIGHT - 1);
                renderSystem.DrawStretchPic(i * ANGLE_GRAPH_STRETCH, 480 - h, 1, h, 0, 0, 1, 1, whiteMaterial);
            }
        }
        public void Draw()
        {
            var fullConsole = false;

            setupScreenLayer();

            if (insideExecuteMapChange)
            {
                guiLoading?.Redraw(com_frameTime);
                if (guiActive == guiMsg)
                    guiMsg.Redraw(com_frameTime);
            }
            else if (guiTest != null)
            {
                // if testing a gui, clear the screen and draw it clear the background, in case the tested gui is transparent
                // NOTE that you can't use this for aviGame recording, it will tick at real com_frameTime between screenshots..
                renderSystem.SetColor(colorBlack);
                renderSystem.DrawStretchPic(0, 0, 640, 480, 0, 0, 1, 1, declManager.FindMaterial("_white"));
                guiTest.Redraw(com_frameTime);
            }
            else if (guiActive && guiActive.State.Get("gameDraw", "0") == "0")
            {
                // draw the frozen gui in the background
                if (guiActive == guiMsg && guiMsgRestore)
                    guiMsgRestore.Redraw(com_frameTime);

                // draw the menus full screen
                if (guiActive == guiTakeNotes && !com_skipGameDraw.Bool)
                    game.Draw(GetLocalClientNum());

                guiActive.Redraw(com_frameTime);
            }
            else if (readDemo != null)
            {
                rw.RenderScene(currentDemoRenderView);
                renderSystem.DrawDemoPics();
            }
            else if (mapSpawned)
            {
                bool gameDraw = false;
                // normal drawing for both single and multi player
                if (!com_skipGameDraw.Bool && GetLocalClientNum() >= 0)
                {
                    // draw the game view
                    var start = SysW.Milliseconds();
                    gameDraw = game.Draw(GetLocalClientNum());
                    var end = SysW.Milliseconds();
                    time_gameDraw += end - start; // note time used for com_speeds
                }
                if (!gameDraw)
                {
                    renderSystem.SetColor(colorBlack);
                    renderSystem.DrawStretchPic(0, 0, 640, 480, 0, 0, 1, 1, declManager.FindMaterial("_white"));
                }

                // save off the 2D drawing from the game
                if (writeDemo != null)
                    renderSystem.WriteDemoPics();
            }
            else
            {
#if ID_CONSOLE_LOCK
                if (com_allowConsole.Bool)
                    console.Draw(true);
                else
                {
                    emptyDrawCount++;
                    if (emptyDrawCount > 5)
                    {
                        // it's best if you can avoid triggering the watchgod by doing the right thing somewhere else
                        Debug.Assert(false);
                        common.Warning("idSession: triggering mainmenu watchdog");
                        emptyDrawCount = 0;
                        StartMenu();
                    }
                    renderSystem.SetColor4(0, 0, 0, 1);
                    renderSystem.DrawStretchPic(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, 0, 0, 1, 1, declManager.FindMaterial("_white"));
                }
#else
                // draw the console full screen - this should only ever happen in developer builds
                console.Draw(true);
#endif
                fullConsole = true;
            }

#if ID_CONSOLE_LOCK
            if (!fullConsole && emptyDrawCount != 0)
            {
                common.DPrintf($"Session: {emptyDrawCount} empty frame draws\n");
                emptyDrawCount = 0;
            }
            fullConsole = false;
#endif

            // draw the wipe material on top of this if it hasn't completed yet
            DrawWipeModel();

            // draw debug graphs
            DrawCmdGraph();

            // draw the half console / notify console on top of everything
            if (!fullConsole)
                console.Draw(false);
        }

        /// <summary>
        /// Dumps the accumulated commands for the current level.
        /// This should still work after disconnecting from a level
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="save">if set to <c>true</c> [save].</param>
        public void WriteCmdDemo(string name, bool save = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                common.Printf("SessionLocal.WriteCmdDemo: no name specified\n");
                return;
            }

            string statsName;
            if (save)
            {
                statsName = name;
                statsName.StripFileExtension();
                statsName.DefaultFileExtension(".stats");
            }

            common.Printf($"writing save data to {name}\n");

            var cmdDemoFile = fileSystem.OpenFileWrite(name);
            if (cmdDemoFile == null)
            {
                common.Printf($"Couldn't open for writing {name}\n");
                return;
            }

            if (save)
                cmdDemoFile.Write(logIndex, sizeof(logIndex));

            SaveCmdDemoToFile(cmdDemoFile);

            if (save)
            {
                var statsFile = fileSystem.OpenFileWrite(statsName);
                if (statsFile != null)
                {
                    statsFile.Write(statIndex, sizeof(statIndex));
                    statsFile.Write(loggedStats, numClients * statIndex * sizeof(loggedStats[0]));
                    fileSystem.CloseFile(statsFile);
                }
            }

            fileSystem.CloseFile(cmdDemoFile);
        }
        public void StartPlayingCmdDemo(string demoName)
        {// exit any current game
            Stop();

            var fullDemoName = "demos/";
            fullDemoName += demoName;
            fullDemoName.DefaultFileExtension(".cdemo");
            cmdDemoFile = fileSystem.OpenFileRead(fullDemoName);

            if (cmdDemoFile == null)
            {
                common.Printf($"Couldn't open {fullDemoName}\n");
                return;
            }

            guiLoading = uiManager.FindGui("guis/map/loading.gui", true, false, true);
            //cmdDemoFile.Read(loadGameTime, sizeof(loadGameTime));

            LoadCmdDemoFromFile(cmdDemoFile);

            // start the map
            ExecuteMapChange();

            cmdDemoFile = fileSystem.OpenFileRead(fullDemoName);

            // have to do this twice as the execmapchange clears the cmddemofile
            LoadCmdDemoFromFile(cmdDemoFile);

            // run one frame to get the view angles correct
            RunGameTic();
        }
        public void TimeCmdDemo(string demoName)
        {
            StartPlayingCmdDemo(demoName);
            ClearWipe();
            UpdateScreen();

            int startTime = SysW.Milliseconds;
            int count = 0;
            int minuteStart, minuteEnd;
            float sec;

            // run all the frames in sequence
            minuteStart = startTime;

            while (cmdDemoFile)
            {
                RunGameTic();
                count++;

                if (count / 3600 != (count - 1) / 3600)
                {
                    minuteEnd = SysW.Milliseconds;
                    sec = (minuteEnd - minuteStart) / 1000.0;
                    minuteStart = minuteEnd;
                    common.Printf($"minute {count / 3600} took {sec:3.1} seconds\n");
                    UpdateScreen();
                }
            }

            var endTime = SysW.Milliseconds;
            sec = (endTime - startTime) / 1000.0;
            common.Printf($"{count / 60} seconds of game, replayed in {sec:5.1} seconds\n");
        }
        public void SaveCmdDemoToFile(VFile file)
        {
            mapSpawnData.serverInfo.WriteToFileHandle(file);

            for (int i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                mapSpawnData.userInfo[i].WriteToFileHandle(file);
                mapSpawnData.persistentPlayerInfo[i].WriteToFileHandle(file);
            }

            file.Write(&mapSpawnData.mapSpawnUsercmd, sizeof(mapSpawnData.mapSpawnUsercmd));

            if (numClients < 1)
                numClients = 1;
            file.Write(loggedUsercmds, numClients * logIndex * sizeof(loggedUsercmds[0]));
        }
        public void LoadCmdDemoFromFile(File file)
        {
            mapSpawnData.serverInfo.ReadFromFileHandle(file);

            for (var i = 0; i < Config.MAX_ASYNC_CLIENTS; i++)
            {
                mapSpawnData.userInfo[i].ReadFromFileHandle(file);
                mapSpawnData.persistentPlayerInfo[i].ReadFromFileHandle(file);
            }
            file.Read(&mapSpawnData.mapSpawnUsercmd, sizeof(mapSpawnData.mapSpawnUsercmd));
        }
        public void StartRecordingRenderDemo(string name)
        {
            if (writeDemo != null)
            {
                // allow it to act like a toggle
                StopRecordingRenderDemo();
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                common.Printf("SessionLocal.StartRecordingRenderDemo: no name specified\n");
                return;
            }

            console.Close();

            writeDemo = new VFileDemo();
            if (!writeDemo.OpenForWriting(name))
            {
                commonPrintf($"error opening {name}\n");
                //delete writeDemo;
                writeDemo = null;
                return;
            }

            common.Printf($"recording to {writeDemo.Name}\n");

            writeDemo.WriteInt(DS_VERSION);
            writeDemo.WriteInt(RENDERDEMO_VERSION);

            // if we are in a map already, dump the current state
            sw.StartWritingDemo(writeDemo);
            rw.StartWritingDemo(writeDemo);
        }
        public void StopRecordingRenderDemo()
        {
            if (writeDemo == null)
            {
                common.Printf("SessionLocal.StopRecordingRenderDemo: not recording\n");
                return;
            }
            sw.StopWritingDemo();
            rw.StopWritingDemo();

            writeDemo.Close();
            common.Printf("stopped recording {writeDemo.GetName()}.\n");
            //delete writeDemo;
            writeDemo = null;
        }
        public void StartPlayingRenderDemo(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                common.Printf("SessionLocal.StartPlayingRenderDemo: no name specified\n");
                return;
            }

            // make sure localSound / GUI intro music shuts up
            sw.StopAllSounds();
            sw.PlayShaderDirectly(string.Empty, 0);
            menuSoundWorld.StopAllSounds();
            menuSoundWorld.PlayShaderDirectly(string.Empty, 0);

            // exit any current game
            Stop();

            // automatically put the console away
            console.Close();

            // bring up the loading screen manually, since demos won't call ExecuteMapChange()
            guiLoading = uiManager.FindGui("guis/map/loading.gui", true, false, true);
            guiLoading.SetStateString("demo", common.LanguageDictGetString("#str_02087"));
            readDemo = new VFileDemo();
            name.DefaultFileExtension(".demo");
            if (!readDemo.OpenForReading(name))
            {
                common.Printf($"couldn't open {name}\n");
                readDemo = null;
                Stop();
                StartMenu();
                soundSystem.SetMute(false);
                return;
            }

            insideExecuteMapChange = true;
            UpdateScreen();
            insideExecuteMapChange = false;
            guiLoading.SetStateString("demo", "");

            // setup default render demo settings that's default for <= Doom3 v1.1
            renderdemoVersion = 1;
            savegameVersion = 16;

            AdvanceRenderDemo(true);

            numDemoFrames = 1;

            lastDemoTic = -1;
            timeDemoStartTime = SysW.Milliseconds;
        }
        /// <summary>
        /// Reports timeDemo numbers and finishes any avi recording
        /// </summary>
        public void StopPlayingRenderDemo()
        {
            if (readDemo == null)
            {
                timeDemo = TD.NO;
                return;
            }

            // Record the stop time before doing anything that could be time consuming
            var timeDemoStopTime = SysW.Milliseconds;

            EndAVICapture();

            readDemo.Close();

            sw.StopAllSounds();
            soundSystem.SetPlayingSoundWorld(menuSoundWorld);

            common.Printf($"stopped playing {readDemo.Name}.\n");
            readDemo = null;

            if (timeDemo)
            {
                // report the stats
                var demoSeconds = (timeDemoStopTime - timeDemoStartTime) * 0.001f;
                var demoFPS = numDemoFrames / demoSeconds;
                var message = $"{numDemoFrames} frames rendered in {demoSeconds:3.1} seconds = {demoFPS:3.1} fps\n");

                common.Printf(message);
                if (timeDemo == TD.YES_THEN_QUIT)
                    cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "quit\n");
                else
                {
                    soundSystem.SetMute(true);
                    MessageBox(MSG_OK, message, "Time Demo Results", true);
                    soundSystem.SetMute(false);
                }
                timeDemo = TD.NO;
            }
        }
        public void CompressDemoFile(string scheme, string name)
        {
            var fullDemoName = $"demos/{name}";
            fullDemoName.DefaultFileExtension(".demo");
            var compressedName = fullDemoName;
            compressedName.StripFileExtension();
            compressedName.Append("_compressed.demo");

            var savedCompression = cvarSystem.GetCVarInteger("com_compressDemos");
            var savedPreload = cvarSystem.GetCVarBool("com_preloadDemos");
            cvarSystem.SetCVarBool("com_preloadDemos", false);
            cvarSystem.SetCVarInteger("com_compressDemos", atoi(scheme));

            VFileDemo demoread, demowrite;
            if (!demoread.OpenForReading(fullDemoName))
            {
                common.Printf($"Could not open {fullDemoName} for reading\n");
                return;
            }
            if (!demowrite.OpenForWriting(compressedName))
            {
                common.Printf($"Could not open {compressedName} for writing\n");
                demoread.Close();
                cvarSystem.SetCVarBool("com_preloadDemos", savedPreload);
                cvarSystem.SetCVarInteger("com_compressDemos", savedCompression);
                return;
            }
            common.SetRefreshOnPrint(true);
            common.Printf($"Compressing {fullDemoName} to {compressedName}...\n");

            static const int bufferSize = 65535;
            char buffer[bufferSize];
            int bytesRead;
            while (0 != (bytesRead = demoread.Read(buffer, bufferSize)))
            {
                demowrite.Write(buffer, bytesRead);
                common.Printf(".");
            }

            demoread.Close();
            demowrite.Close();

            cvarSystem.SetCVarBool("com_preloadDemos", savedPreload);
            cvarSystem.SetCVarInteger("com_compressDemos", savedCompression);

            common.Printf("Done\n");
            common.SetRefreshOnPrint(false);
        }
        public void TimeRenderDemo(string name, bool twice = false)
        {
            // no sound in time demos
            soundSystem.SetMute(true);

            StartPlayingRenderDemo(demo);

            if (twice && readDemo)
            {
                // cycle through once to precache everything
                guiLoading.SetStateString("demo", common.LanguageDictGetString("#str_04852"));
                guiLoading.StateChanged(com_frameTime);
                while (readDemo)
                {
                    insideExecuteMapChange = true;
                    UpdateScreen();
                    insideExecuteMapChange = false;
                    AdvanceRenderDemo(true);
                }
                guiLoading.SetStateString("demo", "");
                StartPlayingRenderDemo(demo);
            }

            if (!readDemo)
                return;

            timeDemo = TD.YES;
        }
        public void AVIRenderDemo(string name)
        {
            StartPlayingRenderDemo(name);
            if (!readDemo)
                return;

            BeginAVICapture(name);

            // I don't understand why I need to do this twice, something strange with the nvidia swapbuffers?
            UpdateScreen();
        }
        public void AVICmdDemo(string name)
        {
            StartPlayingCmdDemo(name);

            BeginAVICapture(name);
        }
        /// <summary>
        /// Start AVI recording the current game session
        /// </summary>
        /// <param name="name">The name.</param>
        public void AVIGame(string name)
        {
            if (aviCaptureMode)
            {
                EndAVICapture();
                return;
            }

            if (!mapSpawned)
                common.Printf("No map spawned.\n");

            if (string.IsNullOrEmpty(name))
            {
                name = FindUnusedFileName("demos/game%03i.game");

                // write a one byte stub .game file just so the FindUnusedFileName works,
                fileSystem.WriteFile(name, name, 1);
            }

            BeginAVICapture(name);
        }
        public void BeginAVICapture(string name)
        {
            name.ExtractFileBase(aviDemoShortName);
            aviCaptureMode = true;
            aviDemoFrameCount = 0;
            aviTicStart = 0;
            sw.AVIOpen($"demos/{aviDemoShortName}/", aviDemoShortName);
        }
        public void EndAVICapture()
        {
            if (!aviCaptureMode)
                return;

            sw.AVIClose();

            // write a .roqParam file so the demo can be converted to a roq file
            var f = fileSystem.OpenFileWrite($"demos/{aviDemoShortName}/{aviDemoShortName}.roqParam");
            f.Printf($"INPUT_DIR demos/{aviDemoShortName}\n");
            f.Printf($"FILENAME demos/{aviDemoShortName}/{aviDemoShortName}.RoQ\n");
            f.Printf("\nINPUT\n");
            f.Printf($"{aviDemoShortName}*.tga [00000-{(int)(aviDemoFrameCount - 1):05}]\n");
            f.Printf("END_INPUT\n");

            common.Printf($"captured {(int)aviDemoFrameCount} frames for {aviDemoShortName}.\n");

            aviCaptureMode = false;
        }

        public void AdvanceRenderDemo(bool singleFrameOnly)
        {
            if (lastDemoTic == -1)
                lastDemoTic = latchedTicNumber - 1;

            var skipFrames = 0;

            if (!aviCaptureMode && !timeDemo && !singleFrameOnly)
            {
                skipFrames = ((latchedTicNumber - lastDemoTic) / USERCMD_PER_DEMO_FRAME) - 1;
                // never skip too many frames, just let it go into slightly slow motion
                if (skipFrames > 4)
                    skipFrames = 4;
                lastDemoTic = latchedTicNumber - latchedTicNumber % USERCMD_PER_DEMO_FRAME;
            }
            else
                // always advance a single frame with avidemo and timedemo
                lastDemoTic = latchedTicNumber;

            while (skipFrames > -1)
            {
                var ds = DS_FINISHED;

                readDemo.ReadInt(ds);
                if (ds == DS_FINISHED)
                {
                    if (numDemoFrames != 1)
                    {
                        // if the demo has a single frame (a demoShot), continuously replay the renderView that has already been read
                        Stop();
                        StartMenu();
                    }
                    break;
                }
                if (ds == DS_RENDER)
                {
                    if (rw.ProcessDemoCommand(readDemo, &currentDemoRenderView, &demoTimeOffset))
                    {
                        // a view is ready to render
                        skipFrames--;
                        numDemoFrames++;
                    }
                    continue;
                }
                if (ds == DS_SOUND)
                {
                    sw.ProcessDemoCommand(readDemo);
                    continue;
                }
                // appears in v1.2, with savegame format 17
                if (ds == DS_VERSION)
                {
                    readDemo.ReadInt(renderdemoVersion);
                    common.Printf($"reading a v{renderdemoVersion} render demo\n");
                    // set the savegameVersion to current for render demo paths that share the savegame paths
                    savegameVersion = SAVEGAME_VERSION;
                    continue;
                }
                common.Error("Bad render demo token");
            }

            if (com_showDemo.Bool)
                common.Printf($"frame:{numDemoFrames} DemoTic:{lastDemoTic} latched:{latchedTicNumber} skip:{skipFrames}\n");
        }
        public void RunGameTic()
        {
            LogCmd logCmd;
            Usercmd cmd;

            // if we are doing a command demo, read or write from the file
            if (cmdDemoFile)
            {
                if (!cmdDemoFile.Read(&logCmd, sizeof(logCmd)))
                {
                    common.Printf("Command demo completed at logIndex %i\n", logIndex);
                    fileSystem.CloseFile(cmdDemoFile);
                    cmdDemoFile = null;
                    if (aviCaptureMode)
                    {
                        EndAVICapture();
                        Shutdown();
                    }
                    // we fall out of the demo to normal commands
                    // the impulse and chat character toggles may not be correct, and the view
                    // angle will definitely be wrong
                }
                else
                {
                    cmd = logCmd.cmd;
                    cmd.ByteSwap();
                    logCmd.consistencyHash = LittleInt(logCmd.consistencyHash);
                }
            }

            // if we didn't get one from the file, get it locally
            if (!cmdDemoFile)
            {
                // get a locally created command
                if (com_asyncInput.GetBool())
                {
                    cmd = usercmdGen.TicCmd(lastGameTic);
                }
                else
                {
                    cmd = usercmdGen.GetDirectUsercmd();
                }
                lastGameTic++;
            }

            // run the game logic every player move
            int start = SysW.Milliseconds;
            var ret = game.RunFrame(&cmd);

            for (var h = 0; h < 2; h++)
                common.Vibrate(h, ret.vibrationLow[h], ret.vibrationHigh[h]);

            var end = SysW.Milliseconds;
            time_gameFrame += end - start;  // note time used for com_speeds

            // check for constency failure from a recorded command
            if (cmdDemoFile)
            {
                if (ret.consistencyHash != logCmd.consistencyHash)
                {
                    common.Printf($"Consistency failure on logIndex {logIndex}\n");
                    Stop();
                    return;
                }
            }

            // save the cmd for cmdDemo archiving
            if (logIndex < MAX_LOGGED_USERCMDS)
            {
                loggedUsercmds[logIndex].cmd = cmd;
                // save the consistencyHash for demo playback verification
                loggedUsercmds[logIndex].consistencyHash = ret.consistencyHash;
                if (logIndex % 30 == 0 && statIndex < MAX_LOGGED_STATS)
                {
                    loggedStats[statIndex].health = ret.health;
                    loggedStats[statIndex].heartRate = ret.heartRate;
                    loggedStats[statIndex].stamina = ret.stamina;
                    loggedStats[statIndex].combat = ret.combat;
                    statIndex++;
                }
                logIndex++;
            }

            syncNextGameFrame = ret.syncNextGameFrame;

            if (ret.sessionCommand[0])
            {
                CmdArgs args = new();

                args.TokenizeString(ret.sessionCommand, false);

                if (string.Equals(args[0], "map", StringComparison.OrdinalIgnoreCase))
                {
                    // get current player states
                    for (var i = 0; i < numClients; i++)
                        mapSpawnData.persistentPlayerInfo[i] = game.GetPersistentPlayerInfo(i);
                    // clear the devmap key on serverinfo, so player spawns won't get the map testing items
                    mapSpawnData.serverInfo.Delete("devmap");

                    // go to the next map
                    MoveToNewMap(args[1]);
                }
                else if (string.Equals(args[0], "devmap", StringComparison.OrdinalIgnoreCase))
                {
                    mapSpawnData.serverInfo.Set("devmap", "1");
                    MoveToNewMap(args[1]);
                }
                else if (string.Equals(args[0], "died", StringComparison.OrdinalIgnoreCase))
                {
                    // restart on the same map
                    UnloadMap();
                    SetGUI(guiRestartMenu, null);
                }
                else if (string.Equals(args[0], "disconnect", StringComparison.OrdinalIgnoreCase))
                    cmdSystem.BufferCommandText(CMD_EXEC.INSERT, "stoprecording ; disconnect");
            }
        }

        public void FinishCmdLoad() { }
        public void LoadLoadingGui(string mapName)
        {
            // load / program a gui to stay up on the screen while loading
            var stripped = mapName;
            stripped.StripFileExtension();
            stripped.StripPath();

            var guiMap = $"guis/map/{stripped}.gui";
            // give the gamecode a chance to override
            game.GetMapLoadingGUI(guiMap);

            guiLoading = uiManager.CheckGui(guiMap)
                ? uiManager.FindGui(guiMap, true, false, true)
                : uiManager.FindGui("guis/map/loading.gui", true, false, true);
            guiLoading.SetStateFloat("map_loading", 0.0f);
        }

        /// <summary>
        /// A demoShot is a single frame demo
        /// </summary>
        /// <param name="name">The name.</param>
        public void DemoShot(string name)
        {
            StartRecordingRenderDemo(name);

            // force draw one frame
            UpdateScreen();

            StopRecordingRenderDemo();
        }

        public void TestGUI(string name)
            => guiTest = !string.IsNullOrEmpty(name) ? uiManager.FindGui(name, true, false, true) : null;

        public int GetBytesNeededForMapLoad(string mapName)
        {
            var mapDecl = declManager.FindType(DECL_MAPDEF, mapName, false);
            var mapDef = (DeclEntityDef)mapDecl;
            return mapDef != null
                ? mapDef.dict.GetInt($"size{3}")
                : 400 * 1024 * 1024;
        }
        public void SetBytesNeededForMapLoad(string mapName, int bytesNeeded)
        {
            var mapDecl = declManager.FindType(DECL_MAPDEF, mapName, false);
            var mapDef = (DeclEntityDef)mapDecl;

            if (com_updateLoadSize.GetBool() && mapDef)
            {
                // we assume that if com_updateLoadSize is true then the file is writable

                mapDef.dict.SetInt($"size{0}", bytesNeeded);

                var declText = "\nmapDef ";
                declText += mapDef.GetName();
                declText += " {\n";
                for (var i = 0; i < mapDef.dict.GetNumKeyVals(); i++)
                {
                    var kv = mapDef.dict.GetKeyVal(i);
                    if (kv && (kv.GetKey().Cmp("classname") != 0))
                        declText += "\t\"" + kv.GetKey() + "\"\t\t\"" + kv.GetValue() + "\"\n";
                }
                declText += "}";
                mapDef.SetText(declText);
                mapDef.ReplaceSourceFileText();
            }
        }

        /// <summary>
        /// Performs the initialization of a game based on mapSpawnData, used for both single player and multiplayer, but not for renderDemos, which don't
        /// create a game at all.
        /// Exits with mapSpawned = true
        /// </summary>
        /// <param name="noFadeWipe">if set to <c>true</c> [no fade wipe].</param>
        public void ExecuteMapChange(bool noFadeWipe = false)
        {
            int i;
            bool reloadingSameMap;

            // close console and remove any prints from the notify lines
            console.Close();

            if (IsMultiplayer)
                // make sure the mp GUI isn't up, or when players get back in the map, mpGame's menu and the gui will be out of sync.
                SetGUI(null, null);

            // mute sound
            soundSystem.SetMute(true);

            // clear all menu sounds
            menuSoundWorld.ClearAllSoundEmitters();

            // unpause the game sound world. NOTE: we UnPause again later down. not sure this is needed
            if (sw.IsPaused())
                sw.UnPause();

            if (!noFadeWipe)
            {
                // capture the current screen and start a wipe
                StartWipe("wipeMaterial", true);

                // immediately complete the wipe to fade out the level transition
                // run the wipe to completion
                CompleteWipe();
            }

            // extract the map name from serverinfo
            var mapString = mapSpawnData.serverInfo.Get("si_map");

            var fullMapName = "maps/";
            fullMapName += mapString;
            fullMapName.StripFileExtension();

            // shut down the existing game if it is running
            UnloadMap();

            // don't do the deferred caching if we are reloading the same map
            if (fullMapName == currentMapName)
                reloadingSameMap = true;
            else
            {
                reloadingSameMap = false;
                currentMapName = fullMapName;
            }

            // note which media we are going to need to load
            if (!reloadingSameMap)
            {
                declManager.BeginLevelLoad();
                renderSystem.BeginLevelLoad();
                soundSystem.BeginLevelLoad();
            }

            uiManager.BeginLevelLoad();
            uiManager.Reload(true);

            // set the loading gui that we will wipe to
            LoadLoadingGui(mapString);

            // cause prints to force screen updates as a pacifier, and draw the loading gui instead of game draws
            insideExecuteMapChange = true;

            // if this works out we will probably want all the sizes in a def file although this solution will
            // work for new maps etc. after the first load. we can also drop the sizes into the default.cfg
            fileSystem.ResetReadCount();
            bytesNeededForMapLoad = !reloadingSameMap
                ? GetBytesNeededForMapLoad(mapString)
                : 30 * 1024 * 1024;

            ClearWipe();

            // let the loading gui spin for 1 second to animate out
            ShowLoadingGui();

            // note any warning prints that happen during the load process
            common.ClearWarnings(mapString);

            // release the mouse cursor before we do this potentially long operation
            SysW.GrabMouseCursor(false);

            // if net play, we get the number of clients during mapSpawnInfo processing
            if (!AsyncNetwork.IsActive())
                numClients = 1;

            var start = SysW.Milliseconds;

            common.Printf("----- Map Initialization -----\n");
            common.Printf($"Map: {mapString}\n");

            // let the renderSystem load all the geometry
            if (!rw.InitFromMap(fullMapName))
                common.Error($"couldn't load {fullMapName}");

            // for the synchronous networking we needed to roll the angles over from level to level, but now we can just clear everything
            usercmdGen.InitForNewMap();
            Array.Clear(mapSpawnData.mapSpawnUsercmd, 0, mapSpawnData.mapSpawnUsercmd.Length);

            // set the user info
            for (i = 0; i < numClients; i++)
            {
                game.SetUserInfo(i, mapSpawnData.userInfo[i], AsyncNetwork.client.IsActive, false);
                game.SetPersistentPlayerInfo(i, mapSpawnData.persistentPlayerInfo[i]);
            }

            // load and spawn all other entities ( from a savegame possibly )
            if (loadingSaveGame && savegameFile)
            {
                if (game.InitFromSaveGame(fullMapName + ".map", rw, sw, savegameFile) == false)
                {
                    // If the loadgame failed, restart the map with the player persistent data
                    loadingSaveGame = false;
                    fileSystem.CloseFile(savegameFile);
                    savegameFile = null;

                    game.SetServerInfo(mapSpawnData.serverInfo);
                    game.InitFromNewMap(fullMapName + ".map", rw, sw, idAsyncNetwork.server.IsActive(), idAsyncNetwork.client.IsActive(), Sys_Milliseconds());
                }
            }
            else
            {
                game.SetServerInfo(mapSpawnData.serverInfo);
                game.InitFromNewMap(fullMapName + ".map", rw, sw, idAsyncNetwork.server.IsActive(), idAsyncNetwork.client.IsActive(), Sys_Milliseconds());
            }

            if (!AsyncNetwork.IsActive && !loadingSaveGame)
                // spawn players
                for (i = 0; i < numClients; i++)
                    game.SpawnPlayer(i);

            // actually purge/load the media
            if (!reloadingSameMap)
            {
                renderSystem.EndLevelLoad();
                soundSystem.EndLevelLoad(mapString);
                declManager.EndLevelLoad();
                SetBytesNeededForMapLoad(mapString, fileSystem.GetReadCount());
            }
            uiManager.EndLevelLoad();

            if (!AsyncNetwork.IsActive && !loadingSaveGame)
                // run a few frames to allow everything to settle
                for (i = 0; i < 10; i++)
                    game.RunFrame(mapSpawnData.mapSpawnUsercmd);

            var msec = SysW.Milliseconds() - start;
            common.Printf($"{msec:6} msec to load {mapString}\n");

            // let the renderSystem generate interactions now that everything is spawned
            rw.GenerateAllInteractions();

            common.PrintWarnings();

            if (guiLoading && bytesNeededForMapLoad)
            {
                var pct = guiLoading.State.GetFloat("map_loading");
                if (pct < 0.0f)
                    pct = 0.0f;
                while (pct < 1.0f)
                {
                    guiLoading.SetStateFloat("map_loading", pct);
                    guiLoading.StateChanged(com_frameTime);
                    SysW.GenerateEvents();
                    UpdateScreen();
                    pct += 0.05f;
                }
            }

            // capture the current screen and start a wipe
            StartWipe("wipe2Material");

            usercmdGen.Clear();

            // start saving commands for possible writeCmdDemo usage
            logIndex = 0;
            statIndex = 0;
            lastSaveIndex = 0;

            // don't bother spinning over all the tics we spent loading
            lastGameTic = latchedTicNumber = com_ticNumber;

            // remove any prints from the notify lines
            console.ClearNotifyLines();

            // stop drawing the laoding screen
            insideExecuteMapChange = false;

            SysW.SetPhysicalWorkMemory(-1, -1);

            // set the game sound world for playback
            soundSystem.SetPlayingSoundWorld(sw);

            // when loading a save game the sound is paused
            if (sw.IsPaused())
                // unpause the game sound world
                sw.UnPause();

            // restart entity sound playback
            soundSystem.SetMute(false);

            // we are valid for game draws now
            mapSpawned = true;
            SysW.ClearEvents();
        }
        /// <summary>
        /// Performs cleanup that needs to happen between maps, or when a game is exited.
        /// Exits with mapSpawned = false
        /// </summary>
        public void UnloadMap()
        {
            StopPlayingRenderDemo();

            // end the current map in the game
            game?.MapShutdown();

            if (cmdDemoFile != null)
            {
                fileSystem.CloseFile(cmdDemoFile);
                cmdDemoFile = null;
            }

            if (writeDemo != null)
                StopRecordingRenderDemo();

            mapSpawned = false;
        }

        //------------------
        // Session_menu.cpp

        public List<string> loadGameList = new List<string>();
        public List<string> modsList = new List<string>();

        public IUserInterface GetActiveMenu() { }

        public void DispatchCommand(IUserInterface gui, string menuCommand, bool doIngame = true) { }
        public void MenuEvent(sysEvent event_);
        public bool HandleSaveGameMenuCommand(out CmdArgs args, out int icmd);
        public void HandleInGameCommands(string menuCommand);
        public void HandleMainMenuCommands(string menuCommand);
        public void HandleChatMenuCommands(string menuCommand);
        public void HandleIntroMenuCommands(string menuCommand);
        public void HandleRestartMenuCommands(string menuCommand);
        public void HandleMsgCommands(string menuCommand);
        public void HandleNoteCommands(string menuCommand);
        public void GetSaveGameList(out List<string> fileList, out List<DateTime> fileTimes);

#if false
        readonly static string[] TakeNotes_people = {
            "Nobody", "Adam", "Brandon", "David", "PHook", "Jay", "Jake",
            "PatJ", "Brett", "Ted", "Darin", "Brian", "Sean"
        };
#else
        readonly static string[] TakeNotes_people = new[]{
            "Tim", "Kenneth", "Robert",
            "Matt", "Mal", "Jerry", "Steve", "Pat",
            "Xian", "Ed", "Fred", "James", "Eric", "Andy", "Seneca", "Patrick", "Kevin",
            "MrElusive", "Jim", "Brian", "John", "Adrian", "Nobody"
        };
#endif
        public void TakeNotes(string p, bool extended = false)
        {
            if (!mapSpawned)
            {
                common.Printf("No map loaded!\n");
                return;
            }

            if (extended)
            {
                guiTakeNotes = uiManager.FindGui("guis/takeNotes2.gui", true, false, true);
                var numPeople = people.Length;

                var guiList_people = uiManager.AllocListGUI();
                guiList_people.Config(guiTakeNotes, "person");
                for (var i = 0; i < numPeople; i++)
                    guiList_people.Push(people[i]);
                uiManager.FreeListGUI(guiList_people);
            }
            else
                guiTakeNotes = uiManager.FindGui("guis/takeNotes.gui", true, false, true);

            SetGUI(guiTakeNotes, null);
            guiActive.SetStateString("note", "");
            guiActive.SetStateString("notefile", p);
            guiActive.SetStateBool("extended", extended);
            guiActive.Activate(true, com_frameTime);
        }
        public void UpdateMPLevelShot() { }

        public void SetSaveGameGuiVars() { }
        public void SetMainMenuGuiVars() { }
        public void SetModsMenuGuiVars() { }
        public void SetMainMenuSkin() { }
        public void SetPbMenuGuiVars() { }

        // DG: true if running the Demo version of Doom3 (for FT_IsDemo, see Common.h)
        public bool IsDemoVersion => demoversion;

        bool BoxDialogSanityCheck() { }

        bool demoversion; // DG: true if running the Demo version of Doom3, for FT_IsDemo (see Common.h)
    }
}