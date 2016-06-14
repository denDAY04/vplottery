using System;
using System.IO;
using System.Net;
using System.Runtime.Hosting;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using Newtonsoft.Json.Linq;
using Velvet_Pearl_Lottery.Models;
using Velvet_Pearl_Lottery.Views;

namespace Velvet_Pearl_Lottery {

    //! Application class.
    public partial class App : Application {

        /*!
            \brief Startup routine for performing required initialization for the application.
            
            If the program is started through a save file, the function retrieves that file's name and stores it 
            for the further applicatoin to use for its initial save file.
        */

        protected override void OnStartup(StartupEventArgs e) {
            var newUpdate = ApplicationUpdates.CheckForUpdate();
            switch (newUpdate) {
                case ApplicationUpdates.QueryStatus.Error:
                    var msg = "Velvet Pearl Lottery could not check for updates.\n\nThis may be caused by a lack of internet connection or Enjin being down for maintenance.\nIf the problem persists, contact denDAY at \nhttp://www.enjin.com/profile/493549.";
                    MessageBox.Show(msg, "Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Information);                    
                    break;

                case ApplicationUpdates.QueryStatus.UpdateAvailable:
                    var choice = MessageBox.Show("An Velvet Pearl Lottery update is available.\n\nDownload and install?",
                        "New Update", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (choice != MessageBoxResult.Yes)
                        break;

                    ApplicationUpdates.DownloadUpdate();
                    break;

                case ApplicationUpdates.QueryStatus.NoUpdate:
                    // No action.
                    break;
            }

            // Retrieve the save file's name, if any. 
            if (e.Args.Length > 0) {
                Properties["LoadfileName"] = e.Args[0];
            } else {
                try {
                    if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0) {
                        var fname = "No filename given";                    
                        fname = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];

                        // It comes in as a URI; this helps to convert it to a path.
                        var uri = new Uri(fname);
                        fname = uri.LocalPath;

                        Properties["LoadfileName"] = fname;        
                    }
                } catch (Exception) {
                    // The file couldn't be retrieved so proceed normal boot of the application.
                }                
            }
            base.OnStartup(e);
        }

        
    }
}
