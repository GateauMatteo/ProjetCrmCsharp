using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infotols
{
    public class Client : INotifyPropertyChanged

    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string prenom;
        public string Prenom
        {
            get => prenom;
            set
            {
                prenom = value;
                OnPropertyChanged(nameof(Prenom));
            }
        }

        private string nom;
        public string Nom
        {
            get => nom;
            set
            {
                nom = value;
                OnPropertyChanged(nameof(Nom));
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string téléphone;
        public string Téléphone
        {
            get => téléphone;
            set
            {
                téléphone = value;
                OnPropertyChanged(nameof(Téléphone));
            }
        }

        private string address;
        public string Address
        {
            get => address;
            set
            {
                address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        private int id;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string NomPrenom => $"{Prenom} {Nom}";
        public override string ToString()
        {
            return $"{Id} - {NomPrenom} - {Email}";
        }
    }
}
