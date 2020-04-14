using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Workforce_Silver_Snakes.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxAttendees { get; set; }
    }
}
