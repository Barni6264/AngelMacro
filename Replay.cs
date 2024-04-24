using SharpHook;
using SharpHook.Native;
using System;
using System.Drawing;
using System.Threading;
using System.Windows;

namespace AngelMacro
{
    public partial class MainWindow
    {
        EventSimulator simulator = new EventSimulator();
        int colorThreshold = 5;

        void ExecuteMacro(string macroText, char commandSeparator, char argsSeparator)
        {
            string[] commands = macroText.Replace("\n", "").Replace("\r", "").Replace("\t", "").Split(commandSeparator);
            string[] command;

            try
            {
                while (currentStatus == MACROSTATUS.RUNNING)
                {
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

                    if (commandSeparator == COMMAND_SEPARATOR2) // if this is a conditional part, only run once
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is not ThreadInterruptedException)
                {
                    MessageBox.Show(ex.Message, COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (commandSeparator == COMMAND_SEPARATOR2) // if this is a conditional part, throw a new Exception to stop the main part too 1 layer above
                {
                    throw;
                }
            }
        }

        void RunColorCheck(string[] command)
        {
            Color color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            if ((Math.Abs(color.R - int.Parse(command[3])) + Math.Abs(color.G - int.Parse(command[4])) + Math.Abs(color.B - int.Parse(command[5]))) < colorThreshold)
            {
                ExecuteMacro(command[6], COMMAND_SEPARATOR2, ARGS_SEPARATOR2);
            }
            else
            {
                ExecuteMacro(command[7], COMMAND_SEPARATOR2, ARGS_SEPARATOR2);
            }
        }

        void RunWhileCheck(string[] command)
        {
            Color color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            while ((Math.Abs(color.R - int.Parse(command[3])) + Math.Abs(color.G - int.Parse(command[4])) + Math.Abs(color.B - int.Parse(command[5]))) < colorThreshold)
            {
                ExecuteMacro(command[6], COMMAND_SEPARATOR2, ARGS_SEPARATOR2);
                color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            }
        }

        void RunWhileNot(string[] command)
        {
            Color color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            while ((Math.Abs(color.R - int.Parse(command[3])) + Math.Abs(color.G - int.Parse(command[4])) + Math.Abs(color.B - int.Parse(command[5]))) > colorThreshold)
            {
                ExecuteMacro(command[6], COMMAND_SEPARATOR2, ARGS_SEPARATOR2);
                color = Condition.GetPixel(int.Parse(command[1]), int.Parse(command[2]));
            }
        }
    }
}