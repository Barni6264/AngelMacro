﻿using System;
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
        public const string TEXT_SCREENSHOT = "SCREENSHOT";
        public const string TEXT_BREAK = "BREAK";
        public const string TEXT_WEBHOOK = "WEBHOOK";

        public static readonly string UNLOCKED_SCRIPT_WARNING = Properties.Resources.script_unlocked;
        public static readonly string OPEN_FILE_TITLE = Properties.Resources.open_macro;
        public static readonly string SAVE_FILE_TITLE = Properties.Resources.save_macro;
        public const string GDI_START = "GO";
        public const string GDI_SECOND = "s";
        public static readonly string COMMAND_ERROR_TEXT = Properties.Resources.command_error_text;
        public static readonly string COMMAND_ERROR_TITLE = Properties.Resources.command_error_title;

        public static readonly string FILE_ERROR_TEXT = Properties.Resources.file_not_found_text;
        public static readonly string FILE_ERROR_TITLE = Properties.Resources.file_error_title;

        public static readonly string COMPILE_CODE_BUTTON_TEXT = Properties.Resources.compile_code;
        public static readonly string RUN_MACRO_BUTTON_TEXT = Properties.Resources.run_macro;

        public static readonly string ERROR_SYNTAX_ERROR = Properties.Resources.syntax_error_text;
        public static readonly string ERROR_INVALID_OPERATION = Properties.Resources.invalid_operation_text;

        public const char COMMAND_SEPARATOR = ';';
        public const char ARGS_SEPARATOR = ':';
        public const char COMMAND_BLOCK_STARTER = '{';
        public const char COMMAND_BLOCK_CLOSER = '}';
        public const char STRING_SEPARATOR = '"';

        public static readonly string CONDITIONAL_MACRO_GUIDE = $"{COMMAND_BLOCK_STARTER}\n   {Properties.Resources.guide_your_macro_success}\n{COMMAND_BLOCK_CLOSER}{COMMAND_BLOCK_STARTER}\n   {Properties.Resources.guide_your_macro_fail}\n{COMMAND_BLOCK_CLOSER}";
        public static readonly string WHILE_MACRO_GUIDE = $"{COMMAND_BLOCK_STARTER}   \n{Properties.Resources.guide_your_macro_success}\n{COMMAND_BLOCK_CLOSER}";
        public static readonly string WEBHOOK_GUIDE = $"{TEXT_WEBHOOK}{ARGS_SEPARATOR}{STRING_SEPARATOR}Hello!{STRING_SEPARATOR}{ARGS_SEPARATOR}{STRING_SEPARATOR}https://example.com/api/webhooks/...{STRING_SEPARATOR}{COMMAND_SEPARATOR}";

        public const int PAUSE_RECORD = 0x75;
        public const int RUN = 0x76;
        public const int STOP = 0x77;

        public static readonly string ANMLANG_CHARSET = $"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-{STRING_SEPARATOR}{COMMAND_SEPARATOR}{COMMAND_BLOCK_STARTER}{COMMAND_BLOCK_CLOSER}{ARGS_SEPARATOR}";
    }

    public class InvalidCommandException : Exception
    {
        public InvalidCommandException() { }
        public InvalidCommandException(string message) : base(message) { }
        public InvalidCommandException (string message, Exception innerException) : base(message, innerException) { }
    }
}
