using System;
using System.Windows.Forms;

namespace GamePilotNative
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Pre-warm database and licensing singletons
            var db = GamePilotDatabase.Instance;
            var lic = LicenseManager.Instance;

            Application.Run(new GamePilotHubForm());
        }
    }
}
