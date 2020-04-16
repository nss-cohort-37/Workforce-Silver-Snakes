using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workforce_Silver_Snakes.Models;
namespace Workforce_Silver_Snakes.Models.ViewModels
{
    public class ComputerDetailsViewModel
    {
        public int ComputerId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? DecomissionDate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int? EmployeeId { get; set; }
        public string Query { get; set; }
        public Employee employee { get; set; }
    }
}
