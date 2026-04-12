using System;
using System.Windows.Forms;
using VinhKhanhGuide.Forms;

namespace VinhKhanhGuide
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // GMap.NET fetches map tiles over HTTPS. On .NET Framework 4.7.2
            // the default is still TLS 1.0, which OpenStreetMap rejects —
            // so we force TLS 1.2 before any request goes out.
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
