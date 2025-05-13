using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Infotols
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Contacts_Click(object sender, RoutedEventArgs e)
        {
            RendezvousWindow contactsWindow = new RendezvousWindow();
            contactsWindow.Show();
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            ClientWindow ClientsWindow = new ClientWindow();
            ClientsWindow.Show();
        }

        private void Produit_Click(object sender, RoutedEventArgs e)
        {
            ProduitWindow produitWindow = new ProduitWindow();
            produitWindow.Show();
        }

        private void Facturation_Click(object sender, RoutedEventArgs e)
        {
            FactureWindow factureWindow = new FactureWindow();
            factureWindow.Show();
        }
    }
}