using Droid.Core;
using System.Text;

namespace Droid.Framework
{
    public class CmdArgs
    {
        public CmdArgs() { argc = 0; }
        public CmdArgs(string text, bool keepAsStrings) => TokenizeString(text, keepAsStrings);

        /// <summary>
        /// The functions that execute commands get their parameters with these functions.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => argc;

        /// <summary>
        /// Argv() will return an empty string, not NULL if arg >= argc.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public string this[int arg] => arg >= 0 && arg < argc ? argv[arg] : string.Empty;

        /// <summary>
        /// Returns a single string containing argv(start) to argv(end) escapeArgs is a fugly way to put the string back into a state ready to tokenize again
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="escapeArgs">if set to <c>true</c> [escape arguments].</param>
        /// <returns></returns>
        public string Args(int start = 1, int end = -1, bool escapeArgs = false)
        {
            if (end < 0) end = argc - 1;
            else if (end >= argc) end = argc - 1;

            var b = new StringBuilder();
            if (escapeArgs) b.Append('"');
            for (var i = start; i <= end; i++)
            {
                if (i > start) b.Append(escapeArgs ? "\" \"" : " ");
                var buf = argv[i];
                if (escapeArgs && buf.Contains('\\'))
                {
                    var p = 0;
                    while (buf[p] != '\0') { b.Append(buf[p] != '\\' ? buf[p] : "\\\\"); p++; }
                }
                else b.Append(buf);
            }
            if (escapeArgs) b.Append('"');
            return b.ToString();
        }

        /// <summary>
        /// Takes a null terminated string and breaks the string up into arg tokens.
        /// Does not need to be /n terminated.
        /// Set keepAsStrings to true to only seperate tokens from whitespace and comments, ignoring punctuation
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="keepAsStrings">if set to <c>true</c> [keep as strings].</param>
        public void TokenizeString(string text, bool keepAsStrings)
        {
            var lex = new Lexer();

            // clear previous args
            argc = 0;

            if (text == null)
                return;

            lex.LoadMemory(text, text.Length, "CmdSystemLocal::TokenizeString");
            lex.Flags = LEXFL.NOERRORS
                        | LEXFL.NOWARNINGS
                        | LEXFL.NOSTRINGCONCAT
                        | LEXFL.ALLOWPATHNAMES
                        | LEXFL.NOSTRINGESCAPECHARS
                        | LEXFL.ALLOWIPADDRESSES | (keepAsStrings ? LEXFL.ONLYSTRINGS : 0);

            while (true)
            {
                if (argc == MAX_COMMAND_ARGS)
                    return; // this is usually something malicious

                if (!lex.ReadToken(out var token))
                    return;

                // check for negative numbers
                if (!keepAsStrings && token == "-" && lex.CheckTokenType(TT.NUMBER, 0, out var number))
                    token = "-" + number;

                // check for cvar expansion
                if (token == "$")
                {
                    if (!lex.ReadToken(out token))
                        return;
                    token = G.cvarSystem != null ? G.cvarSystem.GetCVarString(token) : "<unknown>";
                }

                // add token
                argv[argc] = token;
                argc++;
            }
        }

        public void AppendArg(string text) => argv[argc++] = text;

        public void Clear() => argc = 0;

        public string[] GetArgs(out int argc)
        {
            argc = this.argc;
            return argv;
        }

        const int MAX_COMMAND_ARGS = 64;

        int argc; // number of arguments
        string[] argv = new string[MAX_COMMAND_ARGS]; // points into tokenized
    }
}