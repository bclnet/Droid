using Droid.Sys;
using System.Diagnostics;
using System.Text;
using static Droid.Core.Lib;
using static Droid.K;

namespace Droid.Core
{
    struct AutoComplete
    {
        public bool valid;
        public int length;
        public string completionString;
        public string currentMatch;
        public int matchCount;
        public int matchIndex;
        public int findMatchIndex;
    }

    public class EditField
    {
        const int MAX_EDIT_LINE = 256;

        int cursor;
        int scroll;
        int widthInChars;
        StringBuilder buffer = new StringBuilder();
        AutoComplete autoComplete;

        public EditField()
        {
            widthInChars = 0;
            Clear();
        }

        public void Clear()
        {
            buffer.Clear();
            cursor = 0;
            scroll = 0;
            autoComplete.length = 0;
            autoComplete.valid = false;
        }

        public void SetWidthInChars(int w)
        {
            Debug.Assert(w <= MAX_EDIT_LINE);
            widthInChars = w;
        }

        public int Cursor
        {
            get => cursor;
            set
            {
                Debug.Assert(value <= MAX_EDIT_LINE);
                cursor = value;
            }
        }

        public void ClearAutoComplete()
        {
            if (autoComplete.length > 0 && autoComplete.length <= buffer.Length)
            {
                buffer.Clear();
                if (cursor > autoComplete.length)
                    cursor = autoComplete.length;
            }
            autoComplete.length = 0;
            autoComplete.valid = false;
        }

        public int AutoCompleteLength
            => autoComplete.length;

        public void AutoComplete()
        {
            string completionArgString;
            CmdArgs args = new();

            if (!autoComplete.valid)
            {
                args.TokenizeString(buffer.ToString(), false);
                autoComplete.completionString = args[0];
                completionArgString = args.Args();
                autoComplete.matchCount = 0;
                autoComplete.matchIndex = 0;
                autoComplete.currentMatch = string.Empty;

                if (autoComplete.completionString.Length == 0)
                    return;

                globalAutoComplete = autoComplete;

                cmdSystem.CommandCompletion(FindMatches);
                cvarSystem.CommandCompletion(FindMatches);

                autoComplete = globalAutoComplete;

                if (autoComplete.matchCount == 0)
                    return; // no matches

                // when there's only one match or there's an argument
                buffer.Clear();
                if (autoComplete.matchCount == 1 || completionArgString.Length != 0)
                {
                    // try completing arguments
                    autoComplete.completionString += $" {completionArgString}";
                    autoComplete.matchCount = 0;

                    globalAutoComplete = autoComplete;

                    cmdSystem.ArgCompletion(autoComplete.completionString, FindMatches);
                    cvarSystem.ArgCompletion(autoComplete.completionString, FindMatches);

                    autoComplete = globalAutoComplete;

                    buffer.Append(autoComplete.currentMatch);

                    if (autoComplete.matchCount == 0)
                    {
                        // no argument matches
                        buffer.Append(" ");
                        buffer.Append(completionArgString);
                        Cursor = buffer.Length;
                        return;
                    }
                }
                else
                {
                    // multiple matches, complete to shortest
                    buffer.Append(autoComplete.currentMatch);
                    if (completionArgString.Length != 0)
                    {
                        buffer.Append(" ");
                        buffer.Append(completionArgString);
                    }
                }

                autoComplete.length = buffer.Length;
                autoComplete.valid = autoComplete.matchCount != 1;
                Cursor = autoComplete.length;

                common.Printf($"]{buffer}\n");

                // run through again, printing matches
                globalAutoComplete = autoComplete;

                cmdSystem.CommandCompletion(PrintMatches);
                cmdSystem.ArgCompletion(autoComplete.completionString, PrintMatches);
                cvarSystem.CommandCompletion(PrintCvarMatches);
                cvarSystem.ArgCompletion(autoComplete.completionString, PrintMatches);
            }
            else if (autoComplete.matchCount != 1)
            {
                // get the next match and show instead
                autoComplete.matchIndex++;
                if (autoComplete.matchIndex == autoComplete.matchCount)
                    autoComplete.matchIndex = 0;
                autoComplete.findMatchIndex = 0;

                globalAutoComplete = autoComplete;

                cmdSystem.CommandCompletion(FindIndexMatch);
                cmdSystem.ArgCompletion(autoComplete.completionString, FindIndexMatch);
                cvarSystem.CommandCompletion(FindIndexMatch);
                cvarSystem.ArgCompletion(autoComplete.completionString, FindIndexMatch);

                autoComplete = globalAutoComplete;

                // and print it
                buffer.Append(autoComplete.currentMatch);
                if (autoComplete.length > buffer.Length)
                    autoComplete.length = buffer.Length;
                Cursor = autoComplete.length;
            }
        }

        public void CharEvent(char c)
        {
            if (c == 'v' - 'a' + 1)
            {   // ctrl-v is paste
                Paste();
                return;
            }

            if (c == 'c' - 'a' + 1)
            {   // ctrl-c clears the field
                Clear();
                return;
            }

            var len = buffer.Length;

            if (c == 'h' - 'a' + 1 || c == (char)K_BACKSPACE)
            {   // ctrl-h is backspace
                if (cursor > 0)
                {
                    memmove(buffer + cursor - 1, buffer + cursor, len + 1 - cursor);
                    cursor--;
                    if (cursor < scroll)
                        scroll--;
                }
                return;
            }

            if (c == 'a' - 'a' + 1)
            {   // ctrl-a is home
                cursor = 0;
                scroll = 0;
                return;
            }

            if (c == 'e' - 'a' + 1)
            {   // ctrl-e is end
                cursor = len;
                scroll = cursor - widthInChars;
                return;
            }

            //
            // ignore any other non printable chars
            if (c < 32)
                return;

            if (KeyInput.GetOverstrikeMode())
            {
                if (cursor == MAX_EDIT_LINE - 1)
                    return;
                buffer[cursor] = c;
                cursor++;
            }
            else
            {   // insert mode
                if (len == MAX_EDIT_LINE - 1)
                    return; // all full
                memmove(buffer + cursor + 1, buffer + cursor, len + 1 - cursor);
                buffer[cursor] = c;
                cursor++;
            }

            if (cursor >= widthInChars)
                scroll++;

            if (cursor == len + 1)
                buffer[cursor] = '\0';
        }

        public void KeyDownEvent(K key)
        {
            int len;

            // shift-insert is paste
            if (((key == K_INS) || (key == K_KP_INS)) && idKeyInput::IsDown(K_SHIFT))
            {
                ClearAutoComplete();
                Paste();
                return;
            }

            len = strlen(buffer);

            if (key == K_DEL)
            {
                if (autoComplete.length)
                {
                    ClearAutoComplete();
                }
                else if (cursor < len)
                {
                    memmove(buffer + cursor, buffer + cursor + 1, len - cursor);
                }
                return;
            }

            if (key == K_RIGHTARROW)
            {
                if (idKeyInput::IsDown(K_CTRL))
                {
                    // skip to next word
                    while ((cursor < len) && (buffer[cursor] != ' '))
                    {
                        cursor++;
                    }

                    while ((cursor < len) && (buffer[cursor] == ' '))
                    {
                        cursor++;
                    }
                }
                else
                {
                    cursor++;
                }

                if (cursor > len)
                {
                    cursor = len;
                }

                if (cursor >= scroll + widthInChars)
                {
                    scroll = cursor - widthInChars + 1;
                }

                if (autoComplete.length > 0)
                {
                    autoComplete.length = cursor;
                }
                return;
            }

            if (key == K_LEFTARROW)
            {
                if (idKeyInput::IsDown(K_CTRL))
                {
                    // skip to previous word
                    while ((cursor > 0) && (buffer[cursor - 1] == ' '))
                    {
                        cursor--;
                    }

                    while ((cursor > 0) && (buffer[cursor - 1] != ' '))
                    {
                        cursor--;
                    }
                }
                else
                {
                    cursor--;
                }

                if (cursor < 0)
                {
                    cursor = 0;
                }
                if (cursor < scroll)
                {
                    scroll = cursor;
                }

                if (autoComplete.length)
                {
                    autoComplete.length = cursor;
                }
                return;
            }

            if (key == K_HOME || (tolower(key) == 'a' && idKeyInput::IsDown(K_CTRL)))
            {
                cursor = 0;
                scroll = 0;
                if (autoComplete.length)
                {
                    autoComplete.length = cursor;
                    autoComplete.valid = false;
                }
                return;
            }

            if (key == K_END || (tolower(key) == 'e' && idKeyInput::IsDown(K_CTRL)))
            {
                cursor = len;
                if (cursor >= scroll + widthInChars)
                {
                    scroll = cursor - widthInChars + 1;
                }
                if (autoComplete.length)
                {
                    autoComplete.length = cursor;
                    autoComplete.valid = false;
                }
                return;
            }

            if (key == K_INS)
            {
                idKeyInput::SetOverstrikeMode(!idKeyInput::GetOverstrikeMode());
                return;
            }

            // clear autocompletion buffer on normal key input
            if (key != K_CAPSLOCK && key != K_ALT && key != K_CTRL && key != K_SHIFT)
            {
                ClearAutoComplete();
            }
        }

        public void Paste()
        {
            var cbd = SysX.GetClipboardData();
            if (cbd == null)
                return;

            // send as if typed, so insert / overstrike works properly
            var pasteLen = cbd.Length;
            for (var i = 0; i < pasteLen; i++)
                CharEvent(cbd[i]);
        }

        public StringBuilder Buffer
            => buffer;

        public void SetBuffer(StringBuilder buf)
        {
            Clear();
            buffer.Append(buf);
            Cursor = buffer.Length;
        }

        public void Draw(int x, int y, int width, bool showCursor, object material)
        {
            //int len;
            //int drawLen;
            //int prestep;
            //int cursorChar;
            //char str[MAX_EDIT_LINE];
            //int size;

            //size = SMALLCHAR_WIDTH;

            //drawLen = widthInChars;
            //len = strlen(buffer) + 1;

            //// guarantee that cursor will be visible
            //if (len <= drawLen)
            //    prestep = 0;
            //else
            //{
            //    if (scroll + drawLen > len)
            //    {
            //        scroll = len - drawLen;
            //        if (scroll < 0)
            //            scroll = 0;
            //    }
            //    prestep = scroll;

            //    // Skip color code
            //    if (idStr::IsColor(buffer + prestep))
            //        prestep += 2;
            //    if (prestep > 0 && idStr::IsColor(buffer + prestep - 1))
            //        prestep++;
            //}

            //if (prestep + drawLen > len)
            //    drawLen = len - prestep;

            //// extract <drawLen> characters from the field at <prestep>
            //if (drawLen >= MAX_EDIT_LINE)
            //    common.Error("drawLen >= MAX_EDIT_LINE");

            //memcpy(str, buffer + prestep, drawLen);
            //str[drawLen] = 0;

            //// draw it
            //renderSystem.DrawSmallStringExt(x, y, str, colorWhite, false, shader);

            //// draw the cursor
            //if (!showCursor)
            //    return;

            //if ((int)(com_ticNumber >> 4) & 1)
            //    return;     // off blink

            //if (idKeyInput::GetOverstrikeMode())
            //    cursorChar = 11;
            //else
            //    cursorChar = 10;

            //// Move the cursor back to account for color codes
            //for (var i = 0; i < cursor; i++)
            //    if (idStr::IsColor(&str[i]))
            //    {
            //        i++;
            //        prestep += 2;
            //    }

            //renderSystem.DrawSmallChar(x + (cursor - prestep) * size, y, cursorChar, shader);
        }

        static AutoComplete globalAutoComplete;

        static void FindMatches(string s)
        {
            if (idStr::Icmpn(s, globalAutoComplete.completionString, strlen(globalAutoComplete.completionString)) != 0)
                return;
            globalAutoComplete.matchCount++;
            if (globalAutoComplete.matchCount == 1)
            {
                idStr::Copynz(globalAutoComplete.currentMatch, s, sizeof(globalAutoComplete.currentMatch));
                return;
            }

            // cut currentMatch to the amount common with s
            for (var i = 0; s[i]; i++)
                if (tolower(globalAutoComplete.currentMatch[i]) != tolower(s[i]))
                {
                    globalAutoComplete.currentMatch[i] = 0;
                    break;
                }
            globalAutoComplete.currentMatch[i] = 0;
        }

        static void FindIndexMatch(string s)
        {
            if (idStr::Icmpn(s, globalAutoComplete.completionString, strlen(globalAutoComplete.completionString)) != 0)
                return;

            if (globalAutoComplete.findMatchIndex == globalAutoComplete.matchIndex)
                idStr::Copynz(globalAutoComplete.currentMatch, s, sizeof(globalAutoComplete.currentMatch));

            globalAutoComplete.findMatchIndex++;
        }

        static void PrintMatches(string s)
        {
            if (idStr::Icmpn(s, globalAutoComplete.currentMatch, strlen(globalAutoComplete.currentMatch)) == 0)
                common.Printf("    %s\n", s);
        }

        static void PrintCvarMatches(string s)
        {
            if (idStr::Icmpn(s, globalAutoComplete.currentMatch, strlen(globalAutoComplete.currentMatch)) == 0)
                common.Printf("    %s" S_COLOR_WHITE " = \"%s\"\n", s, cvarSystem->GetCVarString(s));
        }
    }
}