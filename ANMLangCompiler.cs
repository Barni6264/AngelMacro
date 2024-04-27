using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AngelMacro
{
    public static class ANMLangCompiler
    {
        // RULES
        /*
         * We need a "bytecode" that is
         * - easy to generate
         * - easy to interpret
         * We need this to:
         * - optimize the program, as string operations are much slower
         * - add support for nested conditions
         * 
         * OP = Operation; token = element in the array
         * 
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
         * BYTECODE SYNTAX *
         * * * * * * * * * * 
         * 
         * starts with OP ID
         *          KEYUP=1, KEYDOWN=2, etc...
         *          
         * then we have the args
         *          eg. coordinates or delay time
         *          
         * to support conditional OPs, first we'll have the "stack layer", then the OP ID, the args, then the tokens and OPs in the condition
         *          eg. layer0 COLOR x y r g b tokensTrue[] tokensElse[]
         * but to indicate where a block of code ends and where the tokensElse[] starts, we will use the conditional universal marking system (CUMS)
         *          eg. layer3 DELAY time layer3 DELAY newTime layer-3 DELAY elseTime
         *          
         * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
         * ANMLANG SYNTAX  *
         * * * * * * * * * * 
         * 
         * We don't need a complex system, as we do not have variables, objects, classes, etc; only conditionals, that are kind of hard-coded
         * After all, this is only a scripting language for macro recording and playing
         * I will use this language's (CSharp's) syntax where I can, so I don't confuse myself when coding at 3AM
         * 
         * the arg separator character is a colon (:)
         * semicolons (;) indicate the end of an OP. That's not because it's not obvious where the end of a command is when we have fixed arg counts, but it's there so we won't try to parse things like ...5000DELAY...
         * these beautiful brackets, this -->{  and this -->}  tell us where the end of a chunk is. We'll find that out by counting them, and when their count is equal, we reached the end of the block in question
         * we also have the minus (-) sign in the charset. I should not remove it as it's not there because it's part of the syntax, but it's there to indicate if a number is <0.  DUH!!
         * we always have colons (:) before these {, so can split the args and don't end up trying to use 500{DELAY as numbers
         * I don't know yet if we have to close conditionals with semicolon (;) after a bracket }<--  or not
         * 
         * EXAMPLE
         * 
         *      DELAY:10;
         *      KEYDOWN:Vc4;
         *      COLOR:1:2:3:4:5000:{
         *          DELAY:40;
         *          COLOR:5:4:3:2:1:{
         *              DELAY:4;
         *          }{}
         *          DELAY:40;
         *      }{
         *          DELAY:20;
         *      }
         *      DELAY:30;
         *      
         * To be honest, it doesn't look that bad
         */
        static Dispatcher dispatcher;
        static ProgressBar progressBar;
        public static int[] CompileCode(string code, Dispatcher _dispatcher, ProgressBar _progressBar)
        {
            dispatcher = _dispatcher;
            progressBar = _progressBar;

            dispatcher.Invoke(() => { progressBar.Maximum = code.Split(Consts.COMMAND_SEPARATOR).Length; });

            List<int> result = new List<int>();
            Compile(code, result, 0, false);
            return result.ToArray();
        }

        public static string CleanCode(string code)
        {
            return new string(code.Where(c => Consts.ANMLANG_CHARSET.Contains(c)).ToArray());
        }

        static void Compile(string code, List<int> list, int layer, bool marked) // marked could mean lots of things, but in this case it means the it belongs to the else part of a "COLOR" conditional
        {
            while (code.Length > 0)
            {
                string token = code.Split(Consts.COMMAND_SEPARATOR)[0];
                /*
                 * Token can look like this:
                 *      DELAY:10
                 * or
                 *      COLOR:1:2:3:4:5000:{DELAY:40
                 * or
                 *      COLOR:1:2:3:4:5000:{COLOR:1:2:3:4:5000:{DELAY:40
                 */
                string[] args = token.Split(Consts.ARGS_SEPARATOR);
                if (args[0] == "")
                {
                    return;
                }

                list.Add(marked?-layer:layer);
                switch (args[0]) //OP
                {
                    case Consts.TEXT_COLOR_THRESHOLD_CHANGE:
                        list.Add(1);
                        list.Add(int.Parse(args[1]));
                        break;
                    case Consts.TEXT_END:
                        list.Add(0);
                        return; // because anything in the scope under an END OP is just useless and unreachable
                    case Consts.TEXT_DELAY:
                        list.Add(2);
                        list.Add(int.Parse(args[1]));
                        break;
                    case Consts.TEXT_KEY_DOWN:
                        list.Add(3);
                        list.Add((int)Enum.Parse(typeof(KeyCode), args[1]));
                        break;
                    case Consts.TEXT_KEY_UP:
                        list.Add(4);
                        list.Add((int)Enum.Parse(typeof(KeyCode), args[1]));
                        break;
                    case Consts.TEXT_LOCATION:
                        list.Add(5);
                        list.Add(int.Parse(args[1]));
                        list.Add(int.Parse(args[2]));
                        break;
                    case Consts.TEXT_MOUSE_DOWN:
                        list.Add(6);
                        list.Add((int)Enum.Parse(typeof(MouseButton), args[1]));
                        break;
                    case Consts.TEXT_MOUSE_UP:
                        list.Add(7);
                        list.Add((int)Enum.Parse(typeof(MouseButton), args[1]));
                        break;
                    case Consts.TEXT_SCROLL_WHEEL:
                        list.Add(8);
                        list.Add(int.Parse(args[1]));
                        break;

                    // Hard part (I'll suffer...)

                    case Consts.TEXT_COLOR:
                        {
                            list.Add(10);
                            list.Add(int.Parse(args[1])); // X
                            list.Add(int.Parse(args[2])); // Y
                            list.Add(int.Parse(args[3])); // R
                            list.Add(int.Parse(args[4])); // G
                            list.Add(int.Parse(args[5])); // B
                            /*
                             * now, the program will step through the "code" character by character and count the fancy brackets -->{}. First it has 1 opening bracket,
                             * and it loops until it finds a pair (has the same amount of opening and closing brackets)
                             * then it will Substring the block between the opening and last closing bracket and call this function recursively
                             * it also passes the current layer number+1 as parameter
                            */
                            code = code.Substring(code.IndexOf(Consts.COMMAND_BLOCK_STARTER) + 1); // this will return a string like this (cutoff part indicated by | ): ...g:b:{|DELAY:3...
                            int blockStarterCount = 1;
                            int blockEndCount = 0; // the reason above is why blockStarterCount has a default value of 1. This way we don't start with 0=0
                            int index = 0; // that's just the "i" if it was a for loop

                            // if part
                            while (blockEndCount != blockStarterCount)
                            {
                                if (code[index] == Consts.COMMAND_BLOCK_STARTER)
                                {
                                    blockStarterCount++;
                                }
                                else if (code[index] == Consts.COMMAND_BLOCK_CLOSER)
                                {
                                    blockEndCount++;
                                }
                                index++;
                            }
                            Compile(code.Substring(0, index - 1), list, layer + 1, false); // I think this will NOT include the last block closer
                            code = code.Substring(index+1);

                            // else part
                            blockStarterCount = 1;
                            blockEndCount = 0;
                            index = 0;
                            while (blockEndCount != blockStarterCount)
                            {
                                if (code[index] == Consts.COMMAND_BLOCK_STARTER)
                                {
                                    blockStarterCount++;
                                }
                                else if (code[index] == Consts.COMMAND_BLOCK_CLOSER)
                                {
                                    blockEndCount++;
                                }
                                index++;
                            }
                            Compile(code.Substring(0, index - 1), list, layer + 1, true);
                            code = code.Substring(index);
                        }
                        break;
                    case Consts.TEXT_WHILE:
                        { // this is here so we can have variables with different names (pretty clever)
                            list.Add(11);
                            list.Add(int.Parse(args[1]));
                            list.Add(int.Parse(args[2]));
                            list.Add(int.Parse(args[3]));
                            list.Add(int.Parse(args[4]));
                            list.Add(int.Parse(args[5]));

                            code = code.Substring(code.IndexOf(Consts.COMMAND_BLOCK_STARTER) + 1);
                            int blockStarterCount = 1;
                            int blockEndCount = 0;
                            int index = 0;

                            while (blockEndCount != blockStarterCount)
                            {
                                if (code[index] == Consts.COMMAND_BLOCK_STARTER)
                                {
                                    blockStarterCount++;
                                }
                                else if (code[index] == Consts.COMMAND_BLOCK_CLOSER)
                                {
                                    blockEndCount++;
                                }
                                index++;
                            }
                            Compile(code.Substring(0, index - 1), list, layer + 1, false);
                            code = code.Substring(index);
                        }
                        break;
                    case Consts.TEXT_UNTIL:
                        {
                            list.Add(12);
                            list.Add(int.Parse(args[1]));
                            list.Add(int.Parse(args[2]));
                            list.Add(int.Parse(args[3]));
                            list.Add(int.Parse(args[4]));
                            list.Add(int.Parse(args[5]));

                            code = code.Substring(code.IndexOf(Consts.COMMAND_BLOCK_STARTER) + 1);
                            int blockStarterCount = 1;
                            int blockEndCount = 0;
                            int index = 0;

                            while (blockEndCount != blockStarterCount)
                            {
                                if (code[index] == Consts.COMMAND_BLOCK_STARTER)
                                {
                                    blockStarterCount++;
                                }
                                else if (code[index] == Consts.COMMAND_BLOCK_CLOSER)
                                {
                                    blockEndCount++;
                                }
                                index++;
                            }
                            Compile(code.Substring(0, index - 1), list, layer + 1, false);
                            code = code.Substring(index);
                        }
                        break;
                    default:
                        MessageBox.Show(Consts.COMMAND_ERROR_TEXT, Consts.COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }

                if (!(Consts.TEXT_COLOR+Consts.TEXT_UNTIL+Consts.TEXT_WHILE).Contains(args[0]))
                {
                    dispatcher.Invoke(() => { progressBar.Value++; });
                    code = code.Substring(token.Length + 1); // always removes the first command from the code (if it's not a conditional)
                }
            }
        }
    }
}
