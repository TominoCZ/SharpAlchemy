using Alchemy.Properties;
using System;
using System.Windows.Forms;

namespace Alchemy
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var g = new Game())
            {
                g.Icon = Resources.icon;
                g.Run(20, 60);
            }
        }
    }
}