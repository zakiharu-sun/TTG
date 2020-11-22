using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TTG
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [System.STAThreadAttribute()]
        static public void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Startup += App_Startup;
            app.Run();
        }
        private static void App_Startup(object sender, StartupEventArgs e)
        {
            //new TTG_setting().Show();
        }
    }
}
