using System.Collections.Generic;
using System.Windows;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery.Views {
    
    //! Window with a list of all sold tickets.
    public partial class WndTicketList : Window {

        //! Reference to the active lottery session.
        private Lottery LotteryModel { get; set; }

        /*!
            \brief Construct a new window for showing all sold tickets.
            \param tickets List of tickets to show. 
        */
        public WndTicketList(Lottery currentLottery) {
            LotteryModel = currentLottery;

            InitializeComponent();

            LotteryModel.Tickets.ForEach(t => LwTickets.Items.Add(t));
            BtnShowComment.IsEnabled = false;
        }

        //! Close window.
        private void BtnCLose_Click(object sender, RoutedEventArgs e) {
            Owner.Focus();
            Close();
        }

        //! Show comment for the selected ticket, if any.
        private void BtnShowComment_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(((Ticket) LwTickets.SelectedItem)?.Comment))
                return;

            var commentWindow = new WndTicketComment((Ticket) LwTickets.SelectedItem) {Owner = this};
            this.IsEnabled = false;
            commentWindow.ShowDialog();
        }

        //! Enable or disable the comment button whenever the ticket selection changes.
        private void LwTickets_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            BtnShowComment.IsEnabled = !string.IsNullOrEmpty(((Ticket) LwTickets.SelectedItem)?.Comment);
        }

        //! Prompt the end-user to remove the selected ticket (if any) from the list of sold tickets. 
        private void MenuItemRemoveWinner_Click(object sender, RoutedEventArgs e) {
            var selectedTicket = (Ticket) LwTickets.SelectedItem;
            if (selectedTicket == null)
                return;

            const string msg = "Do you want to remove the sold ticket?\nDoing so will make the number puchasable again.";
            if (WndDialogMessage.Show(this, msg, "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            LotteryModel.RemoveTicket(selectedTicket);
            LwTickets.Items.Remove(selectedTicket);
            ((WndMain) Owner).IgnoreSaveOnClosing = false;
        }

        //! Re-enable the owning window. 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Owner != null)
                Owner.IsEnabled = true;
        }

        //! Show comment of the ticket that was double-clicked, if there is any.
        private void LwTickets_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var selectedTicket = (Ticket)LwTickets.SelectedItem;
            if (string.IsNullOrEmpty(selectedTicket?.Comment))
                return;
            var commentWindow = new WndTicketComment(selectedTicket) { Owner = this };
            this.IsEnabled = false;
            commentWindow.ShowDialog();
        }
    }
}
