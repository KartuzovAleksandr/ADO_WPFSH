using System.Windows;
using System.Windows.Input;

namespace ADO_WPFSH
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
