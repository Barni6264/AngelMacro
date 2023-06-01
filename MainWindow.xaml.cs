using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace AngelMacro
{
    public partial class MainWindow : Window
    {
        // all UI stuff and connections to core functions
        public MainWindow()
        {
            InitializeComponent();

            // generates checkboxes under keys
            for (int i = 0; i < keys.Length; i++)
            {
                CheckBox boxToAdd = new CheckBox();
                boxToAdd.Content = keys[i].name;
                boxToAdd.Name = $"_{keys[i].hex}";
                boxToAdd.IsChecked = keys[i].check;
                Keys.Children.Add(boxToAdd);
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            StopRecordButton.IsEnabled = false;
            PauseRecordButton.IsEnabled = true;
            RunButton.IsEnabled = false;
            FileMenu.IsEnabled = false;
            WindowState = WindowState.Minimized;
            Countdown(3);
        }
        private void PauseRecordButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            RecordButton.IsEnabled = true;
            StopRecordButton.IsEnabled = true;
        }

        private void StopRecordButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            RecordButton.IsEnabled = true;
            RunButton.IsEnabled = true;
            FileMenu.IsEnabled = true;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            StopButton.IsEnabled = true;
            RecordButton.IsEnabled = false;
            FileMenu.IsEnabled = false;
            WindowState = WindowState.Minimized;
            Countdown(3);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            RunButton.IsEnabled = true;
            RecordButton.IsEnabled = true;
            FileMenu.IsEnabled = true;
        }

        private void UnlockTextButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            UnlockTextButton.Content = "Script unlocked! Be careful!";
            ScriptBox.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = true;
        }

        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        void Countdown(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                Console.Beep(1400,200);
                Thread.Sleep(800);
            }
            Console.Beep(2400, 400);
        }
    }
}
