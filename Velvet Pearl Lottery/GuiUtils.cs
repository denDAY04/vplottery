using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Velvet_Pearl_Lottery {

    //! Global, static resources for GUI controls.
    public class GuiUtils {

        //! Standard colors global for the application.
        public struct Colors {
            //! Color of the base application background.
            public static readonly SolidColorBrush BaseBackground = new SolidColorBrush(Color.FromRgb(0, 14, 25));
            //! Color of module background.
            public static readonly SolidColorBrush ModuleBackground = new SolidColorBrush(Color.FromRgb(1, 40, 57));


            public static readonly SolidColorBrush ListSelection = new SolidColorBrush(Color.FromRgb(1 , 12, 18));
        }

        //! Thickness of module borders.
        public static readonly Thickness ModuleBorderThickness = new Thickness(2);

        public static readonly Brush ListSelectionColor = new SolidColorBrush(Color.FromRgb(1, 12, 18));

        /*!
            \brief KeyDown event handler for positive-integer-only input fields.

            All keys are filtered from the input unless they are numeric (sign and decimal not included),
            left or right arrow, backspace, delete, or tab. 
        */
        public static void TxtInputs_KeyDown(object sender, KeyEventArgs e) {
            // Assume it's bad input and this filter it by marking the key handled.
            // Then look for a valid key input and change the event back to unhandled if found
            // so that the base event handler will input the key.
            e.Handled = true;
            switch (e.Key) {
                case Key.Back:
                case Key.Delete:
                case Key.Left:
                case Key.Right:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.Tab:
                    e.Handled = false;
                    break;
            }
            
        }

    }
}
