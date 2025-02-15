using Inv_M_Sys.Views.Forms;
using System;
using System.Collections.Generic;
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

namespace Inv_M_Sys.Views.Shared
{
    /// <summary>
    /// Interaction logic for RestockPage.xaml
    /// </summary>
    public partial class RestockPage : Page
    {
        private readonly HomeWindow _homeWindow;

        public RestockPage(HomeWindow homeWindow)
        {
            InitializeComponent();
            _homeWindow = homeWindow;
        }
    }
}
