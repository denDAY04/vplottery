using System.Threading;
using System.Windows;

namespace Velvet_Pearl_Lottery.Views {
    //! Window for showing the status of the application updating.
    public partial class WndUpdateStatus : Window {
        
        
        private CancellationTokenSource CancelTokenHandle { get; set; }

        //! Property for the status text displayed in the window.
        public string StatusText {
            get { return TxtUpdateStatus.Text; }
            set { TxtUpdateStatus.Text = value; }
        }

        /*!
            \brief Construct a new window for displaying update status.

            \param cancellationHandle Source handle for the cancellation 
            token that will be used to cancel the update process.
        */
        public WndUpdateStatus(CancellationTokenSource cancellationHandle) {
            CancelTokenHandle = cancellationHandle;

            InitializeComponent();
        }
        
        //!Cancel the update process and close the window.
        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            CancelTokenHandle.Cancel();
            Close();
        }
    }
}
