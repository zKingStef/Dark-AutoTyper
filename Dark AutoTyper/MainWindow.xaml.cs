using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using WindowsInput;

namespace Dark_AutoTyper
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

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
                await StartTyping(musicSheet, _cancellationTokenSource.Token);
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

        private async Task StartTyping(string sheet, CancellationToken token)
        {
            while (true)
            {
                for (int i = 0; i < sheet.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    char command = sheet[i];

                    if (command == '|')
                    {
                        PressKey("{ENTER}", isSpecialKey: true);

                        if (AutoClickEnabled.IsChecked == true)
                        {
                            await Task.Delay(3000, token);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                            await Task.Delay(300, token);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                            await Task.Delay(400, token);
                        }

                        if (int.TryParse(Cooldown.Text, out int cooldown) && cooldown > 0)
                        {
                            StatusText.Text = $"Status: Cooling down for {cooldown} s...";
                            await Task.Delay(TimeSpan.FromSeconds(cooldown), token);
                        }
                    }
                    else
                    {
                        PressKey(command.ToString());
                        await Task.Delay(300, token);
                    }
                }

                await Task.Delay(300, token);

                if (int.TryParse(Cooldown.Text, out int cooldown2) && cooldown2 > 0)
                {
                    StatusText.Text = $"Status: Cooling down for {cooldown2} s...";
                    await Task.Delay(TimeSpan.FromSeconds(cooldown2), token);
                }
            }
        }

        private static void PressKey(string key, bool isSpecialKey = false)
        {
            var simulator = new InputSimulator();
            Console.WriteLine($"Pressing key: {key}");

            if (isSpecialKey)
            {
                simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
            }
            else
            {
                simulator.Keyboard.TextEntry(key);
            }
        }

        private static void PressKeysSimultaneously(string keys)
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