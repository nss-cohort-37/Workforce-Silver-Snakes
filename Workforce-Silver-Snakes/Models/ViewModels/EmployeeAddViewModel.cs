using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Workforce_Silver_Snakes.Models.ViewModels
{
    public class EmployeeAddViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public string Email { get; set; }
        public bool IsSupervisor { get; set; }
        public int ComputerId { get; set; }
        public Department Department { get; set; }
        public List<SelectListItem> DepartmentOptions { get; set; }
    }
}
