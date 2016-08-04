using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace ScanAPIDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args[0], args[1]));
        }
    }
}