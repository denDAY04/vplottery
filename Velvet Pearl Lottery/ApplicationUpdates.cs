using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery {

    //! API class for querying, downloading, and running application updates.
    public class ApplicationUpdates {
        
        //! URI of the file that contains the version number of the latest application update.
        private const string AppVersionJsonUri = "http://files.enjin.com/72287/apps/velvet_pearl_lottery/newest_release.json";

        private const string AppMsiUri = "http://files.enjin.com/72287/apps/velvet_pearl_lottery/vp_lottery_setup.msi";

        //! Possible results of querying for whether an update is available.
        public enum QueryStatus {
            //! A new update is available.
            UpdateAvailable,
            //! No new update is available.
            NoUpdate,
            //! The query failed.
            Error
        }

        /*!
            \brief Check if a new update is available for the application.

            \return QueryStatus.UpdateAvailable if a new update is available,
            QueryStatus.NoUpdate if the installed version is already the most 
            recent, and QueryStatus.Error if an error occured preventing the function
            from determing the update relation.  
        */
        public static QueryStatus CheckForUpdate() {
            var installedVersion = ReadCurrentInstallVersion();
            if (string.IsNullOrEmpty(installedVersion))
                return QueryStatus.Error;

            var webController = new WebClient();
            Stream dataStream = null;
            ApplicationVersion newestVersion;

            try {
                dataStream = webController.OpenRead(AppVersionJsonUri);
                if (dataStream == null)
                    return QueryStatus.Error;

                var stringReponse = new StreamReader(dataStream).ReadToEnd();
                var jsonResponse = JObject.Parse(stringReponse);
                newestVersion = JsonConvert.DeserializeObject<ApplicationVersion>(jsonResponse.ToString());
            }
            catch (WebException) {
                return QueryStatus.Error;
            }
            finally {                
                dataStream?.Close();
            }

            return (String.Compare(newestVersion.VersionNumber, installedVersion, StringComparison.Ordinal) > 0 ? QueryStatus.UpdateAvailable : QueryStatus.NoUpdate);
        }

        /*!
            \brief Read the version number of the application and trim it to three numbers.
            \return The read version number trimmed to format "[Major].[Minor].[Build]". 
        */
        private static string ReadCurrentInstallVersion() {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fileVersionInfo.ProductVersion;
            
            return version.Substring(0, version.LastIndexOf(".", StringComparison.Ordinal));
        }

        public static Task DownloadUpdate() {
            var webController = new WebClient();
            var saveFile = new SaveFileDialog {
                Filter = "Windows Installer (*.msi)|*.msi",
                OverwritePrompt = true,
                FileName = "vp_lottery_setup",
                DefaultExt = "msi",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            var filedChosen = saveFile.ShowDialog();
            if (filedChosen != null && !filedChosen.Value)
                return null;
            
            // Open and close file to ensure that it exists before downloading the data into it.
            var targetFile = saveFile.OpenFile();
            targetFile.Close();
            try {
                return webController.DownloadFileTaskAsync(AppMsiUri, saveFile.FileName);
            }
            catch (Exception) {
                return null;
            }
        }
    }
}
