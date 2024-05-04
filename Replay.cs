using SharpHook;
using System;
using System.Drawing;
using System.Threading;
using SharpHook.Native;

namespace AngelMacro
{
    public partial class MainWindow
    {
        EventSimulator simulator = new EventSimulator();
        int colorThreshold = 5;
        int[] code;
        int index = 0;

        void ExecuteCode(int[] _code) //TODO COMPLIER rewrite from scratch
        {
            code = _code;
            while (currentStatus == Consts.MACROSTATUS.RUNNING)
            {
                index = 0;
                Execute();
            }

            /*
                    for (int i = 0; (i < commands.Length) && (currentStatus == MACROSTATUS.RUNNING); i++)
                    {
                        if (commands[i].Length < 2)
                        {
                            continue;
                        }

                        command = commands[i].Split(argsSeparator);

                        switch (command[0].Trim())
                        {
                            case TEXT_COLOR_THRESHOLD_CHANGE:
                                colorThreshold = int.Parse(command[1]);
                                break;
                            case TEXT_COLOR:
                                RunColorCheck(command);
                                break;
                            case TEXT_WHILE:
                                RunWhileCheck(command);
                                break;
                            case TEXT_UNTIL:
                                RunWhileNot(command);
                                break;
                            case TEXT_END:
                                Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                                Console.Beep(600, 200);
                                break;
                            case TEXT_DELAY:
                                Thread.Sleep(int.Parse(command[1]));
                                break;
                            case TEXT_KEY_DOWN:
                                simulator.SimulateKeyPress((KeyCode)Enum.Parse(typeof(KeyCode), command[1]));
                                break;
                            case TEXT_KEY_UP:
                                simulator.SimulateKeyRelease((KeyCode)Enum.Parse(typeof(KeyCode), command[1]));
                                break;
                            case TEXT_LOCATION:
                                simulator.SimulateMouseMovement(short.Parse(command[1]), short.Parse(command[2]));
                                break;
                            case TEXT_MOUSE_DOWN:
                                simulator.SimulateMousePress((MouseButton)Enum.Parse(typeof(MouseButton), command[1]));
                                break;
                            case TEXT_MOUSE_UP:
                                simulator.SimulateMouseRelease((MouseButton)Enum.Parse(typeof(MouseButton), command[1]));
                                break;
                            case TEXT_SCROLL_WHEEL:
                                simulator.SimulateMouseWheel(short.Parse(command[1]));
                                break;
                            default:
                                Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                                MessageBox.Show(COMMAND_ERROR_TEXT, COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                                break;

                        }
                    }
            */
        }

        void Execute(bool marked = false) // returns the new index
        {
            int firstOPlayer = code[index];
            int currentLayer = firstOPlayer;

            if (marked) // if marked, skip to the marked (-) part
            {
                index = Array.IndexOf(code, -firstOPlayer, index);
                /* TODO
                 * this is not okay, as args can have values that match the layer
                 * another problem is if the Array.IndexOf can't find the element we're looking for (eg. we reach the end of the array) then we're screwed
                 * maybe we can split the array into a 2D jagged array of OPs and args
                 * and we also have another problem. we might not be able to find (anywhere where we use IndexOf) the next OP in the same layer, because it is possible that we have
                 * an OP that is already a layer above, so we should use something like IndexOf(array, thisValueOrSmallerThanThis, startIndex)
                 */
                firstOPlayer *= -1;
                currentLayer = firstOPlayer;
                /*
                 * I think this while loop is slower than the Array.IndexOf, but I'll leave it here as a comment as a backup
                 * 
                while (code[index] != -firstOPlayer)
                {
                    index++;
                }
                 */
            }

            while (currentStatus == Consts.MACROSTATUS.RUNNING && firstOPlayer == currentLayer)
            {
                switch (code[index+1])
                {
                    case 0:
                        Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                        Console.Beep(600, 200);
                        break;
                    case 1:
                        colorThreshold = code[index+2];
                        index += (2 + 1); // 2+1, because the first number is always 2 (1 to skip the layer indicator, 1 to skip the OP) + (number of args)
                        break;
                    case 2:
                        Thread.Sleep(code[index+2]);
                        index += (2 + 1);
                        break;
                    case 3:
                        simulator.SimulateKeyPress((KeyCode)code[index+2]);
                        index += (2 + 1);
                        break;
                    case 4:
                        simulator.SimulateKeyRelease((KeyCode)code[index + 2]);
                        index += (2 + 1);
                        break;
                    case 5:
                        simulator.SimulateMouseMovement((short)code[index + 2], (short)code[index + 3]);
                        index += (2 + 2);
                        break;
                    case 6:
                        simulator.SimulateMousePress((MouseButton)code[index + 2]);
                        index += (2 + 1);
                        break;
                    case 7:
                        simulator.SimulateMouseRelease((MouseButton)code[index + 2]);
                        index += (2 + 1);
                        break;
                    case 8:
                        simulator.SimulateMouseWheel((short)code[index + 2]);
                        break;
                    case 10: // OP if
                        /*
                         * This is what a normal person would do (I think)
                         * 
                        if (IsColor(code[index + 2], code[index + 3], code[index + 4], code[index + 5], code[index + 6]))
                        {
                            index += (2 + 5);
                            Execute();
                        }
                        else
                        {
                            index += (2 + 5);
                            Execute(true);
                        }
                        break;
                        */
                        // first I increment, to avoid repetition
                        // I changed my mind mid way, but I will leave everything here as a backup just in case
                        /*
                        index += (2 + 5);
                        if (IsColor(code[index - 5], code[index - 4], code[index - 3], code[index - 2], code[index - 1]))
                        {
                            Execute();
                        }
                        else
                        {
                            Execute(true);
                        }
                        break;
                        */
                        index += (2 + 5);
                        Execute(!IsColor(code[index - 5], code[index - 4], code[index - 3], code[index - 2], code[index - 1])); // one-liner

                        // everywhere where we have conditionals, we have to skip the nested part before the break (the "else" part of the if OP is an exception from this) // TODO
                        index = Array.IndexOf(code, firstOPlayer, index); // we can use the index here without adding anything to it, as we already added 7
                        break;
                    case 11: // OP while
                        {
                            int savedIndex = index;
                            while (IsColor(code[index - 5], code[index - 4], code[index - 3], code[index - 2], code[index - 1]))
                            {
                                Execute();
                                index = savedIndex; // reverts back to the original index
                            }
                            index = Array.IndexOf(code, firstOPlayer, savedIndex + 1); // +1 so it doesn't include the original
                        }
                        break;
                    case 12: // OP while !condition
                        {
                            int savedIndex = index;
                            while (!IsColor(code[index - 5], code[index - 4], code[index - 3], code[index - 2], code[index - 1]))
                            {
                                Execute();
                                index = savedIndex; // reverts back to the original index
                            }
                            index = Array.IndexOf(code, firstOPlayer, savedIndex + 1); // +1 so it doesn't include the original
                        }
                        break;
                }

                currentLayer = code[index];
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