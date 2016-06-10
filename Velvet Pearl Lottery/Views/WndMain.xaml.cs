using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery.Views {
    
    //! Main window for a lottery session.
    public partial class WndMain : Window {
        //! The active lottery session.
        public Lottery LotteryModel { get; set; }
        //! The initial directory to show when saving a lottery to file.
        private string InitialSaveDirectory { get; set; }
        //! Full path to the save file for the lottery.
        private string SaveFilename { get; set; }
        //! Flag for whether to not prompt for save on closing.
        public bool IgnoreSaveOnClosing { get; set; }
        //! Flag for whether the new lottery creation is being canceled.
        public bool CancelNewLottery { get; set; }

        //! Result of initializing a lottery.
        public enum LotteryInitResult {
            //! Successful initialization.
            Success,
            //! The max lottery number argument was invalid.
            InvalidNumberLimit,
            //! The ticket price argument was invalid.
            InvalidTicketPrice,
            //! Unspecified error.
            GeneralError
        }

        /*!
            \brief Construct a new main window for a lottery session.
            \param allowInitialCloseWithoutSave Flag signaling whether the wondow 
            should prompt for save--upon closing the window--from the beginning, regardless of 
            whether changes have been made to the lottery model. This is useful when loading in 
            a save file, whereupon setting the flag true will not promt a save action when closing 
            the window if the model has not been updated yet. 
            Default value is false, meaning a prompt will appear.
            \param initialSaveFilename The full filename of the file to initially save to. Point this 
            to the import file when the session is initiated from a save file.
        */
        public WndMain(bool allowInitialCloseWithoutSave = false, string initialSaveFilename = "") {
            LotteryModel = null;
            CancelNewLottery = false;
            InitialSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SaveFilename = initialSaveFilename;
            IgnoreSaveOnClosing = allowInitialCloseWithoutSave;

            InitializeComponent();

            if (!string.IsNullOrEmpty(SaveFilename)) {
                ChkAutoSave.IsEnabled = true;
                ChkAutoSave.IsChecked = true;
            } else {
                ChkAutoSave.IsEnabled = false;
                ChkAutoSave.Visibility = Visibility.Hidden;
            }
        }

        /*!
            \brief Initialize a new lottery session.
            
            \param lotteryNumberLimit The larget nunmber among the lottery numbers that can be bought.
            \param ticketPrice Price per lottery ticket. 

            \return LotteryInitResult.InvalidNumberLimit if the lotteryNumberLimit is invalid, 
            LotteryInitResult.InvalidTicketPrice if the ticketPrice parameter is invalid, 
            LotteryInitResult.InvalidTicketPrice if an unspecified error occures, and 
            LotteryInitResult.Success if the initialization is completed successfully. 
        */
        public LotteryInitResult InitNewLotteryModel(int lotteryNumberLimit, int ticketPrice) {
            try {
                LotteryModel = new Lottery(lotteryNumberLimit, ticketPrice);
            } catch (ArgumentException argExc) {
                if (argExc.Message == Lottery.InvalidLotteryNumberErrorMsg)
                    return LotteryInitResult.InvalidNumberLimit;
                if (argExc.Message == Lottery.InvalidTicketPriceErrorMsg)
                    return LotteryInitResult.InvalidTicketPrice;
                
                return LotteryInitResult.GeneralError;
            }

            return LotteryInitResult.Success;
        }

        /*!
            \brief Asks the end-user if the lottery "should be saved before closing". 
            \return true if the end-user canceled the dialog; false otherwise, regadless if the 
            end-user chose to save or not.
        */
        private bool PromptSaveOnClose() {
            var result = WndDialogMessage.Show(this, "Save before closing?", "Closing Lottery", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) {
                SaveToFile();
            } else if (result == MessageBoxResult.Cancel) {
                return true;
            }
            return false;
        }

        /*!
            \brief Enabling/disabling the show-comment button when winning-tickets list selection change.

            Enabled the button to show a winning ticket's comment if a ticket is selected and it 
            has a comment. Otherwise, if no ticket is selected or the selected ticket has no comment
            the button is disabled.
        */
        private void LwWinningTickets_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedTicket = (Ticket) LwWinningTickets.SelectedItem;

            if (string.IsNullOrEmpty(selectedTicket?.Comment))
                BtnShowTicketComment.IsEnabled = false;
            else
                BtnShowTicketComment.IsEnabled = true;
        }

        /*!
            \brief Draw a winning number and add the ticket to the list of winning tickets.

            If no tickets have yet be sold a message box informs the end-user of this and returns 
            without further action. Equally it shows a message and returns without further action if 
            all sold tickets have already won.
        */
        private void BtnDrawWinner_Click(object sender, RoutedEventArgs e) {
            if (LotteryModel.Tickets.Count < 1) {
                WndDialogMessage.Show(this, "No tickets have yet been sold.\nNo winner is drawn.", "No Tickets Sold", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var winner = LotteryModel.DrawWinningNumber();
            if (winner == null) {
                WndDialogMessage.Show(this, "All solds tickets have already won.\nNo winner is drawn.", "All Tickets Won", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            LwWinningTickets.Items.Add(winner);
            IgnoreSaveOnClosing = false;
            ProcessAutoSave();
        }

        //! Set Lottery info whenever the window is activated.
        private void LotteryWindow_Activated(object sender, EventArgs e) {
            if (LotteryModel == null)
                return;
            
            // Set text boxes info
            LabdataLotteryNumberRange.Text = "" + Lottery.LotteryNumberMin + " - " + LotteryModel.LotteryNumberMax;
            LabdataTicketPrize.Text = "" + LotteryModel.TicketPrice + " credits";
            LabdataTicketsLeft.Text = "" + LotteryModel.NumLotteryNumbersLeft + " left";
            LabdataTicketsSold.Text = "" + LotteryModel.Tickets.Count + " sold";
            LabdataProfit.Text = "" + (LotteryModel.Tickets.Count * LotteryModel.TicketPrice) + " credits";

            // Set list of winners
            LwWinningTickets.Items.Clear();
            LotteryModel.WinningLotteryNumbers.ForEach(winNum => 
                    LwWinningTickets.Items.Add(LotteryModel.Tickets.Find(t => 
                        t.LotteryNumber == winNum)));

            // Only enable show-comment button if there's a ticket selected and it has a comment.
            if (LwWinningTickets.SelectedItem == null)
                BtnShowTicketComment.IsEnabled = false;
            else
                BtnShowTicketComment.IsEnabled = !string.IsNullOrEmpty(((Ticket) LwWinningTickets.SelectedItem).Comment);
        }

        /*!
            \brief Save the lottery to a file. 
            
            If a save file has not already been stored in the program's memory
            the end-user is prompted to choose a save file and location through the 
            Save As dialog. Should the end-user cancel this prompt then no saving is 
            done.
        */
        private void SaveToFile() {
            if (SaveFilename == "") {
                if (!ShowSaveAsDialog())
                    return;
            } 
                
            var filestream = new FileStream(SaveFilename, FileMode.Truncate);       
            LotteryModel.ExportToFile(filestream);
            filestream.Close();
            IgnoreSaveOnClosing = true;
        }

        /*!
            \brief Open the Save As dialog, prompting the end-user to pick a file to save to.

            If the end-user accepts the choice of the dialog, the given savefile and 
            the directory is saved in member fields for later save-use, and autosave is enabled.

            \return true if the user chose to accept the save, and false if not.
        */
        private bool ShowSaveAsDialog() {
            var saveExportFileDialog = new SaveFileDialog {
                Filter = "Velvet Pearl Lottery Files (*.vplf)|*.vplf",
                OverwritePrompt = true,
                FileName = "Velvet Pearl lottery",
                DefaultExt = "vplf",
                InitialDirectory = InitialSaveDirectory
            };

            var result = saveExportFileDialog.ShowDialog();
            if (result == null || !result.Value)
                return false;
            
            // Store save-file name and the folder location.
            SaveFilename = saveExportFileDialog.FileName;
            InitialSaveDirectory = new FileInfo(SaveFilename).Directory?.FullName;
            
            // Create file by opening and closing it right away.
            var filestream = saveExportFileDialog.OpenFile();
            filestream.Close();

            ChkAutoSave.IsEnabled = true;
            ChkAutoSave.IsChecked = true;
            ChkAutoSave.Visibility = Visibility.Visible;

            return true;
        }

        /*!
            \brief Make end-user choose save-file and save to it.

            If the end-user cancels the Save As dialog, no saving is performed.
        */
        private void BtnSaveAs_Click(object sender, RoutedEventArgs e) {
            if (ShowSaveAsDialog())
                SaveToFile();
        }

        //! Listen for ctrl + s to save file.
        private void LotteryWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                SaveToFile();
        }

        //! Save button click.
        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            SaveToFile();
        }

        // Prompt for possible save upon the window closing.
        private void LotteryWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!IgnoreSaveOnClosing)
                if (PromptSaveOnClose())
                    e.Cancel = true;
        }

        //! Display a comment window if there is a ticket with a comment selected.
        private void BtnShowTicketComment_Click(object sender, RoutedEventArgs e) {
            var selectedTicket = (Ticket) LwWinningTickets.SelectedItem;
            if (string.IsNullOrEmpty(selectedTicket?.Comment))
                return;

            var commentWindow = new WndTicketComment(selectedTicket) {Owner = this };
            this.IsEnabled = false;
            commentWindow.ShowDialog();
        }

        //! Open ticket list window upon clicking on the TIckets Sold box.
        private void BtnTicketsSold_Click(object sender, RoutedEventArgs e) {
            var ticketlistWindow = new WndTicketList(LotteryModel) { Owner = this };
            this.IsEnabled = false;
            ticketlistWindow.ShowDialog();
            ProcessAutoSave();
        }

        //! Open a new window for adding new tickets to the lottery.
        private void BtnSellTickets_Click(object sender, RoutedEventArgs e) {
            if (LotteryModel.NumLotteryNumbersLeft < 1) {
                WndDialogMessage.Show(this, "There are no more tickets to sell.", "Sold Out", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var newTicketsWindow = new WndNewTickets(LotteryModel) { Owner = this };
            IgnoreSaveOnClosing = false;
            this.IsEnabled = false;
            newTicketsWindow.ShowDialog();        
            ProcessAutoSave();   
        }

        //! Copy selected ticket's owner (if any) to clipboard.
        private void MenuItemCopyOwner_Click(object sender, RoutedEventArgs e) {
            var selectedTicket = (Ticket) LwWinningTickets.SelectedItem;
            if (selectedTicket != null)
                Clipboard.SetText(selectedTicket.Owner ?? "");
        }

        //! Prompt the end-user to remove the selected (if any) ticket from the list of winners.
        private void MenuItemRemoveWinner_Click(object sender, RoutedEventArgs e) {
            var selectedTicket = (Ticket) LwWinningTickets.SelectedItem;
            if (selectedTicket == null)
                return;

            const string msg = "Do you want to remove the selected ticket from the pool of winners?\n\nThis DOES NOT delete the ticket.";
            if (WndDialogMessage.Show(this, msg, "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            LotteryModel.RemoveWinningTicket(selectedTicket.LotteryNumber);
            LwWinningTickets.Items.Remove(selectedTicket);
            IgnoreSaveOnClosing = false;
            ProcessAutoSave();
        }

        //! Open the window for starting a new lottery. 
        public void OpenNewLotteryWindow() {
            var newLotteryDlg = new WndNewLottery(this) { Owner = this };
            this.IsEnabled = false;
            newLotteryDlg.ShowDialog();
            if (CancelNewLottery)
                this.Close();
        }

        //! Open the comment window if a winning ticket with a comment is double-clicked. 
        private void LwWinningTickets_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selectedTicket = (Ticket) LwWinningTickets.SelectedItem;
            if (string.IsNullOrEmpty(selectedTicket?.Comment))
                return;
            var commentWindow = new WndTicketComment(selectedTicket) {Owner = this};
            this.IsEnabled = false;
            commentWindow.ShowDialog();
        }

        //! If auto-save is checked and a save file has been set, save the file without end-user promt. 
        private void ProcessAutoSave() {
            if (!string.IsNullOrEmpty(SaveFilename) && ChkAutoSave.IsChecked != null && ChkAutoSave.IsChecked.Value)
                SaveToFile();
        }
    }
}
