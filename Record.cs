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
        //int recordDelayTimer = 0;
        Stopwatch stopwatch = new Stopwatch();

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
                    case PAUSE_RECORD: // F6
                        Dispatcher.Invoke(() => { PauseRecordButton_Click(PauseRecordButton, null); });
                        break;
                    case RUN: // F7
                        Dispatcher.Invoke(() => { RunButton_Click(RunButton, null); });
                        break;
                    case STOP: // F8
                        Dispatcher.Invoke(()=> { StopButton_Click(StopButton, null); });
                        break;
                    default: // other key
                        if (listeningKeys.Contains(e.Data.RawCode) && currentStatus == MACROSTATUS.RECORDING) // if the recorder is running
                        {
                            if (!keysDown.Contains(e.Data.RawCode))
                            {
                                RecordDelay();
                                Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_KEY_DOWN}{ARGS_SEPARATOR}{(int)e.Data.KeyCode}{COMMAND_SEPARATOR}\n"); });
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
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_KEY_UP}{ARGS_SEPARATOR}{(int)e.Data.KeyCode}{COMMAND_SEPARATOR}\n"); });
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
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_MOUSE_DOWN}{ARGS_SEPARATOR}{(int)e.Data.Button}{COMMAND_SEPARATOR}\n"); });
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
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_MOUSE_UP}{ARGS_SEPARATOR}{(int)e.Data.Button}{COMMAND_SEPARATOR}\n"); });
                        mouseKeysDown.Remove(e.Data.Button);
                    }
                }
            };

            globalHook.MouseMoved += (s, e) =>
            {
                if (recordMouseLocation && currentStatus == MACROSTATUS.RECORDING)
                {
                    RecordDelay();
                    Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_LOCATION}{ARGS_SEPARATOR}{e.Data.X}{ARGS_SEPARATOR}{e.Data.Y}{COMMAND_SEPARATOR}\n"); });
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
            stopwatch.Start();
            while (currentStatus == MACROSTATUS.RECORDING)
            {
                //Thread.Sleep(DELAY_RECORD_MS);
                //recordDelayTimer += DELAY_RECORD_MS;
            }
            stopwatch.Reset();
            stopwatch.Stop();
        }

        void RecordDelay()
        {
            //if (recordDelayTimer != 0)
            //{
            //    Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_DELAY}{ARGS_SEPARATOR}{recordDelayTimer}{COMMAND_SEPARATOR}\n"); });
            //    recordDelayTimer = 0;
            //}
            if (recordDelay)
            {
                Dispatcher.Invoke(() => { ScriptBox.AppendText($"{TEXT_DELAY}{ARGS_SEPARATOR}{(int)stopwatch.Elapsed.TotalMilliseconds - CSHARP_LAG_COMPENSATION}{COMMAND_SEPARATOR}\n"); });
                stopwatch.Restart();
            }
        }
    }
}
