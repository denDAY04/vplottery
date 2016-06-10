using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Velvet_Pearl_Lottery.Models {

    //! Result of attempting to buy a specific lottery number.
    public enum LotteryNumberSaleResult {
        //! Sale was successful.
        Success,
        //! Lottery number was already sold.
        NumberAlreadySold,
        //! Lottery number is not within the valid range.
        NumberOutOfRange
    }

    //! Result of exporting or importing a lottery data file.
    public enum LotteryExportImportResult {
        //! Export or import action succeeded.
        Success,
        //! The program does not have access to the specified file.
        BadAuthorization,
        //! The specified file does not qualify as a legal file.
        InvalidFileName,
        //! Something unspecified, such an IO exception, occured.
        UnspecifiedError
    }

    //! Model class for a lottery session.
    public class Lottery
    {
        //! Const for invalid lottery numbers.
        public const  int InvalidLotteryNumber = -1;
        //! Const for invalid lottery number error message.
        public static readonly string InvalidLotteryNumberErrorMsg = "Invalid lottery number";
        //! Const for invalid ticket prices.
        public static readonly int InvalidTicketPrice = -1;
        //! Const for invalid ticket price error message.
        public static readonly string InvalidTicketPriceErrorMsg = "Invalid ticket price";
        //! Const for the minimum lottery number that can be drawn.
        public static readonly int LotteryNumberMin = 1;        

        //! Tickets bought for the lottery.
        public List<Ticket> Tickets { get; set; }
        //! Lottery numbers drawn for winning. 
        public List<int> WinningLotteryNumbers { get; set; }
        //! Availible lottery numbers still left; the number matching its position in the list.
        public List<int> LotteryNumbers { get; set; }
        /*!
            \brief The upper limit of lottery numbers in this lottery. 

            That is, if this member has the value 100, the range of lottery
            numbers are Lottery.LotteryNumberMin through 100.
        */
        public int LotteryNumberMax { get; private set; }
        //! Price per lottery ticket.
        public int TicketPrice { get; private set; }
        //! Number of lottery numbers that have not yet been sold.
        public int NumLotteryNumbersLeft { get; private set; }

        //! The random number generator for determining winning lottery numbers.
        private Random Rng { get; set; }


        /*!
            \brief Constructor for a lottery session.

            \param lotteryNumberLimit The maximum value of lottery number.
            \param ticketPrice The price per ticket.
            \param existingTickets List of tickets already sold for the lottery session. Defaults to null.
            \param winningLottryNumbers List of lottery numbers already drawn for winning. Defaults to null. 

            \throw System.ArgumentException if either lotteryNumberLimit equals Lottery.InvalidLotteryNumber,
            or ticketPrice equals Lottery.InvalidTicketPrice, in which case the exception message is set to 
            Lottery.InvalidLotteryNumberErrorMsg or Lottery.InvalidTicketPriceErrorMsg respectfully.
         */
        public Lottery(int lotteryNumberLimit, int ticketPrice, List<Ticket> existingTickets = null, List<int> winningLottryNumbers = null ) {
            if (lotteryNumberLimit < LotteryNumberMin)
                throw new ArgumentException(InvalidLotteryNumberErrorMsg);
            if (ticketPrice < 0)
                throw new ArgumentException(InvalidTicketPriceErrorMsg);

            LotteryNumberMax = lotteryNumberLimit;
            TicketPrice = ticketPrice;
            WinningLotteryNumbers = winningLottryNumbers ?? new List<int>();
            Rng = new Random();
            NumLotteryNumbersLeft = LotteryNumberMax;

            // Flag unused numbers and then store the lottery number-range.
            LotteryNumbers = new List<int>(LotteryNumberMin + LotteryNumberMax);
            for (var unusedPos = 0; unusedPos < LotteryNumberMin; unusedPos++){
                LotteryNumbers.Add(InvalidLotteryNumber);
            }
            for (var lotteryNumber = LotteryNumberMin; lotteryNumber <= LotteryNumberMax; lotteryNumber++) {
                LotteryNumbers.Add(lotteryNumber);
            }

            // If existing session, loads tickets and remove their numbers from the availible number range. 
            if (existingTickets != null) {
                Tickets = existingTickets;
                foreach (var ticket in Tickets) {
                    LotteryNumbers[ticket.LotteryNumber] = InvalidLotteryNumber;
                    --NumLotteryNumbersLeft;
                }
            } else {
                Tickets = new List<Ticket>();
            }
        }

        /*!
            \brief Get the sum of the price of all tickets sold.
            \return  The sum of the price for every sold ticket.
        */
        public int ProfitFromTickets()
        {
            return Tickets.Sum(ticket => ticket.Price);
        }

        /*!
            \brief Draw a winning number for the lottery. 
            
            The draw number is ensured to not have bee drawn before in this lottery, 
            and to have been sold for a ticket.

            \return null if no lottery numbers have been sold, or all tickets have won; 
            otherwise the ticket with the winning number is returned. 
        */
        public Ticket DrawWinningNumber() {
            // If there are no sold tickets, don't draw a number.
            if (Tickets.Count < 1)
                return null;
            // If every sold ticket has won, don't draw a number.
            if (WinningLotteryNumbers.Count == Tickets.Count)
                return null;

            // Pick a winning number, checking that it's not still availibe 
            // (i.e. a ticket has this number) and it's not already been picked.
            var winningNumber = Rng.Next(LotteryNumberMin, LotteryNumberMax + 1);
            while (LotteryNumbers[winningNumber] != InvalidLotteryNumber || WinningLotteryNumbers.Contains(winningNumber)){
                winningNumber = Rng.Next(LotteryNumberMin, LotteryNumberMax + 1);
            }

            // Store winning number and return the winning ticket.
            WinningLotteryNumbers.Add(winningNumber);
            return Tickets.Find(T => T.LotteryNumber == winningNumber);
        }

        /*!
            \brief Try and buy a specific lottery number.
            
            \param lotteryNumber The desired lottery number.
            \param buyer Name of the person buying the lottery number.

            \return LotteryNumberSaleResult.NumberOutOfRange if the number is 
            out of the lottery range, LotteryNumberSaleResult.NumberAlreadySold if
            the number has already been sold, and LotteryNumberSaleResult.Success if 
            the number is succesfully sold.
        */
        public LotteryNumberSaleResult BuyLotteryNumber(int lotteryNumber, string buyer)
        {
            if (lotteryNumber > LotteryNumberMax)
                return LotteryNumberSaleResult.NumberOutOfRange;

            // Check if number is already sold - also indirectly takes care of no numbers left for sale.
            if (LotteryNumbers[lotteryNumber] == InvalidLotteryNumber)
                return LotteryNumberSaleResult.NumberAlreadySold;

            --NumLotteryNumbersLeft;
            LotteryNumbers[lotteryNumber] = InvalidLotteryNumber;
            Tickets.Add(new Ticket {LotteryNumber = lotteryNumber, Owner = buyer, Price = TicketPrice});
            return LotteryNumberSaleResult.Success;
        }

        /*!
            \brief Try and buy a random lottery number.
            
            \param buyer Name of the person buying the lottery number.
            
            \return Lottery.InvalidLotteryNumber if there are no lottery numbers left to be 
            bought, and the the bought lottery number if the sale is successful.  
        */
        public int BuyLotteryNumber(string buyer) {
            if (NumLotteryNumbersLeft < 1)
                return InvalidLotteryNumber;

            // Pick number and try and buy it.
            var lotteryNumber = Rng.Next(LotteryNumberMin, LotteryNumberMax) + 1;
            var result = BuyLotteryNumber(lotteryNumber, buyer);
            while (result == LotteryNumberSaleResult.NumberAlreadySold) {
                lotteryNumber = Rng.Next(LotteryNumberMin, LotteryNumberMax + 1);
                result = BuyLotteryNumber(lotteryNumber, buyer);
            }

            if (result == LotteryNumberSaleResult.NumberOutOfRange)
                throw new ArgumentException("Internal error for random-bought ticket: number was out of the lottery range.");

            return lotteryNumber;
        }
        
        /*!
            \brief Remove a ticket from the lottery.
            
            If the ticket has not been sold no action will be made. 
            Otherwise, the ticket's number will be added to the availible numbers again,
            and if it was a winning number it will be removed from the list of winners as well. 
        */
        public void RemoveTicket(Ticket ticket)
        {
            var target = Tickets.Find(T => T.LotteryNumber == ticket.LotteryNumber);
            if (target == null)
                return;

            // Remove the number from winners (if there) and add it to the availible numbers again,
            // before removing it from the list of sold tickets.
            WinningLotteryNumbers.Remove(target.LotteryNumber);
            ++NumLotteryNumbersLeft;
            LotteryNumbers[target.LotteryNumber] = target.LotteryNumber;
            Tickets.Remove(target);
        }

        //! Structure for holding constants for the various xml tags and their names.
        private struct XmlTagNames {
            //! XML root name node for the lottery.
            public const string Root = "VeletPearLottery";
            //! XML node name for the upper limit of the lottery range.
            public const string MaxLotteryNumber = "MaxLotteryNumber";
            //! XML node name for the sold tickets.
            public const string Tickets = "Tickets";
            //! XML attribute name for the price of the tickets.
            public const string TicketsPriceAttribute = "Price";
            //! XML node name for a ticket.
            public const string Ticket = "Ticket";
            //! XML node name for the ower of a ticket.
            public const string TicketOwner = "Owner";
            //! XML node namefor the lottery number of a ticket.
            public const string TicketLotteryNumber = "LotteryNumber";
            //! XML node name for the comment of a ticket.
            public const string TicketComment = "Comment";
            //! XML node name for the winning lottery numbers.
            public const string WinningLotteryNumbers = "Winners";
            //! XML node name for a winning lottery number.
            public const string WinningLotteryNumber = "Winner";
            //! XML attribute name for the lottery number of the winning node.
            public const string WinningLotteryNumberAttribute = "LotteryNumber";
        }

        /*!
            \brief Export the lottery session to a given file.            
            \param exportFile String stream denoting target export file.
        */
        public void ExportToFile(Stream exportFile) {
            var xmlRoot = new XElement(XmlTagNames.Root, 
                new XElement(XmlTagNames.MaxLotteryNumber, LotteryNumberMax),
                new XElement(XmlTagNames.Tickets, new XAttribute(XmlTagNames.TicketsPriceAttribute, TicketPrice), 
                    Tickets.Select(t => new XElement(XmlTagNames.Ticket, 
                        // Only lottery number is mandatory. Owner and comment are only included if not empty.
                        new XElement(XmlTagNames.TicketLotteryNumber, t.LotteryNumber),
                        (string.IsNullOrEmpty(t.Owner) ? null : new XElement(XmlTagNames.TicketOwner, t.Owner)),
                        (string.IsNullOrEmpty(t.Comment) ? null : new XElement(XmlTagNames.TicketComment, t.Comment))
                    ))
                ),
                // Winners are optional. If none have been drawn, don't empty tag.
                (WinningLotteryNumbers.Any() ? new XElement("Winners", 
                    WinningLotteryNumbers.Select(w => new XElement("Winner", new XAttribute("LotteryNumber", w))))
                : null)
            );
            
            xmlRoot.Save(exportFile);
        }

        //! Result of trying to import a lottery save file.
        public enum ImportResult {
            //! The import was successful.
            Success,
            //! The import failed; the filestream did not denote a valid save file.
            InvalidFile,
            //! The import failed; malformed data.
            ParseError
        }

        /*!
            \brief Import the lottery from a save file.

            NOTE that this function DOES NOT close the file's stream.

            \param xmlImportFIle Filestream denoting the file to import the lottery from. Must be xml. 

            \param importedLottery [out] Return parameter to hold a reference to the imported lottery. 
            Is null if an error occured.

            \param parseErrorMsg [out] Return parameter to hold an error message in the event that 
            the import failed.

            \return Lottery.ImportResult.Success if the import completed without any errors. 
            Lottery.ImportResult.InvalidFile if the filestream does not denote a valid xml file.
            Lottery.ImportResult.ParseError if an orror occured during the parsing of the xml tree,
            either due to malformed tags or malfored data. 
            In both the case of ParseError and InvalidFile, importedLottery will be null and 
            parseErrorMsg will be set to a descriptie message of the error.
        */
        public static ImportResult ImportFromFile(Stream xmlImportFIle, out Lottery importedLottery, out string parseErrorMsg) {
            var xmlRoot = XDocument.Load(xmlImportFIle).Root;
            importedLottery = null;
            parseErrorMsg = "";

            // Check for root tag.
            if (xmlRoot == null) {
                return ImportResult.InvalidFile;
            }
            if (xmlRoot.Name != XmlTagNames.Root) {
                parseErrorMsg = "Missing root tag " + XmlTagNames.Root;
                return ImportResult.ParseError;
            }

            // Parse data

            // MaxLotteryNumber
            int maxLotteryNumber;
            var node = xmlRoot.Descendants(XmlTagNames.MaxLotteryNumber).FirstOrDefault();
            if (node == null) {
                parseErrorMsg = "Missing tag " + XmlTagNames.MaxLotteryNumber;
                return ImportResult.ParseError;
            }
            try {
                maxLotteryNumber = int.Parse(node.Value);
            } catch (FormatException) {
                parseErrorMsg = "Malformed data in tag " + XmlTagNames.MaxLotteryNumber;
                return ImportResult.ParseError;
            }

            // Tickets
            var tickets = new List<Ticket>();
            node = xmlRoot.Descendants(XmlTagNames.Tickets).FirstOrDefault();
            if (node == null) {
                parseErrorMsg = "Missing tag " + XmlTagNames.Tickets;
                return ImportResult.ParseError;
            }

            // Ticket price attribute
            int ticketPrice;
            var priceAttribute = node.Attribute(XmlTagNames.TicketsPriceAttribute);
            if (priceAttribute == null) {
                parseErrorMsg = "Missing " + XmlTagNames.TicketsPriceAttribute + " attribute in tag" +
                                XmlTagNames.Tickets;
                return ImportResult.ParseError;
            }
            try {
                ticketPrice = int.Parse(priceAttribute.Value);
            } catch (FormatException) {
                parseErrorMsg = "Malformed data in attribute " + XmlTagNames.TicketsPriceAttribute + " of tag " +
                                XmlTagNames.Tickets;
                return ImportResult.ParseError;
            }

            // Sequence of tickets - Optional
            if (node.Descendants(XmlTagNames.Ticket).Any()) {
                foreach (var ticket in node.Descendants(XmlTagNames.Ticket)) {
                    // Mandatory lottery number
                    Ticket parsedTicket;
                    var lotteryNumberNode = ticket.Descendants(XmlTagNames.TicketLotteryNumber).FirstOrDefault();
                    if (lotteryNumberNode == null) {
                        parseErrorMsg = "Missing a " + XmlTagNames.TicketLotteryNumber + " sub-tag in tag " +
                                        XmlTagNames.Tickets;
                        return ImportResult.ParseError;
                    }
                    try {
                        parsedTicket = new Ticket {LotteryNumber = int.Parse(lotteryNumberNode.Value), Price = ticketPrice};
                    } catch (FormatException) {
                        parseErrorMsg = "Malformed data in sub-tag " + XmlTagNames.TicketLotteryNumber + " of tag " + XmlTagNames.Ticket;
                        return ImportResult.ParseError;
                    }

                    // Optional owner
                    var ownerNode = ticket.Descendants(XmlTagNames.TicketOwner).FirstOrDefault();
                    if (ownerNode != null) 
                        parsedTicket.Owner = ownerNode.Value;

                    // Optional comment
                    var commentNode = ticket.Descendants(XmlTagNames.TicketComment).FirstOrDefault();
                    if (commentNode != null)
                        parsedTicket.Comment = commentNode.Value;

                    tickets.Add(parsedTicket);
                }
            }        
            
            // Sequence of winning numbers - Optional
            var winningNumbers = new List<int>();
            node = xmlRoot.Descendants(XmlTagNames.WinningLotteryNumbers).FirstOrDefault();
            if ( node != null && node.Descendants(XmlTagNames.WinningLotteryNumber).Any()) {
                foreach (var winner in node.Descendants(XmlTagNames.WinningLotteryNumber)) {
                    // Mandatory lottery number attribute
                    var lotteryNumberAttribute = winner.Attribute(XmlTagNames.WinningLotteryNumberAttribute);
                    if (lotteryNumberAttribute == null) {
                        parseErrorMsg = "Missing a " + XmlTagNames.WinningLotteryNumberAttribute + " attribute in tag " +
                                        XmlTagNames.WinningLotteryNumber;
                        return ImportResult.ParseError;
                    }
                    try {
                        winningNumbers.Add(int.Parse(lotteryNumberAttribute.Value));
                    } catch (FormatException) {
                        parseErrorMsg = "Malformed data in attribute " + XmlTagNames.WinningLotteryNumberAttribute + " of tag " + XmlTagNames.WinningLotteryNumber;
                        return ImportResult.ParseError;
                    }
                }
            }

            importedLottery = new Lottery(maxLotteryNumber, ticketPrice, tickets, winningNumbers);
            return ImportResult.Success;    
        }

        /*!
            \brief Remove a winning lottery number, if it's in the collection.
            
            NOTE that this DOES NOT delete the ticket with the winning number, 
            it simply removes the winning association.  
        */
        public void RemoveWinningTicket(int winningLotteryNumber) {
            WinningLotteryNumbers.Remove(winningLotteryNumber);
        }
    }
}
