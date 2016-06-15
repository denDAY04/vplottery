using System;
using System.Windows;

namespace Velvet_Pearl_Lottery {

    //! Application class.
    public partial class App : Application {

        /*!
            \brief Startup routine for performing required initialization for the application.
            
            If the program is started through a save file, the function retrieves that file's name and stores it 
            for the further applicatoin to use for its initial save file.
        */
        protected override void OnStartup(StartupEventArgs e) {
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
