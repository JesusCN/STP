using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SimpleTaskPlanner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]


        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            STPlanner stplanner = new STPlanner();

            Form1 form1 = new Form1();
            form1.stplanner = stplanner;

            Application.Run(form1);
        
        }
    }
}
