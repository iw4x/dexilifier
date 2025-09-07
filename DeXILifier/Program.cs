namespace DeXILifier
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal class Program
    {
        [STAThread] 
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            MainWindow window = new MainWindow();
            Application.Run(window);
        }

    }
}
