using SharpHook;
using System;
using System.Drawing;
using System.Threading;
using SharpHook.Native;
using System.Net;
using System.Text;
using System.Collections.Specialized;

namespace AngelMacro
{
    public partial class MainWindow
    {
        EventSimulator simulator = new EventSimulator();
        int colorThreshold = 5;
        TreeNode code;

        void ExecuteCode(TreeNode _code)
        {
            code = _code;
            while (currentStatus == Consts.MACROSTATUS.RUNNING)
            {
                Execute(code, true);
            }
        }
        // I have absolutely no idea if the code below works or not, but "it should do its job"
        void Execute(TreeNode node, bool binaryOption)
        {
            if (binaryOption || node.isRootNode)
            {
                for (int i = 0; i < node.conditionIfTrue.Count; i++)
                {
                    switch (node.conditionIfTrue[i].nodeArgs[0])
                    {
                        case 0:
                            Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                            Console.Beep(600, 200);
                            break;
                        case 1:
                            colorThreshold = node.conditionIfTrue[i].nodeArgs[1];
                            break;
                        case 2:
                            Thread.Sleep(node.conditionIfTrue[i].nodeArgs[1]);
                            break;
                        case 3:
                            simulator.SimulateKeyPress((KeyCode)node.conditionIfTrue[i].nodeArgs[1]);
                            break;
                        case 4:
                            simulator.SimulateKeyRelease((KeyCode)node.conditionIfTrue[i].nodeArgs[1]);
                            break;
                        case 5:
                            simulator.SimulateMouseMovement((short)node.conditionIfTrue[i].nodeArgs[1], (short)node.conditionIfTrue[i].nodeArgs[2]);
                            break;
                        case 6:
                            simulator.SimulateMousePress((MouseButton)node.conditionIfTrue[i].nodeArgs[1]);
                            break;
                        case 7:
                            simulator.SimulateMouseRelease((MouseButton)node.conditionIfTrue[i].nodeArgs[1]);
                            break;
                        case 8:
                            simulator.SimulateMouseWheel((short)node.conditionIfTrue[i].nodeArgs[1]);
                            break;
                        case 9:
                            Condition.ScreenShot();
                            break;
                        case 10: // OP if
                            Execute(node.conditionIfTrue[i], IsColor(node.conditionIfTrue[i].nodeArgs[1], node.conditionIfTrue[i].nodeArgs[2], node.conditionIfTrue[i].nodeArgs[3], node.conditionIfTrue[i].nodeArgs[4], node.conditionIfTrue[i].nodeArgs[5]));
                            break;
                        case 11: // OP while
                            while (IsColor(node.conditionIfTrue[i].nodeArgs[1], node.conditionIfTrue[i].nodeArgs[2], node.conditionIfTrue[i].nodeArgs[3], node.conditionIfTrue[i].nodeArgs[4], node.conditionIfTrue[i].nodeArgs[5]))
                            {
                                Execute(node.conditionIfTrue[i], true);
                            }
                            break;
                        case 12: // OP while !condition
                            while (!IsColor(node.conditionIfTrue[i].nodeArgs[1], node.conditionIfTrue[i].nodeArgs[2], node.conditionIfTrue[i].nodeArgs[3], node.conditionIfTrue[i].nodeArgs[4], node.conditionIfTrue[i].nodeArgs[5]))
                            {
                                Execute(node.conditionIfTrue[i], true);
                            }
                            break;
                        case 13:
                            return;
                        case 14:
                            byte[] byteArgs = new byte[node.conditionIfTrue[i].nodeArgs.Length];
                            for (int j = 0; j < byteArgs.Length; j++)
                            {
                                byteArgs[j] = (byte)node.conditionIfTrue[i].nodeArgs[j];
                            }
                            int indexOfNullCharacter = Array.IndexOf(node.conditionIfTrue[i].nodeArgs, '\0');
                            string message = Encoding.UTF8.GetString(byteArgs, 1, indexOfNullCharacter - 1);
                            string webhook = Encoding.UTF8.GetString(byteArgs, indexOfNullCharacter + 1, byteArgs.Length - indexOfNullCharacter - 1);

                            SendWebhook(webhook, message);
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < node.conditionElseFalse.Count; i++)
                {
                    switch (node.conditionElseFalse[i].nodeArgs[0])
                    {
                        case 0:
                            Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                            Console.Beep(600, 200);
                            break;
                        case 1:
                            colorThreshold = node.conditionElseFalse[i].nodeArgs[1];
                            break;
                        case 2:
                            Thread.Sleep(node.conditionElseFalse[i].nodeArgs[1]);
                            break;
                        case 3:
                            simulator.SimulateKeyPress((KeyCode)node.conditionElseFalse[i].nodeArgs[1]);
                            break;
                        case 4:
                            simulator.SimulateKeyRelease((KeyCode)node.conditionElseFalse[i].nodeArgs[1]);
                            break;
                        case 5:
                            simulator.SimulateMouseMovement((short)node.conditionElseFalse[i].nodeArgs[1], (short)node.conditionElseFalse[i].nodeArgs[2]);
                            break;
                        case 6:
                            simulator.SimulateMousePress((MouseButton)node.conditionElseFalse[i].nodeArgs[1]);
                            break;
                        case 7:
                            simulator.SimulateMouseRelease((MouseButton)node.conditionElseFalse[i].nodeArgs[1]);
                            break;
                        case 8:
                            simulator.SimulateMouseWheel((short)node.conditionElseFalse[i].nodeArgs[1]);
                            break;
                        case 9:
                            Condition.ScreenShot();
                            break;
                        case 10: // OP if
                            Execute(node.conditionElseFalse[i], IsColor(node.conditionElseFalse[i].nodeArgs[1], node.conditionElseFalse[i].nodeArgs[2], node.conditionElseFalse[i].nodeArgs[3], node.conditionElseFalse[i].nodeArgs[4], node.conditionElseFalse[i].nodeArgs[5]));
                            break;
                        case 11: // OP while
                            while (IsColor(node.conditionElseFalse[i].nodeArgs[1], node.conditionElseFalse[i].nodeArgs[2], node.conditionElseFalse[i].nodeArgs[3], node.conditionElseFalse[i].nodeArgs[4], node.conditionElseFalse[i].nodeArgs[5]))
                            {
                                Execute(node.conditionElseFalse[i], true);
                            }
                            break;
                        case 12: // OP while !condition
                            while (!IsColor(node.conditionElseFalse[i].nodeArgs[1], node.conditionElseFalse[i].nodeArgs[2], node.conditionElseFalse[i].nodeArgs[3], node.conditionElseFalse[i].nodeArgs[4], node.conditionElseFalse[i].nodeArgs[5]))
                            {
                                Execute(node.conditionElseFalse[i], true);
                            }
                            break;
                        case 13:
                            return;
                        case 14:
                            byte[] byteArgs = new byte[node.conditionElseFalse[i].nodeArgs.Length];
                            for (int j = 0; j < byteArgs.Length; j++)
                            {
                                byteArgs[j] = (byte)node.conditionElseFalse[i].nodeArgs[j];
                            }
                            int indexOfNullCharacter = Array.IndexOf(node.conditionElseFalse[i].nodeArgs, '\0');
                            string message = Encoding.UTF8.GetString(byteArgs, 1, indexOfNullCharacter-1);
                            string webhook = Encoding.UTF8.GetString(byteArgs, indexOfNullCharacter+1, byteArgs.Length-indexOfNullCharacter-1);

                            SendWebhook(webhook, message);
                            break;
                    }
                }
            }
        }

        bool IsColor(int x, int y, int r, int g, int b)
        {
            Color color = Condition.GetPixel(x, y);
            return (Math.Abs(color.R - r) + Math.Abs(color.G - g) + Math.Abs(color.B - b))<=colorThreshold;
        }

        void SendWebhook(string url, string message)
        {
            NameValueCollection nameValueCollection = new NameValueCollection
            {
                { "content", message }
            };
            new WebClient().UploadValues(url, nameValueCollection);
        }
    }
}