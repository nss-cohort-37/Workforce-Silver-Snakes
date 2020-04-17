using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Workforce_Silver_Snakes.Models.ViewModels
{
    public class TrainingProgramDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Display(Name = "Capacity")]
        public int MaxAttendees { get; set; }

        public List<Employee> Attendees { get; set; }
    }
}
