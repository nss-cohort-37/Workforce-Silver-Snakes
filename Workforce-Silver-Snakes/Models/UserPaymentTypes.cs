using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workforce_Silver_Snakes.Models
{
    public class UserPaymentTypes
    {
        public int Id { get; set; }
        public string AcctNumber { get; set; }
        public bool Active { get; set; }
        public int CustomerId { get; set; }
        public int PaymentTypeId { get; set; }
    }
}
