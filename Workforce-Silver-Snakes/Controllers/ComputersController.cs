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
        public IActionResult Index()

        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.PurchaseDate, c.Make, c.Model
                    FROM Computer c";

                    var reader = cmd.ExecuteReader();

                    
                   List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        
                            Computer computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))

                            };
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
            return View();
        }



        //    // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Computer computer)
        {
            try
            {
                // TODO: Add insert logic here

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, Make, Model)
                                            OUTPUT INSERTED.Id
                                            VALUES (@purchaseDate, @make, @model)";

                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@model", computer.Model));

                        var id = (int)cmd.ExecuteScalar();
                        computer.Id = id;


                        return RedirectToAction(nameof(Index));
                    }
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
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
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



        // COMPUTER HELPER METHOD
        private Computer GetComputerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, PurchaseDate, Make, Model FROM Computer WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    var reader = cmd.ExecuteReader();
                    Computer computer = null;

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))
                        };

                    }
                    reader.Close();
                    return computer;
                }
            }
        }
    }
}