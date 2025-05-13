using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using MySqlX.XDevAPI;
using System.Windows;

namespace Infotols
{
    public class Bdd
    {
        private static readonly string _connectionString = "SERVER=mysql-matteo-gateau.alwaysdata.net;DATABASE=matteo-gateau_infootools;UID=405350;PASSWORD=Linxtoast71/;";
        public static int CurrentEmployerId { get; set; } // ID de l'employé connecté

        private static void LogError(string message, Exception ex)
        {
            Console.WriteLine($"{message} : {ex.Message}");
        }

        // --- Connexion ---
        public static bool Login(string email, string password)
        {
            const string query = "SELECT employer_id FROM employers WHERE email = @Email AND password = @Password";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    var result = cmd.ExecuteScalar(); // Récupérer le premier champ de la première ligne
                    if (result != null)
                    {
                        // Stocker l'ID de l'employé connecté
                        CurrentEmployerId = Convert.ToInt32(result);
                        return true; // Connexion réussie
                    }
                }
            }
            return false; // Connexion échouée
        }
        public static void RehashPlainPasswords()
        {
            const string querySelect = "SELECT employer_id, password FROM employers";
            const string queryUpdate = "UPDATE employers SET password = @Password WHERE employer_id = @EmployerId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var selectCmd = new MySqlCommand(querySelect, connection))
                {
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        var passwordsToUpdate = new List<(int EmployerId, string HashedPassword)>();

                        while (reader.Read())
                        {
                            int employerId = reader.GetInt32("employer_id");
                            string plainPassword = reader.GetString("password");

                            // Si le mot de passe n'est pas haché, préparez-le pour mise à jour
                            if (!plainPassword.StartsWith("$2"))
                            {
                                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
                                passwordsToUpdate.Add((employerId, hashedPassword));
                            }
                        }

                        reader.Close();

                        // Mettez à jour les mots de passe dans la base
                        foreach (var (employerId, hashedPassword) in passwordsToUpdate)
                        {
                            using (var updateCmd = new MySqlCommand(queryUpdate, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                                updateCmd.Parameters.AddWithValue("@EmployerId", employerId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Tous les mots de passe non hachés ont été rehashés.");
        }

        public static Employers ValidateEmployerLogin(string email, string password)
        {
            const string query = "SELECT employer_id, last_name, first_name, email, role, password FROM employers WHERE email = @Email";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Vérifier le mot de passe avec BCrypt
                            string hashedPassword = reader.GetString("password");
                            if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                            {
                                return new Employers
                                {
                                    EmployerId = reader.GetInt32("employer_id"),
                                    LastName = reader.GetString("last_name"),
                                    FirstName = reader.GetString("first_name"),
                                    Email = reader.GetString("email"),
                                    Role = reader.GetString("role")
                                };
                            }
                        }
                    }
                }
            }

            return null; // Retourne null si la connexion échoue
        }


        // --- CRUD pour Clients ---
        public static int InsertClient(string lastName, string firstName, string email, string phone, string address, string type)
        {
            const string query = "INSERT INTO clients (last_name, first_name, email, phone, address, type ) " +
                                 "VALUES (@LastName, @FirstName, @Email, @Phone, @Address, @Type ); SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Type", type);

                    try
                    {
                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine($"Client ajouté avec l'ID : {newId}");
                        return newId;
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de l'insertion du client", ex);
                        throw;
                    }
                }
            }
        }

        public static void UpdateClient(int clientId, string lastName, string firstName, string email, string phone, string address, string type)
        {
            const string query = "UPDATE clients SET last_name = @LastName, first_name = @FirstName, email = @Email, phone = @Phone, " +
                                 "address = @Address, type = @Type WHERE client_id = @ClientId ";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Address", address);
                    cmd.Parameters.AddWithValue("@Type", type);


                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Aucun client trouvé avec l'ID {clientId} pour cet employé.");
                        }
                        else
                        {
                            Console.WriteLine($"Client ID {clientId} mis à jour avec succès.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de la mise à jour du client", ex);
                        throw;
                    }
                }
            }
        }

        public static void DeleteClient(int client_id)
        {
            const string query = "DELETE FROM clients WHERE client_id = @ClientId ";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", client_id);


                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Aucun client trouvé avec l'ID {client_id} pour cet employé.");
                        }
                        else
                        {
                            Console.WriteLine($"Client ID {client_id} supprimé avec succès.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("Erreur lors de la suppression du client", ex);
                    throw;
                }
            }
        }

        public static ObservableCollection<Client> GetAllClients()
        {
            const string query = @"
        SELECT client_id, last_name, first_name, email, phone, address, type 
        FROM clients";

            var clients = new ObservableCollection<Client>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client
                            {
                                Id = reader.GetInt32("client_id"),
                                Nom = reader.GetString("last_name"),
                                Prenom = reader.GetString("first_name"),
                                Email = reader.GetString("email"),
                                Téléphone = reader.GetString("phone"),
                                Address = reader.GetString("address"),
                                Type = reader.GetString("type")
                            });
                        }
                    }
                }
            }
            return clients;
        }



        public static void InsertAppointment(int clientId, int SalePersonId, DateTime date, string location, Status status)
        {
            const string query = "INSERT INTO appointments (client_id, salesperson_id, date_time, location, status) " +
                                 "VALUES (@ClientId, @salepersonId, @Date, @Location, @Status)";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@salepersonId", SalePersonId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@Status", status.ToString());

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Rendez-vous ajouté avec succès.");
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de l'insertion du rendez-vous", ex);
                        throw;
                    }
                }
            }
        }

        public static List<Rendez_vous> GetAllAppointments()
        {
            const string query = @"
    SELECT appointment_id, client_id, salesperson_id, date_time, location, status 
    FROM appointments";

            var appointments = new List<Rendez_vous>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Parsing sécurisé du statut
                            Status statusValue;
                            string statusRaw = reader.GetString("status");

                            if (!Enum.TryParse(statusRaw, out statusValue))
                            {
                                Console.WriteLine($"Statut invalide détecté : {statusRaw}");
                                continue; // Ignore cette ligne et passe à la suivante
                            }

                            appointments.Add(new Rendez_vous
                            {
                                Id = reader.GetInt32("appointment_id"),
                                ClientId = reader.GetInt32("client_id"),
                                SalespersonId = reader.GetInt32("salesperson_id"),
                                Date = reader.GetDateTime("date_time"),
                                Location = reader.GetString("location"),
                                Status = statusValue
                            });
                        }
                    }
                }
            }
            return appointments;
        }




        public static void UpdateAppointment(int appointmentId, DateTime date, int SalePersonId, string location, Status status)
        {
            const string query = "UPDATE appointments SET date_time = @Date, salesperson_id = @salepersonid, location = @Location, status = @Status\r\n" +
                                 "WHERE appointment_id = @AppointmentId ";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    cmd.Parameters.AddWithValue("@salepersonid", SalePersonId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@Status", status.ToString());

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Aucun rendez-vous trouvé avec l'ID {appointmentId} pour cet employé.");
                        }
                        else
                        {
                            Console.WriteLine($"Rendez-vous ID {appointmentId} mis à jour avec succès.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de la mise à jour du rendez-vous", ex);
                        throw;
                    }
                }
            }
        }
        public static ObservableCollection<Saleperson> GetAllSalespersons()
        {
            const string query = "SELECT employer_id, last_name, first_name, role FROM employers WHERE role = 'Salesperson'";

            var salespersons = new ObservableCollection<Saleperson>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salespersons.Add(new Saleperson
                            {
                                Id = reader.GetInt32("employer_id"),
                                LastName = reader.GetString("last_name"),
                                FirstName = reader.GetString("first_name"),
                                Role = reader.GetString("role")
                            });
                        }
                    }
                }
            }

            return salespersons;
        }

        public static void DeleteAppointment(int appointmentId)
        {
            const string query = "DELETE FROM appointments WHERE appointment_id = @AppointmentId ";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);


                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Rendez-vous ID {appointmentId} supprimé avec succès.");
                        }
                        else
                        {
                            Console.WriteLine($"Aucun rendez-vous trouvé avec l'ID {appointmentId} pour cet employé.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("Erreur lors de la suppression du rendez-vous", ex);
                    throw;
                }
            }
        }

        public static int InsertEmployer(string lastName, string firstName, string email, string password)
        {
            const string query = "INSERT INTO employers (last_name, first_name, email, password) " +
                                 "VALUES (@LastName, @FirstName, @Email, @Password); SELECT LAST_INSERT_ID();";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception openEx)
                    {
                        LogError("Erreur d'ouverture de la connexion MySQL", openEx);
                        return -1;
                    }

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);

                        try
                        {
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());
                            Console.WriteLine($"Employé ajouté avec l'ID : {newId}");
                            return newId;
                        }
                        catch (Exception execEx)
                        {
                            LogError("Erreur lors de l'insertion de l'employé", execEx);
                            return -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Erreur générale dans InsertEmployer", ex);
                return -1;
            }
        }

        public static List<Produit> GetAllProduits()
        {
            const string query = "SELECT product_id, name, description, price, stock FROM products";
            var produits = new List<Produit>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            produits.Add(new Produit
                            {
                                Id = reader.GetInt32("product_id"),
                                Name = reader.GetString("name"),
                                Description = reader.GetString("description"),
                                Price = reader.GetDecimal("price"),
                                Stock = reader.GetInt32("stock")
                            });
                        }
                    }
                }
            }
            return produits;
        }

        public static int InsertProduit(string name, string description, decimal price, int stock)
        {
            const string query = "INSERT INTO products (name, description, price, stock) " +
                                 "VALUES (@Name, @Description, @Price, @Stock); SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@Stock", stock);

                    try
                    {
                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine($"Produit ajouté avec l'ID : {newId}");
                        return newId;
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de l'insertion du produit", ex);
                        throw;
                    }
                }
            }
        }

        public static void UpdateProduit(int productId, string name, string description, decimal price, int stock)
        {
            const string query = "UPDATE products SET name = @Name, description = @Description, price = @Price, stock = @Stock " +
                                 "WHERE product_id = @ProductId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@Stock", stock);

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Aucun produit trouvé avec l'ID {productId}.");
                        }
                        else
                        {
                            Console.WriteLine($"Produit ID {productId} mis à jour avec succès.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de la mise à jour du produit", ex);
                        throw;
                    }
                }
            }
        }

        public static void DeleteProduit(int productId)
        {
            const string query = "DELETE FROM products WHERE product_id = @ProductId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Produit ID {productId} supprimé avec succès.");
                        }
                        else
                        {
                            Console.WriteLine($"Aucun produit trouvé avec l'ID {productId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("Erreur lors de la suppression du produit", ex);
                    throw;
                }
            }
        }
        public static int InsertInvoice(int clientId, DateTime invoiceDate, decimal total, string status)
        {
            const string query = "INSERT INTO invoices (client_id, date, total, status) " +
                                 "VALUES (@ClientId, @Date, @Total, @Status);";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@Date", invoiceDate); // nom de colonne correct : date
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Status", status);

                    try
                    {
                        cmd.ExecuteNonQuery();  // Exécuter l'insertion

                        // Récupérer l'ID de la facture insérée
                        cmd.CommandText = "SELECT LAST_INSERT_ID()";
                        int newInvoiceId = Convert.ToInt32(cmd.ExecuteScalar());

                        Console.WriteLine($"Facture ajoutée avec l'ID : {newInvoiceId}");
                        return newInvoiceId;
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de l'insertion de la facture", ex);
                        throw;
                    }
                }
            }
        }

        public static void UpdateInvoice(int invoiceId, int clientId, DateTime invoiceDate, decimal total, string status)
        {
            const string query = "UPDATE invoices SET client_id = @ClientId, date = @InvoiceDate, total = @Total, status = @Status " +
                     "WHERE invoice_id = @InvoiceId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Status", status);
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Aucune facture trouvée avec l'ID {invoiceId}.");
                        }
                        else
                        {
                            Console.WriteLine($"Facture ID {invoiceId} mise à jour avec succès.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de la mise à jour de la facture", ex);
                        throw;
                    }
                }
            }
        }


        public static void DeleteInvoice(int invoiceId)
        {
            const string query = "DELETE FROM invoices WHERE invoice_id = @InvoiceId";
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Facture ID {invoiceId} supprimée avec succès.");
                        }
                        else
                        {
                            Console.WriteLine($"Aucune facture trouvée avec l'ID {invoiceId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("Erreur lors de la suppression de la facture", ex);
                    throw;
                }
            }
        }


        public static List<Invoice> GetAllInvoices()
        {
            const string query = "SELECT invoice_id, client_id, date, total, status FROM invoices";
            var invoices = new List<Invoice>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(new Invoice
                            {
                                InvoiceId = reader.GetInt32("invoice_id"),
                                ClientId = reader.GetInt32("client_id"),
                                InvoiceDate = reader.GetDateTime("date"),
                                Total = reader.GetDecimal("total"),
                                Status = reader.GetString("status")
                            });
                        }
                    }
                }
            }
            return invoices;
        }
        public static int InsertInvoiceLine(int invoiceId, int productId, int quantity, decimal unitPrice)
        {
            const string query = "INSERT INTO invoice_lines (invoice_id, product_id, quantity, unit_price, line_total) " +
                                 "VALUES (@InvoiceId, @ProductId, @Quantity, @UnitPrice, @LineTotal); SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    decimal lineTotal = quantity * unitPrice;
                    cmd.Parameters.AddWithValue("@LineTotal", lineTotal);

                    try
                    {
                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine($"Ligne de facture ajoutée avec l'ID : {newId}");
                        return newId;
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de l'insertion de la ligne de facture", ex);
                        throw;
                    }
                }
            }
        }
        public static void UpdateInvoiceLine(int lineId, int quantity, decimal unitPrice)
        {
            const string query = "UPDATE invoice_lines SET quantity = @Quantity, unit_price = @UnitPrice, " +
                                 "line_total = @LineTotal WHERE line_id = @LineId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@LineId", lineId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    decimal lineTotal = quantity * unitPrice;
                    cmd.Parameters.AddWithValue("@LineTotal", lineTotal);

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Aucune ligne de facture trouvée avec l'ID {lineId}.");
                        }
                        else
                        {
                            Console.WriteLine($"Ligne de facture ID {lineId} mise à jour avec succès.");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Erreur lors de la mise à jour de la ligne de facture", ex);
                        throw;
                    }
                }
            }
        }
        public static void DeleteInvoiceLine(int lineId)
        {
            const string query = "DELETE FROM invoice_lines WHERE line_id = @LineId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@LineId", lineId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Ligne de facture ID {lineId} supprimée avec succès.");
                        }
                        else
                        {
                            Console.WriteLine($"Aucune ligne de facture trouvée avec l'ID {lineId}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError("Erreur lors de la suppression de la ligne de facture", ex);
                    throw;
                }
            }
        }

        public static void DeleteInvoiceLinesByInvoiceId(int invoiceId)
        {
            const string query = "DELETE FROM invoice_lines WHERE invoice_id = @InvoiceId";

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                try
                {
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    LogError("Erreur lors de la suppresion de la ligne de facture", ex);
                    throw;
                }
            }
        }

        public static List<InvoiceLine> GetInvoiceLinesByInvoiceId(int invoiceId)
        {
            const string query = "SELECT line_id, invoice_id, product_id, quantity, unit_price FROM invoice_lines WHERE invoice_id = @InvoiceId";

            var lines = new List<InvoiceLine>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lines.Add(new InvoiceLine
                            {
                                LineId = reader.GetInt32("line_id"),
                                InvoiceId = reader.GetInt32("invoice_id"),
                                ProductId = reader.GetInt32("product_id"),
                                Quantity = reader.GetInt32("quantity"),
                                UnitPrice = reader.GetDecimal("unit_price")
                            });
                        }
                    }
                }
            }

            return lines;
        }
    }
}
