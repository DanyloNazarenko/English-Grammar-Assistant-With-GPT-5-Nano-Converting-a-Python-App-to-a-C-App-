using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents globalHook;
        private IntPtr nextClipboardViewer;
        private bool isDragging = false;
        public IntPtr originalWindow;
        public string SelText = "";
        public bool flatingmode = false;
        public Form1()
        {
            
            InitializeComponent();
            SubscribeGlobalHooks();
            SetupClipboardViewer();

            //Rectangle bounds = new Rectangle(0, 0, this.btnFloatingMode.Width, this.btnFloatingMode.Height);
            //GraphicsPath path = new GraphicsPath();
            //path.StartFigure();
            //path.AddArc(bounds.Left, bounds.Top, 16, 16, 180, 90);
            //path.AddArc(bounds.Right - 16, bounds.Top, 16, 16, 270, 90);
            //path.AddArc(bounds.Right - 16, bounds.Bottom - 16, 16, 16, 0, 90);
            //path.AddArc(bounds.Left, bounds.Bottom - 16, 16, 16, 90, 90);
            //path.CloseFigure();
            //this.btnFloatingMode.Region = new Region(path);
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Font = new Font("Segoe UI", 9F, GraphicsUnit.Point); 
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.MaximizeBox = false;
            this.MinimumSize = new Size(500, 400);

        }

        #region Global Hooks
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnsubscribeGlobalHooks();
            RemoveClipboardViewer();
            base.OnFormClosing(e);
        }
        private void SubscribeGlobalHooks()
        {
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
            globalHook.MouseDoubleClick += GlobalHook_MouseDoubleClick;
            globalHook.MouseDown += GlobalHook_MouseDown;
            globalHook.MouseUp += GlobalHook_MouseUp;

        }
        private void UnsubscribeGlobalHooks()
        {
            if (globalHook != null)
            {
                globalHook.KeyDown -= GlobalHook_KeyDown;
                globalHook.MouseDoubleClick -= GlobalHook_MouseDoubleClick;
                globalHook.MouseDown -= GlobalHook_MouseDown;
                globalHook.MouseUp -= GlobalHook_MouseUp;
                globalHook.Dispose();
            }
        }

        private async void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (flatingmode)
            {
                if (e.Control && e.KeyCode == Keys.A)
                {
                    TryGetSelectedText("");

                    await Task.Delay(100);

                    SendCtrlC();
                }
                else if (e.Control && e.KeyCode == Keys.C)
                {
                    TryGetSelectedText("");
                }
            }
        }
        private void SendCtrlC()
        {
            SendKeys.SendWait("^c");
        }
        private void GlobalHook_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TryGetSelectedText("");
        }

        private void GlobalHook_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
            }
        }

        private async void GlobalHook_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDragging)
            {
                isDragging = false;
                await Task.Delay(100);
                TryGetSelectedText("Mouse drag selection detected");
                if (flatingmode)
                {
                    SendCtrlC();
                }
                
            }
        }

        #endregion

        #region Clipboard Monitoring

        private void SetupClipboardViewer()
        {
            nextClipboardViewer = NativeMethods.SetClipboardViewer(this.Handle);
        }

        private void RemoveClipboardViewer()
        {
            NativeMethods.ChangeClipboardChain(this.Handle, nextClipboardViewer);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DRAWCLIPBOARD = 0x0308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    OnClipboardChanged();
                    NativeMethods.SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        NativeMethods.SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
            }
            const int WM_DPICHANGED = 0x02E0;

            if (m.Msg == WM_DPICHANGED)
            {
                var newDpi = (int)(m.WParam.ToInt64() & 0xFFFF);
                float scalingFactor = newDpi / 96f; // 96 is default DPI

                this.Font = new Font("Segoe UI", 9f * scalingFactor, GraphicsUnit.Point);

                //// Resize the window to suggested dimensions
                RECT suggestedRect = Marshal.PtrToStructure<RECT>(m.LParam);
                this.SetBounds(suggestedRect.Left, suggestedRect.Top,
                               suggestedRect.Right - suggestedRect.Left,
                               suggestedRect.Bottom - suggestedRect.Top);
            }
            base.WndProc(ref m);
        }
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private void OnClipboardChanged()
        {
            if (flatingmode)
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    AppendDetectedText(clipboardText);
                }
            }
        }

        #endregion

        #region UI Automation Text Extraction

        private void TryGetSelectedText(string trigger)
        {
                try
                {
                    originalWindow = NativeMethods.GetForegroundWindow();
                    var focusedElement = AutomationElement.FocusedElement;
                    if (focusedElement == null)
                    {
                        AppendDetectedText($"{trigger}: No focused element.");
                        return;
                    }

                    if (focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out object patternObj))
                    {
                        var textPattern = (TextPattern)patternObj;
                        var selectionRanges = textPattern.GetSelection();

                        if (selectionRanges.Length > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var range in selectionRanges)
                            {
                                sb.Append(range.GetText(-1));
                            }
                            string selectedText = sb.ToString();

                            if (!string.IsNullOrWhiteSpace(selectedText))
                            {
                                AppendDetectedText($"{trigger}: {selectedText}");
                                return;
                            }
                        }
                    }

                    if (focusedElement.TryGetCurrentPattern(ValuePattern.Pattern, out object valPatternObj))
                    {
                        var valPattern = (ValuePattern)valPatternObj;
                        string val = valPattern.Current.Value;
                        return;
                    }

                    AppendDetectedText("");
                }
                catch (Exception ex)
                {
                    AppendDetectedText(ex.ToString());
                }
        }

        #endregion

        #region UI Helper

        private void AppendDetectedText(string text)
        {
            SelText = text;
        }

        #endregion
    
        
       

        private async void btnFixText_Click(object sender, EventArgs e)
        {
            string input = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please enter text.");
                return;
            }

            btnFixText.Enabled = false;
            btnFixText.Text = "Fixing...";
            lblFixedText.Text = "Processing... Please wait";
            txtOutput.Text = "";
            StringBuilder buffer = new StringBuilder();
            DateTime lastFlush = DateTime.Now;

            var client = new GptNanoStreamingClient();


            await client.StreamGrammarCorrectionAsync(
            inputText: input,
            onToken: token =>
            {
                buffer.Append(token);

                if ((DateTime.Now - lastFlush).TotalMilliseconds > 100)
                {
                    this.Invoke(() =>
                    {
                        txtOutput.AppendText(buffer.ToString());
                        buffer.Clear();
                    });
                    lastFlush = DateTime.Now;
                }
            },
            onComplete: error =>
            {
                this.Invoke(() =>
                {
                    if (buffer.Length > 0)
                        txtOutput.AppendText(buffer.ToString());
                    if (!string.IsNullOrEmpty(error))
                        txtOutput.AppendText($"\n[Error] {error}");

                    btnFixText.Text = "Fix Text";
                    lblFixedText.Text = "Fixed text:";
                    btnFixText.Enabled = true;
                });
            }
            );
        }

        private void btnCopyFixedText_Click(object sender, EventArgs e)
        {
            btnCopyFixedText.Enabled = false;
            lblFixedText.Text = "Copying...";

            if (!string.IsNullOrWhiteSpace(txtOutput.Text))
            {
                bool success = ClipboardHelper.ForceSetClipboardText(txtOutput.Text);

                if (success)
                    lblFixedText.Text = "Copied";
                else
                {
                    lblFixedText.Text = "Copy failed";
                    MessageBox.Show("Failed to copy to clipboard. Try clicking again.");
                }
            }
            else
            {
                MessageBox.Show("Nothing to copy.");
            }
            btnCopyFixedText.Enabled = true;
        }


        private void btnFloatingMode_Click(object sender, EventArgs e)
        {
            flatingmode = true;
            FloatingFixForm floatingFixForm = new FloatingFixForm();
            floatingFixForm.MainFormRef = this;
            floatingFixForm.Show(this);
            this.Hide();
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            lblFixedText.Text = "Fixed text:";
        }

        private void btnChangePrompt_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Application.StartupPath, "EnglishFixer.json");
            string param = "You are an assistant that corrects English text or translates other languages into English. Fix grammar and spelling mistakes.Do not rephrase unless necessary. Make the text polite. Never use an em dash (—). Do not change the meaning";
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);

                    // Anonymous type to deserialize only system_prompt
                    var jsonData = JsonSerializer.Deserialize<PromptData>(json);

                    if (!string.IsNullOrEmpty(jsonData?.system_prompt))
                    {
                        param = jsonData.system_prompt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load prompt:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            EditPromptForm editPrompt = new EditPromptForm(param);
            editPrompt.MainFormRef = this;
            editPrompt.ShowDialog(this);
        }
    }

    #region gptnano

    public class GptNanoStreamingClient
    {
        private static readonly HttpClient client = new HttpClient();
        private const string apiKey = "";

        private const string endpoint = "";

        public async Task StreamGrammarCorrectionAsync(
            string inputText,
            Action<string> onToken,
            Action<string> onComplete)
        {
            var requestBody = new
            {
                model = "gpt-4",
                temperature = 0,
                stream = true,
                messages = new[]
                {
                new { role = "system", content = "Fix grammar and spelling. Output only the corrected sentence." },
                new { role = "user", content = inputText }
            }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    onComplete?.Invoke($"Error: {response.StatusCode}\n{error}");
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

                    var jsonLine = line.Substring(5).Trim();
                    if (jsonLine == "[DONE]")
                    {
                        onComplete?.Invoke(null);
                        break;
                    }

                    try
                    {
                        dynamic parsed = JsonConvert.DeserializeObject(jsonLine);
                        string token = parsed?.choices[0]?.delta?.content;
                        if (!string.IsNullOrEmpty(token))
                        {
                            onToken(token);
                        }
                    }
                    catch (Exception ex)
                    {
                        onComplete?.Invoke("Error parsing token: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                onComplete?.Invoke("Stream error: " + ex.Message);
            }
        }
    }


    #endregion



    #region Prompt
    public class PromptData
    {
        public string system_prompt { get; set; }
    }

    public partial class EditPromptForm : Form
    {
        public string PromptText { get; private set; }
        public Form MainFormRef { get; set; }
        private TextBox textBoxPrompt;
        private Button buttonSave;
        private Button buttonCancel;

        private void InitializeComponent()
        {
            this.textBoxPrompt = new TextBox();
            this.buttonSave = new Button();
            this.buttonCancel = new Button();
            this.SuspendLayout();

            // textBoxPrompt
            this.textBoxPrompt.Multiline = true;
            this.textBoxPrompt.ScrollBars = ScrollBars.Vertical;
            this.textBoxPrompt.Location = new System.Drawing.Point(12, 12);
            this.textBoxPrompt.Size = new System.Drawing.Size(360, 120);
            this.textBoxPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // buttonSave
            this.buttonSave.Text = "Save";
            this.buttonSave.Location = new System.Drawing.Point(216, 140);
            this.buttonSave.Click += new EventHandler(this.buttonSave_Click);

            // buttonCancel
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Location = new System.Drawing.Point(297, 140);
            this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);

            // EditPromptForm
            this.ClientSize = new System.Drawing.Size(384, 181);
            this.Controls.Add(this.textBoxPrompt);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Edit Prompt";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = this.buttonSave;
            this.CancelButton = this.buttonCancel;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        public EditPromptForm(string initialText)
        {
            InitializeComponent();
            textBoxPrompt.Text = initialText;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            PromptText = textBoxPrompt.Text;
            var promptObject = new
            {
                system_prompt = PromptText
            };

            try
            {
                string json = JsonSerializer.Serialize(promptObject, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Save to app folder as "EnglishFixer.json"
                string filePath = Path.Combine(Application.StartupPath, "EnglishFixer.json");

                File.WriteAllText(filePath, json);

                // Optional: update property for use in parent form
                this.PromptText = PromptText;
                this.DialogResult = DialogResult.OK;
                MessageBox.Show("System prompt updated successfully and saved","Prompt Updated", MessageBoxButtons.OK);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save Prompt:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    #endregion

    public static class ClipboardHelper
    {
        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        public static bool ForceSetClipboardText(string text, int maxRetries = 10, int delayMs = 50)
        {
            while (maxRetries-- > 0)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        EmptyClipboard();

                        int bytes = (text.Length + 1) * 2;
                        IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes);

                        if (hGlobal == IntPtr.Zero)
                            return false;

                        IntPtr target = GlobalLock(hGlobal);

                        if (target == IntPtr.Zero)
                            return false;

                        // Copy the string to unmanaged memory
                        Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                        Marshal.WriteInt16(target, text.Length * 2, 0); // Null-terminator
                        GlobalUnlock(hGlobal);

                        SetClipboardData(CF_UNICODETEXT, hGlobal);
                        return true;
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }

                Thread.Sleep(delayMs); // Wait and retry
            }

            return false;
        }
    }

    public class FloatingFixForm : Form
    {
        public Form MainFormRef { get; set; }

        private Button btnFix;
        private Button btnNormalMode;
        private Label label;

        

        public FloatingFixForm()
        {
            this.Text = "Floating Fix";
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = true;
            this.Icon = Application.OpenForms[0].Icon;
            this.ShowInTaskbar = true;

            label = new Label() { Text = "Select text and press Fix", Left = 50, Top = 20, Width = 160 };
            
            int diameter = 32;

            btnFix = new Button() { Text = "Fix", Width = 160, Height = 28, BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            btnFix.Click += BtnFix_Click;
            btnFix.AutoSize = true;

            btnNormalMode = new Button() { Text = "Normal Mode",Width = 160,Height=28 };
            btnNormalMode.Click += BtnNormalMode_Click;

            Rectangle bounds = new Rectangle(0, 0, btnFix.Width, btnFix.Height);
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            var layout = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10),
                Dock = DockStyle.Fill,
                WrapContents = false
            };

            layout.Controls.Add(label);
            layout.Controls.Add(btnFix);
            layout.Controls.Add(btnNormalMode);



            btnFix.Region = new Region(path);
            this.FormClosed += formclose;
            btnFix.Anchor= AnchorStyles.Left|AnchorStyles.Right;
            btnFix.FlatStyle = FlatStyle.Flat;
            this.TopMost = true;
            this.Controls.Add(layout);
        }

        private void formclose(object sender, FormClosedEventArgs e)
        {
            var mains = MainFormRef as Form1;
            mains.flatingmode = false;
            this.MainFormRef.Show();
        }
        private void FloatingFixForm_Deactivate(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Activate();
        }
        private async void BtnFix_Click(object sender, EventArgs e)
        {
            var main = MainFormRef as Form1;
            if (main != null)
            {
                string selected = main.SelText;

                if (string.IsNullOrWhiteSpace(main.SelText))
                {
                    label.Text="No text selected or copy failed";
                }
                else
                {
                    try
                    {
                        
                        StringBuilder buffer = new StringBuilder();
                        DateTime lastFlush = DateTime.Now;

                        var client = new GptNanoStreamingClient();
                        string tempstr = "";
                        this.Hide();
                        await client.StreamGrammarCorrectionAsync(
                        inputText: main.SelText,
                        onToken: token =>
                        {
                            buffer.Append(token);

                            if ((DateTime.Now - lastFlush).TotalMilliseconds > 100)
                            {
                                this.Invoke(() =>
                                {
                                    tempstr+=buffer.ToString();
                                    buffer.Clear();
                                });
                                lastFlush = DateTime.Now;
                            }
                        },
                        onComplete: error =>
                        {
                            this.Invoke(() =>
                            {
                                tempstr += buffer.ToString();

                                //if (TrySetClipboardText(tempstr))
                                //{
                                    Clipboard.SetText(tempstr);
                                    Thread.Sleep(300); // Give clipboard time to settle

                                    NativeMethods.SetForegroundWindow(main.originalWindow);

                                    SendKeys.SendWait("^v"); // Paste

                                    Thread.Sleep(100); // Ensure paste completes
                                    Clipboard.Clear(); // Optional: clear after use

                                    main.SelText = "";
                                    this.Show();
                                    label.Text = "Text fixed";
                                //}
                            });
                        }
                        );

                        
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }

                }

            }


        }
        private bool TrySetClipboardText(string text)
        {
            int retries = 5;
            while (retries-- > 0)
            {
                try
                {
                    Clipboard.SetText(text);
                    return true;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
            return false;
        }
        private void BtnNormalMode_Click(object sender, EventArgs e)
        {
            MainFormRef.Show();
            this.Close();
        }
    }
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }

}
