using System;
using System.Dynamic;

namespace AngelMacro
{
    public partial class MainWindow
    {
        enum MACROSTATUS { RECORDING, RUNNING, IDLE }

        const string FILE_NAME = "macro";
        const string FILE_EXTENSION = "*.amacro";
        const string FILE_FILTER = $"AngelMacro file|{FILE_EXTENSION}";

        const int CSHARP_LAG_COMPENSATION = 0;

        const string TEXT_DELAY = "DELAY";
        const string TEXT_LOCATION = "LOCATION";
        const string TEXT_SCROLL_WHEEL = "SCROLL";
        const string TEXT_MOUSE_UP = "MOUSEUP";
        const string TEXT_MOUSE_DOWN = "MOUSEDOWN";
        const string TEXT_KEY_UP = "KEYUP";
        const string TEXT_KEY_DOWN = "KEYDOWN";
        const string TEXT_COLOR = "COLOR";
        const string TEXT_WHILE = "WHILE";
        const string TEXT_UNTIL = "UNTIL";
        const string TEXT_END = "END";
        const string TEXT_COLOR_THRESHOLD_CHANGE = "THRESHOLD";

        readonly string UNLOCKED_SCRIPT_WARNING = Properties.Resources.script_unlocked;
        readonly string OPEN_FILE_TITLE = Properties.Resources.open_macro;
        readonly string SAVE_FILE_TITLE = Properties.Resources.save_macro;
        const string GDI_START = "GO";
        const string GDI_SECOND = "s";
        readonly string COMMAND_ERROR_TEXT = Properties.Resources.command_error_text;
        readonly string COMMAND_ERROR_TITLE = Properties.Resources.command_error_title;

        readonly string COMPILE_CODE_BUTTON_TEXT = Properties.Resources.compile_code;
        readonly string RUN_MACRO_BUTTON_TEXT = Properties.Resources.run_macro;

        const char COMMAND_SEPARATOR = ';';
        const char ARGS_SEPARATOR = ':';
        const char COMMAND_BLOCK_STARTER = '{';
        const char COMMAND_BLOCK_CLOSER = '}';

        readonly string CONDITIONAL_MACRO_GUIDE = $"{COMMAND_BLOCK_STARTER}\n   Put your macro here\n{COMMAND_BLOCK_CLOSER}{COMMAND_BLOCK_STARTER}\n   Put your macro here if the condition fails\n{COMMAND_BLOCK_CLOSER}";
        readonly string WHILE_MACRO_GUIDE = $"{COMMAND_BLOCK_STARTER}   \nPut your macro here\n{COMMAND_BLOCK_CLOSER}";

        const int PAUSE_RECORD = 0x75;
        const int RUN = 0x76;
        const int STOP = 0x77;

        readonly string ANMLANG_CHARSET = $"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-{COMMAND_SEPARATOR}{COMMAND_BLOCK_STARTER}{COMMAND_BLOCK_CLOSER}{ARGS_SEPARATOR}";
    }
}
