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
                    cmd.CommandText = @"SELECT e.Id, e.ProfileAvatarId, e.FirstName, e.LastName, e.DepartmentId, e.ComputerId, e.IsSupervisor, d.Name, c.Make, c.Model, t.Id trainingProgramId, t.Name as TrainingProgram, t.StartDate, t.EndDate, pa.Id as AvatarId, pa.AvatarPath 
                                        FROM Employee e
                                        LEFT JOIN Department d
                                        ON e.DepartmentId = d.Id
                                        LEFT JOIN Computer c
                                        ON e.ComputerId = c.Id
                                        LEFT JOIN EmployeeTraining et
                                        ON et.EmployeeId = e.Id
                                        LEFT JOIN TrainingProgram t
                                        ON et.TrainingProgramId = t.Id
                                        LEFT JOIN ProfileAvatar pa
                                        ON pa.Id = e.ProfileAvatarId
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
                                ProfileAvatarId = reader.GetInt32(reader.GetOrdinal("ProfileAvatarId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
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
                                ProfileAvatar = new ProfileAvatar()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("AvatarId")),
                                    AvatarPath = reader.GetString(reader.GetOrdinal("AvatarPath"))
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
            var profileAvatarOptions = GetProfileAvatarOptions();
            var computerOptions = GetAvailableComputers();
            var viewModel = new EmployeeAddViewModel()
            {
                DepartmentOptions = departmentOptions,
                ComputerOptions = computerOptions,
                ProfileAvatarOptions = profileAvatarOptions
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
                        cmd.CommandText = @"INSERT INTO Employee (ProfileAvatarId, FirstName, LastName, DepartmentId, ComputerId, IsSupervisor, Email)
                                            OUTPUT INSERTED.Id
                                            VALUES (@profileAvatarId, @firstName, @lastName, @departmentId, @computerId, @isSupervisor, @email)";

                        cmd.Parameters.Add(new SqlParameter("@profileAvatarId", employee.ProfileAvatarId));
                        cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@computerId", employee.ComputerId));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", employee.IsSupervisor));
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
            var trainingprogramIds = new List<int>();
            foreach(var item in employee.TrainingPrograms)
            {
                trainingprogramIds.Add(item.Id);
            }
            var viewModel = new EmployeeAddViewModel()
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                TrainingProgramIds = trainingprogramIds,
                TrainingProgramsOptions = trainingOptions
            };
            return View(viewModel);
        }

        // POST: Employees/assign/5

            //FOR EACH HELPER METHOD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTrainingPrograms(EmployeeAddViewModel employee)
        {
            employee.EmployeeTrainings = GetEmployeeTrainings(employee.Id);
            foreach(var id in employee.TrainingProgramIds)
            {
                    if (!employee.EmployeeTrainings.Any(et => et.TrainingProgramId == id))
                    {
                        AddSingleTrainingProgram(employee, id);
                    }     

            }                  
            return RedirectToAction("Details", new { employee.Id });
       
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
                                        WHERE StartDate > GETDATE()
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
        private List<EmployeeTraining> GetEmployeeTrainings(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT et.Id, et.EmployeeId, et.TrainingProgramId
                                        FROM EmployeeTraining et
                                         WHERE EmployeeId = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    var employeeTrainings = new List<EmployeeTraining>();

                    while (reader.Read())
                    {
                        employeeTrainings.Add(new EmployeeTraining()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            TrainingProgramId = reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))
                        });

                    }
                    reader.Close();
                    return employeeTrainings;
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
        private ActionResult AddSingleTrainingProgram(EmployeeAddViewModel employee, int trainingProgramId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                            OUTPUT INSERTED.Id
                                            VALUES (@employeeId, @trainingProgramId)";
                       

                        cmd.Parameters.Add(new SqlParameter("@employeeId", employee.Id));
                        cmd.Parameters.Add(new SqlParameter("@trainingProgramId", trainingProgramId));

                        var id = (int)cmd.ExecuteScalar();
                 
                        return RedirectToAction(nameof(Index));
                    }
                }


            }
            catch (Exception ex)
            {
                return View();
    }
}
        private List<SelectListItem> GetProfileAvatarOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, AvatarPath FROM ProfileAvatar";
                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("AvatarPath")),
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
                    cmd.CommandText = @"SELECT e.Id, e.ProfileAvatarId, e.FirstName, e.LastName, e.DepartmentId, e.ComputerId, e.IsSupervisor, t.Id trainingProgramId, t.Name TrainingProgram, t.StartDate, t.EndDate, pa.Id as AvatarId, pa.AvatarPath
                                    FROM Employee e 
                                    LEFT JOIN EmployeeTraining et
                                    ON et.EmployeeId = e.Id
                                    LEFT JOIN TrainingProgram t
                                    ON et.TrainingProgramId = t.Id
                                    LEFT JOIN ProfileAvatar pa
                                    ON pa.Id = e.ProfileAvatarId
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
                                ProfileAvatarId = reader.GetInt32(reader.GetOrdinal("ProfileAvatarId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                TrainingPrograms = new List<TrainingProgram>(),
                                ProfileAvatar = new ProfileAvatar() 
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("AvatarId")),
                                    AvatarPath = reader.GetString(reader.GetOrdinal("AvatarPath"))
                                }
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

