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
    /// Logique d'interaction pour RendezvousWindow.xaml
    /// </summary>
    public partial class RendezvousWindow : Window
    {
        private ObservableCollection<Rendez_vous> rendezVousList = new ObservableCollection<Rendez_vous>();
        private ObservableCollection<Client> clientsList = new ObservableCollection<Client>();
        private ObservableCollection<Saleperson> salespersonList = new ObservableCollection<Saleperson>();
        private Rendez_vous selectedRendezVous;

        public RendezvousWindow()
        {
            InitializeComponent();
            RendezvousListBox.ItemsSource = rendezVousList;
            LoadAppointments();
            LoadClients();
            LoadSalespersons();
            RendezvousListBox.SelectionChanged += RendezvousListBox_SelectionChanged;
        }
        private void LoadAppointments()
        {
            try
            {
                var appointments = Bdd.GetAllAppointments();
                rendezVousList.Clear();

                foreach (var appointment in appointments)
                {
                    rendezVousList.Add(appointment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des rendez-vous : {ex.Message}");
            }
        }

        private void LoadClients()
        {
            try
            {
                var clients = Bdd.GetAllClients();
                clientsList.Clear();

                foreach (var client in clients)
                {
                    clientsList.Add(client);
                }

                ClientComboBox.ItemsSource = clientsList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des clients : {ex.Message}");
            }
        }

        private void LoadSalespersons()
        {
            try
            {
                var salespersons = Bdd.GetAllSalespersons();
                salespersonList.Clear();

                foreach (var salesperson in salespersons)
                {
                    salespersonList.Add(salesperson);
                }

                SalepersonComboBox.ItemsSource = salespersonList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des commerciaux : {ex.Message}");
            }
        }


        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            if (DateDatePicker.SelectedDate.HasValue &&
                !string.IsNullOrWhiteSpace(LocationTextBox.Text) &&
                ClientComboBox.SelectedValue != null &&
                SalepersonComboBox.SelectedValue != null &&
                StatusCombobox.SelectedIndex >= 0)
            {
                try
                {
                    var rendezVous = new Rendez_vous
                    {
                        ClientId = (int)ClientComboBox.SelectedValue,
                        Date = DateDatePicker.SelectedDate.Value,
                        Location = LocationTextBox.Text,
                        Status = (Status)StatusCombobox.SelectedIndex
                    };

                    int salespersonId = (int)SalepersonComboBox.SelectedValue;

                    Bdd.InsertAppointment(rendezVous.ClientId, salespersonId, rendezVous.Date, rendezVous.Location, rendezVous.Status);

                    rendezVousList.Add(rendezVous);
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout du rendez-vous : {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
            }
        }

        private void Modifier_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRendezVous != null &&
                DateDatePicker.SelectedDate.HasValue &&
                !string.IsNullOrWhiteSpace(LocationTextBox.Text) &&
                ClientComboBox.SelectedValue != null &&
                SalepersonComboBox.SelectedValue != null &&
                StatusCombobox.SelectedIndex >= 0)
            {
                try
                {
                    selectedRendezVous.Date = DateDatePicker.SelectedDate.Value;
                    selectedRendezVous.Location = LocationTextBox.Text;
                    selectedRendezVous.Status = (Status)StatusCombobox.SelectedIndex;
                    selectedRendezVous.ClientId = (int)ClientComboBox.SelectedValue;

                    int salespersonId = (int)SalepersonComboBox.SelectedValue;

                    Bdd.UpdateAppointment(selectedRendezVous.Id, selectedRendezVous.Date, salespersonId, selectedRendezVous.Location, selectedRendezVous.Status);

                    LoadAppointments();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la modification du rendez-vous : {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
            }
        }


        private void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRendezVous != null)
            {
                try
                {
                    Bdd.DeleteAppointment(selectedRendezVous.Id);
                    rendezVousList.Remove(selectedRendezVous);
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression du rendez-vous : {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un rendez-vous à supprimer.");
            }
        }

        private void ClearFields()
        {
            LocationTextBox.Text = string.Empty;
            DateDatePicker.SelectedDate = null;
            StatusCombobox.SelectedIndex = -1;
            ClientComboBox.SelectedIndex = -1;
            SalepersonComboBox.SelectedIndex = -1;
            selectedRendezVous = null;
        }

        private void RendezvousListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RendezvousListBox.SelectedItem is Rendez_vous rendezVous)
            {
                selectedRendezVous = rendezVous;

                LocationTextBox.Text = selectedRendezVous.Location;
                DateDatePicker.SelectedDate = selectedRendezVous.Date;
                StatusCombobox.SelectedIndex = (int)selectedRendezVous.Status;
                ClientComboBox.SelectedValue = selectedRendezVous.ClientId;
                SalepersonComboBox.SelectedValue = selectedRendezVous.SalespersonId;
            }
            else
            {
                ClearFields();
            }
        }
    }
}
