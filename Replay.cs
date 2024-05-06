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
        int[][] code;
        int index;

        void ExecuteCode(int[][] _code) //TODO COMPLIER rewrite from scratch
        {
            code = _code;
            while (currentStatus == Consts.MACROSTATUS.RUNNING)
            {
                index = 0;
                Execute(0, ignoreLayer:true);
            }
        }
        // I have absolutely no idea if the code below works or not, but "it should do its job"
        // TODO fix stackoverflow bug
        void Execute(int callingLayer, bool marked = false, bool ignoreLayer = false)
        {
            int layer = code[index][0];
            int nextLayer = layer;
            int currentLayerAbs = Math.Abs(layer);
            int desiredLayerAbs = ignoreLayer?0:Math.Abs(callingLayer)+1;

            // if marked, skip to the marked part or to the end of the condition or to the end of the array
            if (marked)
            {
                int i = index;
                index = code.Length; // skip to the end, if there's still code, revert this
                for (; i < code.Length; i++)
                {
                    if (code[i][0] == desiredLayerAbs * -1)
                    {
                        index = i; break; // found the "else" part, continue with the "else" part
                    }
                    if (Math.Abs(code[i][0]) < desiredLayerAbs)
                    {
                        index = i; return; // "else" not found, but there is still code, so return to the layer above
                    }
                }
            }
            // we do not need to skip the potential "else" part when executing the "if true" branch, because when the sign flips to (-) it returns

            while (currentStatus == Consts.MACROSTATUS.RUNNING && index < code.Length && nextLayer == layer && desiredLayerAbs == currentLayerAbs && (marked == false?layer>=0:true))
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
                        index++;
                        break;
                    case 2:
                        Thread.Sleep(code[index][2]);
                        index++;
                        break;
                    case 3:
                        simulator.SimulateKeyPress((KeyCode)code[index][2]);
                        index++;
                        break;
                    case 4:
                        simulator.SimulateKeyRelease((KeyCode)code[index][2]);
                        index++;
                        break;
                    case 5:
                        simulator.SimulateMouseMovement((short)code[index][2], (short)code[index][3]);
                        index++;
                        break;
                    case 6:
                        simulator.SimulateMousePress((MouseButton)code[index][2]);
                        index++;
                        break;
                    case 7:
                        simulator.SimulateMouseRelease((MouseButton)code[index][2]);
                        index++;
                        break;
                    case 8:
                        simulator.SimulateMouseWheel((short)code[index][2]);
                        index++;
                        break;
                    case 9:
                        Condition.ScreenShot();
                        index++;
                        break;
                    case 10: // OP if
                        index++;
                        Execute(layer, !IsColor(code[index-1][2], code[index-1][3], code[index-1][4], code[index-1][5], code[index-1][6]));
                        // at the end of an "Execute" call, we have to skip to the next OP that is on the same layer as we are
                        if (SkipNested(layer, currentLayerAbs))
                        {
                            return;
                        }
                        break;
                    case 11: // OP while
                        index++;
                        int whileIndexBackup = index;
                        while (IsColor(code[index-1][2], code[index-1][3], code[index-1][4], code[index-1][5], code[index - 1][6]))
                        {
                            Execute(layer);
                            index = whileIndexBackup;
                        }
                        if (SkipNested(layer, currentLayerAbs))
                        {
                            return;
                        }
                        break;
                    case 12: // OP while !condition
                        index++;
                        int whileNotIndexBackup = index;
                        while (!IsColor(code[index - 1][2], code[index - 1][3], code[index - 1][4], code[index - 1][5], code[index - 1][6]))
                        {
                            Execute(layer);
                            index = whileNotIndexBackup;
                        }
                        if (SkipNested(layer, currentLayerAbs))
                        {
                            return;
                        }
                        break;
                }

                if (index < code.Length)
                {
                    nextLayer = code[index][0];
                }
            }
        }

        bool IsColor(int x, int y, int r, int g, int b)
        {
            Color color = Condition.GetPixel(x, y);
            return (Math.Abs(color.R - r) + Math.Abs(color.G - g) + Math.Abs(color.B - b))<=colorThreshold;
        }

        bool SkipNested(int layer, int currentLayerAbs)
        {
            int i = index;
            index = code.Length; // skip to the end, if there's still code, revert this
            for (; i < code.Length; i++)
            {
                if (code[i][0] == layer)
                {
                    index = i;
                    return false; // we're good to go, continue with the execution
                }
                if (Math.Abs(code[i][0]) < currentLayerAbs)
                {
                    index = i;
                    return true;
                }
            }
            return true;
        }
    }
}