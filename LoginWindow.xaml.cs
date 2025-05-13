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
using System.Windows.Shapes;

namespace Infotols
{
    /// <summary>
    /// Logique d'interaction pour LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Veuillez entrer un email et un mot de passe.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Validation des identifiants dans la base de données
                var employer = Bdd.ValidateEmployerLogin(email, password);

                if (employer != null)
                {
                    // Si les identifiants sont corrects, afficher un message et ouvrir la fenêtre principale
                    MessageBox.Show($"Bienvenue {employer.FirstName} {employer.LastName} ({employer.Role})", "Connexion réussie", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ouvrir la fenêtre principale (MainWindow)
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    // Fermer la fenêtre de connexion
                    this.Close();
                }
                else
                {
                    // Si les identifiants sont incorrects
                    MessageBox.Show("Email ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Gestion des erreurs
                MessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gestion de l'inscription d'un nouvel utilisateur en tant que commercial.
        /// </summary>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string email = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Veuillez remplir tous les champs pour vous inscrire.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Vérifiez si l'email existe déjà
                var existingEmployer = Bdd.ValidateEmployerLogin(email, password);
                if (existingEmployer != null)
                {
                    MessageBox.Show("Un utilisateur avec cet email existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Insérez un nouvel employé
                Bdd.InsertEmployer(lastName, firstName, email, password);
                MessageBox.Show("Inscription réussie en tant que commercial !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur s'est produite lors de l'inscription : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Efface les champs après une inscription réussie.
        /// </summary>
        private void ClearFields()
        {
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
        }
    }
}
