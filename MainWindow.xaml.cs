using Microsoft.Win32;
using SharpHook;
using SharpHook.Native;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace AngelMacro
{
    public partial class MainWindow : Window
    {
        // all UI stuff and connections to core functions

        MACROSTATUS currentStatus = MACROSTATUS.IDLE;
        bool recordDelay;

        public MainWindow()
        {
            InitializeComponent();

            // generates checkboxes under keys
            for (int i = 0; i < KEYCODE_VALUES.Length; i++)
            {
                CheckBox boxToAdd = new CheckBox();
                boxToAdd.Content = KEYCODE_NAMES[i];
                boxToAdd.Name = KEYCODE_NAMES[i];
                boxToAdd.Click += delegate // keyboard
                {
                    if ((bool)boxToAdd.IsChecked)
                    {
                        AddKey(KEYCODE_VALUES[Array.IndexOf(KEYCODE_NAMES, boxToAdd.Name)]);
                    }
                    else
                    {
                        RemoveKey(KEYCODE_VALUES[Array.IndexOf(KEYCODE_NAMES, boxToAdd.Name)]);
                    }
                };
                Keys.Children.Add(boxToAdd);
            }

            // registers hotkeys for the buttons
            InitKeys();

            // exits when X is pressed
            Closing += delegate
            {
                //if (MessageBox.Show(""))
                Environment.Exit(0);
            };
        }

        //DONE
        private void ToggleLeft(object o, RoutedEventArgs e)
        {
            if ((bool)leftButtonToggle.IsChecked)
            {
                AddMouseKey(MouseButton.Button1);
            }
            else
            {
                RemoveMouseKey(MouseButton.Button1);
            }
        }

        //DONE
        private void ToggleRight(object o, RoutedEventArgs e)
        {
            if ((bool)rightButtonToggle.IsChecked)
            {
                AddMouseKey(MouseButton.Button2);
            }
            else
            {
                RemoveMouseKey(MouseButton.Button2);
            }
        }

        //DONE
        private void ToggleMouseLocation(object o, RoutedEventArgs e)
        {
            if ((bool)mouseLocationToggle.IsChecked)
            {
                recordMouseLocation = true;
            }
            else
            {
                recordMouseLocation = false;
            }
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            Countdown(5, () =>
            {
                Tuple<System.Drawing.Point, Color> cursorInfo = Condition.GetCursorInfo();
                Dispatcher.Invoke(() =>
                {
                    ScriptBox.AppendText($"{TEXT_COLOR}{ARGS_SEPARATOR}{cursorInfo.Item1.X}{ARGS_SEPARATOR}{cursorInfo.Item1.Y}{ARGS_SEPARATOR}{cursorInfo.Item2.R}{ARGS_SEPARATOR}{cursorInfo.Item2.G}{ARGS_SEPARATOR}{cursorInfo.Item2.B}{ARGS_SEPARATOR}\n{CONDITIONAL_MACRO_GUIDE}{COMMAND_SEPARATOR}\n");
                    WindowState = WindowState.Normal;
                    Activate();
                });
                return true;
            });
        }

        //DONE
        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            PauseRecordButton.IsEnabled = true;
            RunButton.IsEnabled = false;
            FileMenu.IsEnabled = false;
            WindowState = WindowState.Minimized;
            recordDelay = (bool)delayToggle.IsChecked;
            Countdown(3, () =>
            {
                currentStatus = MACROSTATUS.RECORDING;
                if (recordDelay)
                {
                    RecordDelaysLoop();
                }
                return true;
            });
        }

        //DONE
        private void PauseRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled == true)
            {
                ((Button)sender).IsEnabled = false;
                RecordButton.IsEnabled = true;
                RunButton.IsEnabled = true;
                FileMenu.IsEnabled = true;
                currentStatus = MACROSTATUS.IDLE;
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled == true)
            {
                ((Button)sender).IsEnabled = false;
                StopButton.IsEnabled = true;
                RecordButton.IsEnabled = false;
                FileMenu.IsEnabled = false;
                WindowState = WindowState.Minimized;
                Countdown(3, () =>
                {
                    currentStatus = MACROSTATUS.RUNNING;
                    ExecuteMacro(Dispatcher.Invoke(() => { return ScriptBox.Text; }), COMMAND_SEPARATOR, ARGS_SEPARATOR);
                    return true;
                });
            }
        }

        //DONE
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled == true)
            {
                ((Button)sender).IsEnabled = false;
                RunButton.IsEnabled = true;
                RecordButton.IsEnabled = true;
                FileMenu.IsEnabled = true;
                currentStatus = MACROSTATUS.IDLE;
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        //DONE
        private void UnlockTextButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            UnlockTextButton.Content = UNLOCKED_SCRIPT_WARNING;
            ScriptBox.IsEnabled = true;
        }

        //DONE
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = FILE_NAME;
            saveFileDialog.DefaultExt = FILE_EXTENSION;
            saveFileDialog.Filter = FILE_FILTER;
            saveFileDialog.Title = SAVE_FILE_TITLE;
            saveFileDialog.FileOk += delegate
            {
                File.WriteAllText(saveFileDialog.FileName, ScriptBox.Text);
            };
            saveFileDialog.ShowDialog();
        }

        //DONE
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = true;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = FILE_EXTENSION;
            openFileDialog.Filter = FILE_FILTER;
            openFileDialog.Title = OPEN_FILE_TITLE;
            openFileDialog.FileOk += delegate
            {
                ScriptBox.Text = File.ReadAllText(openFileDialog.FileName);
            };
            openFileDialog.ShowDialog();
        }

        //DONE
        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        //DONE
        void Countdown(int seconds, Func<bool> func)
        {
            new Thread(() =>
            {
                for (int i = 0; i < seconds; i++)
                {
                    GDI.WriteTextDuration(0, 0, $"{seconds - i}{GDI_SECOND}", 1000, Brushes.Black, Brushes.AliceBlue);
                    Console.Beep(1400, 200);
                    Thread.Sleep(800);
                }
                GDI.WriteTextDuration(0, 0, GDI_START, 1000, Brushes.Black, Brushes.AliceBlue);
                Console.Beep(2400, 200);
                func();
            }).Start();
        }
    }
}
