using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Threading;

namespace AngelMacro
{
    public partial class MainWindow
    {
        SimpleGlobalHook globalHook = new SimpleGlobalHook();
        List<uint> listeningKeys = new List<uint>();
        List<uint> keysDown = new List<uint>();
        List<MouseButton> listeningMouseKeys = new List<MouseButton>();
        List<MouseButton> mouseKeysDown = new List<MouseButton>();
        bool recordMouseLocation;
        int recordDelayTimer = 0;

        void AddKey(uint keyCode)
        {
            listeningKeys.Add(keyCode);
        }

        void RemoveKey(uint keyCode)
        {
            listeningKeys.Remove(keyCode);
        }

        void InitKeys()
        {
            globalHook.KeyPressed += (s, e) =>
            {
                switch (e.RawEvent.Keyboard.RawCode)
                {
                    case 0x75: // F6
                        Dispatcher.Invoke(() => { PauseRecordButton_Click(PauseRecordButton, null); });
                        break;
                    case 0x76: // F7
                        Dispatcher.Invoke(() => { RunButton_Click(RunButton, null); });
                        break;
                    case 0x77: // F8
                        Dispatcher.Invoke(()=> { StopButton_Click(StopButton, null); });
                        break;
                    default: // other key
                        if (listeningKeys.Contains(e.Data.RawCode) && currentStatus == MACROSTATUS.RECORDING) // if the recorder is running
                        {
                            if (!keysDown.Contains(e.Data.RawCode))
                            {
                                RecordDelay();
                                Dispatcher.Invoke(() => { ScriptBox.AppendText($"KEYDOWN:{e.Data.RawCode};\n"); });
                                keysDown.Add(e.Data.RawCode);
                            }
                        }
                        break;
                }
            };

            globalHook.KeyReleased += (s, e) =>
            {
                if (listeningKeys.Contains(e.Data.RawCode) && currentStatus == MACROSTATUS.RECORDING) // if the recorder is running
                {
                    if (keysDown.Contains(e.Data.RawCode))
                    {
                        RecordDelay();
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"KEYUP:{e.Data.RawCode};\n"); });
                        keysDown.Remove(e.Data.RawCode);
                    }
                }
            };

            globalHook.MousePressed += (s, e) =>
            {
                if (listeningMouseKeys.Contains(e.Data.Button) && currentStatus == MACROSTATUS.RECORDING)
                {
                    if (!mouseKeysDown.Contains(e.Data.Button))
                    {
                        RecordDelay();
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"MOUSEDOWN:{e.Data.Button};\n"); });
                        mouseKeysDown.Add(e.Data.Button);
                    }
                }
            };

            globalHook.MouseReleased += (s, e) =>
            {
                if (listeningMouseKeys.Contains(e.Data.Button) && currentStatus == MACROSTATUS.RECORDING)
                {
                    if (mouseKeysDown.Contains(e.Data.Button))
                    {
                        RecordDelay();
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"MOUSEUP:{e.Data.Button};\n"); });
                        mouseKeysDown.Remove(e.Data.Button);
                    }
                }
            };

            globalHook.MouseMoved += (s, e) =>
            {
                if (recordMouseLocation && currentStatus == MACROSTATUS.RECORDING)
                {
                    RecordDelay();
                    Dispatcher.Invoke(() => { ScriptBox.AppendText($"LOCATION:{e.Data.X}:{e.Data.Y};\n"); });
                }
            };

            globalHook.RunAsync();
        }

        void AddMouseKey(MouseButton mouseKey)
        {
            listeningMouseKeys.Add(mouseKey);
        }

        void RemoveMouseKey(MouseButton mouseKey)
        {
            listeningMouseKeys.Remove(mouseKey);
        }

        void RecordDelaysLoop()
        {
            while (currentStatus == MACROSTATUS.RECORDING)
            {
                Thread.Sleep(50);
                recordDelayTimer += 50;
            }
        }

        void RecordDelay()
        {
            if (recordDelayTimer != 0)
            {
                Dispatcher.Invoke(() => { ScriptBox.AppendText($"DELAY:{recordDelayTimer};\n"); });
                recordDelayTimer = 0;
            }
        }
    }
}
