using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infotols
{
    public class Invoice
    {
        public int InvoiceId { get; set; }         // ID de la facture
        public int ClientId { get; set; }          // ID du client
        public string ClientName { get; set; }     // Nom du client (affiché dans le DataGrid)
        public DateTime InvoiceDate { get; set; }  // Date de la facture
        public decimal Total { get; set; }         // Montant total
        public string Status { get; set; }         // Statut de la facture (Payé / Non Payé)

        public List<InvoiceLine> InvoiceLines { get; set; }
    }
}
