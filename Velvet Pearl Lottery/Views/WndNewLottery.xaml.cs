using System;
using System.Windows;
using System.Windows.Media;
using Velvet_Pearl_Lottery.Models;

namespace Velvet_Pearl_Lottery.Views {
    
    //! Dialog window for creating a new lottery session.
    public partial class WndNewLottery : Window {
        
        //! Reference to the main window for the new lottery.
        private WndMain LotteryWindow { get;}
        //! Flag for whether the window is closing in a canceling action.
        private bool IsCanceling { get; set; }

        /*!
            \brief Construct a new dialog window for creating a new lottery session.

            \param lotteryWindow Main window for the lottery session to be created.
        */
        public WndNewLottery(WndMain lotteryWindow) {
            LotteryWindow = lotteryWindow;
            IsCanceling = true;

            InitializeComponent();

            TxtLotteryNumRangeMin.Text = Lottery.LotteryNumberMin.ToString();
            TxtLotteryNumRangeMax.Text = "";
            TxtTicketPrice.Text = "";
            TxtLotteryNumRangeMin.IsReadOnly = true;
            TxtLabErrorMsg.Foreground = Brushes.Red;

            TxtTicketPrice.KeyDown += GuiUtils.TxtInputs_KeyDown;
            TxtLotteryNumRangeMax.KeyDown += GuiUtils.TxtInputs_KeyDown;

            TxtLotteryNumRangeMax.Focus();
        }

        /*!
            \brief ButtonClick event handler for creating a new lottery session.

            If there is error in the input arguments, preventing the lottery from being 
            created, the function sets the error message and marks the area of 
            error by setting its label to a red color.

            \param sender Object that triggered the ButtonClick event.
            \param e Event information.
        */
        private void BtnCreate_Click(object sender, RoutedEventArgs e) {
            LabLotteryNumRange.Foreground = Brushes.Black;
            LabTicketPrice.Foreground = Brushes.Black;


            int lotteryNumMax;
            try {
                lotteryNumMax = int.Parse(TxtLotteryNumRangeMax.Text);
            } catch (FormatException) {
                LabLotteryNumRange.Foreground = Brushes.Red;
                TxtLabErrorMsg.Text = "Maximum lottery number is not a valid integer.";
                return;
            }
            if (lotteryNumMax < Lottery.LotteryNumberMin) {
                TxtLabErrorMsg.Text = "Maximum lottery cannot be less than " + Lottery.LotteryNumberMin + ".";
                return;
            }


            int ticketPrice;
            try {
                ticketPrice = int.Parse(TxtTicketPrice.Text);
            } catch (FormatException) {
                LabTicketPrice.Foreground = Brushes.Red;
                TxtLabErrorMsg.Text = "Ticket price is not a valid integer.";
                return;
            }
            if (ticketPrice < 0) {
                TxtLabErrorMsg.Text = "Ticket price cannot be negative.";
                return;
            }

            var resutt = LotteryWindow.InitNewLotteryModel(lotteryNumMax, ticketPrice);
            switch (resutt)
            {
                case WndMain.LotteryInitResult.Success:
                    break;

                case WndMain.LotteryInitResult.InvalidNumberLimit:
                    LabLotteryNumRange.Foreground = Brushes.Red;
                    TxtLabErrorMsg.Text = Lottery.InvalidLotteryNumberErrorMsg;
                    return;

                case WndMain.LotteryInitResult.InvalidTicketPrice:
                    LabTicketPrice.Foreground = Brushes.Red;
                    TxtLabErrorMsg.Text = Lottery.InvalidTicketPriceErrorMsg;
                    return;

                case WndMain.LotteryInitResult.GeneralError:
                    TxtLabErrorMsg.Text = "An unspecified error occured.";
                    return;
            }

            IsCanceling = false;
            Owner.IsEnabled = true;
            Owner.Focus();
            LotteryWindow.IgnoreSaveOnClosing = false;
            this.Close();
        }

        /*!
            \brief ButtonClick event handler to go back to the Welcome window.
            
            This member closes the new lottery dialog and the main window,
            and opens the welcoming window again.
            
            \param sender Object that triggered the ButtonClick event.
            \param e Event information.
        */
        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            IsCanceling = true;
            this.Close();
        }

        /*!
            \brief Go back to welcome screen if closing is a canceling action.

            If the window is closing in any other way than through the create button, 
            the closing of this window will cause the program to go back to the welcome 
            splash screen. If, however, the closing is due to the create button, no special
            action is taken.
        */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!IsCanceling)
                return;

            var welcomeWindow = new WndWelcome() { Owner = this, SkipUpdateCheck = true};
            welcomeWindow.Show();
            welcomeWindow.Owner = null;

            LotteryWindow.CancelNewLottery = true;
        }
    }
}
