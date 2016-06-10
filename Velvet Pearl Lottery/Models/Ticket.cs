//! Namespace for model classes.
namespace Velvet_Pearl_Lottery.Models {

    //! Model class for a lottery ticket.
    public class Ticket
    {
        //! Name of the ticket's owner.
        public string Owner { get; set; }

        //! The lottery number of the ticket.
        public int LotteryNumber { get; set; }

        //! The price of the ticket.
        public int Price { get; set; }

        //! Any additional comment for the ticket.
        public string Comment { get; set; }
    }
}
