using System.ComponentModel;
using System.Linq;
using System.Windows;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery.Views {

    //! Window for creating new tickets for a person.
    public partial class WndNewTickets : Window {

        //! Reference to the lottery the tickets will belong to.
        private Lottery CurrentLottery { get; set; }
        //! Flag for whether the created tickets should be discarded upon closing the window.
        private bool DiscardTicketsOnClose { get; set; }
        //! Flag for whether a ticket will be assigned a random or specific lottery number.
        private bool BuyRandomLotteryNumber { get; set; }

        /*!
            \brief Create a new window for creating tickets to a person.
            \param lottery Reference to the lottery which the tickets are being sold for. 
        */
        public WndNewTickets(Lottery lottery) {
            CurrentLottery = lottery;
            DiscardTicketsOnClose = true;
            BuyRandomLotteryNumber = true;

            InitializeComponent();

            TxtPrize.Text = "0";

            TxtSpecificLotteryNumber.KeyDown += GuiUtils.TxtInputs_KeyDown;
            BtnAddComment.IsEnabled = false;
            BtnRemoveTicket.IsEnabled = false;
        }

        //! Closes the window without discarding the saved tickets.
        private void BtnCreate_Click(object sender, RoutedEventArgs e) {
            var ticketholder = TxtOwner.Text;
            foreach (Ticket ticket in LwTickets.Items) {
                ticket.Owner = ticketholder;
            }
            DiscardTicketsOnClose = false;      // Prevent the create tickets from being discarded.
            this.Close();
        }

        //! Close the window without saving the new ticket.
        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            Owner?.Focus();
            this.Close();
        }
        
        /*!
            \brief Open the ticket comment window to add a comment.
            
            If no lottery number is selected in the list of tickets
            an error message is displayed for the end-user and no 
            further action is taken.
        */
        private void BtnAddComment_Click(object sender, RoutedEventArgs e) {
            if (!HasTicketSelected()) 
                return;

            var commentWindow = new WndTicketComment((Ticket)LwTickets.SelectedItem, true) {Owner = this};
            this.IsEnabled = false;
            commentWindow.ShowDialog();
        }

        /*!
            \brief  Remove the selected ticket (if any) from the lottery.

            If no ticket is selected, the function does nothing besides showing an error message. 
            If removing the selected ticket leaves the list empty, the add-comment button 
            and delete-ticket button are disabled.
        */
        private void BtnRemoveTicket_Click(object sender, RoutedEventArgs e) {
            if (!HasTicketSelected())
                return;

            var ticket = (Ticket) LwTickets.SelectedItem;

            if (WndDialogMessage.Show(this, "Do you want to delete this ticket?\n\nLottery number: " + ticket.LotteryNumber, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            CurrentLottery.RemoveTicket(ticket);
            LwTickets.Items.Remove(ticket);

            var oldPrice = int.Parse(TxtPrize.Text);
            TxtPrize.Text = "" + (oldPrice - ticket.Price);

            if (LwTickets.Items.Count < 1) {
                BtnAddComment.IsEnabled = false;
                BtnRemoveTicket.IsEnabled = false;
            }
            
        }

        /*!
            \brief Test if end-user has selected a lottery number in the list of tickets.

            If no selection is active an appropriate error message is shown.

            \return true if there is an item selected, and false if not. 
        */
        private bool HasTicketSelected() {
            if (LwTickets.SelectedItem != null)
                return true;

            WndDialogMessage.Show(this, "No lottery number selected.", "Missing selection", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        //! Discards the tickets on the window closing if member DiscardTicketsOnClose is true. 
        private void NewTicketsWindow_OnClosing(object sender, CancelEventArgs e) {
            if (DiscardTicketsOnClose) {
                foreach (Ticket ticket in LwTickets.Items) {
                    CurrentLottery.RemoveTicket(ticket);
                }
            }
            if (Owner != null)
                Owner.IsEnabled = true;
        }

        //! Attempt to add a new ticket, either with a random or specific lottery number.
        private void BtnAddTicket_Click(object sender, RoutedEventArgs e) {
            int actualLotteryNumber;

            if (BuyRandomLotteryNumber) {
                actualLotteryNumber = CurrentLottery.BuyLotteryNumber(TxtOwner.Text);
                // Makes sure that there was a ticket left to buy.
                if (actualLotteryNumber == Lottery.InvalidLotteryNumber) {
                    WndDialogMessage.Show(this, "No more tickets available.", "No tickets left", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            } else {
                if (string.IsNullOrEmpty(TxtSpecificLotteryNumber.Text)) {
                    WndDialogMessage.Show(this, "No lottery number given.\nIf you want a random one, check the random box.",
                        "Missing Number", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var userLotteryNumber = int.Parse(TxtSpecificLotteryNumber.Text);
                var result = CurrentLottery.BuyLotteryNumber(userLotteryNumber, TxtOwner.Text);
                switch (result) {
                    case LotteryNumberSaleResult.Success:
                        actualLotteryNumber = userLotteryNumber;
                        break;

                    case LotteryNumberSaleResult.NumberAlreadySold:
                        WndDialogMessage.Show(this, "The number has already been sold.", "Number Unavailable",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;

                    case LotteryNumberSaleResult.NumberOutOfRange:
                        var msg = "The number is not in the alllowed range: " + Lottery.LotteryNumberMin + " - " +
                                  CurrentLottery.LotteryNumberMax + ".";
                        WndDialogMessage.Show(this, msg, "Invalid Number", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;

                    default:
                        WndDialogMessage.Show(this, "An unspecified error occured.", "Unspecified Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }
            }

            // Add to list and update the tottal price.
            var ticket = CurrentLottery.Tickets.Find(T => T.LotteryNumber == actualLotteryNumber);
            LwTickets.Items.Add(ticket);
            LwTickets.ScrollIntoView(ticket);
            var oldPrice = int.Parse(TxtPrize.Text);
            TxtPrize.Text = "" + (oldPrice + ticket.Price);

            // Enabled the buttons for the list of tickets.
            BtnAddComment.IsEnabled = true;
            BtnRemoveTicket.IsEnabled = true;

        }

        //! Disable input field for specific lottery number and set flag for buying random number.
        private void ChkRandomLotteryNumber_Checked(object sender, RoutedEventArgs e) {
            TxtSpecificLotteryNumber.IsEnabled = false;
            BuyRandomLotteryNumber = true;
        }

        //! Ensable input field for specific lottery number and remove flag for buying random number.
        private void ChkRandomLotteryNumber_Unchecked(object sender, RoutedEventArgs e) {
            TxtSpecificLotteryNumber.IsEnabled = true;
            BuyRandomLotteryNumber = false;
        }
    }
}
