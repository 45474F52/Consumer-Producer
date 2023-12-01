using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Configuration;
using System.Drawing.Imaging;
using AForge.Video.DirectShow;
using System.Runtime.InteropServices;

namespace Producer
{
    internal class Program
    {
        const byte SW_HIDE = 0;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, byte nCmdShow);

        private static IPEndPoint consumerEndPoint;
        private static readonly UdpClient udpClient = new UdpClient();

        static void Main(string[] args)
        {
            string consumerIP = ConfigurationManager.AppSettings.Get("consumerIP");
            int consumerPort = int.Parse(ConfigurationManager.AppSettings.Get("consumerPort"));
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIP), consumerPort);
            Console.WriteLine($"Consumer: {consumerEndPoint}");

            Console.WriteLine("\n...Press ENTER to hide the console...");
            Console.WriteLine("\n...Press ANY OTHER key to exit.......");
            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;
                videoSource.Start();

                Console.Beep(800, 300);
                Console.Beep(800, 300);
                ShowWindow(GetConsoleWindow(), SW_HIDE);
            }
            else
            {
                Console.Beep(800, 1000);
                Thread.Sleep(700);
                Environment.Exit(0);
            }
        }

        private static void VideoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap bMap = new Bitmap(eventArgs.Frame, 800, 600);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bMap.Save(ms, ImageFormat.Jpeg);
                    byte[] bytes = ms.ToArray();
                    udpClient.Send(bytes, bytes.Length, consumerEndPoint);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
}