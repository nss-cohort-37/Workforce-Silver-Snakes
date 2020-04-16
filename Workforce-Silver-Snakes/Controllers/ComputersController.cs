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
    public class ComputersController : Controller
    {

        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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
        // GET: Computers
        public IActionResult Index(string searchString)

        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.PurchaseDate, c.Make, c.Model, e.FirstName, e.LastName, e.ComputerId
                    FROM Computer c
                    LEFT JOIN Employee e
                    ON e.ComputerId = c.Id
                    WHERE 1=1";
                    if (searchString != null)
                    {
                        cmd.CommandText += " AND Make LIKE @searchString OR Model LIKE @searchString";
                        cmd.Parameters.Add(new SqlParameter("@searchString", "%" + searchString + "%"));
                    }
                    var reader = cmd.ExecuteReader();



                    List<ComputerAddEmployeeViewModel> computers = new List<ComputerAddEmployeeViewModel>();

                    while (reader.Read())
                    {

                        ComputerAddEmployeeViewModel computer = new ComputerAddEmployeeViewModel
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))

                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("FirstName")))
                        {
                            computer.employee = new Employee
                            {
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))

                            };
                        }
                        else
                        {
                            computer.employee = new Employee
                            {
                                FirstName = null
                            };
                        }
                        
                        computers.Add(computer);


                    }
                    reader.Close();

                    return View(computers);
                }
            }
        }

        // GET: Computers/Details/5
        public ActionResult Details(int id)

        {
            var computer = GetComputerById(id);
            return View(computer);
        }

        // GET: Computers/Create
        public ActionResult Create()
        {
            var employee = GetEmployees();
            var employeeOptions = GetEmployeeOptions();
            var viewModel = new ComputerAddEmployeeViewModel()
            {
                EmployeeOptions = employeeOptions
            };
            return View(viewModel);
        }

        // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ComputerAddEmployeeViewModel computer)
        {
            try
            {

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, Make, Model)
                                            OUTPUT INSERTED.Id 
                                            VALUES (@purchaseDate, @make, @model);
                                            ";

                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@id", computer.EmployeeId));

                        var id = (int)cmd.ExecuteScalar();
                        computer.Id = id;

                    }

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET ComputerId = @computerId
                                            WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", computer.EmployeeId));
                    cmd.Parameters.Add(new SqlParameter("@computerId", computer.Id));
                    cmd.ExecuteNonQuery();

                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        // GET: Computers/Edit/5
        public ActionResult Edit(int id)
        {
            var computer = GetComputerById(id);
            return View(computer);
        }

        // POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer
                                            SET PurchaseDate = @purchaseDate,
                                                Make = @make,
                                                Model = @model
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        var rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected < 1)
                        {
                            return NotFound();
                        }
                    }
                }


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Computers/Delete/5
        public ActionResult Delete(int id)
        {
            var computer = GetComputerById(id);
            return View(computer);
        }

        // POST: Computers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Computer computer)
        {
            try
            {
                // TODO: Add delete logic here
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM Computer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        private List<SelectListItem> GetEmployeeOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, CONCAT(FirstName, ' ', LastName) AS FullName FROM Employee";

                    var reader = cmd.ExecuteReader();
                    var options = new List<SelectListItem>();
                    options.Add(new SelectListItem()
                    {
                        Text = "Unassigned",
                        Value = null
                    });

                    while (reader.Read())
                    {
                        var option = new SelectListItem()
                        {
                            Text = reader.GetString(reader.GetOrdinal("FullName")),
                            Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
                        };

                        options.Add(option);

                    }
                    reader.Close();
                    return options;
                }
            }
        }

        // COMPUTER HELPER METHOD
        private ComputerAddEmployeeViewModel GetComputerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.PurchaseDate, c.Make, c.Model, COALESCE(e.[FirstName] + ' ' + e.LastName, 'N/A' ) AS        EmployeeName, e.ComputerId
                    FROM Computer c
                    LEFT JOIN Employee e
                    ON e.ComputerId = c.Id
                    WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    ComputerAddEmployeeViewModel computer = null;

                    if (reader.Read())
                    {
                        computer = new ComputerAddEmployeeViewModel
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model")),
                            employee = new Employee
                            {
                                FirstName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                            }
                        };

                    }
                    reader.Close();
                    return computer;
                }
            }
        }
        private List<Employee> GetEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName
                                      FROM Employee e";
                    var reader = cmd.ExecuteReader();
                    var employees = new List<Employee>();

                    while (reader.Read())
                    {
                        employees.Add(new Employee()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }
                    reader.Close();
                    return employees;
                }
            }
        }
    }
}