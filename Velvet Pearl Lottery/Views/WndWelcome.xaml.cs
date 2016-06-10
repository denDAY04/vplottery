using System;
using System.IO;
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

        //! Construct a new welcoming window.
        public WndWelcome() {
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
            this.Close();
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
            this.Hide();
            mainWindow.OpenNewLotteryWindow();
            this.Close();
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
