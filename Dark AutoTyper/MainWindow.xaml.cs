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
            int startingCooldown = 3;
            string musicSheet = MusicSheetInput.Text;

            if (string.IsNullOrWhiteSpace(musicSheet))
            {
                StatusText.Text = "Status: Please enter a valid template!";
                return;
            }

            await RunCountdown(
                                startingCooldown,
                                "Status: Preparing to type, remaining",
                                _cancellationTokenSource.Token
                            );

            await Task.Delay(3000);

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartTyping(musicSheet, _cancellationTokenSource.Token);
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
                // Read UI values once per loop iteration
                bool autoClick = AutoClickEnabled.IsChecked == true;

                int perMessageCooldown = 0;
                int afterRunCooldown = 0;

                int.TryParse(CooldownPerMessage.Text, out perMessageCooldown);
                int.TryParse(CooldownAfterRun.Text, out afterRunCooldown);

                for (int i = 0; i < sheet.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    char command = sheet[i];

                    // Show typing status for normal characters
                    StatusText.Text = "Status: Typing...";

                    if (command == '|')
                    {
                        // Inform that message is being sent
                        StatusText.Text = "Status: Sending message...";

                        // Send enter key
                        PressKey("{ENTER}", isSpecialKey: true);

                        // Auto click if enabled
                        if (autoClick)
                        {
                            // Wait before clicking so UI can react
                            await Task.Delay(3000, token);

                            // Mouse down
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                            await Task.Delay(300, token);

                            // Mouse up
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                            await Task.Delay(400, token);
                        }

                        // Check if this is the last message (no further '|' in the sheet)
                        bool isLastMessage = sheet.IndexOf('|', i + 1) == -1;

                        // Per-message cooldown only if there is another message after this one
                        if (!isLastMessage && perMessageCooldown > 0)
                        {
                            await RunCountdown(
                                perMessageCooldown,
                                "Status: Cooling down, remaining",
                                token
                            );
                        }
                    }
                    else
                    {
                        // Type normal character
                        PressKey(command.ToString());

                        // Delay between characters to simulate typing speed
                        await Task.Delay(300, token);
                    }
                }

                // Small technical delay after a full run (optional)
                await Task.Delay(300, token);

                // After-run cooldown (always after completing the sheet)
                if (afterRunCooldown > 0)
                {
                    await RunCountdown(
                        afterRunCooldown,
                        "Status: Cooling down after run, remaining",
                        token
                    );
                }


                StatusText.Text = "Status: Run finished";
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


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Not implemented yet
        }


        private async Task RunCountdown(int seconds, string prefix, CancellationToken token)
        {
            // Simple countdown that updates StatusText once per second
            for (int remaining = seconds; remaining > 0; remaining--)
            {
                token.ThrowIfCancellationRequested();

                StatusText.Text = $"{prefix} {remaining}s...";
                await Task.Delay(1000, token); // Wait 1 second between updates
            }
        }
    }
}