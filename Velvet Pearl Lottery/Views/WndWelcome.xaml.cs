using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Velvet_Pearl_Lottery.Models;

//! Namespace for all views/windows of the program.
namespace Velvet_Pearl_Lottery.Views {

    /*! 
        \brief Welcoming window for the program.

        This windows allows the user to either start a new lottery session or 
        import an old session from a file.
    */
    public partial class WndWelcome : Window {

        //! Flag for whether the end-user canceled the ongoing update.
        private bool UpdateWasCancelled { get; set; }
        //! The window displaying the update status.
        private WndUpdateStatus UpdateStatusWnd { get; set; }

        private ApplicationUpdate UpdateProcess { get; set; }

        //! Construct a new welcoming window.
        public WndWelcome() {
            UpdateWasCancelled = false;

            InitializeComponent();

            Loaded += new RoutedEventHandler(WelcomeWindow_Loaded);
        }

        /*!
            \brief Check if a load-file name is stored and parse it if so.

            This function, run upon the welcome window being loaded, checks the 
            application's dynamic memory for a filename on a lottery save file 
            the application was started with (e.g. by double-clicking a save file).

            If such a file is found, it is parsed and the application shows the main 
            lottery window, assuming no parse error occured.
        */
        private void WelcomeWindow_Loaded(object sender, RoutedEventArgs e) {
            CheckForUpdates();
            if (UpdateProcess != null && UpdateProcess.UpdateAvailable && !UpdateWasCancelled) {
                // Close the application since the update installer has been opened.
                Close();
                return;
            }

            if (Application.Current.Properties["LoadfileName"] == null)
                return;

            var filename = Application.Current.Properties["LoadfileName"].ToString();
            Lottery importedLottery;
            try {
                Stream filestream = new FileStream(filename, FileMode.Open);
                importedLottery = InitiateImport(filestream);
            }
            catch (Exception ex) {
                WndDialogMessage.Show(this, "An unexpected error occured during loading of the save file:\n" + ex.Message,
                    "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (importedLottery == null)
                return;

            var mainWindow = new WndMain(true, filename) {Owner = this, LotteryModel = importedLottery};
            mainWindow.Show();
            mainWindow.Owner = null;
            Close();
        }

        //! Check for updates, prompting the end-user to download and install if found.
        private void CheckForUpdates() {
            UpdateProcess = ApplicationUpdate.CheckForUpdate();
            if (UpdateProcess == null) {
                const string msg = "Velvet Pearl Lottery could not check for updates.\n\nThis may be caused by a lack of internet connection or Enjin being down for maintenance." +
                                           "If the problem persists, contact denDAY at \nwww.enjin.com/profile/493549.";
                WndDialogMessage.Show(this, msg, "Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            } else if (!UpdateProcess.UpdateAvailable)
                return;

            var choice = WndDialogMessage.Show(this, "A new update is available.\n\nDownload and install?", "New Update", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (choice != MessageBoxResult.Yes)
                return;

            // Download and register the install function (event handler) and cancel token.
            if (!UpdateProcess.DownloadUpdateAsync(InstallUpdate))
                return;
            var cancelToken = new CancellationTokenSource();
            cancelToken.Token.Register(CancelUpdate);
            UpdateStatusWnd = new WndUpdateStatus(cancelToken) {StatusText = "Downloading update ...", Owner = this};
            UpdateStatusWnd.ShowDialog();
        }

        //! Cancel the process downloading the update.
        public void CancelUpdate() {
            UpdateWasCancelled = true;
            UpdateProcess.CancelDownload();
            UpdateProcess = null;
        }

        /*!
            \brief Run the downloaded installer (in another process) if no error occured.
            
            If an error occured the method sets the status text in the active WndUpdateStatus
            window. If, however, the update was cancelled or downloaded without error, the window
            is closed.
        */
        private void InstallUpdate(object sender, AsyncCompletedEventArgs e) {
            if (e.Error != null) {
                UpdateStatusWnd.StatusText = "The update has stopped because of an error.";
                return;
            }
            // Close the update window and execute downloaded file in another process if the update wasn't canceled.
            UpdateStatusWnd.Close();
            if (!e.Cancelled)
                System.Diagnostics.Process.Start(UpdateProcess.UpdateInstallerFile);
        }

        /*!
            \brief ButtonClick event handler for creating a new lottery.
            
            This function opens a new main window for the new lottery and then
            the dialog window for creating the new lottery, in which the user
            will input the required data.

            Once the two new windows have been created and enabled, the
            function closes its own welcoming window.
            
            \param sender Object that triggered the ButtonClick event.
            \para e Event information. 
        */
        private void BtwNewLottery_Click(object sender, RoutedEventArgs e) {
            var mainWindow = new WndMain(true) { Owner = this };
            mainWindow.Show();            
            mainWindow.Owner = null;
            Hide();
            mainWindow.OpenNewLotteryWindow();
            Close();
        }

        //! Open dialog to open a save file for import.
        private void BtnLoadLottery_Click(object sender, RoutedEventArgs e) {
            var openfileDialog = new OpenFileDialog() {
                Filter = "Velvet Pearl Lottery Files (*.vplf)|*.vplf",
                FileName = "Velvet Pearl lottery",
                DefaultExt = "vplf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            var result = openfileDialog.ShowDialog();
            if (result == null || !result.Value)
                return;

            var filestream = openfileDialog.OpenFile();
            var importedLottery = InitiateImport(filestream);
            filestream.Close();
            if (importedLottery == null)
                return;

            var mainWindow = new WndMain(true, openfileDialog.FileName) { Owner = this, LotteryModel = importedLottery};
            mainWindow.Show();
            mainWindow.Owner = null;
            this.Close();
        }

        /*!
            \brief Initate importing a lottery save file into the application.

            NOTE that this function DOES NOT close the file's stream.

            \param filestream Open read-stream to the save file.
        */
        private Lottery InitiateImport(Stream filestream) {
            string errorMsg;
            Lottery importedLottery;
            var importResult = Lottery.ImportFromFile(filestream, out importedLottery, out errorMsg);
            filestream.Close();
            switch (importResult) {
                case Lottery.ImportResult.InvalidFile:
                    WndDialogMessage.Show(this, "The import file is not a valid vplf file.", "Import Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;

                case Lottery.ImportResult.ParseError:
                    WndDialogMessage.Show(this, "Import of lottery file failed with error:\n" + errorMsg, "Import Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
            }
            return importedLottery;
        }
    }
}
