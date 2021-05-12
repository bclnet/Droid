using System;

namespace Droid.Framework
{
    /// <summary>
    /// the console will query the cvar and command systems for command completion information
    /// </summary>
    internal partial class ConsoleLocal : Console
    {
        const int LINE_WIDTH = 78;
        const int NUM_CON_TIMES = 4;
        const int CON_TEXTSIZE = 0x30000;
        const int TOTAL_LINES = (CON_TEXTSIZE / LINE_WIDTH);
        const int CONSOLE_FIRSTREPEAT = 200;
        const int CONSOLE_REPEAT = 100;

        const int COMMAND_HISTORY = 64;

        public override void Init()
        {
            int i;

            keyCatching = false;

            lastKeyEvent = -1;
            nextKeyEvent = CONSOLE_FIRSTREPEAT;

            consoleField.Clear();

            consoleField.SetWidthInChars(LINE_WIDTH);

            for (i = 0; i < COMMAND_HISTORY; i++)
            {
                historyEditLines[i].Clear();
                historyEditLines[i].SetWidthInChars(LINE_WIDTH);
            }

            G.cmdSystem.AddCommand("clear", Con_Clear_f, CMD_FL.SYSTEM, "clears the console");
            G.cmdSystem.AddCommand("conDump", Con_Dump_f, CMD_FL.SYSTEM, "dumps the console text to a file");
        }
        public override void Shutdown()
        {
            G.cmdSystem.RemoveCommand("clear");
            G.cmdSystem.RemoveCommand("conDump");
        }
        /// <summary>
        /// Can't be combined with init, because init happens before the renderSystem is initialized
        /// </summary>
        public override void LoadGraphics()
        {
            charSetShader = G.declManager.FindMaterial("textures/bigchars");
            whiteShader = G.declManager.FindMaterial("_white");
            consoleShader = G.declManager.FindMaterial("console");
        }
        public override bool ProcessEvent(sysEvent e, bool forceAccept)
        {
            bool consoleKey = false;
            if (e.evType == SE_KEY)
                if (e.evValue == Sys_GetConsoleKey(false) || e.evValue == Sys_GetConsoleKey(true) || (e.evValue == K_ESCAPE && KeyInput.IsDown(K_SHIFT))) // shift+esc should also open console
                    consoleKey = true;

#if ID_CONSOLE_LOCK
            // If the console's not already down, and we have it turned off, check for ctrl+alt
            if (!keyCatching && !com_allowConsole.Bool)
                if (!KeyInput.IsDown(K_CTRL) || !KeyInput.IsDown(K_ALT))
                    consoleKey = false;
#endif

            // we always catch the console key e
            if (!forceAccept && consoleKey)
            {
                // ignore up es
                if (e.evValue2 == 0)
                    return true;

                consoleField.ClearAutoComplete();

                // a down e will toggle the destination lines
                if (keyCatching)
                {
                    Close();
                    Sys_GrabMouseCursor(true);
                    G.cvarSystem.SetCVarBool("ui_chat", false);
                }
                else
                {
                    consoleField.Clear();
                    keyCatching = true;
                    // if the shift key is down, don't open the console as much except we used shift+esc.
                    SetDisplayFraction(KeyInput.IsDown(K_SHIFT) && e.evValue != K_ESCAPE ? 0.2f : 0.5f);
                    G.cvarSystem.SetCVarBool("ui_chat", true);
                }
                return true;
            }

            // if we aren't key catching, dump all the other es
            if (!forceAccept && !keyCatching)
                return false;

            // handle key and character es
            if (e.evType == SE_CHAR)
            {
                // never send the console key as a character
                if (e.evValue != Sys_GetConsoleKey(false) && e.evValue != Sys_GetConsoleKey(true))
                    consoleField.CharEvent(e.evValue);
                return true;
            }

            if (e.evType == SE_KEY)
            {
                // ignore up key es
                if (e.evValue2 == 0)
                    return true;

                KeyDownEvent(e.evValue);
                return true;
            }

            // we don't handle things like mouse, joystick, and network packets
            return false;
        }
        public override bool Active => keyCatching;
        public override void ClearNotifyLines() => Array.Clear(times, 0, NUM_CON_TIMES);
        public override void Close()
        {
            keyCatching = false;
            SetDisplayFraction(0);
            displayFrac = 0;    // don't scroll to that point, go immediately
            ClearNotifyLines();
        }
        /// <summary>
        /// Handles cursor positioning, line wrapping, etc
        /// </summary>
        /// <param name="text">The text.</param>
        public override void Print(string text)
        {
            int y;
            int c, l;
            int color;

#if ID_ALLOW_TOOLS
            RadiantPrint(txt);

            if (com_editors & EDITOR_MATERIAL)
                MaterialEditorPrintConsole(txt);
#endif

            color = string.ColorIndex(G.C_COLOR_CYAN);

            while ((c = *(const unsigned char*)txt) != 0 ) {
                if (idStr::IsColor(txt))
                {
                    if (*(txt + 1) == C_COLOR_DEFAULT)
                    {
                        color = idStr::ColorIndex(C_COLOR_CYAN);
                    }
                    else
                    {
                        color = idStr::ColorIndex(*(txt + 1));
                    }
                    txt += 2;
                    continue;
                }

                y = current % TOTAL_LINES;

                // if we are about to print a new word, check to see
                // if we should wrap to the new line
                if (c > ' ' && (x == 0 || text[y * LINE_WIDTH + x - 1] <= ' '))
                {
                    // count word length
                    for (l = 0; l < LINE_WIDTH; l++)
                        if (txt[l] <= ' ')
                            break;

                    // word wrap
                    if (l != LINE_WIDTH && (x + l >= LINE_WIDTH))
                        Linefeed();
                }

                txt++;

                switch (c)
                {
                    case '\n':
                        Linefeed();
                        break;
                    case '\t':
                        do
                        {
                            text[y * LINE_WIDTH + x] = (color << 8) | ' ';
                            x++;
                            if (x >= LINE_WIDTH)
                            {
                                Linefeed();
                                x = 0;
                            }
                        } while (x & 3);
                        break;
                    case '\r':
                        x = 0;
                        break;
                    default:    // display character and advance
                        text[y * LINE_WIDTH + x] = (color << 8) | c;
                        x++;
                        if (x >= LINE_WIDTH)
                        {
                            Linefeed();
                            x = 0;
                        }
                        break;
                }
            }

            // mark time for transparent overlay
            if (current >= 0)
                times[current % NUM_CON_TIMES] = com_frameTime;
        }

        /// <summary>
        /// ForceFullScreen is used by the editor
        /// </summary>
        /// <param name="forceFullScreen">if set to <c>true</c> [force full screen].</param>
        public override void Draw(bool forceFullScreen)
        {
            var y = 0.0f;

            if (!charSetShader)
                return;

            if (forceFullScreen)
            {
                // if we are forced full screen because of a disconnect, we want the console closed when we go back to a session state
                Close();
                // we are however catching keyboard input
                keyCatching = true;
            }

            Scroll();

            UpdateDisplayFraction();

            if (forceFullScreen)
                DrawSolidConsole(1.0f);
            else if (displayFrac != 0)
                DrawSolidConsole(displayFrac);
            // only draw the notify lines if the developer cvar is set, or we are a debug build
            else if (!con_noPrint.Bool)
                DrawNotify();

            if (com_showFPS.Bool)
                y = SCR_DrawFPS(0);
            if (com_showMemoryUsage.Bool)
                y = SCR_DrawMemoryUsage(y);
            if (com_showAsyncStats.Bool)
                y = SCR_DrawAsyncStats(y);
            if (com_showSoundDecoders.Bool)
                y = SCR_DrawSoundDecoders(y);
        }

        /// <summary>
        /// Save the console contents out to a file
        /// </summary>
        /// <param name="toFile">To file.</param>
        public void Dump(string toFile)
        {
            int l, x, i;
            short* line;
            char buffer[LINE_WIDTH + 3];

            var f = fileSystem.OpenFileWrite(fileName);
            if (f == null)
            {
                G.common.Warning($"couldn't open {fileName}");
                return;
            }

            // skip empty lines
            l = current - TOTAL_LINES + 1;
            if (l < 0)
                l = 0;
            for (; l <= current; l++)
            {
                line = text + (l % TOTAL_LINES) * LINE_WIDTH;
                for (x = 0; x < LINE_WIDTH; x++)
                    if ((line[x] & 0xff) > ' ')
                        break;
                if (x != LINE_WIDTH)
                    break;
            }

            // write the remaining lines
            for (; l <= current; l++)
            {
                line = text + (l % TOTAL_LINES) * LINE_WIDTH;
                for (i = 0; i < LINE_WIDTH; i++)
                    buffer[i] = line[i] & 0xff;
                for (x = LINE_WIDTH - 1; x >= 0; x--)
                {
                    if (buffer[x] <= ' ')
                        buffer[x] = 0;
                    else
                        break;
                }
                buffer[x + 1] = '\r';
                buffer[x + 2] = '\n';
                buffer[x + 3] = 0;
                f.Write(buffer, strlen(buffer));
            }

            fileSystem.CloseFile(f);
        }
        public void Clear()
        {
            for (var i = 0; i < CON_TEXTSIZE; i++)
                text[i] = (string.ColorIndex(G.C_COLOR_CYAN) << 8) | ' ';
            Bottom();       // go to end
        }

        public override void SaveHistory()
        {
            var f = fileSystem.OpenFileWrite("consolehistory.dat");
            for (int i = 0; i < COMMAND_HISTORY; ++i)
            {
                // make sure the history is in the right order
                int line = (nextHistoryLine + i) % COMMAND_HISTORY;
                string s = historyEditLines[line].GetBuffer();
                if (s && s[0])
                {
                    f.WriteString(s);
                }
            }
            fileSystem.CloseFile(f);
        }
        public override void LoadHistory()
        {
            var f = fileSystem.OpenFileRead("consolehistory.dat");
            if (f == null) // file doesn't exist
                return;

            historyLine = 0;
            idStr tmp;
            for (int i = 0; i < COMMAND_HISTORY; ++i)
            {
                if (f.Tell() >= f.Length())
                {
                    break; // EOF is reached
                }
                f.ReadString(tmp);
                historyEditLines[i].SetBuffer(tmp.c_str());
                ++historyLine;
            }
            nextHistoryLine = historyLine;
            fileSystem.CloseFile(f);
        }

        public Material charSetShader;

        /// <summary>
        /// Handles history and console scrollback
        /// </summary>
        /// <param name="key">The key.</param>
        void KeyDownEvent(int key)
        {
            // Execute F key bindings
            if (key >= K_F1 && key <= K_F12)
            {
                KeyInput.ExecKeyBinding(key);
                return;
            }

            // ctrl-L clears screen
            if (key == 'l' && KeyInput.IsDown(K_CTRL))
            {
                Clear();
                return;
            }

            // enter finishes the line
            if (key == K_ENTER || key == K_KP_ENTER)
            {
                G.common.Printf("]%s\n", consoleField.GetBuffer());

                G.cmdSystem.BufferCommandText(CMD_EXEC.APPEND, consoleField.GetBuffer());    // valid command
                G.cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "\n");

                // copy line to history buffer, if it isn't the same as the last command
                if (idStr::Cmp(consoleField.GetBuffer(), historyEditLines[(nextHistoryLine + COMMAND_HISTORY - 1) % COMMAND_HISTORY].GetBuffer()) != 0)
                {
                    historyEditLines[nextHistoryLine % COMMAND_HISTORY] = consoleField;
                    nextHistoryLine++;
                }

                historyLine = nextHistoryLine;
                // clear the next line from old garbage, else the oldest history entry turns up when pressing DOWN
                historyEditLines[nextHistoryLine % COMMAND_HISTORY].Clear();

                consoleField.Clear();
                consoleField.SetWidthInChars(LINE_WIDTH);

                G.session.UpdateScreen(); // force an update, because the command may take some time
                return;
            }

            // command completion

            if (key == K_TAB)
            {
                consoleField.AutoComplete();
                return;
            }

            // command history (ctrl-p ctrl-n for unix style)

            if ((key == K_UPARROW) || ((char.ToLowerInvariant((char)key) == 'p') && KeyInput.IsDown(K_CTRL)))
            {
                if (nextHistoryLine - historyLine < COMMAND_HISTORY && historyLine > 0)
                    historyLine--;
                consoleField = historyEditLines[historyLine % COMMAND_HISTORY];
                return;
            }

            if ((key == K_DOWNARROW) || ((char.ToLowerInvariant((char)key) == 'n') && KeyInput.IsDown(K_CTRL)))
            {
                if (historyLine == nextHistoryLine)
                    return;
                historyLine++;
                consoleField = historyEditLines[historyLine % COMMAND_HISTORY];
                return;
            }

            // console scrolling
            if (key == K_PGUP)
            {
                PageUp();
                lastKeyEvent = G.eventLoop.Milliseconds();
                nextKeyEvent = CONSOLE_FIRSTREPEAT;
                return;
            }

            if (key == K_PGDN)
            {
                PageDown();
                lastKeyEvent = G.eventLoop.Milliseconds();
                nextKeyEvent = CONSOLE_FIRSTREPEAT;
                return;
            }

            if (key == K_MWHEELUP)
            {
                PageUp();
                return;
            }

            if (key == K_MWHEELDOWN)
            {
                PageDown();
                return;
            }

            // ctrl-home = top of console
            if (key == K_HOME && KeyInput.IsDown(K_CTRL))
            {
                Top();
                return;
            }

            // ctrl-end = bottom of console
            if (key == K_END && KeyInput.IsDown(K_CTRL))
            {
                Bottom();
                return;
            }

            // pass to the normal editline routine
            consoleField.KeyDownEvent(key);
        }

        void Linefeed()
        {
            int i;

            // mark time for transparent overlay
            if (current >= 0)
                times[current % NUM_CON_TIMES] = com_frameTime;

            x = 0;
            if (display == current)
                display++;
            current++;
            for (i = 0; i < LINE_WIDTH; i++)
                text[(current % TOTAL_LINES) * LINE_WIDTH + i] = (idStr::ColorIndex(C_COLOR_CYAN) << 8) | ' ';
        }

        void PageUp()
        {
            display -= 2;
            if (current - display >= TOTAL_LINES)
                display = current - TOTAL_LINES + 1;
        }
        void PageDown()
        {
            display += 2;
            if (display > current)
                display = current;
        }
        void Top() => display = 0;
        void Bottom() => display = current;

        /// <summary>
        /// Draw the editline after a ] prompt
        /// </summary>
        void DrawInput()
        {
            int y, autoCompleteLength;

            y = vislines - (SMALLCHAR_HEIGHT * 2);

            if (consoleField.GetAutoCompleteLength() != 0)
            {
                autoCompleteLength = strlen(consoleField.GetBuffer()) - consoleField.GetAutoCompleteLength();

                if (autoCompleteLength > 0)
                {
                    renderSystem.SetColor4(.8f, .2f, .2f, .45f);

                    renderSystem.DrawStretchPic(2 * SMALLCHAR_WIDTH + consoleField.GetAutoCompleteLength() * SMALLCHAR_WIDTH,
                                    y + 2, autoCompleteLength * SMALLCHAR_WIDTH, SMALLCHAR_HEIGHT - 2, 0, 0, 0, 0, whiteShader);

                }
            }

            renderSystem.SetColor(idStr::ColorForIndex(G.C_COLOR_CYAN));

            renderSystem.DrawSmallChar(1 * SMALLCHAR_WIDTH, y, ']', localConsole.charSetShader);

            consoleField.Draw(2 * SMALLCHAR_WIDTH, y, SCREEN_WIDTH - 3 * SMALLCHAR_WIDTH, true, charSetShader);
        }
        /// <summary>
        /// Draws the last few lines of output transparently over the game top
        /// </summary>
        void DrawNotify()
        {
            int x, v;
            short* text_p;
            int i;
            int time;
            int currentColor;

            if (con_noPrint.GetBool())
            {
                return;
            }

            currentColor = idStr::ColorIndex(G.C_COLOR_WHITE);
            renderSystem.SetColor(idStr::ColorForIndex(currentColor));

            v = 0;
            for (i = current - NUM_CON_TIMES + 1; i <= current; i++)
            {
                if (i < 0)
                {
                    continue;
                }
                time = times[i % NUM_CON_TIMES];
                if (time == 0)
                {
                    continue;
                }
                time = com_frameTime - time;
                if (time > con_notifyTime.Float * 1000)
                {
                    continue;
                }
                text_p = text + (i % TOTAL_LINES) * LINE_WIDTH;

                for (x = 0; x < LINE_WIDTH; x++)
                {
                    if ((text_p[x] & 0xff) == ' ')
                    {
                        continue;
                    }
                    if (idStr::ColorIndex(text_p[x] >> 8) != currentColor)
                    {
                        currentColor = idStr::ColorIndex(text_p[x] >> 8);
                        renderSystem.SetColor(idStr::ColorForIndex(currentColor));
                    }
                    renderSystem.DrawSmallChar((x + 1) * SMALLCHAR_WIDTH, v, text_p[x] & 0xff, localConsole.charSetShader);
                }

                v += SMALLCHAR_HEIGHT;
            }

            renderSystem.SetColor(colorCyan);
        }
        /// <summary>
        /// Draws the console with the solid background
        /// </summary>
        /// <param name="frac">The frac.</param>
        void DrawSolidConsole(float frac)
        {
            int i, x;
            float y;
            int rows;
            short* text_p;
            int row;
            int lines;
            int currentColor;

            lines = idMath::FtoiFast(SCREEN_HEIGHT * frac);
            if (lines <= 0)
            {
                return;
            }

            if (lines > SCREEN_HEIGHT)
            {
                lines = SCREEN_HEIGHT;
            }

            // draw the background
            y = frac * SCREEN_HEIGHT - 2;
            if (y < 1.0f)
            {
                y = 0.0f;
            }
            else
            {
                renderSystem.DrawStretchPic(0, 0, SCREEN_WIDTH, y, 0, 1.0f - displayFrac, 1, 1, consoleShader);
            }

            renderSystem.SetColor(colorCyan);
            renderSystem.DrawStretchPic(0, y, SCREEN_WIDTH, 2, 0, 0, 0, 0, whiteShader);
            renderSystem.SetColor(colorWhite);

            // draw the version number

            renderSystem.SetColor(idStr::ColorForIndex(C_COLOR_CYAN));

            idStr version = va("%s.%i", ENGINE_VERSION, BUILD_NUMBER);
            i = version.Length();

            for (x = 0; x < i; x++)
            {
                renderSystem.DrawSmallChar(SCREEN_WIDTH - (i - x) * SMALLCHAR_WIDTH,
                    (lines - (SMALLCHAR_HEIGHT + SMALLCHAR_HEIGHT / 2)), version[x], localConsole.charSetShader);

            }


            // draw the text
            vislines = lines;
            rows = (lines - SMALLCHAR_WIDTH) / SMALLCHAR_WIDTH;     // rows of text to draw

            y = lines - (SMALLCHAR_HEIGHT * 3);

            // draw from the bottom up
            if (display != current)
            {
                // draw arrows to show the buffer is backscrolled
                renderSystem.SetColor(idStr::ColorForIndex(C_COLOR_CYAN));
                for (x = 0; x < LINE_WIDTH; x += 4)
                {
                    renderSystem.DrawSmallChar((x + 1) * SMALLCHAR_WIDTH, idMath::FtoiFast(y), '^', localConsole.charSetShader);
                }
                y -= SMALLCHAR_HEIGHT;
                rows--;
            }

            row = display;

            if (x == 0)
            {
                row--;
            }

            currentColor = idStr::ColorIndex(C_COLOR_WHITE);
            renderSystem.SetColor(idStr::ColorForIndex(currentColor));

            for (i = 0; i < rows; i++, y -= SMALLCHAR_HEIGHT, row--)
            {
                if (row < 0)
                {
                    break;
                }
                if (current - row >= TOTAL_LINES)
                {
                    // past scrollback wrap point
                    continue;
                }

                text_p = text + (row % TOTAL_LINES) * LINE_WIDTH;

                for (x = 0; x < LINE_WIDTH; x++)
                {
                    if ((text_p[x] & 0xff) == ' ')
                    {
                        continue;
                    }

                    if (idStr::ColorIndex(text_p[x] >> 8) != currentColor)
                    {
                        currentColor = idStr::ColorIndex(text_p[x] >> 8);
                        renderSystem.SetColor(idStr::ColorForIndex(currentColor));
                    }
                    renderSystem.DrawSmallChar((x + 1) * SMALLCHAR_WIDTH, idMath::FtoiFast(y), text_p[x] & 0xff, localConsole.charSetShader);
                }
            }

            // draw the input prompt, user text, and cursor if desired
            DrawInput();

            renderSystem.SetColor(colorCyan);
        }

        /// <summary>
        /// deals with scrolling text because we don't have key repeat
        /// </summary>
        void Scroll()
        {
            if (lastKeyEvent == -1 || (lastKeyEvent + 200) > G.eventLoop.Milliseconds())
                return;
            // console scrolling
            if (KeyInput.IsDown(K_PGUP))
            {
                PageUp();
                nextKeyEvent = CONSOLE_REPEAT;
                return;
            }

            if (KeyInput.IsDown(K_PGDN))
            {
                PageDown();
                nextKeyEvent = CONSOLE_REPEAT;
                return;
            }
        }
        /// <summary>
        /// Causes the console to start opening the desired amount.
        /// </summary>
        /// <param name="frac">The frac.</param>
        void SetDisplayFraction(float frac)
        {
            finalFrac = frac;
            fracTime = com_frameTime;
        }
        /// <summary>
        /// Scrolls the console up or down based on conspeed
        /// </summary>
        void UpdateDisplayFraction()
        {
            if (con_speed.GetFloat() <= 0.1f)
            {
                fracTime = com_frameTime;
                displayFrac = finalFrac;
                return;
            }

            // scroll towards the destination height
            if (finalFrac < displayFrac)
            {
                displayFrac -= con_speed.GetFloat() * (com_frameTime - fracTime) * 0.001f;
                if (finalFrac > displayFrac)
                {
                    displayFrac = finalFrac;
                }
                fracTime = com_frameTime;
            }
            else if (finalFrac > displayFrac)
            {
                displayFrac += con_speed.GetFloat() * (com_frameTime - fracTime) * 0.001f;
                if (finalFrac < displayFrac)
                {
                    displayFrac = finalFrac;
                }
                fracTime = com_frameTime;
            }
        }

        //============================

        bool keyCatching;

        short[] text = new short[CON_TEXTSIZE];
        int current;        // line where next message will be printed
        int x;              // offset in current line for next print
        int display;        // bottom of console displays this line
        int lastKeyEvent;   // time of last key event for scroll delay
        int nextKeyEvent;   // keyboard repeat rate

        float displayFrac;  // approaches finalFrac at scr_conspeed
        float finalFrac;        // 0.0 to 1.0 lines of console to display
        int fracTime;       // time of last displayFrac update

        int vislines;       // in scanlines

        int[] times = new int[NUM_CON_TIMES];   // cls.realtime time the line was generated
                                                // for transparent notify lines
        idVec4 color;

        EditField[] historyEditLines = new EditField[COMMAND_HISTORY];

        int nextHistoryLine;// the last line in the history buffer, not masked
        int historyLine;    // the line being displayed from history buffer
                            // will be <= nextHistoryLine

        EditField consoleField;

        static CVar con_speed = new("con_speed", "3", CVAR.SYSTEM, "speed at which the console moves up and down");
        static CVar con_notifyTime = new("con_notifyTime", "3", CVAR.SYSTEM, "time messages are displayed onscreen when console is pulled up");
#if DEBUG
        static CVar con_noPrint = new("con_noPrint", "0", CVAR.BOOL | CVAR.SYSTEM | CVAR.NOCHEAT, "print on the console but not onscreen when console is pulled up");
#else
        static CVar con_noPrint = new("con_noPrint", "1", CVAR.BOOL | CVAR.SYSTEM | CVAR.NOCHEAT, "print on the console but not onscreen when console is pulled up");
#endif

        Material whiteShader;
        Material consoleShader;
    }
}
