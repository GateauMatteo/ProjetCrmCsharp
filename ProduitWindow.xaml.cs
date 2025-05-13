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
    /// Logique d'interaction pour ProduitWindow.xaml
    /// </summary>
    public partial class ProduitWindow : Window
    {
        private ObservableCollection<Produit> ProduitList = new ObservableCollection<Produit>();
        private Produit ProduitSelectionne;
        public ProduitWindow()
        {
            InitializeComponent();
            ProduitListBox.ItemsSource = ProduitList;

            // Charger les produits dès l'ouverture de la fenêtre
            LoadProduits();
        }

        private void LoadProduits()
        {
            try
            {
                var produits = Bdd.GetAllProduits();
                ProduitList.Clear();
                foreach (var produit in produits)
                {
                    ProduitList.Add(produit);
                }

                if (ProduitList.Count == 0)
                {
                    MessageBox.Show("Aucun produit trouvé.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des produits : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Efface les champs de saisie et réinitialise le produit sélectionné.
        /// </summary>
        private void ClearFields()
        {
            NomTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            PrixTextBox.Text = string.Empty;
            StockTextBox.Text = string.Empty;
            ProduitSelectionne = null;
        }

        /// <summary>
        /// Ajoute un nouveau produit dans la base de données et l'affiche dans la liste.
        /// </summary>
        private void AjouterProduit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrixTextBox.Text) ||
                string.IsNullOrWhiteSpace(StockTextBox.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PrixTextBox.Text, out decimal price) || !int.TryParse(StockTextBox.Text, out int stock))
            {
                MessageBox.Show("Le prix et le stock doivent être des nombres valides.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newProduit = new Produit
                {
                    Name = NomTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Price = price,
                    Stock = stock
                };

                // Insérer le produit dans la base de données et récupérer l'ID
                newProduit.Id = Bdd.InsertProduit(newProduit.Name, newProduit.Description, newProduit.Price, newProduit.Stock);

                // Ajouter le produit à la liste locale
                ProduitList.Add(newProduit);

                MessageBox.Show("Produit ajouté avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout du produit : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Modifie les informations d'un produit existant.
        /// </summary>
        private void ModifierProduit_Click(object sender, RoutedEventArgs e)
        {
            if (ProduitSelectionne != null &&
                !string.IsNullOrWhiteSpace(NomTextBox.Text) &&
                !string.IsNullOrWhiteSpace(DescriptionTextBox.Text) &&
                !string.IsNullOrWhiteSpace(PrixTextBox.Text) &&
                !string.IsNullOrWhiteSpace(StockTextBox.Text))
            {
                if (!decimal.TryParse(PrixTextBox.Text, out decimal price) || !int.TryParse(StockTextBox.Text, out int stock))
                {
                    MessageBox.Show("Le prix et le stock doivent être des nombres valides.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Mise à jour de l'objet sélectionné
                    ProduitSelectionne.Name = NomTextBox.Text;
                    ProduitSelectionne.Description = DescriptionTextBox.Text;
                    ProduitSelectionne.Price = price;
                    ProduitSelectionne.Stock = stock;

                    // Mise à jour dans la base de données
                    Bdd.UpdateProduit(ProduitSelectionne.Id, ProduitSelectionne.Name, ProduitSelectionne.Description, ProduitSelectionne.Price, ProduitSelectionne.Stock);

                    // Rafraîchir la liste (en rechargeant les produits)
                    LoadProduits();

                    ClearFields();
                    MessageBox.Show("Produit modifié avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la modification du produit : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un produit et remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Supprime un produit de la base de données et de la liste.
        /// </summary>
        private void SupprimerProduit_Click(object sender, RoutedEventArgs e)
        {
            if (ProduitListBox.SelectedItem is Produit produitToDelete)
            {
                try
                {
                    Console.WriteLine($"Tentative de suppression du produit ID : {produitToDelete.Id}");

                    // Suppression dans la base de données
                    Bdd.DeleteProduit(produitToDelete.Id);

                    // Suppression dans la liste en mémoire
                    ProduitList.Remove(produitToDelete);

                    // Efface les champs de saisie après suppression
                    ClearFields();

                    MessageBox.Show("Produit supprimé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression du produit : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un produit à supprimer.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Gère la sélection d'un produit dans la `ListBox`.
        /// </summary>
        private void ProduitListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProduitListBox.SelectedItem is Produit produit)
            {
                ProduitSelectionne = produit;
                NomTextBox.Text = produit.Name;
                DescriptionTextBox.Text = produit.Description;
                PrixTextBox.Text = produit.Price.ToString();
                StockTextBox.Text = produit.Stock.ToString();

                Console.WriteLine($"Produit sélectionné : ID={produit.Id}, Nom={produit.Name}");
            }
            else
            {
                ClearFields();
                Console.WriteLine("Aucun produit sélectionné.");
            }
        }
    }
}
