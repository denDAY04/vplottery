using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;


namespace Velvet_Pearl_Lottery.Views {

    //! Dialog message, mimicing System.Windows.WndDialogMessage but with custom visuals.
    public partial class WndDialogMessage : Window {

        //! The result of the dialog box, as determined by the button pressed.
        private MessageBoxResult Result { get; set; }

        // Resources for removing window frame buttons (and icon)
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        // ----

        //! Construt a new (empty) dialog window.
        public WndDialogMessage() {
            InitializeComponent();
            Result = MessageBoxResult.None;
        }

        /*!
            \brief Set the message icon in the window. 
            
            \param icon The icon to be added.
        */
        private void SetIconImage(MessageBoxImage icon) {
            switch (icon) {
                case MessageBoxImage.Error:
                    Image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    break;
                case MessageBoxImage.Information:
                    Image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    break;
                case MessageBoxImage.Question:
                    Image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Question.Handle, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    break;
                case MessageBoxImage.Warning:
                    Image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Warning.Handle, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    break;
            }
        }

        /*!
            \brief Set the buttons in the window.
            
            \param buttons The buttons to be added.
        */
        private void SetButtons(MessageBoxButton buttons) {
            Thickness margin;
            switch (buttons) {
                case MessageBoxButton.OK:
                    // Do nothiing; OK button is activated as default.
                    break;
                case MessageBoxButton.OKCancel: 
                    BtnCancel.Visibility = Visibility.Visible;
                    // |ok|  |cancel|
                    margin = BtnOk.Margin;
                    BtnOk.Margin = new Thickness(margin.Left, margin.Top, 2 * margin.Right + BtnCancel.Width, margin.Bottom);
                    break;
                case MessageBoxButton.YesNo: 
                    BtnOk.Visibility = Visibility.Collapsed;
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    // |Yes|  |No|
                    margin = BtnYes.Margin;
                    BtnYes.Margin = new Thickness(margin.Left, margin.Top, 2 * margin.Right + BtnNo.Width, margin.Bottom);
                    break;

                case MessageBoxButton.YesNoCancel:
                    BtnOk.Visibility = Visibility.Collapsed;
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;

                    // |Yes|  |No|  |Cancel| 
                    margin = BtnCancel.Margin;
                    BtnNo.Margin = new Thickness(margin.Left, margin.Top, 2 * margin.Right + BtnCancel.Width, margin.Bottom);
                    margin = BtnNo.Margin;
                    BtnYes.Margin = new Thickness(margin.Left, margin.Top, margin.Right + BtnCancel.Width + BtnCancel.Margin.Right, margin.Bottom);
                    break;
            }
        }

        /*!
            \brief Set the dialog message in the window.
            
            \param message The message to be added.
        */
        private void SetMessage(string message) {
            // For now, just set the message. Further formating possible here in this function if needed.
            TxtblkMessage.Text = message;
        }

        /*!
            \brief Show a dialog message with no icon, window caption and an OK button.
            
            \param owner The owner of the window on which the dialog will be centered.
            \param message The message to be shown in the dialog window.
            
            \return The corresponding button that was pressed, or MessageBoxResult.None 
            if the window was closed without pushing a button.
        */
        public static MessageBoxResult Show(Window owner, string message) {
            var dialog = new WndDialogMessage() {Owner = owner};
            dialog.SetMessage(message);
            dialog.ShowDialog();
            return dialog.Result;
        }

        /*!
            \brief Show a dialog message with no icon, a window caption and an OK button.
            
            \param owner The owner of the window on which the dialog will be centered.
            \param message The message to be shown in the dialog window.
            \param caption The title that is displayed in the window's frame.
            
            \return The corresponding button that was pressed, or MessageBoxResult.None 
            if the window was closed without pushing a button.
        */
        public static MessageBoxResult Show(Window owner, string message, string caption) {
            var dialog = new WndDialogMessage() { Owner = owner, Title = caption ?? "" };
            dialog.SetMessage(message);
            dialog.ShowDialog();
            return dialog.Result;
        }

        /*!
            \brief Show a dialog message with no icon, a window caption and a given set of buttons.
            
            \param owner The owner of the window on which the dialog will be centered.
            \param message The message to be shown in the dialog window.
            \param caption The title that is displayed in the window's frame.
            \param buttons The button configuration of the dialog window.
            
            \return The corresponding button that was pressed, or MessageBoxResult.None 
            if the window was closed without pushing a button.
        */
        public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton buttons) {
            var dialog = new WndDialogMessage() { Owner = owner, Title = caption ?? "" };
            dialog.SetMessage(message);
            dialog.SetButtons(buttons);
            dialog.ShowDialog();
            return dialog.Result;
        }

        /*!
            \brief Show a dialog message with a given icon, a window caption and a given set of buttons.
            
            \param owner The owner of the window on which the dialog will be centered.
            \param message The message to be shown in the dialog window.
            \param caption The title that is displayed in the window's frame.
            \param buttons The button configuration of the dialog window.
            \param icon The image icon that should be displayed in the dialog window.
            
            \return The corresponding button that was pressed, or MessageBoxResult.None 
            if the window was closed without pushing a button.
        */
        public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton buttons, MessageBoxImage icon) {
            var dialog = new WndDialogMessage() { Owner = owner, Title = caption ?? "" };
            dialog.SetMessage(message);
            dialog.SetButtons(buttons);
            dialog.SetIconImage(icon);
            dialog.ShowDialog();
            return dialog.Result;
        }

        //! The available message icons displayed in the dialog window.
        public enum MessageIcon {
            //! Red X for an error message.
            Error,
            //! Yellow exclamation mark for a warning.
            Warning,
            //! Blue I for an informative message.
            Information,
            //! Blue question mark for a question. 
            Question
        }

        //! The available configuration of buttons for the dialog window.
        public enum MessageButtons {
            //! A single OK button.
            Ok,
            //! An OK button and a Cancel button.
            OkCancel,
            //! A Yes button and a No button.
            YesNo,
            // A Yes button, No button, and a Cancel button.
            YesNoCancel            
        }

        //! Set the result of the dialog to the OK button.
        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            Result = MessageBoxResult.OK;
            Close();
        }

        //! Set the result of the dialog to the Cancel button.
        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        //! Set the result of the dialog to the Yes button.
        private void BtnYes_Click(object sender, RoutedEventArgs e) {
            Result = MessageBoxResult.Yes;
            Close();
        }

        //! Set the result of the dialog to the No button.
        private void BtnNo_Click(object sender, RoutedEventArgs e) {
            Result = MessageBoxResult.No;
            Close();
        }

        //! Remove window frame buttons upon the dialog box loading.
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        //! Cancle the closing of the dialog if no result has been set by a button.
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Result == MessageBoxResult.None)
                e.Cancel = true;
        }
    }
}
