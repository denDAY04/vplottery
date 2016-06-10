using System.Windows;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery.Views {
    
    //! Simple window for displaying a ticket's comment.
    public partial class WndTicketComment : Window {

        //! Ticket for which to show (and possible edit) the comment. 
        public Ticket CurrentTicket { get; private set; }

        //! Flag for whether the windows should allow the ticket comment to be edited.
        private bool AllowCommentEdit { get; set; }

        /*!
            \brief Construct a new ticket-comment window. 
            \param ticket Ticket for which to display and possible edit the comment.
            \param enableEdit Flag for whether the window will alow the user to edit the comment. 
            Defaults to false.
        */
        public WndTicketComment(Ticket ticket, bool enableEdit = false) {
            CurrentTicket = ticket;
            AllowCommentEdit = enableEdit;
            InitializeComponent();

            TxtTicketComment.Text = CurrentTicket.Comment;
            TxtTicketComment.IsReadOnly = !AllowCommentEdit;
        }

        /*!
            \brief ButtonClick event handler that closes the window.         

            If the window was constructed with parameter enableEdit as true, the 
            comment will be saved to the ticket before the window is closed.
        */
        private void BtnClose_Click(object sender, RoutedEventArgs e){
            if (AllowCommentEdit)
                CurrentTicket.Comment = TxtTicketComment.Text;
            Owner.Focus();
            this.Close();
        }

        private void Window_Closed(object sender, System.EventArgs e) {
            if (Owner != null)
                Owner.IsEnabled = true;
        }
    }
}
