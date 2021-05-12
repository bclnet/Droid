using System;
using System.Collections.Generic;

namespace Droid.Framework
{
    class CommandDef
    {
        public CommandDef next;
        public string name;
        public CmdFunction function;
        public ArgCompletion argCompletion;
        public CMD_FL flags;
        public string description;
    }

    class CmdSystemLocal : CmdSystem
    {
        public CmdSystemLocal()
        {
            AddCommand("listCmds", List_f, CMD_FL.SYSTEM, "lists commands");
            AddCommand("listSystemCmds", SystemList_f, CMD_FL.SYSTEM, "lists system commands");
            AddCommand("listRendererCmds", RendererList_f, CMD_FL.SYSTEM, "lists renderer commands");
            AddCommand("listSoundCmds", SoundList_f, CMD_FL.SYSTEM, "lists sound commands");
            AddCommand("listGameCmds", GameList_f, CMD_FL.SYSTEM, "lists game commands");
            AddCommand("listToolCmds", ToolList_f, CMD_FL.SYSTEM, "lists tool commands");
            AddCommand("exec", Exec_f, CMD_FL.SYSTEM, "executes a config file", ArgCompletion_ConfigName);
            AddCommand("vstr", Vstr_f, CMD_FL.SYSTEM, "inserts the current value of a cvar as command text");
            AddCommand("echo", Echo_f, CMD_FL.SYSTEM, "prints text");
            AddCommand("parse", Parse_f, CMD_FL.SYSTEM, "prints tokenized string");
            AddCommand("wait", Wait_f, CMD_FL.SYSTEM, "delays remaining buffered commands one or more frames");

            completionString = "*";

            textLength = 0;
        }

        public void Dispose() { }

        public override void AddCommand(string cmdName, CmdFunction function, CMD_FL flags, string description, ArgCompletion argCompletion = null)
        {
            CommandDef cmd;

            // fail if the command already exists
            for (cmd = commands; cmd != null; cmd = cmd.next)
                if (cmdName == cmd.name && function != cmd.function)
                {
                    G.common.Printf($"CmdSystemLocal::AddCommand: {cmdName} already defined\n");
                    return;
                }

            cmd = new CommandDef
            {
                name = cmdName,
                function = function,
                argCompletion = argCompletion,
                flags = flags,
                description = description,
                next = commands
            };
            commands = cmd;
        }
        public override void RemoveCommand(string cmdName)
        {
            CommandDef cmd; ref CommandDef last = ref commands;
            for (cmd = last; cmd != null; cmd = last)
            {
                if (cmdName == cmd.name)
                {
                    last = cmd.next;
                    return;
                }
                last = cmd.next;
            }
        }
        public override void RemoveFlaggedCommands(CMD_FL flags)
        {
            CommandDef cmd; ref CommandDef last = ref commands;
            for (cmd = last; cmd != null; cmd = last)
            {
                if ((cmd.flags & flags) != 0)
                {
                    last = cmd.next;
                    continue;
                }
                last = cmd.next;
            }
        }

        public override void CommandCompletion(Action<string> callback)
        {
            CommandDef cmd;
            for (cmd = commands; cmd != null; cmd = cmd.next)
                callback(cmd.name);
        }
        public override void ArgCompletion(string cmdString, Action<string> callback)
        {
            CommandDef cmd;
            CmdArgs args;

            args.TokenizeString(cmdString, false);

            for (cmd = commands; cmd != null; cmd = cmd.next)
            {
                if (cmd.argCompletion == null)
                    continue;
                if (args[0] == cmd.name)
                {
                    cmd.argCompletion(args, callback);
                    break;
                }
            }
        }

        public override void BufferCommandText(CMD_EXEC exec, string text)
        {
            switch (exec)
            {
                case CMD_EXEC.NOW: ExecuteCommandText(text); break;
                case CMD_EXEC.INSERT: InsertCommandText(text); break;
                case CMD_EXEC.APPEND: AppendCommandText(text); break;
                default: G.common.FatalError("CmdSystemLocal::BufferCommandText: bad exec type"); break;
            }
        }
        public override void ExecuteCommandBuffer()
        {
            int i;
            string text;
            int quotes;
            var args = new CmdArgs();

            while (textLength != 0)
            {

                if (wait != 0)
                {
                    // skip out while text still remains in buffer, leaving it for next frame
                    wait--;
                    break;
                }

                // find a \n or ; line break
                text = textBuf;

                quotes = 0;
                for (i = 0; i < textLength; i++)
                {
                    if (text[i] == '"') quotes++;
                    if ((quotes & 1) == 0 && text[i] == ';') break;  // don't break if inside a quoted string
                    if (text[i] == '\n' || text[i] == '\r') break;
                }

                text[i] = 0;

                if (text == "_execTokenized")
                {
                    args = tokenizedCmds[0];
                    tokenizedCmds.RemoveAt(0);
                }
                else args.TokenizeString(text, false);

                // delete the text from the command buffer and move remaining commands down
                // this is necessary because commands (exec) can insert data at the
                // beginning of the text buffer

                if (i == textLength)
                    textLength = 0;
                else
                {
                    i++;
                    textLength -= i;
                    memmove(text, text + i, textLength);
                }

                // execute the command line that we have already tokenized
                ExecuteTokenizedString(args);
            }
        }

        public override void ArgCompletion_FolderExtension(CmdArgs args, Action<string> callback, string folder, bool stripFolder, params string[] extensions)
        {
            int i;
            string extension;

            var s = $"{args[0]} {args[1]}";

            if (!string.Equals(s, completionString, StringComparison.OrdinalIgnoreCase))
            {
                string parm, path;
                FileList names;

                completionString = s;
                completionParms.Clear();

                parm = args[1];
                parm.ExtractFilePath(path);
                if (stripFolder || path.Length == 0)
                    path = folder + path;
                path.TrimEnd('/');

                // list folders
                names = G.fileSystem.ListFiles(path, "/", true, true);
                for (i = 0; i < names.GetNumFiles(); i++)
                {
                    var name = names.GetFile(i);
                    name.Strip(stripFolder ? folder : "/");
                    name = $"{args.Argv(0)} {name}/";
                    completionParms.Add(name);
                }
                G.fileSystem.FreeFileList(names);

                // list files
                va_start(argPtr, stripFolder);
                for (extension = va_arg(argPtr, string); extension; extension = va_arg(argPtr, string))
                {
                    names = fileSystem.ListFiles(path, extension, true, true);
                    for (i = 0; i < names.GetNumFiles(); i++)
                    {
                        idStr name = names.GetFile(i);
                        if (stripFolder)
                        {
                            name.Strip(folder);
                        }
                        else
                        {
                            name.Strip("/");
                        }
                        name = args.Argv(0) + (" " + name);
                        completionParms.Append(name);
                    }
                    fileSystem.FreeFileList(names);
                }
                va_end(argPtr);
            }
            for (i = 0; i < completionParms.Count; i++)
                callback(completionParms[i]);
        }
        public override void ArgCompletion_DeclName(CmdArgs args, Action<string> callback, int type)
        {
            int i, num;
            if (declManager == null)
                return;
            num = declManager.GetNumDecls((declType_t)type);
            for (i = 0; i < num; i++)
                callback($"{args.Argv(0)} {declManager.DeclByIndex((declType_t)type, i, false).GetName()}");
        }

        public override void BufferCommandArgs(CMD_EXEC exec, CmdArgs args)
        {
            switch (exec)
            {
                case CMD_EXEC.NOW: ExecuteTokenizedString(args); break;
                case CMD_EXEC.APPEND: AppendCommandText("_execTokenized\n"); tokenizedCmds.Add(args); break;
                default: G.common.FatalError("CmdSystemLocal::BufferCommandArgs: bad exec type"); break;
            }
        }

        public override void SetupReloadEngine(CmdArgs args)
        {
            BufferCommandText(CMD_EXEC.APPEND, "reloadEngine\n");
            postReload = args;
        }

        public override bool PostReloadEngine()
        {
            if (postReload.Count == 0)
                return false;
            BufferCommandArgs(CMD_EXEC.APPEND, postReload);
            postReload.Clear();
            return true;
        }

        public void SetWait(int numFrames) => wait = numFrames;
        public CommandDef Commands => commands;

        const int MAX_CMD_BUFFER = 0x10000;

        CommandDef commands;

        int wait;
        int textLength;
        byte[] textBuf = new byte[MAX_CMD_BUFFER];

        string completionString;
        List<string> completionParms = new List<string>();

        // piggybacks on the text buffer, avoids tokenize again and screwing it up
        List<CmdArgs> tokenizedCmds;

        // a command stored to be executed after a reloadEngine and all associated commands have been processed
        CmdArgs postReload;

        void ExecuteTokenizedString(CmdArgs args)
        {
            CommandDef cmd; ref CommandDef prev = ref commands;

            // execute the command line
            if (args.Count == 0)
                return;     // no tokens

            // check registered command functions
            for (; prev != null; prev = cmd.next)
            {
                cmd = prev;
                if (string.Equals(args[0], cmd.name, StringComparison.OrdinalIgnoreCase))
                {
                    // rearrange the links so that the command will be near the head of the list next time it is used
                    prev = cmd.next;
                    cmd.next = commands;
                    commands = cmd;

                    if ((cmd.flags & (CMD_FL.CHEAT | CMD_FL.TOOL)) != 0 && G.session != null && G.session.IsMultiplayer() && !G.cvarSystem.GetCVarBool("net_allowCheats"))
                    {
                        G.common.Printf("Command '%s' not valid in multiplayer mode.\n", cmd.name);
                        return;
                    }
                    // perform the action
                    if (cmd.function == null)
                        break;
                    cmd.function(args);
                    return;
                }
            }

            // check cvars
            if (G.cvarSystem.Command(args))
                return;

            G.common.Printf($"Unknown command '{args[0]}'\n");
        }

        void ExecuteCommandText(string text) => ExecuteTokenizedString(new CmdArgs(text, false));

        void InsertCommandText(string text)
        {
            int len;
            int i;

            len = text.Length + 1;
            if (len + textLength > (int)sizeof(textBuf))
            {
                G.common.Printf("CmdSystemLocal::InsertText: buffer overflow\n");
                return;
            }

            // move the existing command text
            for (i = textLength - 1; i >= 0; i--)
            {
                textBuf[i + len] = textBuf[i];
            }

            // copy the new text in
            memcpy(textBuf, text, len - 1);

            // add a \n
            textBuf[len - 1] = '\n';

            textLength += len;
        }

        void AppendCommandText(string text)
        {
            int l;

            l = strlen(text);

            if (textLength + l >= (int)sizeof(textBuf))
            {
                G.common.Printf("CmdSystemLocal::AppendText: buffer overflow\n");
                return;
            }
            memcpy(textBuf + textLength, text, l);
            textLength += l;
        }

        // NOTE: the const wonkyness is required to make msvc happy
        //    template<>
        //    ID_INLINE int idListSortCompare(const commandDef_t* const* a, const commandDef_t* const* b)
        //{
        //    return idStr::Icmp((* a).name, (* b).name);
        //}

        static void ListByFlags(CmdArgs args, CMD_FL flags)
        {
            string match;
            if (args.Count > 1)
            {
                match = args[1, -1];
                match.Replace(" ", "");
            }
            else
                match = string.Empty;

            CommandDef cmd;
            var cmdList = new List<CommandDef>();
            for (cmd = G.cmdSystemLocal.Commands; cmd != null; cmd = cmd.next)
            {
                if ((cmd.flags & flags) == 0)
                    continue;
                if (match.Length != 0 && cmd.name.Filter(match, StringComparison.OrdinalIgnoreCase))
                    continue;

                cmdList.Add(cmd);
            }

            cmdList.Sort();

            for (var i = 0; i < cmdList.Count; i++)
            {
                cmd = cmdList[i];

                G.common.Printf($"  {cmd.name:-21} {cmd.description}\n");
            }

            G.common.Printf($"{cmdList.Count} commands\n");
        }
        static void List_f(CmdArgs args) => ListByFlags(args, CMD_FL.ALL);
        static void SystemList_f(CmdArgs args) => ListByFlags(args, CMD_FL.SYSTEM);
        static void RendererList_f(CmdArgs args) => ListByFlags(args, CMD_FL.RENDERER);
        static void SoundList_f(CmdArgs args) => ListByFlags(args, CMD_FL.SOUND);
        static void GameList_f(CmdArgs args) => ListByFlags(args, CMD_FL.GAME);
        static void ToolList_f(CmdArgs args) => ListByFlags(args, CMD_FL.TOOL);

        static void Exec_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                G.common.Printf("exec <filename> : execute a script file\n");
                return;
            }

            var filename = args[1];
            filename.DefaultFileExtension(".cfg");
            byte[] f;
            G.fileSystem.ReadFile(filename, out f, null);
            if (f == null)
            {
                G.common.Printf($"couldn't exec {args[1]}\n");
                return;
            }
            G.common.Printf($"execing {args[1]}\n");

            G.cmdSystemLocal.BufferCommandText(CMD_EXEC.INSERT, f);

            G.fileSystem.FreeFile(f);
        }
        static void Vstr_f(CmdArgs args)
        {
            if (args.Count != 2)
            {
                G.common.Printf("vstr <variablename> : execute a variable command\n");
                return;
            }

            var v = G.cvarSystem.GetCVarString(args[1]);

            G.cmdSystemLocal.BufferCommandText(CMD_EXEC.APPEND, $"{v}\n");
        }
        static void Echo_f(CmdArgs args)
        {
            for (var i = 1; i < args.Count; i++)
                G.common.Printf($"{args[i]} ");
            G.common.Printf("\n");
        }
        static void Parse_f(CmdArgs args)
        {
            for (var i = 0; i < args.Count; i++)
                G.common.Printf($"{i}: {args[i]}\n");
        }
        static void Wait_f(CmdArgs args) => G.cmdSystemLocal.SetWait(args.Count == 2 ? int.TryParse(args[1], out var z) ? z : 1 : 1);
        static void PrintMemInfo_f(CmdArgs args) { }
    }

}