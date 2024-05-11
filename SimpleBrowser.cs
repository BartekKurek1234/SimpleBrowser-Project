using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SimpleWebBrowser
{
    public partial class SimpleBrowserForm : Form
    {
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        private WebBrowser webBrowser; // Deklaracja kontrolki WebBrowser
        private TextBox urlTextBox; // Deklaracja pola tekstowego na adres URL

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        public SimpleBrowserForm()
        {
            InitializeComponent();

            InitializeWebBrowser();
            InitializeUI();

            // Włączenie obsługi CSS
            SetIECSS(true);
        }

        private void InitializeComponent()
        {
            // Inicjalizacja komponentów
            urlTextBox = new TextBox();
            urlTextBox.Dock = DockStyle.Top;
            urlTextBox.KeyPress += UrlTextBox_KeyPress; // Obsługa zdarzenia naciśnięcia klawisza w polu tekstowym

            Button goButton = new Button();
            goButton.Text = "Go";
            goButton.Dock = DockStyle.Top;
            goButton.Click += GoButton_Click; // Obsługa zdarzenia kliknięcia przycisku "Go"

            Button exitButton = new Button();
            exitButton.Text = "Exit";
            exitButton.Dock = DockStyle.Top;
            exitButton.Click += ExitButton_Click; // Obsługa zdarzenia kliknięcia przycisku "Exit"

            Controls.Add(urlTextBox);
            Controls.Add(goButton);
            Controls.Add(exitButton);
        }

        private void InitializeWebBrowser()
        {
            // Tworzenie kontrolki WebBrowser
            webBrowser = new WebBrowser();
            webBrowser.Dock = DockStyle.Fill;
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.Navigating += WebBrowser_Navigating;
            webBrowser.DocumentTitleChanged += WebBrowser_DocumentTitleChanged;
            webBrowser.AllowNavigation = true;

            // Obsługa zdarzenia ładowania dokumentu w kontrolce WebBrowser
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

            Controls.Add(webBrowser);
        }

        private void InitializeUI()
        {
            Text = "Prosta Przeglądarka";
            Width = 800;
            Height = 600;
        }

        private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Text = "Prosta Przeglądarka - " + e.Url;
        }

        private void WebBrowser_DocumentTitleChanged(object sender, EventArgs e)
        {
            Text = "Prosta Przeglądarka - " + webBrowser.DocumentTitle;
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Włączanie obsługi skryptów JavaScript w ramkach (iframe)
            foreach (HtmlWindow frame in webBrowser.Document.Window.Frames)
            {
                frame.AttachEventHandler("onload", (s, ev) =>
                {
                    ((HtmlWindow)s).Document.InvokeScript("eval", new[] { "window.external={invoke:function(s){window.external.Notify(s);}}" });
                });
            }
        }

        private void SetIECSS(bool enableCSS)
        {
            int setting = enableCSS ? 1 : 0;
            IntPtr zero = IntPtr.Zero;
            InternetSetOption(zero, INTERNET_OPTION_SETTINGS_CHANGED, zero, 0);
            InternetSetOption(zero, INTERNET_OPTION_REFRESH, zero, 0);
        }

        private void NavigateToUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                webBrowser.Navigate(url);
            }
        }

        private void UrlTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                NavigateToUrl(urlTextBox.Text);
                e.Handled = true;
            }
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            NavigateToUrl(urlTextBox.Text);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SimpleBrowserForm());
        }
    }
}
