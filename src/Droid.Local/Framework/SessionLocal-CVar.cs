namespace Droid.Framework
{
    partial class SessionLocal
    {

        /*
        =================
        Session_RescanSI_f
        =================
        */
        static void Session_RescanSI_f(CmdArgs args)
        {
            G.sessLocal.mapSpawnData.serverInfo = *cvarSystem->MoveCVarsToDict(CVAR_SERVERINFO);
            if (game && idAsyncNetwork::server.IsActive())
            {
                game->SetServerInfo(sessLocal.mapSpawnData.serverInfo);
            }
        }

#if !ID_DEDICATED
        /*
        ==================
        Session_Map_f

        Restart the server on a different map
        ==================
        */
        static void Session_Map_f(CmdArgs args)
        {
            idStr map, string;
            findFile_t ff;
            idCmdArgs rl_args;

            map = args.Argv(1);
            if (!map.Length())
            {
                return;
            }
            map.StripFileExtension();

            // make sure the level exists before trying to change, so that
            // a typo at the server console won't end the game
            // handle addon packs through reloadEngine
            sprintf(string, "maps/%s.map", map.c_str());
            ff = fileSystem->FindFile(string, true);
            switch (ff)
            {
                case FIND_NO:
                    common->Printf("Can't find map %s\n", string.c_str());
                    return;
                case FIND_ADDON:
                    common->Printf("map %s is in an addon pak - reloading\n", string.c_str());
                    rl_args.AppendArg("map");
                    rl_args.AppendArg(map);
                    cmdSystem->SetupReloadEngine(rl_args);
                    return;
                default:
                    break;
            }

            cvarSystem->SetCVarBool("developer", false);
            sessLocal.StartNewGame(map, true);
        }

        /*
        ==================
        Session_DevMap_f

        Restart the server on a different map in developer mode
        ==================
        */
        static void Session_DevMap_f(CmdArgs args)
        {
            idStr map, string;
            findFile_t ff;
            idCmdArgs rl_args;

            map = args.Argv(1);
            if (!map.Length())
            {
                return;
            }
            map.StripFileExtension();

            // make sure the level exists before trying to change, so that
            // a typo at the server console won't end the game
            // handle addon packs through reloadEngine
            sprintf(string, "maps/%s.map", map.c_str());
            ff = fileSystem->FindFile(string, true);
            switch (ff)
            {
                case FIND_NO:
                    common->Printf("Can't find map %s\n", string.c_str());
                    return;
                case FIND_ADDON:
                    common->Printf("map %s is in an addon pak - reloading\n", string.c_str());
                    rl_args.AppendArg("devmap");
                    rl_args.AppendArg(map);
                    cmdSystem->SetupReloadEngine(rl_args);
                    return;
                default:
                    break;
            }

            cvarSystem->SetCVarBool("developer", true);
            sessLocal.StartNewGame(map, true);
        }

        /*
        ==================
        Session_TestMap_f
        ==================
        */
        static void Session_TestMap_f(CmdArgs args)
        {
            idStr map, string;

            map = args.Argv(1);
            if (!map.Length())
            {
                return;
            }
            map.StripFileExtension();

            cmdSystem->BufferCommandText(CMD_EXEC_NOW, "disconnect");

            sprintf(string, "dmap maps/%s.map", map.c_str());
            cmdSystem->BufferCommandText(CMD_EXEC_NOW, string);

            sprintf(string, "devmap %s", map.c_str());
            cmdSystem->BufferCommandText(CMD_EXEC_NOW, string);
        }
#endif

        /*
        ==================
        Sess_WritePrecache_f
        ==================
        */
        static void Sess_WritePrecache_f(CmdArgs args)
        {
            if (args.Argc() != 2)
            {
                common->Printf("USAGE: writePrecache <execFile>\n");
                return;
            }
            idStr str = args.Argv(1);
            str.DefaultFileExtension(".cfg");
            idFile* f = fileSystem->OpenFileWrite(str, "fs_configpath");
            declManager->WritePrecacheCommands(f);
            renderModelManager->WritePrecacheCommands(f);
            uiManager->WritePrecacheCommands(f);

            fileSystem->CloseFile(f);
        }

        /*
        ===================
        Session_PromptKey_f
        ===================
        */
        static void Session_PromptKey_f(CmdArgs args)
        {
            const char* retkey;
            bool valid[2];
            static bool recursed = false;

            if (recursed)
            {
                common->Warning("promptKey recursed - aborted");
                return;
            }
            recursed = true;

            do
            {
                // in case we're already waiting for an auth to come back to us ( may happen exceptionally )
                if (sessLocal.MaybeWaitOnCDKey())
                {
                    if (sessLocal.CDKeysAreValid(true))
                    {
                        recursed = false;
                        return;
                    }
                }
                // the auth server may have replied and set an error message, otherwise use a default
                const char* prompt_msg = sessLocal.GetAuthMsg();
                if (prompt_msg[0] == '\0')
                {
                    prompt_msg = common->GetLanguageDict()->GetString("#str_04308");
                }
                retkey = sessLocal.MessageBox(MSG_CDKEY, prompt_msg, common->GetLanguageDict()->GetString("#str_04305"), true, NULL, NULL, true);
                if (retkey)
                {
                    if (sessLocal.CheckKey(retkey, false, valid))
                    {
                        // if all went right, then we may have sent an auth request to the master ( unless the prompt is used during a net connect )
                        bool canExit = true;
                        if (sessLocal.MaybeWaitOnCDKey())
                        {
                            // wait on auth reply, and got denied, prompt again
                            if (!sessLocal.CDKeysAreValid(true))
                            {
                                // server says key is invalid - MaybeWaitOnCDKey was interrupted by a CDKeysAuthReply call, which has set the right error message
                                // the invalid keys have also been cleared in the process
                                sessLocal.MessageBox(MSG_OK, sessLocal.GetAuthMsg(), common->GetLanguageDict()->GetString("#str_04310"), true, NULL, NULL, true);
                                canExit = false;
                            }
                        }
                        if (canExit)
                        {
                            // make sure that's saved on file
                            sessLocal.WriteCDKey();
                            sessLocal.MessageBox(MSG_OK, common->GetLanguageDict()->GetString("#str_04307"), common->GetLanguageDict()->GetString("#str_04305"), true, NULL, NULL, true);
                            break;
                        }
                    }
                    else
                    {
                        // offline check sees key invalid
                        // build a message about keys being wrong. do not attempt to change the current key state though
                        // ( the keys may be valid, but user would have clicked on the dialog anyway, that kind of thing )
                        idStr msg;
                        idAsyncNetwork::BuildInvalidKeyMsg(msg, valid);
                        sessLocal.MessageBox(MSG_OK, msg, common->GetLanguageDict()->GetString("#str_04310"), true, NULL, NULL, true);
                    }
                }
                else if (args.Argc() == 2 && idStr::Icmp(args.Argv(1), "force") == 0)
                {
                    // cancelled in force mode
                    cmdSystem->BufferCommandText(CMD_EXEC_APPEND, "quit\n");
                    cmdSystem->ExecuteCommandBuffer();
                }
            } while (retkey);
            recursed = false;
        }

        /*
        ================
        Session_TestGUI_f
        ================
        */
        static void Session_TestGUI_f(CmdArgs args) => G.sessLocal.TestGUI(args.Argv(1));

        /*
        ================
        FindUnusedFileName
        ================
        */
        static string FindUnusedFileName(string format)
        {
            int i;
            char filename[1024];

            for (i = 0; i < 999; i++)
            {
                sprintf(filename, format, i);
                int len = fileSystem->ReadFile(filename, NULL, NULL);
                if (len <= 0)
                {
                    return filename;    // file doesn't exist
                }
            }

            return filename;
        }

        /*
        ================
        Session_DemoShot_f
        ================
        */
        static void Session_DemoShot_f(CmdArgs args)
        {
            if (args.Argc() != 2)
            {
                idStr filename = FindUnusedFileName("demos/shot%03i.demo");
                sessLocal.DemoShot(filename);
            }
            else
            {
                sessLocal.DemoShot(va("demos/shot_%s.demo", args.Argv(1)));
            }
        }

#if !ID_DEDICATED
        /*
        ================
        Session_RecordDemo_f
        ================
        */
        static void Session_RecordDemo_f(CmdArgs args)
        {
            if (args.Argc() != 2)
            {
                idStr filename = FindUnusedFileName("demos/demo%03i.demo");
                sessLocal.StartRecordingRenderDemo(filename);
            }
            else
            {
                sessLocal.StartRecordingRenderDemo(va("demos/%s.demo", args.Argv(1)));
            }
        }

        /*
        ================
        Session_CompressDemo_f
        ================
        */
        static void Session_CompressDemo_f(CmdArgs args)
        {
            if (args.Argc() == 2)
            {
                sessLocal.CompressDemoFile("2", args.Argv(1));
            }
            else if (args.Argc() == 3)
            {
                sessLocal.CompressDemoFile(args.Argv(2), args.Argv(1));
            }
            else
            {
                common->Printf("use: CompressDemo <file> [scheme]\nscheme is the same as com_compressDemo, defaults to 2");
            }
        }

        /*
        ================
        Session_StopRecordingDemo_f
        ================
        */
        static void Session_StopRecordingDemo_f(CmdArgs args)
        {
            sessLocal.StopRecordingRenderDemo();
        }

        /*
        ================
        Session_PlayDemo_f
        ================
        */
        static void Session_PlayDemo_f(CmdArgs args)
        {
            if (args.Argc() >= 2)
            {
                sessLocal.StartPlayingRenderDemo(va("demos/%s", args.Argv(1)));
            }
        }

        /*
        ================
        Session_TimeDemo_f
        ================
        */
        static void Session_TimeDemo_f(CmdArgs args)
        {
            if (args.Argc() >= 2)
            {
                sessLocal.TimeRenderDemo(va("demos/%s", args.Argv(1)), (args.Argc() > 2));
            }
        }

        /*
        ================
        Session_TimeDemoQuit_f
        ================
        */
        static void Session_TimeDemoQuit_f(CmdArgs args)
        {
            sessLocal.TimeRenderDemo(va("demos/%s", args.Argv(1)));
            if (sessLocal.timeDemo == TD_YES)
            {
                // this allows hardware vendors to automate some testing
                sessLocal.timeDemo = TD_YES_THEN_QUIT;
            }
        }

        /*
        ================
        Session_AVIDemo_f
        ================
        */
        static void Session_AVIDemo_f(CmdArgs args)
        {
            sessLocal.AVIRenderDemo(va("demos/%s", args.Argv(1)));
        }

        /*
        ================
        Session_AVIGame_f
        ================
        */
        static void Session_AVIGame_f(CmdArgs args)
        {
            sessLocal.AVIGame(args.Argv(1));
        }

        /*
        ================
        Session_AVICmdDemo_f
        ================
        */
        static void Session_AVICmdDemo_f(CmdArgs args)
        {
            sessLocal.AVICmdDemo(args.Argv(1));
        }

        /*
        ================
        Session_WriteCmdDemo_f
        ================
        */
        static void Session_WriteCmdDemo_f(CmdArgs args)
        {
            if (args.Argc() == 1)
            {
                idStr filename = FindUnusedFileName("demos/cmdDemo%03i.cdemo");
                sessLocal.WriteCmdDemo(filename);
            }
            else if (args.Argc() == 2)
            {
                sessLocal.WriteCmdDemo(va("demos/%s.cdemo", args.Argv(1)));
            }
            else
            {
                common->Printf("usage: writeCmdDemo [demoName]\n");
            }
        }

        /*
        ================
        Session_PlayCmdDemo_f
        ================
        */
        static void Session_PlayCmdDemo_f(CmdArgs args)
        {
            sessLocal.StartPlayingCmdDemo(args.Argv(1));
        }

        /*
        ================
        Session_TimeCmdDemo_f
        ================
        */
        static void Session_TimeCmdDemo_f(CmdArgs args)
        {
            sessLocal.TimeCmdDemo(args.Argv(1));
        }
#endif

        /*
        ================
        Session_Disconnect_f
        ================
        */
        static void Session_Disconnect_f(CmdArgs args)
        {
            sessLocal.Stop();
            sessLocal.StartMenu();
            if (soundSystem)
            {
                soundSystem->SetMute(false);
            }
        }

#if !ID_DEDICATED
        /*
        ================
        Session_ExitCmdDemo_f
        ================
        */
        static void Session_ExitCmdDemo_f(CmdArgs args)
        {
            if (!sessLocal.cmdDemoFile)
            {
                common->Printf("not reading from a cmdDemo\n");
                return;
            }
            fileSystem->CloseFile(sessLocal.cmdDemoFile);
            common->Printf("Command demo exited at logIndex %i\n", sessLocal.logIndex);
            sessLocal.cmdDemoFile = NULL;
        }
#endif

        /*
        ===============
        LoadGame_f
        ===============
        */
        void LoadGame_f(CmdArgs args)
        {
            console->Close();
            if (args.Argc() < 2 || idStr::Icmp(args.Argv(1), "quick") == 0)
            {
                idStr saveName = common->GetLanguageDict()->GetString("#str_07178");
                sessLocal.LoadGame(saveName);
            }
            else
            {
                sessLocal.LoadGame(args.Argv(1));
            }
        }

        /*
        ===============
        SaveGame_f
        ===============
        */
        void SaveGame_f(CmdArgs args)
        {
            if (args.Argc() < 2 || idStr::Icmp(args.Argv(1), "quick") == 0)
            {
                idStr saveName = common->GetLanguageDict()->GetString("#str_07178");
                if (sessLocal.SaveGame(saveName))
                {
                    common->Printf("%s\n", saveName.c_str());
                }
            }
            else
            {
                if (sessLocal.SaveGame(args.Argv(1)))
                {
                    common->Printf("Saved %s\n", args.Argv(1));
                }
            }
        }

        /*
        ===============
        TakeViewNotes_f
        ===============
        */
        void TakeViewNotes_f(CmdArgs args)
        {
            const char* p = (args.Argc() > 1) ? args.Argv(1) : "";
            sessLocal.TakeNotes(p);
        }

        /*
        ===============
        TakeViewNotes2_f
        ===============
        */
        void TakeViewNotes2_f(CmdArgs args)
        {
            const char* p = (args.Argc() > 1) ? args.Argv(1) : "";
            sessLocal.TakeNotes(p, true);
        }


        /*
        ===============
        Session_Hitch_f
        ===============
        */
        void Session_Hitch_f(CmdArgs args)
        {
            idSoundWorld* sw = soundSystem->GetPlayingSoundWorld();
            if (sw)
            {
                soundSystem->SetMute(true);
                sw->Pause();
                Sys_EnterCriticalSection();
            }
            if (args.Argc() == 2)
            {
                Sys_Sleep(atoi(args.Argv(1)));
            }
            else
            {
                Sys_Sleep(100);
            }
            if (sw)
            {
                Sys_LeaveCriticalSection();
                sw->UnPause();
                soundSystem->SetMute(false);
            }
        }
    }
}