﻿using SharpHook.Native;
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
         * - optimize the program, as string operations and repeated int.Parse-ings are much slower
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
         * TODO rewrite the whole "bytecode" and create a new one that is faster and easier to interpret
         */
        static Dispatcher dispatcher;
        static ProgressBar progressBar;
        public static TreeNode CompileCode(string code, Dispatcher _dispatcher, ProgressBar _progressBar)
        {
            dispatcher = _dispatcher;
            progressBar = _progressBar;

            dispatcher.Invoke(() => { progressBar.Maximum = code.Split(Consts.COMMAND_SEPARATOR).Length; });

            TreeNode result = new TreeNode(true);
            Compile(code, result, true);

            return result;
        }

        public static string CleanCode(string code)
        {
            return new string(code.Where(c => Consts.ANMLANG_CHARSET.Contains(c)).ToArray());
        }
        // TODO exception for unknown enum

        // after that we might have something like this
        // OP:"12:34:56:78":23;
        public static string ConvertStringToArgs(string code)
        {
            string[] parts = code.Split(Consts.STRING_SEPARATOR);
            string[] result = new string[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 1)
                {
                    result[i] = string.Join(Consts.ARGS_SEPARATOR,Encoding.UTF8.GetBytes(parts[i]));
                }
                else
                {
                    result[i] = parts[i];
                }
            }

            return string.Join(Consts.STRING_SEPARATOR, result);
        }

        static void Compile(string code, TreeNode parentNode, bool binaryOption) // binaryOption could mean lots of things, but in this case it means the code belongs to the if-true part of a "COLOR" conditional
        {
            while (code.Length > 0)
            {
                List<int> result = new List<int>();
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

                TreeNode newNode = new TreeNode();

                switch (args[0]) //OP
                {
                    case Consts.TEXT_COLOR_THRESHOLD_CHANGE:
                        result.Add(1);
                        result.Add(int.Parse(args[1]));
                        break;
                    case Consts.TEXT_END:
                        result.Add(0);
                        break;
                    case Consts.TEXT_DELAY:
                        result.Add(2);
                        result.Add(int.Parse(args[1]));
                        break;
                    case Consts.TEXT_KEY_DOWN:
                        result.Add(3);
                        result.Add((int)(ushort)Enum.Parse(typeof(KeyCode), args[1]));
                        break;
                    case Consts.TEXT_KEY_UP:
                        result.Add(4);
                        result.Add((int)(ushort)Enum.Parse(typeof(KeyCode), args[1]));
                        break;
                    case Consts.TEXT_LOCATION:
                        result.Add(5);
                        result.Add(int.Parse(args[1]));
                        result.Add(int.Parse(args[2]));
                        break;
                    case Consts.TEXT_MOUSE_DOWN:
                        result.Add(6);
                        result.Add((int)(ushort)Enum.Parse(typeof(MouseButton), args[1]));
                        break;
                    case Consts.TEXT_MOUSE_UP:
                        result.Add(7);
                        result.Add((int)(ushort)Enum.Parse(typeof(MouseButton), args[1]));
                        break;
                    case Consts.TEXT_SCROLL_WHEEL:
                        result.Add(8);
                        result.Add(int.Parse(args[1]));
                        break;
                    case Consts.TEXT_SCREENSHOT:
                        result.Add(9);
                        break;

                    // TODO create function for conditionals
                    case Consts.TEXT_COLOR:
                        {
                            result.Add(10);
                            result.Add(int.Parse(args[1])); // X
                            result.Add(int.Parse(args[2])); // Y
                            result.Add(int.Parse(args[3])); // R
                            result.Add(int.Parse(args[4])); // G
                            result.Add(int.Parse(args[5])); // B
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
                            Compile(code.Substring(0, index - 1), newNode, true); // I think this will NOT include the last block closer
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
                            Compile(code.Substring(0, index - 1), newNode, false);
                            code = code.Substring(index);
                        }
                        break;
                    case Consts.TEXT_WHILE:
                        { // this is here so we can have variables with different names (pretty clever)
                            result.Add(11);
                            result.Add(int.Parse(args[1]));
                            result.Add(int.Parse(args[2]));
                            result.Add(int.Parse(args[3]));
                            result.Add(int.Parse(args[4]));
                            result.Add(int.Parse(args[5]));

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
                            Compile(code.Substring(0, index - 1), newNode, true);
                            code = code.Substring(index);
                        }
                        break;
                    case Consts.TEXT_UNTIL:
                        {
                            result.Add(12);
                            result.Add(int.Parse(args[1]));
                            result.Add(int.Parse(args[2]));
                            result.Add(int.Parse(args[3]));
                            result.Add(int.Parse(args[4]));
                            result.Add(int.Parse(args[5]));

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
                            Compile(code.Substring(0, index - 1), newNode, true);
                            code = code.Substring(index);
                        }
                        break;
                    case Consts.TEXT_BREAK:
                        result.Add(13);
                        break;
                    case Consts.TEXT_WEBHOOK:
                        result.Add(14);
                        string[] strings = token.Split(Consts.STRING_SEPARATOR);
                        string[] messageArgs = strings[1].Split(Consts.ARGS_SEPARATOR);
                        string[] webhookArgs = strings[3].Split(Consts.ARGS_SEPARATOR);

                        for (int i = 0; i < messageArgs.Length; i++)
                        {
                            result.Add(int.Parse(messageArgs[i]));
                        }
                        result.Add('\0');
                        for (int i = 0; i < webhookArgs.Length; i++)
                        {
                            result.Add(int.Parse(webhookArgs[i]));
                        }
                        break;
                    default:
                        throw new InvalidCommandException (Consts.COMMAND_ERROR_TEXT);
                }

                newNode.nodeArgs = result.ToArray();

                if (!(Consts.TEXT_COLOR + Consts.TEXT_UNTIL + Consts.TEXT_WHILE).Contains(args[0]))
                {
                    dispatcher.Invoke(() => { progressBar.Value++; });
                    code = code.Substring(token.Length + 1); // always removes the first command from the code (if it's not a conditional)

                    if (result[0] == 0) // if "end" script
                    {
                        return;
                    }
                }

                if (binaryOption)
                {
                    parentNode.conditionIfTrue.Add(newNode);
                }
                else
                {
                    parentNode.conditionElseFalse.Add(newNode);
                }
            }
        }
    }
}
