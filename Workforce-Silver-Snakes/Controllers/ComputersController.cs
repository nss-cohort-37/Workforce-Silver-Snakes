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
            return View();
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
                        //cmd.Parameters.Add(new SqlParameter("@decomissionDate", computer.DecomissionDate));
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

        //    private List<SelectListItem> GetComputersOptions()
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"SELECT Id, Name FROM Computers";



        //                var reader = cmd.ExecuteReader();
        //                var options = new List<SelectListItem>();

        //                while (reader.Read())
        //                {
        //                    var option = new SelectListItem()
        //                    {
        //                        Text = reader.GetString(reader.GetOrdinal("Name")),
        //                        Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString()
        //                    };
        //                    options.Add(option);
        //                }


        //                reader.Close();
        //                return options;
        //            }
        //        }
        //    }

        // GET: Computers/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //    // POST: Computers/Edit/5
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult Edit(int id, IFormCollection collection)
        //    {
        //        try
        //        {
        //            // TODO: Add update logic here

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch
        //        {
        //            return View();
        //        }
        //    }

        //    // GET: Computers/Delete/5
        //    public ActionResult Delete(int id)
        //    {
        //        return View();
        //    }

        //    // POST: Computers/Delete/5
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult Delete(int id, IFormCollection collection)
        //    {
        //        try
        //        {
        //            // TODO: Add delete logic here

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch
        //        {
        //            return View();
        //        }
        //    }
    }
}