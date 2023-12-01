using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Configuration;

namespace Consumer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            int port = int.Parse(ConfigurationManager.AppSettings.Get("port"));
            UdpClient client = new UdpClient(port);

            while (true)
            {
                UdpReceiveResult data = await client.ReceiveAsync();
                using (MemoryStream ms = new MemoryStream(data.Buffer))
                {
                    pictureBox1.Image = new Bitmap(ms);
                }
                Text = $"Received bytes: {data.Buffer.Length * sizeof(byte)}";
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = null;
                Invalidate();
                Text = string.Empty;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            MessageBox.Show(string.Join("\n", host.AddressList.
                Where(i => i.AddressFamily == AddressFamily.InterNetwork)), "IPv4");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(exePath, @"HelpImg\tutorial.png");
                pictureBox1.Image = new Bitmap(path);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        #region Drawing
        private Point _lastPoint = Point.Empty;
        private bool _isDragging = false;
        private readonly Pen _pen = new Pen(Color.Black, 2);

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _lastPoint = e.Location;
            _isDragging = true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                if (pictureBox1.Image == null)
                {
                    Bitmap bitMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    pictureBox1.Image = bitMap;
                }
                using (Graphics graphics = Graphics.FromImage(pictureBox1.Image))
                {
                    graphics.DrawLine(_pen, _lastPoint, e.Location);
                }
                pictureBox1.Invalidate();
                _lastPoint = e.Location;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _lastPoint = Point.Empty;
        }
        #endregion
    }
}