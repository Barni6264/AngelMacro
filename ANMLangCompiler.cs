using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
         * if we have a conditional OP, then first we have the OP ID, the args, then the number of tokens and then the tokens and OPs in the condition
         *          eg. COLOR x y r g b tokenCountTrue tokenCountElse tokensTrue[] tokensElse[]
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
        public static int[] CompileCode(string code, Dispatcher dispatcher, ProgressBar progressBar)
        {
            dispatcher.Invoke(() => { progressBar.Maximum = 0; }); //TODO COMPLIER change 0 to value

            List<int> result = new List<int>();
            for (; ; ) //TODO COMPLIER finish for loop or replace with recursive function
            {
                dispatcher.Invoke(() => { progressBar.Value++; });
            }

            return result.ToArray();
        }

        public static string CleanCode(string code, string charset)
        {
            return new string(code.Where(c => charset.Contains(c)).ToArray());
        }
    }
}
