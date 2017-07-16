using MeasuringTools.Output.Ipc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MeasuringTools.Gui
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // handle command line args*
            PipeClient client = new PipeClient();
            client.PipeClientMain(e.Args);

            var test = new Test();
        }
    }
}
