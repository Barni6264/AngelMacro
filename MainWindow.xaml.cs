﻿using Microsoft.Win32;
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

        enum MACROSTATUS { RECORDING, RUNNING, IDLE }
        MACROSTATUS currentStatus = MACROSTATUS.IDLE;

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
                Dispatcher.Invoke(() => { ScriptBox.AppendText($"COLOR:{cursorInfo.Item1.X}:{cursorInfo.Item1.Y}:{cursorInfo.Item2.R}:{cursorInfo.Item2.G}:{cursorInfo.Item2.B}:\n\tYOUR_MACRO_HERE (make sure to replace all ; with !):\n\tELSE_YOUR_MACRO_HERE (make sure to replace all ; with !)\n;\n"); });
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
            Countdown(3, () =>
            {
                currentStatus = MACROSTATUS.RECORDING;
                if (Dispatcher.Invoke(() => { return (bool)delayToggle.IsChecked; }))
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
            }
        }

        //DONE
        private void UnlockTextButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            UnlockTextButton.Content = "Script unlocked! Be careful!";
            ScriptBox.IsEnabled = true;
        }

        //DONE
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "macro";
            saveFileDialog.DefaultExt = FILE_EXTENSION;
            saveFileDialog.Filter = FILE_FILTER;
            saveFileDialog.Title = "Save macro file";
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
            openFileDialog.Title = "Open saved macro file";
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

        void Countdown(int seconds, Func<bool> func)
        {
            new Thread(() =>
            {
                for (int i = 0; i < seconds; i++)
                {
                    GDI.WriteTextDuration(0, 0, $"{seconds - i}s", 1000, Brushes.Black, Brushes.AliceBlue);
                    Console.Beep(1400, 200);
                    Thread.Sleep(800);
                }
                GDI.WriteTextDuration(0, 0, "GO", 1000, Brushes.Black, Brushes.AliceBlue);
                Console.Beep(2400, 200);
                func();
            }).Start();
        }
    }
}
