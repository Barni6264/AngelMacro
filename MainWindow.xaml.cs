﻿using Microsoft.Win32;
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

        Consts.MACROSTATUS currentStatus = Consts.MACROSTATUS.IDLE;
        Thread countdownThread;
        public bool codeChanged = true;
        public TreeNode compiledCode;

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

        private void ToggleLeft_Click(object o, RoutedEventArgs e)
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

        private void ToggleRight_Click(object o, RoutedEventArgs e)
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

        private void ToggleMouseLocation_Click(object o, RoutedEventArgs e)
        {
            recordMouseLocation = (bool)mouseLocationToggle.IsChecked;
        }

        private void ToggleScrollWheel_Click(object o, RoutedEventArgs e)
        {
            recordScrollWheel = (bool)scrollWheelToggle.IsChecked;
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)AutoMinimize.IsChecked)
            {
                WindowState = WindowState.Minimized;
            }
            Countdown(5, () =>
            {
                Condition.ScreenShot();
                Tuple<System.Drawing.Point, Color> cursorInfo = Condition.GetCursorInfo();
                Dispatcher.Invoke(() =>
                {
                    switch (((MenuItem)sender).Name)
                    {
                        case "AddConditionButton":
                            ScriptBox.AppendText($"{Consts.TEXT_COLOR}{Consts.ARGS_SEPARATOR}{cursorInfo.Item1.X}{Consts.ARGS_SEPARATOR}{cursorInfo.Item1.Y}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.R}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.G}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.B}{Consts.ARGS_SEPARATOR}\n{Consts.CONDITIONAL_MACRO_GUIDE}\n");
                            break;
                        case "AddWhileButton":
                            ScriptBox.AppendText($"{Consts.TEXT_WHILE}{Consts.ARGS_SEPARATOR}{cursorInfo.Item1.X}{Consts.ARGS_SEPARATOR}{cursorInfo.Item1.Y}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.R}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.G}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.B}{Consts.ARGS_SEPARATOR}\n{Consts.WHILE_MACRO_GUIDE}\n");
                            break;
                        case "AddUntilButton":
                            ScriptBox.AppendText($"{Consts.TEXT_UNTIL}{Consts.ARGS_SEPARATOR}{cursorInfo.Item1.X}{Consts.ARGS_SEPARATOR}{cursorInfo.Item1.Y}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.R}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.G}{Consts.ARGS_SEPARATOR}{cursorInfo.Item2.B}{Consts.ARGS_SEPARATOR}\n{Consts.WHILE_MACRO_GUIDE}\n");
                            break;
                    }
                    if ((bool)AutoMinimize.IsChecked)
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
            toAddToMacroText = new System.Text.StringBuilder();
            if ((bool)AutoMinimize.IsChecked)
            {
                WindowState = WindowState.Minimized;
            }
            recordDelay = (bool)DelayToggle.IsChecked;
            Countdown((bool)FastStart.IsChecked ? 0 : 3, () =>
            {
                currentStatus = Consts.MACROSTATUS.RECORDING;
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
                currentStatus = Consts.MACROSTATUS.IDLE;
                ScriptBox.Text += toAddToMacroText.ToString();
                toAddToMacroText.Clear();
                if ((bool)AutoMinimize.IsChecked)
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
                if (codeChanged)
                {
                    this.IsEnabled = false;

                    CompilingCodeMessageBox messageBox = new CompilingCodeMessageBox();
                    messageBox.Show();

                    string rawMacro = ScriptBox.Text;
                    new Thread(() =>
                    {
                        try
                        {
                            rawMacro = ANMLangCompiler.ConvertStringToArgs(rawMacro);
                            rawMacro = ANMLangCompiler.CleanCode(rawMacro);
                            compiledCode = ANMLangCompiler.CompileCode(rawMacro, Dispatcher, messageBox.CompilingProgress);

                            // if the code above ran successfully
                            Dispatcher.Invoke(() =>
                            {
                                ((Button)sender).Content = Consts.RUN_MACRO_BUTTON_TEXT;
                            });
                            codeChanged = false;
                        }
                        catch (Exception ex)
                        {
                            if (ex is InvalidCommandException)
                            {
                                MessageBox.Show($"{Consts.COMMAND_ERROR_TEXT}\n{Consts.ERROR_INVALID_OPERATION}", Consts.COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else if (ex is ArgumentOutOfRangeException)
                            {
                                MessageBox.Show($"{Consts.COMMAND_ERROR_TEXT}\n{Consts.ERROR_SYNTAX_ERROR}\n{ex.Message}", Consts.COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        Dispatcher.Invoke(() => { this.IsEnabled = true; messageBox.Close(); });
                    }).Start();

                    return;
                }

                ((Button)sender).IsEnabled = false;
                StopButton.IsEnabled = true;
                RecordButton.IsEnabled = false;
                FileMenu.IsEnabled = false;
                if ((bool)AutoMinimize.IsChecked)
                {
                    WindowState = WindowState.Minimized;
                }

                int.TryParse(ColorThresholdBox.Text, out colorThreshold);

                Countdown((bool)FastStart.IsChecked ? 0 : 3, () =>
                {
                    currentStatus = Consts.MACROSTATUS.RUNNING;
                    try
                    {
                        ExecuteCode(compiledCode);
                    }
                    catch (Exception ex)
                    {
                        if (ex is not ThreadInterruptedException)
                            MessageBox.Show(ex.Message, Consts.COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
                currentStatus = Consts.MACROSTATUS.IDLE;
                countdownThread.Interrupt();
                if ((bool)AutoMinimize.IsChecked)
                {
                    WindowState = WindowState.Normal;
                    Activate();
                }
            }
        }

        private void UnlockTextButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            UnlockTextButton.Content = Consts.UNLOCKED_SCRIPT_WARNING;
            ScriptBox.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = Consts.FILE_NAME;
            saveFileDialog.DefaultExt = Consts.FILE_EXTENSION;
            saveFileDialog.Filter = Consts.FILE_FILTER;
            saveFileDialog.Title = Consts.SAVE_FILE_TITLE;
            saveFileDialog.FileOk += delegate
            {
                File.WriteAllText(saveFileDialog.FileName, ScriptBox.Text);
            };
            saveFileDialog.ShowDialog();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = Consts.FILE_EXTENSION;
            openFileDialog.Filter = Consts.FILE_FILTER;
            openFileDialog.Title = Consts.OPEN_FILE_TITLE;
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
                        GDI.WriteTextDuration(0, 0, $"{seconds - i}{Consts.GDI_SECOND}", 1000, Brushes.Black, Brushes.AliceBlue);
                        Console.Beep(1400, 200);
                        Thread.Sleep(800);
                    }
                    if (seconds > 0)
                    {
                        GDI.WriteTextDuration(0, 0, Consts.GDI_START, 1000, Brushes.Black, Brushes.AliceBlue);
                        Console.Beep(2400, 200);
                    }

                    Dispatcher.Invoke(() => { this.IsEnabled = true; });
                    func();
                }
                    catch (Exception ex)
                {
                        if (ex is not ThreadInterruptedException)
                {
                    MessageBox.Show(ex.Message, Consts.COMMAND_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
    });
            countdownThread.Start();
        }

        private void AddEndMacroButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{Consts.TEXT_END}{Consts.COMMAND_SEPARATOR}\n");
        }

        private void AddColorThresholdChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{Consts.TEXT_COLOR_THRESHOLD_CHANGE}{Consts.ARGS_SEPARATOR}5{Consts.COMMAND_SEPARATOR}\n");
        }

        private void ScriptBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            codeChanged = true;
            RunButton.Content = Consts.COMPILE_CODE_BUTTON_TEXT;
        }

        private void AddScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{Consts.TEXT_SCREENSHOT}{Consts.COMMAND_SEPARATOR}\n");
        }

        private void InvertAllKeysButton_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox;
            for (int i = 2; i < Keys.Children.Count; i++) // starts at 2, because the first is the button itself, the second is the separator
            {
                checkBox = (CheckBox)Keys.Children[i];
                checkBox.IsChecked = !checkBox.IsChecked; 
                checkBox.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
            }
        }

        private void AddBreakButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{Consts.TEXT_BREAK}{Consts.COMMAND_SEPARATOR}\n");
        }

        private void AddWebhookButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptBox.AppendText($"{Consts.WEBHOOK_GUIDE}\n");
        }
    }
}
