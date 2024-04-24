using Microsoft.Win32;
using SharpHook.Native;
using System;
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
        Thread countdownThread;

        public MainWindow()
        {
            InitializeComponent();

            // generates checkboxes under keys - OLD
            string[] names = Enum.GetNames(typeof(KeyCode));
            for (int i = 0; i < names.Length; i++)
            {
                CheckBox boxToAdd = new CheckBox();
                boxToAdd.Content = names[i];
                boxToAdd.Name = names[i];
                boxToAdd.Click += delegate // keyboard
                {
                    if ((bool)boxToAdd.IsChecked)
                    {
                        AddKey((KeyCode)Enum.Parse(typeof(KeyCode), boxToAdd.Name));
                    }
                    else
                    {
                        RemoveKey((KeyCode)Enum.Parse(typeof(KeyCode), boxToAdd.Name));
                    }
                };
                Keys.Children.Add(boxToAdd);
            }

            // registers hotkeys for the buttons
            InitKeys();

            // exits when X is pressed
            Closing += delegate
            {
                // TODO exit
                Environment.Exit(0);
            };
        }

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

        private void ToggleMouseLocation(object o, RoutedEventArgs e)
        {
            recordMouseLocation = (bool)mouseLocationToggle.IsChecked;
        }

        private void ToggleScrollWheel(object o, RoutedEventArgs e)
        {
            recordScrollWheel = (bool)scrollWheelToggle.IsChecked;
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)autoMinimize.IsChecked)
            {
                WindowState = WindowState.Minimized;
            }
            Countdown(5, () =>
            {
                Tuple<System.Drawing.Point, Color> cursorInfo = Condition.GetCursorInfo();
                Dispatcher.Invoke(() =>
                {
                    switch (((MenuItem)sender).Name)
                    {
                        case "AddConditionButton":
                            ScriptBox.AppendText($"{TEXT_COLOR}{ARGS_SEPARATOR}{cursorInfo.Item1.X}{ARGS_SEPARATOR}{cursorInfo.Item1.Y}{ARGS_SEPARATOR}{cursorInfo.Item2.R}{ARGS_SEPARATOR}{cursorInfo.Item2.G}{ARGS_SEPARATOR}{cursorInfo.Item2.B}{ARGS_SEPARATOR}\n{CONDITIONAL_MACRO_GUIDE}{COMMAND_SEPARATOR}\n");
                            break;
                        case "AddWhileButton":
                            ScriptBox.AppendText($"{TEXT_WHILE}{ARGS_SEPARATOR}{cursorInfo.Item1.X}{ARGS_SEPARATOR}{cursorInfo.Item1.Y}{ARGS_SEPARATOR}{cursorInfo.Item2.R}{ARGS_SEPARATOR}{cursorInfo.Item2.G}{ARGS_SEPARATOR}{cursorInfo.Item2.B}{ARGS_SEPARATOR}\n{WHILE_MACRO_GUIDE}{COMMAND_SEPARATOR}\n");
                            break;
                        case "AddUntilButton":
                            ScriptBox.AppendText($"{TEXT_UNTIL}{ARGS_SEPARATOR}{cursorInfo.Item1.X}{ARGS_SEPARATOR}{cursorInfo.Item1.Y}{ARGS_SEPARATOR}{cursorInfo.Item2.R}{ARGS_SEPARATOR}{cursorInfo.Item2.G}{ARGS_SEPARATOR}{cursorInfo.Item2.B}{ARGS_SEPARATOR}\n{WHILE_MACRO_GUIDE}{COMMAND_SEPARATOR}\n");
                            break;
                    }
                    if ((bool)autoMinimize.IsChecked)
                    {
                        WindowState = WindowState.Normal;
                        Activate();
                    }
                });
                return true;
            });
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            PauseRecordButton.IsEnabled = true;
            RunButton.IsEnabled = false;
            FileMenu.IsEnabled = false;
            if ((bool)autoMinimize.IsChecked)
            {
                WindowState = WindowState.Minimized;
            }
            recordDelay = (bool)delayToggle.IsChecked;
            Countdown((bool)fastStart.IsChecked ? 0 : 3, () =>
            {
                currentStatus = MACROSTATUS.RECORDING;
                if (recordDelay)
                {
                    RecordDelaysLoop();
                }
                return true;
            });
        }

        private void PauseRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled == true && this.IsEnabled)
            {
                ((Button)sender).IsEnabled = false;
                RecordButton.IsEnabled = true;
                RunButton.IsEnabled = true;
                FileMenu.IsEnabled = true;
                currentStatus = MACROSTATUS.IDLE;
                if ((bool)autoMinimize.IsChecked)
                {
                    WindowState = WindowState.Normal;
                    Activate();
                }
            }
        }


        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled == true && this.IsEnabled)
            {
                ((Button)sender).IsEnabled = false;
                StopButton.IsEnabled = true;
                RecordButton.IsEnabled = false;
                FileMenu.IsEnabled = false;
                if ((bool)autoMinimize.IsChecked)
                {
                    WindowState = WindowState.Minimized;
                }

                int.TryParse(ColorThresholdBox.Text, out colorThreshold);

                Countdown((bool)fastStart.IsChecked ? 0 : 3, () =>
                {
                    currentStatus = MACROSTATUS.RUNNING;
                    ExecuteMacro(Dispatcher.Invoke(() => { return ScriptBox.Text; }), COMMAND_SEPARATOR, ARGS_SEPARATOR);
                    return true;
                });
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled == true && this.IsEnabled)
            {
                ((Button)sender).IsEnabled = false;
                RunButton.IsEnabled = true;
                RecordButton.IsEnabled = true;
                FileMenu.IsEnabled = true;
                currentStatus = MACROSTATUS.IDLE;
                countdownThread.Interrupt();
                if ((bool)autoMinimize.IsChecked)
                {
                    WindowState = WindowState.Normal;
                    Activate();
                }
            }
        }

        private void UnlockTextButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            UnlockTextButton.Content = UNLOCKED_SCRIPT_WARNING;
            ScriptBox.IsEnabled = true;
        }

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

        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        void Countdown(int seconds, Func<bool> func)
        {
            this.IsEnabled = false;

            countdownThread = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < seconds; i++)
                    {
                        GDI.WriteTextDuration(0, 0, $"{seconds - i}{GDI_SECOND}", 1000, Brushes.Black, Brushes.AliceBlue);
                        Console.Beep(1400, 200);
                        Thread.Sleep(800);
                    }
                    if (seconds > 0)
                    {
                        GDI.WriteTextDuration(0, 0, GDI_START, 1000, Brushes.Black, Brushes.AliceBlue);
                        Console.Beep(2400, 200);
                    }

                    Dispatcher.Invoke(() => { this.IsEnabled = true; });
                    func();
                }
                catch (Exception ex)
                {
                    if (ex is not ThreadInterruptedException)
                    {
                        MessageBox.Show(ex.Message, COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
            countdownThread.Start();
        }

        private void AddEndMacroButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{TEXT_END}{COMMAND_SEPARATOR}\n");
        }

        private void AddColorThresholdChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{TEXT_COLOR_THRESHOLD_CHANGE}{ARGS_SEPARATOR}5{COMMAND_SEPARATOR}\n");
        }
    }
}
