using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Workforce_Silver_Snakes.Models.ViewModels;

namespace Workforce_Silver_Snakes.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly IConfiguration _config;
        public DepartmentsController(IConfiguration config)
        {
            _config = config;
        }
        //COMPUTED PROPERTY FOR THE CONNECTION
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Departments
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT d.Id, d.[Name], d.Budget, COUNT(e.DepartmentId) as 'Employees'
                                        FROM Department d
                                        LEFT JOIN Employee e
                                        ON	e.DepartmentId = d.Id
                                        GROUP BY d.Name, d.Id, d.Budget ";
                    var reader = cmd.ExecuteReader();
                    var departments = new List<DepartmentsViewModel>();

                    while (reader.Read())
                    {
                        departments.Add(new DepartmentsViewModel()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            Employees = reader.GetInt32(reader.GetOrdinal("Employees"))
                        }
                        );
                    }
                    reader.Close();
                    return View(departments);
                }
            }
        }

        // GET: Departments/Details/5
        public ActionResult Details(int id)
        {
            var department = GetDepartmentById(id);
            return View(department);
        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DepartmentsViewModel department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Department (Name)
                                            OUTPUT INSERTED.Id
                                            VALUES (@name)";

                        cmd.Parameters.Add(new SqlParameter("@name", department.Name));

                        var id = (int)cmd.ExecuteScalar();
                        department.Id = id;
                        return RedirectToAction(nameof(Index));
                    }
                }


            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Departments/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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

        private DepartmentsViewModel GetDepartmentById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT d.Id, d.[Name], d.Budget, COUNT(e.DepartmentId) as 'Employees'
                                        FROM Department d
                                        LEFT JOIN Employee e
                                        ON	e.DepartmentId = d.Id
                                        WHERE d.id = @id
                                        GROUP BY d.Name, d.Id, d.Budget ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    var reader = cmd.ExecuteReader();
                    DepartmentsViewModel department = null;

                    if (reader.Read())
                    {
                        department = new DepartmentsViewModel()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            Employees = reader.GetInt32(reader.GetOrdinal("Employees"))
                        };
                    
                    }
                    reader.Close();
                    return department;
                }
            }
        }
    }
}