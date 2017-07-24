using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Witcher3MapViewer
{
    /// <summary>
    /// Interaction logic for SearchBox2.xaml
    /// </summary>
    public partial class SearchBox2 : UserControl
    {
        public SearchBox2()
        {
            InitializeComponent();
            RefreshButtonState();
        }      

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value);}
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object),
              typeof(SearchBox2), new PropertyMetadata(null));

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            if (QueryText.Text == "")
            {
                NextItemButton.IsEnabled = false;
                PrevItemButton.IsEnabled = false;
            }
            else if (!NextItemButton.IsEnabled)
            {
                NextItemButton.IsEnabled = true;
                PrevItemButton.IsEnabled = true;
            }
            Value = QueryText.Text;
        }
    }
}
