using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Workforce_Silver_Snakes.Models;
using Workforce_Silver_Snakes.Models.ViewModels;

namespace Workforce_Silver_Snakes.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IConfiguration _config;
        public EmployeesController(IConfiguration config)
        {
            _config = config;
        }
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Employees
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId, d.Name
                                      FROM Employee e
                                      LEFT JOIN Department d
                                      ON e.DepartmentId = d.Id";
                    var reader = cmd.ExecuteReader();
                    var employees = new List<Employee>();

                    while (reader.Read())
                    {
                        employees.Add(new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        }
                        );
                    }
                    reader.Close();
                    return View(employees);
                }
            }
        }

        // GET: Employees/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId, e.ComputerId, d.Name, c.Make, c.Model, t.Id trainingProgramId, t.Name as TrainingProgram, t.StartDate, t.EndDate 
                                        FROM Employee e
                                        LEFT JOIN Department d
                                        ON e.DepartmentId = d.Id
                                        LEFT JOIN Computer c
                                        ON e.ComputerId = c.Id
                                        LEFT JOIN EmployeeTraining et
                                        ON et.EmployeeId = e.Id
                                        LEFT JOIN TrainingProgram t
                                        ON et.TrainingProgramId = t.Id
                                        WHERE e.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    EmployeeAddViewModel employee = null;

                   while (reader.Read())
                    {
                        if (employee == null)
                        {
                            employee = new EmployeeAddViewModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Computer = new Computer()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                    Make = reader.GetString(reader.GetOrdinal("Make")),
                                    Model = reader.GetString(reader.GetOrdinal("Model"))
                                },
                                Department = new Department()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name"))
                                },
                                TrainingPrograms = new List<TrainingProgram>()
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("trainingProgramId")))
                        {
                            employee.TrainingPrograms.Add(new TrainingProgram()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("trainingProgramId")),
                                Name = reader.GetString(reader.GetOrdinal("TrainingProgram")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
                            });
                        }
                    }
                    reader.Close();
                    return View(employee);
                }
            }
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            var departmentOptions = GetDepartmentOptions();
            var computerOptions = GetAvailableComputers();
            var viewModel = new EmployeeAddViewModel()
            {
                DepartmentOptions = departmentOptions,
                ComputerOptions = computerOptions
            };
            return View(viewModel);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Employee (FirstName, LastName, DepartmentId, ComputerId, Email)
                                            OUTPUT INSERTED.Id
                                            VALUES (@firstName, @lastName, @departmentId, @computerId, @email)";

                        cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@computerId", employee.ComputerId));
                        cmd.Parameters.Add(new SqlParameter("@email", employee.Email));


                        var id = (int)cmd.ExecuteScalar();
                        employee.Id = id;
                        return RedirectToAction(nameof(Index));
                    }
                }


            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int id)
        {
            var employee = GetEmployeeById(id);
            var departmentOptions = GetDepartmentOptions();
            var computerOptions = GetUsersComputerOrAvailable(id);
            var viewModel = new EmployeeAddViewModel()
            {
                DepartmentId = employee.DepartmentId,
                ComputerId = employee.ComputerId,
                LastName = employee.LastName,  
                DepartmentOptions = departmentOptions,
                ComputerOptions = computerOptions
            };
            return View(viewModel);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                           SET LastName = @lastName,
                                               DepartmentId = @departmentId,
                                               ComputerId = @computerId
                                               WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@computerId", employee.ComputerId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                        throw new Exception("No rows affected");


                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Employees/assign/5
        public ActionResult AssignTrainingPrograms(int id)
        {
            var employee = GetEmployeeById(id);
            var trainingOptions = GetAvailableTrainingPrograms();
            var viewModel = new EmployeeAddViewModel()
            {
                FirstName = employee.FirstName,
                TrainingPrograms = employee.TrainingPrograms,
                TrainingProgramsOptions = trainingOptions
            };
            return View(viewModel);
        }

        // POST: Employees/assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTrainingPrograms(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        private List<SelectListItem> GetDepartmentOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Department";
                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Name")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()

                        };
                        options.Add(option);
                    }
                    reader.Close();
                    return options;
                }
            }
        }
        private List<SelectListItem> GetAvailableComputers()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, CONCAT(c.Make, ' ', c.Model) as 'Computer' FROM Computer c
                                      LEFT JOIN Employee e
                                      ON e.ComputerId = c.Id
                                      WHERE e.ComputerId IS NULL";
                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Computer")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()

                        };
                        options.Add(option);
                    }
                    reader.Close();
                    return options;
                }
            }
        }

        private List<SelectListItem> GetAvailableTrainingPrograms()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT t.Id, t.Name, t.StartDate, t.EndDate, t.MaxAttendees, COUNT(et.EmployeeId) Attendees
                                        FROM EmployeeTraining et
                                        LEFT JOIN TrainingProgram t
                                        ON et.TrainingProgramId = t.Id
                                        WHERE 1 = 1
                                        Group BY t.Id, t.Name, StartDate, EndDate, MaxAttendees
                                        HAVING COUNT(et.EmployeeId) < MaxAttendees";
                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while(reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Name")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()

                        };
                        options.Add(option);
                    }
                    reader.Close();
                    return options;
                }
            }
        }
        private List<SelectListItem> GetUsersComputerOrAvailable(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, CONCAT(c.Make, ' ', c.Model) as 'Computer' FROM Computer c
                                        LEFT JOIN Employee e
                                        ON e.ComputerId = c.Id
                                        WHERE e.Id = @id
                                        OR e.ComputerId IS NULL";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("Computer")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()

                        };
                        options.Add(option);
                    }
                    reader.Close();
                    return options;
                }
            }
        }
        private EmployeeAddViewModel GetEmployeeById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId, e.ComputerId, e.IsSupervisor, t.Id trainingProgramId, t.Name TrainingProgram, t.StartDate, t.EndDate 
                                    FROM Employee e 
                                    LEFT JOIN EmployeeTraining et
                                    ON et.EmployeeId = e.Id
                                    LEFT JOIN TrainingProgram t
                                    ON et.TrainingProgramId = t.Id
                                    WHERE e.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    EmployeeAddViewModel employee = null;

                    while (reader.Read())
                    {
                        if (employee == null)
                        {
                            employee = new EmployeeAddViewModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                TrainingPrograms = new List<TrainingProgram>()
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("trainingProgramId")))
                        {
                            employee.TrainingPrograms.Add(new TrainingProgram()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("trainingProgramId")),
                                Name = reader.GetString(reader.GetOrdinal("TrainingProgram")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
                            });
                        }
                    }
                    reader.Close();
                    return employee;
                }
            }
        }
    }
}