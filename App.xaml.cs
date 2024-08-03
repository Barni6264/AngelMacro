using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace AngelMacro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            //if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower() == "hu")
            //{
            //    System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("hu-HU");
            //}
        }

        void AppStartup (object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            if (e.Args.Length == 1)
            {
                try
                {
                    mainWindow.ScriptBox.Text = File.ReadAllText(e.Args[0]);
                    string rawMacro = mainWindow.ScriptBox.Text;
                    
                    rawMacro = ANMLangCompiler.ConvertStringToArgs(rawMacro);
                    rawMacro = ANMLangCompiler.CleanCode(rawMacro);
                    mainWindow.compiledCode = ANMLangCompiler.CompileCode(rawMacro, Dispatcher, new ProgressBar());

                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.RunButton.Content = Consts.RUN_MACRO_BUTTON_TEXT;
                    });
                    mainWindow.codeChanged = false;

                    mainWindow.FastStart.IsChecked = true;

                    Console.Beep(2400, 200);
                    Thread.Sleep(200);
                    Console.Beep(2400, 200);

                    mainWindow.RunButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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
                    else if (ex is FileNotFoundException)
                    {
                        MessageBox.Show($"{Consts.FILE_ERROR_TEXT}\n{ex.Message}", Consts.FILE_ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
