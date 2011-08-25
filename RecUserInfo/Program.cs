using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Text;
using System.Reflection;
namespace NotifyIconEmployeeChecker
{
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.ComponentModel.IContainer components;
        private ClipboardMonitor clipboardMonitor;
        Assembly _assembly;
        Stream _imageStream;
        StreamReader _textStreamReader;


        [STAThread]
        static void Main()
        {
            Form f = new Form1();
            Application.Run(f);
        }

        public Form1()
        {
            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += new EventHandler<ClipboardChangedEventArgs>(clipboardMonitor_ClipboardChanged);

            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1
            this.contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { this.menuItem1 });

            // Initialize menuItem1
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "E&xit";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);

            // Set up how the form should be displayed.
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Text = "Notify Icon Example";

            _assembly = Assembly.GetExecutingAssembly();
            Console.WriteLine(_assembly.GetManifestResourceNames() + " <--- nazwy");
            _imageStream = _assembly.GetManifestResourceStream("RecUserInfo.User.ico"
);
            // Create the NotifyIcon.
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            //notifyIcon1.Icon = new Icon("User.ico");
            notifyIcon1.Icon = new Icon(_imageStream);


            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            notifyIcon1.ContextMenu = this.contextMenu1;

            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            notifyIcon1.Text = "REC_User_Info";
            notifyIcon1.Visible = true;

            // Handle the DoubleClick event to activate the form.
            notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);

        }
        protected override void SetVisibleCore(bool value)
        {
            if (!this.IsHandleCreated)
            {
                CreateHandle();
                value = false;
            }
            base.SetVisibleCore(value);
        }


        protected override void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }

        private void notifyIcon1_DoubleClick(object Sender, EventArgs e)
        {

        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            this.Close();
        }

        private string getUserInfo(string input)
        {
            //string pattern = @"Author: REC_User_(...)";
            string pattern = @"REC_User_(...)";
            Regex rgx = new Regex(pattern);
            MatchCollection matches = rgx.Matches(input);
            //Console.WriteLine("{0} ({1} matches):", input, matches.Count);

            Match oneMatch = rgx.Match(input);
            Group grupka = oneMatch.Groups[1];
            //Console.WriteLine("Dopasowalem: " + grupka.Value);
            string number = grupka.Value;

            foreach (Match match in matches)
            {
                //Console.WriteLine("Match " + match.Value);
                GroupCollection groups = match.Groups;
                /*Console.WriteLine("'{0}' repeated at positions {1} and {2}",
                                  groups["word"].Value,
                                  groups[0].Index,
                                  groups[1].Index);*/
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    Group grupa = match.Groups[i];

                    if (grupa.Success)
                    {
                        //Console.WriteLine("\tMatched:" + grupa.Value);
                    }
                    else
                    {
                        //Console.WriteLine("\tGroup didn't match");
                    }
                }

            }
            Console.WriteLine("to jest numer! --->" + number + "<----");
            if (number.Length == 0) return null;
            string parameters = "name=" + number;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://10.0.3.150/check/");
            request.Method = "POST";
            Stream writeStream = request.GetRequestStream();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(parameters);

            writeStream.Write(bytes, 0, bytes.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
            string result = readStream.ReadToEnd();
            //Console.WriteLine(result);

            string pattern2 = @"</html>(.*)";
            Regex rgx2 = new Regex(pattern2);
            Match secondMatch = rgx2.Match(result);
            Group grupka2 = secondMatch.Groups[1];
            //Console.WriteLine("Dopasowalem: " + grupka2.Value);
            return grupka2.Value;
        }

        private bool printMe(char line)
        {
            //Console.WriteLine(line);
            return true;
        }

        void clipboardMonitor_ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            //clipboardContent.Text = "Schowek!" + e.ClipboardContent;
            //clipboardContent.Text = getUserInfo(e.ClipboardContent).Trim();
            notifyIcon1.Visible = true;
            string result = getUserInfo(e.ClipboardContent);
            if ( result != null )
            {
                notifyIcon1.ShowBalloonTip(2000, "Information", result.Trim(),
                ToolTipIcon.Info);
            }
        }

    }
}