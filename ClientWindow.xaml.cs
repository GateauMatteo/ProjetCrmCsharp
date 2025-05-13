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
    /// Logique d'interaction pour ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private ObservableCollection<Client> ClientList = new ObservableCollection<Client>();
        private Client selectedClient;
        public ClientWindow()
        {
            InitializeComponent();
            var clients = Bdd.GetAllClients();
            ClientsListBox.ItemsSource = ClientList;

            // Charger les clients dès l'ouverture de la fenêtre
            LoadClients();
        }
        private void LoadClients()
        {
            try
            {
                Console.WriteLine($"CurrentEmployerId: {Bdd.CurrentEmployerId}"); // Vérification de l'ID employé

                var clients = Bdd.GetAllClients(); // Modification pour charger tous les clients

                ClientList.Clear();
                foreach (var client in clients)
                {
                    Console.WriteLine($"Client récupéré : ID={client.Id}, Nom={client.Nom}, Prenom={client.Prenom}");
                    ClientList.Add(client);
                }

                if (ClientList.Count == 0)
                {
                    MessageBox.Show("Aucun client trouvé.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des clients : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Efface les champs de saisie et réinitialise le client sélectionné.
        /// </summary>
        private void ClearFields()
        {
            Prenom.Text = string.Empty;
            Nom.Text = string.Empty;
            emailTextBox.Text = string.Empty;
            TelephoneTextBox.Text = string.Empty;
            AdresseTextBox.Text = string.Empty;
            TypeComboBox.SelectedIndex = -1; // Désélectionner le ComboBox
            selectedClient = null;
        }




        /// <summary>
        /// Ajoute un nouveau client dans la base de données et l'affiche dans la liste.
        /// </summary>
        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Prenom.Text) || string.IsNullOrWhiteSpace(Nom.Text) || string.IsNullOrWhiteSpace(emailTextBox.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Création du client
                var newClient = new Client
                {
                    Prenom = Prenom.Text,
                    Nom = Nom.Text,
                    Email = emailTextBox.Text,
                    Téléphone = TelephoneTextBox.Text,
                    Address = AdresseTextBox.Text,
                    Type = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                // Insérer le client dans la base de données et récupérer l'ID
                newClient.Id = Bdd.InsertClient(newClient.Nom, newClient.Prenom, newClient.Email, newClient.Téléphone, newClient.Address, newClient.Type);

                // Ajouter le client à la liste locale
                ClientList.Add(newClient);

                MessageBox.Show("Client ajouté avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout du client : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        /// <summary>
        /// Modifie les informations d'un client existant.
        /// </summary>
        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClient != null &&
                !string.IsNullOrWhiteSpace(Prenom.Text) &&
                !string.IsNullOrWhiteSpace(Nom.Text) &&
                !string.IsNullOrWhiteSpace(emailTextBox.Text) &&
                !string.IsNullOrWhiteSpace(TelephoneTextBox.Text) &&
                !string.IsNullOrWhiteSpace(AdresseTextBox.Text) &&
                TypeComboBox.SelectedItem != null)
            {
                try
                {
                    var type = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString();

                    // Mise à jour de l'objet sélectionné
                    selectedClient.Prenom = Prenom.Text;
                    selectedClient.Nom = Nom.Text;
                    selectedClient.Email = emailTextBox.Text;
                    selectedClient.Téléphone = TelephoneTextBox.Text;
                    selectedClient.Address = AdresseTextBox.Text;
                    selectedClient.Type = type;

                    // Mise à jour dans la base de données
                    Bdd.UpdateClient(selectedClient.Id, selectedClient.Nom, selectedClient.Prenom, selectedClient.Email, selectedClient.Téléphone, selectedClient.Address, selectedClient.Type);

                    // Rafraîchir la liste (en rechargeant les clients)
                    LoadClients();

                    ClearFields();
                    MessageBox.Show("Client modifié avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la modification du client : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un client et remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        /// <summary>
        /// Supprime un client de la base de données et de la liste.
        /// </summary>
        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedItem is Client clientToDelete)
            {
                try
                {
                    Console.WriteLine($"Tentative de suppression du client ID : {clientToDelete.Id}");

                    // Suppression dans la base de données
                    Bdd.DeleteClient(clientToDelete.Id);

                    // Suppression dans la liste en mémoire
                    ClientList.Remove(clientToDelete);

                    // Efface les champs de saisie après suppression
                    ClearFields();

                    MessageBox.Show("Client supprimé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression du client : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un client à supprimer.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClientsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsListBox.SelectedItem is Client client)
            {
                selectedClient = client;
                Prenom.Text = client.Prenom;
                Nom.Text = client.Nom;
                emailTextBox.Text = client.Email;
                TelephoneTextBox.Text = client.Téléphone;
                AdresseTextBox.Text = client.Address;

                // Mettre à jour le TypeComboBox
                foreach (ComboBoxItem item in TypeComboBox.Items)
                {
                    if (item.Content.ToString() == client.Type)
                    {
                        TypeComboBox.SelectedItem = item;
                        break;
                    }
                }

                Console.WriteLine($"Client sélectionné : ID={client.Id}, Nom={client.Nom}, Prenom={client.Prenom}");
            }
            else
            {
                ClearFields();
                Console.WriteLine("Aucun client sélectionné.");
            }
        }
    }
}
