using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Infotols
{
    /// <summary>
    /// Logique d'interaction pour FactureWindow.xaml
    /// </summary>
    public partial class FactureWindow : Window
    {
        private ObservableCollection<Invoice> factures;
        public FactureWindow()
        {
            InitializeComponent();
            LoadClients();
            LoadProduits();
            LoadFactures();
        }
        // Charger les clients dans le ComboBox
        private void LoadClients()
        {
            var clients = Bdd.GetAllClients();
            ClientComboBox.ItemsSource = clients;
            ClientComboBox.DisplayMemberPath = "Nom";
            ClientComboBox.SelectedValuePath = "Id";
        }

        // Charger les produits dans le ComboBox
        private void LoadProduits()
        {
            var produits = Bdd.GetAllProduits();
            ProduitComboBox.ItemsSource = produits;
            ProduitComboBox.DisplayMemberPath = "Name";
            ProduitComboBox.SelectedValuePath = "Id";
        }

        // Ajouter un produit au DataGrid
        private void AjouterProduitButton_Click(object sender, RoutedEventArgs e)
        {
            var produit = (Produit)ProduitComboBox.SelectedItem;
            if (produit == null)
            {
                MessageBox.Show("Veuillez sélectionner un produit.");
                return;
            }

            if (!int.TryParse(QuantiteTextBox.Text, out int quantite) || quantite <= 0)
            {
                MessageBox.Show("Veuillez saisir une quantité valide.");
                return;
            }
            if (quantite > produit.Stock)
            {
                MessageBox.Show("Stock Insufissant. Stock actuel: {produit.Stock}");
                return;
            }
            decimal montant = quantite * produit.Price;

            var ligne = new
            {
                Produit = produit,
                Quantite = quantite,
                PrixUnitaire = produit.Price,
                Montant = montant
            };

            ProduitsDataGrid.Items.Add(ligne);
            produit.Stock -= quantite;
            ProduitComboBox.Items.Refresh();
            RecalculerTotal();
        }

        // Calculer le total
        private void RecalculerTotal()
        {
            decimal total = 0;
            foreach (var item in ProduitsDataGrid.Items)
            {
                dynamic ligne = item;
                total += ligne.Montant;
            }
            TotalTextBox.Text = total.ToString("F2");
        }

        // Ajouter une nouvelle facture
        private void EnregistrerFactureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ClientComboBox.SelectedValue == null || DateFacturePicker.SelectedDate == null || StatutComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez remplir tous les champs.");
                    return;
                }

                int clientId = (int)ClientComboBox.SelectedValue;
                DateTime date = DateFacturePicker.SelectedDate.Value;
                decimal total = decimal.Parse(TotalTextBox.Text);
                string statut = ((ComboBoxItem)StatutComboBox.SelectedItem).Content.ToString();

                int invoiceId = Bdd.InsertInvoice(clientId, date, total, statut);

                foreach (var item in ProduitsDataGrid.Items)
                {
                    dynamic ligne = item;
                    int produitId = ligne.Produit.Id;
                    int quantite = ligne.Quantite;
                    decimal unitPrice = ligne.PrixUnitaire;

                    // Enregistrer la ligne de facture
                    Bdd.InsertInvoiceLine(invoiceId, produitId, quantite, unitPrice);

                    // Mettre à jour le stock
                    var produit = ligne.Produit;
                    produit.Stock -= quantite;
                    Bdd.UpdateProduit(produit.Id, produit.Name, produit.Description, produit.Price, produit.Stock);
                }

                MessageBox.Show("Facture ajoutée avec succès.");
                ResetForm();
                LoadFactures();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        // Charger les factures dans la DataGrid
        private void LoadFactures()
        {
            var allFactures = Bdd.GetAllInvoices();
            foreach (var f in allFactures)
            {
                var client = Bdd.GetAllClients().FirstOrDefault(c => c.Id == f.ClientId);
                f.ClientName = client != null ? client.Nom : "Inconnu";
            }

            factures = new ObservableCollection<Invoice>(allFactures);
            FacturesDataGrid.ItemsSource = factures;
        }

        // Sélection d'une facture pour modification
        private void ModifierButton_Click(object sender, RoutedEventArgs e)
        {
            if (FacturesDataGrid.SelectedItem is not Invoice selected)
            {
                MessageBox.Show("Veuillez sélectionner une facture.");
                return;
            }

            var lignes = Bdd.GetInvoiceLinesByInvoiceId(selected.InvoiceId);
            ProduitsDataGrid.Items.Clear();
            foreach (var ligne in lignes)
            {
                var produit = Bdd.GetAllProduits().FirstOrDefault(p => p.Id == ligne.ProductId);
                if (produit != null)
                {
                    ProduitsDataGrid.Items.Add(new
                    {
                        Produit = produit,
                        Quantite = ligne.Quantity,
                        PrixUnitaire = ligne.UnitPrice,
                        Montant = ligne.Quantity * ligne.UnitPrice
                    });
                }
            }

            ClientComboBox.SelectedValue = selected.ClientId;
            DateFacturePicker.SelectedDate = selected.InvoiceDate;
            TotalTextBox.Text = selected.Total.ToString("F2");

            foreach (ComboBoxItem item in StatutComboBox.Items)
            {
                if (item.Content.ToString() == selected.Status)
                {
                    StatutComboBox.SelectedItem = item;
                    break;
                }
            }

            // Supprimer l'ancienne facture pour la remplacer
            Bdd.DeleteInvoiceLinesByInvoiceId(selected.InvoiceId);
            Bdd.DeleteInvoice(selected.InvoiceId);
        }

        // Supprimer une facture
        private void SupprimerFactureButton_Click(object sender, RoutedEventArgs e)
        {
            if (FacturesDataGrid.SelectedItem is not Invoice selected)
            {
                MessageBox.Show("Veuillez sélectionner une facture à supprimer.");
                return;
            }

            var result = MessageBox.Show("Confirmer la suppression de cette facture ?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Bdd.DeleteInvoiceLinesByInvoiceId(selected.InvoiceId);
                Bdd.DeleteInvoice(selected.InvoiceId);
                LoadFactures();
                ResetForm();
                MessageBox.Show("Facture supprimée.");
            }
        }

        // Réinitialiser le formulaire
        private void ResetForm()
        {
            ClientComboBox.SelectedIndex = -1;
            ProduitComboBox.SelectedIndex = -1;
            QuantiteTextBox.Clear();
            ProduitsDataGrid.Items.Clear();
            DateFacturePicker.SelectedDate = null;
            StatutComboBox.SelectedIndex = -1;
            TotalTextBox.Text = "0.00";
        }
    }
}
