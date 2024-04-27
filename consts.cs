using System;
using System.Dynamic;

namespace AngelMacro
{
    public static class Consts
    {
        public enum MACROSTATUS { RECORDING, RUNNING, IDLE }

        public const string FILE_NAME = "macro";
        public const string FILE_EXTENSION = "*.amacro";
        public const string FILE_FILTER = $"AngelMacro file|{FILE_EXTENSION}";

        public const int CSHARP_LAG_COMPENSATION = 0;

        public const string TEXT_DELAY = "DELAY";
        public const string TEXT_LOCATION = "LOCATION";
        public const string TEXT_SCROLL_WHEEL = "SCROLL";
        public const string TEXT_MOUSE_UP = "MOUSEUP";
        public const string TEXT_MOUSE_DOWN = "MOUSEDOWN";
        public const string TEXT_KEY_UP = "KEYUP";
        public const string TEXT_KEY_DOWN = "KEYDOWN";
        public const string TEXT_COLOR = "COLOR";
        public const string TEXT_WHILE = "WHILE";
        public const string TEXT_UNTIL = "UNTIL";
        public const string TEXT_END = "END";
        public const string TEXT_COLOR_THRESHOLD_CHANGE = "THRESHOLD";

        public static readonly string UNLOCKED_SCRIPT_WARNING = Properties.Resources.script_unlocked;
        public static readonly string OPEN_FILE_TITLE = Properties.Resources.open_macro;
        public static readonly string SAVE_FILE_TITLE = Properties.Resources.save_macro;
        public const string GDI_START = "GO";
        public const string GDI_SECOND = "s";
        public static readonly string COMMAND_ERROR_TEXT = Properties.Resources.command_error_text;
        public static readonly string COMMAND_ERROR_TITLE = Properties.Resources.command_error_title;

        public static readonly string COMPILE_CODE_BUTTON_TEXT = Properties.Resources.compile_code;
        public static readonly string RUN_MACRO_BUTTON_TEXT = Properties.Resources.run_macro;

        public const char COMMAND_SEPARATOR = ';';
        public const char ARGS_SEPARATOR = ':';
        public const char COMMAND_BLOCK_STARTER = '{';
        public const char COMMAND_BLOCK_CLOSER = '}';

        public static readonly string CONDITIONAL_MACRO_GUIDE = $"{COMMAND_BLOCK_STARTER}\n   Put your macro here\n{COMMAND_BLOCK_CLOSER}{COMMAND_BLOCK_STARTER}\n   Put your macro here if the condition fails\n{COMMAND_BLOCK_CLOSER}";
        public static readonly string WHILE_MACRO_GUIDE = $"{COMMAND_BLOCK_STARTER}   \nPut your macro here\n{COMMAND_BLOCK_CLOSER}";

        public const int PAUSE_RECORD = 0x75;
        public const int RUN = 0x76;
        public const int STOP = 0x77;

        public static readonly string ANMLANG_CHARSET = $"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-{COMMAND_SEPARATOR}{COMMAND_BLOCK_STARTER}{COMMAND_BLOCK_CLOSER}{ARGS_SEPARATOR}";
    }
}
