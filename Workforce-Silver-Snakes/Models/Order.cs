using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workforce_Silver_Snakes.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int UserPaymentTypeId { get; set; }

    }
}
