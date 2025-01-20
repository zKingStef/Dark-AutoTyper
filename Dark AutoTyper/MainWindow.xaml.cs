using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;

namespace Dark_AutoTyper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string musicSheet = MusicSheetInput.Text;

            if (string.IsNullOrWhiteSpace(musicSheet))
            {
                StatusText.Text = "Status: Please enter a valid template!";
                return;
            }

            StatusText.Text = "Status: Preparing to type...";

            await Task.Delay(3000);

            StatusText.Text = "Status: Typing...";
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await PlayMusicSheet(musicSheet, _cancellationTokenSource.Token);
                StatusText.Text = "Status: Finished typing!";
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = "Status: Typing stopped.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Status: Error - {ex.Message}";
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task PlayMusicSheet(string sheet, CancellationToken token)
        {
            for (int i = 0; i < sheet.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                char command = sheet[i];

                await Task.Delay(200, token);

                if (command == ' ')
                {
                    await Task.Delay(300, token);
                }
                else
                {
                    PressKey(command.ToString());
                    await Task.Delay(100, token);
                }
            }
        }

        private void PressKey(string key)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing key: {key}");

            simulator.Keyboard.TextEntry(key);
        }

        private void PressKeysSimultaneously(string keys)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing keys simultaneously: {keys}");

            foreach (char key in keys)
            {
                simulator.Keyboard.TextEntry(key.ToString());
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MusicSheetInput.Text = File.ReadAllText(openFileDialog.FileName);

                    Title.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                    StatusText.Text = "Status: Type Sheet loaded successfully!";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Status: Error loading file - {ex.Message}";
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Title.Text))
            {
                StatusText.Text = "Status: Please enter a title for the type sheet.";
                return;
            }

            string defaultFileName = Title.Text.Trim();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, MusicSheetInput.Text);
                    StatusText.Text = "Status: Type sheet saved successfully!";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Status: Error saving file - {ex.Message}";
                }
            }
        }
    }
}