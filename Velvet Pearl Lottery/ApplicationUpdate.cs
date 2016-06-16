using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery {

    //! API for querying, downloading, and running application updates.
    public class ApplicationUpdate : WebClient {
        
        //! URI of the file that contains the version number of the latest application update.
        private const string AppVersionJsonUri = "http://files.enjin.com/72287/apps/velvet_pearl_lottery/newest_release.json";
        //! URI for the application setup file.
        private const string AppMsiUri = "http://files.enjin.com/72287/apps/velvet_pearl_lottery/vp_lottery_setup.msi";
        //! Filename of the error log for updates.
        private const string ErrorLogSubFilename = "\\Velvet Pearl\\Lottery\\Update error log.txt";

        //! Flag for whether an update is available.
        public bool UpdateAvailable { get; private set; }
        //! Full path and filename for the downloaded update installer.
        public string UpdateInstallerFile { get; private set; }


        /*!
            \brief Construct a new Update object with flag for update availability.     
                   
            \param updateIsAvailable Flag for whether an update is available.
        */
        private ApplicationUpdate(bool updateIsAvailable) {
            UpdateAvailable = updateIsAvailable;
        }

        /*!
            \brief Check if a new update is available for the application.

            \return a new ApplicationUpdate object with its UpdateAvailable
            property flag set to true if an update is available, or otherwise false.
            If an error occured during the check, null is returned.
        */
        public static ApplicationUpdate CheckForUpdate() {
            var installedVersion = ReadCurrentInstallVersion();
            if (string.IsNullOrEmpty(installedVersion)) {
                LogError("Could not determine version of installed application.");
                return null;
            }
                

            var webController = new WebClient();
            Stream dataStream = null;
            ApplicationVersion newestVersion;

            try {
                dataStream = webController.OpenRead(AppVersionJsonUri);
                if (dataStream == null) {
                    LogError("Data stream for JSON version file " + AppVersionJsonUri + " was null.");
                    return null;
                }
                    
                var stringReponse = new StreamReader(dataStream).ReadToEnd();
                var jsonResponse = JObject.Parse(stringReponse);
                newestVersion = JsonConvert.DeserializeObject<ApplicationVersion>(jsonResponse.ToString());
            }
            catch (WebException e) {
                LogError(e.Message + " Targe URI: " + AppVersionJsonUri);
                return null;
            }
            finally {                
                dataStream?.Close();
            }

            var updatePending = String.Compare(newestVersion.VersionNumber, installedVersion, StringComparison.Ordinal) > 0;
            return new ApplicationUpdate(updatePending);
        }

        /*!
            \brief Write an error to the log file, pre-fixed with a timestamp. 

            \param The message that should be written to the log.
        */
        private static void LogError(string message) {
            try {            
                var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                System.IO.File.AppendAllText(path + ErrorLogSubFilename, $"{timestamp}\t\t{message}");
            }
            catch (Exception) {                
                // Do nothing.
            }
        }

        /*!
            \brief Read the version number of the application and trim it to three numbers.
            \return The read version number trimmed to format "[Major].[Minor].[Build]". 
        */
        private static string ReadCurrentInstallVersion() {
            var assembly = (typeof(ApplicationUpdate)).Assembly;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fileVersionInfo.ProductVersion;
            
            return version.Substring(0, version.LastIndexOf(".", StringComparison.Ordinal));
        }

        /*!
            \brief Download the update installer to a user-defined file without blocking the current thread.

            \param downloadCompleteFunc Function to be called when the download finishes.

            \return True if the download process was successfully started, and false if the 
            end-user canceled the dialog choosing where to save the downloading file to, or an 
            error occurred.
        */
        public bool DownloadUpdateAsync(AsyncCompletedEventHandler downloadCompleteFunc) {            
            DownloadFileCompleted += downloadCompleteFunc;

            var saveFile = new SaveFileDialog {
                Title = "Save Download As",
                Filter = "Windows Installer (*.msi)|*.msi",
                OverwritePrompt = true,
                FileName = "vp_lottery_setup",
                DefaultExt = "msi",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            var filedChosen = saveFile.ShowDialog();
            if (filedChosen == null || !filedChosen.Value)
                return false;
            
            // Open and close file to ensure that it exists before downloading the data into it.
            var targetFile = saveFile.OpenFile();
            targetFile.Close();
            try {
                UpdateInstallerFile = saveFile.FileName;
                DownloadFileTaskAsync(AppMsiUri, UpdateInstallerFile);
                return true;
            }
            catch (Exception e) {
                LogError(e.Message + " Target URI: " + AppMsiUri);
                return false;
            }
        }

        //! Cancel the downloading thread.
        public void CancelDownload() {
            CancelAsync();
        }

    }
}
