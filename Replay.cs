using SharpHook;
using System;
using System.Drawing;
using System.Threading;
using SharpHook.Native;
using System.Linq;

namespace AngelMacro
{
    public partial class MainWindow
    {
        EventSimulator simulator = new EventSimulator();
        int colorThreshold = 5;
        int[] emptyIntArray = { };

        void ExecuteCode(int[][] code) //TODO COMPLIER rewrite from scratch
        {
            while (currentStatus == Consts.MACROSTATUS.RUNNING)
            {
                Execute(code);
            }
        }

        void Execute(int[][] code, bool marked = false)
        {
            int index = 0;
            int layer = Math.Abs(code[0][0]); // unmarked layer

            /* TODO
             * another problem is if the Array.IndexOf can't find the element we're looking for (eg. we reach the end of the array) then we're screwed
             * and we also have another problem. we might not be able to find (anywhere where we use IndexOf) the next OP in the same layer, because it is possible that we have
             * an OP that is already a layer above, so we should use something like IndexOf(array, thisValueOrSmallerThanThis, startIndex)
            */

            while (currentStatus == Consts.MACROSTATUS.RUNNING && index < code.Length)
            {
                // 0:layer, 1:OP, 2...args
                switch (code[index][1])
                {
                    case 0:
                        Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                        Console.Beep(600, 200);
                        break;
                    case 1:
                        colorThreshold = code[index][2];
                        break;
                    case 2:
                        Thread.Sleep(code[index][2]);
                        break;
                    case 3:
                        simulator.SimulateKeyPress((KeyCode)code[index][2]);
                        break;
                    case 4:
                        simulator.SimulateKeyRelease((KeyCode)code[index][2]);
                        break;
                    case 5:
                        simulator.SimulateMouseMovement((short)code[index][2], (short)code[index][3]);
                        break;
                    case 6:
                        simulator.SimulateMousePress((MouseButton)code[index][2]);
                        break;
                    case 7:
                        simulator.SimulateMouseRelease((MouseButton)code[index][2]);
                        index += (2 + 1);
                        break;
                    case 8:
                        simulator.SimulateMouseWheel((short)code[index][2]);
                        break;
                    case 10: // OP if
                        /*
                         * if true:
                         * - go to the end of the "if true" part
                         * - execute it
                         * - go to the end of the "else" part
                         * - skip by incrementing the index
                         * else:
                         * - go to the end of the "if true" part
                         * - skip it by incrementing the index
                         * - go to the end of the "else" part
                         * - execute it
                         * - skip by incrementing the index
                         */

                        /*
                         * This abomination finds the first element of the code array whose first element is smaller or equal to the current layer, then it get's the index of the element
                         * * it finds the index of the next OP with the same or lower layer number. If it can't find it, it returns -1
                         * It should be noted that because this is an if-else pair, we can also have the "else" part after the "if true" OPs
                         * * so we have to search for the -layer pair too, not just the layer above
                         */
                        int toTake = Array.IndexOf(code,
                            code.Skip(index + 1)
                            .FirstOrDefault(num => (Math.Abs(num[0]) <= layer || num[0] == -1 * (layer + 1)), emptyIntArray)
                        );
                        // returns 0 if it finds the end immediately, -1 if it reaches the end of the full code, 1 if there's only 1 OP inside, 2 if 2 OPs, 3, etc...

                        if (IsColor(code[index][2], code[index][3], code[index][4], code[index][5], code[index][6]))
                        {
                            // if toTake is -1 (couldn't find other instructions with smaller or equal layer number), then skip the ".Take" part
                            if (toTake == -1)
                            {
                                Execute(code.Skip(index+1).ToArray());
                            }
                            else
                            {
                                Execute(code.Skip(index+1).Take(toTake).ToArray());
                            }
                        }
                        else
                        {
                            
                        }
                        break;
                    case 11: // OP while
                        break;
                    case 12: // OP while !condition
                        break;
                }

                index++;
            }
        }

        bool IsColor(int x, int y, int r, int g, int b)
        {
            Color color = Condition.GetPixel(x, y);
            return (Math.Abs(color.R - r) + Math.Abs(color.G - g) + Math.Abs(color.B - b))<=colorThreshold;
        }

        void RunColorCheck(string[] command)
        {
            Color color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            if ((Math.Abs(color.R - int.Parse(command[3])) + Math.Abs(color.G - int.Parse(command[4])) + Math.Abs(color.B - int.Parse(command[5]))) < colorThreshold)
            {
                //ExecuteMacro(command[6], COMMAND_SEPARATOR2, ARGS_SEPARATOR2); //TODO COMPLIER rewrite
            }
            else
            {
                //ExecuteMacro(command[7], COMMAND_SEPARATOR2, ARGS_SEPARATOR2); //TODO COMPLIER rewrite
            }
        }

        void RunWhileCheck(string[] command)
        {
            Color color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            while ((Math.Abs(color.R - int.Parse(command[3])) + Math.Abs(color.G - int.Parse(command[4])) + Math.Abs(color.B - int.Parse(command[5]))) < colorThreshold)
            {
                //ExecuteMacro(command[6], COMMAND_SEPARATOR2, ARGS_SEPARATOR2); //TODO COMPLIER rewrite
                color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            }
        }

        void RunWhileNot(string[] command)
        {
            Color color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            while ((Math.Abs(color.R - int.Parse(command[3])) + Math.Abs(color.G - int.Parse(command[4])) + Math.Abs(color.B - int.Parse(command[5]))) > colorThreshold)
            {
                //ExecuteMacro(command[6], COMMAND_SEPARATOR2, ARGS_SEPARATOR2); //TODO COMPLIER rewrite
                color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            }
        }
    }
}