using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infotols
{
   public class Rendez_vous
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int SalespersonId { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public Status Status { get; set; }
        public override string ToString()
        {
            return $"{Date.ToShortDateString()} - {Location} - {Status}";
        }
    }
    public enum Status
    {
        Planned,
        Realized,
        Canceled
    }
}

