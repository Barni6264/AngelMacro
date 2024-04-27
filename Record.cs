using SharpHook;
using SharpHook.Native;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace AngelMacro
{
    public partial class MainWindow
    {
        SimpleGlobalHook globalHook = new SimpleGlobalHook();
        List<KeyCode> listeningKeys = new List<KeyCode>();
        List<KeyCode> keysDown = new List<KeyCode>();
        List<MouseButton> listeningMouseKeys = new List<MouseButton>();
        List<MouseButton> mouseKeysDown = new List<MouseButton>();
        bool recordMouseLocation, recordScrollWheel, recordDelay;
        Stopwatch stopwatch = new Stopwatch();
        // TODO string toAddToMacroText; // TODO precise recording

        #region Add/remove keys
        void AddKey(KeyCode keyCode)
        {
            listeningKeys.Add(keyCode);
        }

        void RemoveKey(KeyCode keyCode)
        {
            listeningKeys.Remove(keyCode);
        }
        void AddMouseKey(MouseButton mouseKey)
        {
            listeningMouseKeys.Add(mouseKey);
        }

        void RemoveMouseKey(MouseButton mouseKey)
        {
            listeningMouseKeys.Remove(mouseKey);
        }
        #endregion

        void InitKeys()
        {
            globalHook.KeyPressed += (s, e) =>
            {
                switch (e.RawEvent.Keyboard.RawCode)
                {
                    case Consts.PAUSE_RECORD: // F6
                        Dispatcher.Invoke(() => { PauseRecordButton_Click(PauseRecordButton, null); });
                        break;
                    case Consts.RUN: // F7
                        Dispatcher.Invoke(() => { RunButton_Click(RunButton, null); });
                        break;
                    case Consts.STOP: // F8
                        Dispatcher.Invoke(() => { StopButton_Click(StopButton, null); });
                        break;
                    default: // other key
                        if (listeningKeys.Contains(e.Data.KeyCode) && currentStatus == Consts.MACROSTATUS.RECORDING) // if the recorder is running
                        {
                            if (!keysDown.Contains(e.Data.KeyCode))
                            {
                                RecordDelay();
                                Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_KEY_DOWN}{Consts.ARGS_SEPARATOR}{e.Data.KeyCode}{Consts.COMMAND_SEPARATOR}\n"); });
                                keysDown.Add(e.Data.KeyCode);
                            }
                        }
                        break;
                }
            };

            globalHook.KeyReleased += (s, e) =>
            {
                if (listeningKeys.Contains(e.Data.KeyCode) && currentStatus == Consts.MACROSTATUS.RECORDING) // if the recorder is running
                {
                    if (keysDown.Contains(e.Data.KeyCode))
                    {
                        RecordDelay();
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_KEY_UP}{Consts.ARGS_SEPARATOR}{e.Data.KeyCode}{Consts.COMMAND_SEPARATOR}\n"); });
                        keysDown.Remove(e.Data.KeyCode);
                    }
                }
            };

            globalHook.MousePressed += (s, e) =>
            {
                if (listeningMouseKeys.Contains(e.Data.Button) && currentStatus == Consts.MACROSTATUS.RECORDING)
                {
                    if (!mouseKeysDown.Contains(e.Data.Button))
                    {
                        RecordDelay();
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_MOUSE_DOWN}{Consts.ARGS_SEPARATOR}{e.Data.Button}{Consts.COMMAND_SEPARATOR}\n"); });
                        mouseKeysDown.Add(e.Data.Button);
                    }
                }
            };

            globalHook.MouseReleased += (s, e) =>
            {
                if (listeningMouseKeys.Contains(e.Data.Button) && currentStatus == Consts.MACROSTATUS.RECORDING)
                {
                    if (mouseKeysDown.Contains(e.Data.Button))
                    {
                        RecordDelay();
                        Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_MOUSE_UP}{Consts.ARGS_SEPARATOR}{e.Data.Button}{Consts.COMMAND_SEPARATOR}\n"); });
                        mouseKeysDown.Remove(e.Data.Button);
                    }
                }
            };

            globalHook.MouseMoved += (s, e) =>
            {
                if (recordMouseLocation && currentStatus == Consts.MACROSTATUS.RECORDING)
                {
                    RecordDelay();
                    Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_LOCATION}{Consts.ARGS_SEPARATOR}{e.Data.X}{Consts.ARGS_SEPARATOR}{e.Data.Y}{Consts.COMMAND_SEPARATOR}\n"); });
                }
            };

            globalHook.MouseWheel += (s, e) =>
            {
                if (recordScrollWheel && currentStatus == Consts.MACROSTATUS.RECORDING)
                {
                    RecordDelay();
                    Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_SCROLL_WHEEL}{Consts.ARGS_SEPARATOR}{e.Data.Rotation}{Consts.COMMAND_SEPARATOR}\n"); });
                }
            };

            globalHook.RunAsync();
        }

        void RecordDelaysLoop()
        {
            stopwatch.Start();
            while (currentStatus == Consts.MACROSTATUS.RECORDING) ;
            stopwatch.Reset();
            stopwatch.Stop();
        }

        void RecordDelay()
        {
            if (recordDelay)
            {
                Dispatcher.Invoke(() => { ScriptBox.AppendText($"{Consts.TEXT_DELAY}{Consts.ARGS_SEPARATOR}{(int)stopwatch.Elapsed.TotalMilliseconds - Consts.CSHARP_LAG_COMPENSATION}{Consts.COMMAND_SEPARATOR}\n"); });
                stopwatch.Restart();
            }
        }
    }
}
