using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Workforce_Silver_Snakes.Models.ViewModels
{
    public class DepartmentsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Department Name")]
        public string Name { get; set; }
        public int Budget { get; set; }

        [Display(Name = "Employee Count")]
        public int Employees { get; set; }

    }
}
