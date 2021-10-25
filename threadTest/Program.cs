using Serilog;
using System;
using System.Windows.Forms;

namespace threadTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //serilog kurulum
            Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-{Date}.log", rollingInterval: RollingInterval.Month)
          .CreateLogger();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
