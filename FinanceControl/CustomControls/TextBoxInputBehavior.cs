using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;


namespace FinanceControl.CustomControls
{
    public class TextBoxInputBehavior
    {
        public static readonly DependencyProperty AllowOnlyDecimalInputProperty =
            DependencyProperty.RegisterAttached("AllowOnlyDecimalInput", typeof(bool), typeof(TextBoxInputBehavior),
                new FrameworkPropertyMetadata(false, OnAllowOnlyDecimalInputChanged));

        public static bool GetAllowOnlyDecimalInput(DependencyObject obj)
        {
            return (bool)obj.GetValue(AllowOnlyDecimalInputProperty);
        }

        public static void SetAllowOnlyDecimalInput(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowOnlyDecimalInputProperty, value);
        }

        private static void OnAllowOnlyDecimalInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                    textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                }
                else
                {
                    textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                    textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                }
            }
        }
        public static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !IsValidInput(textBox.Text + e.Text))
            {
                e.Handled = true;
            }
        }
        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                return; // Разрешаем Backspace и Delete
            }


        }

        private static bool IsValidInput(string input)
        {
            // Разрешаем цифры, точку и запятую
            return input.All(c => char.IsDigit(c) || c == '.' || c == ',');
        }
        private static bool IsValidKey(Key key)
        {
            return IsDigitKey(key) || key == Key.Decimal || key == Key.OemComma || (key >= Key.D0 && key <= Key.D9);
        }



        private static bool IsDigitKey(Key key)
        {
            return key >= Key.D0 && key <= Key.D9;
        }

    }

}
