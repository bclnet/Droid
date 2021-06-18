using System;

namespace Droid.Core
{
    // command flags
    [Flags]
    public enum CMD_FL
    {
        ALL = -1,
        CHEAT = 1 << 0,    // command is considered a cheat
        SYSTEM = 1 << 1,   // system command
        RENDERER = 1 << 2, // renderer command
        SOUND = 1 << 3,    // sound command
        GAME = 1 << 4, // game command
        TOOL = 1 << 5, // tool command
    }

    // parameters for command buffer stuffing
    public enum CMD_EXEC
    {
        NOW,                        // don't return until completed
        INSERT,                 // insert at current position, but don't run yet
        APPEND                      // add to end of the command buffer (normal case)
    }

    // command function
    public delegate void CmdFunction(CmdArgs args);

    public interface ICmdSystem
    {
        // Registers a command and the function to call for it.
        void AddCommand(string cmdName, CmdFunction function, CMD_FL flags, string description, ArgCompletion argCompletion = null);
        // Removes a command.
        void RemoveCommand(string cmdName);
        // Remove all commands with one of the flags set.
        void RemoveFlaggedCommands(CMD_FL flags);

        // Command and argument completion using callback for each valid string.
        void CommandCompletion(Action<string> callback);
        void ArgCompletion(string cmdString, Action<string> callback);

        // Adds command text to the command buffer, does not add a final \n
        void BufferCommandText(CMD_EXEC exec, string text);
        // Pulls off \n \r or ; terminated lines of text from the command buffer and
        // executes the commands. Stops when the buffer is empty.
        // Normally called once per frame, but may be explicitly invoked.
        void ExecuteCommandBuffer();

        // Base for path/file auto-completion.
        void ArgCompletion_FolderExtension(CmdArgs args, Action<string> callback, string folder, bool stripFolder, params string[] extensions);
        // Base for decl name auto-completion.
        void ArgCompletion_DeclName(CmdArgs args, Action<string> callback, int type);

        // Adds to the command buffer in tokenized form ( CMD_EXEC_NOW or CMD_EXEC_APPEND only )
        void BufferCommandArgs(CMD_EXEC exec, CmdArgs args);

        // Setup a reloadEngine to happen on next command run, and give a command to execute after reload
        void SetupReloadEngine(CmdArgs args);
        bool PostReloadEngine();
    }
}