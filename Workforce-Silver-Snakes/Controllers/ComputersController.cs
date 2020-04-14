using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
                    cmd.CommandText = @"SELECT o.[Name] AS OwnerName, n.[Name] AS 'Neighborhood Name', Count(d.[Name]) AS 'Number of Dogs'
                        FROM Owner o
                        LEFT JOIN Neighborhood n 
                        ON n.Id = o.NeighborhoodId
                        LEFT JOIN Dog d 
                        ON d.OwnerId = o.Id 
                        GROUP BY o.[Name], n.[Name]";


                    SqlDataReader reader = cmd.ExecuteReader();


                    var neighborhoodOwnersViewModels = new List<NeighborhoodOwnersViewModel>();



                    while (reader.Read())
                    {
                        var existingNeighborhood = neighborhoodOwnersViewModels.FirstOrDefault(n => {
                            return n.NeighborhoodName == reader.GetString(reader.GetOrdinal("Neighborhood Name"));
                        }
                        );
                        if (existingNeighborhood == null)
                        {


                            var neighborhoodOwnerViewModel = new NeighborhoodOwnersViewModel
                            {
                                NeighborhoodName = reader.GetString(reader.GetOrdinal("Neighborhood Name")),
                                OwnerViewModels = new List<OwnerViewModel>()

                            };

                            neighborhoodOwnerViewModel.OwnerViewModels.Add(new OwnerViewModel()
                            {

                                OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                                DogCount = reader.GetInt32(reader.GetOrdinal("Number of Dogs"))


                            });

                            neighborhoodOwnersViewModels.Add(neighborhoodOwnerViewModel);
                        }
                        else
                        {
                            existingNeighborhood.OwnerViewModels.Add(new OwnerViewModel()
                            {

                                OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                                DogCount = reader.GetInt32(reader.GetOrdinal("Number of Dogs"))


                            });
                        }


                    }

                    var viewModel = new AllOwnersByNeighborhoodViewModel();
                    viewModel.NeighborhoodOwnersViewModels = neighborhoodOwnersViewModels;

                    reader.Close();

                    return View(viewModel);
                }
            }

        }

        // GET: Owner/Details/5
        public ActionResult Details(int id)

        {
            return View();
        }



        // GET: Owner/Create
        public ActionResult Create()

        {
            var viewModel = new AddOwnerViewModel()
            {
                NeighborhoodOptions = GetNeighborhoodOptions()
            };
            return View(viewModel);
        }

        // POST: Owner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AddOwnerViewModel owner)
        {
            try
            {
                // TODO: Add insert logic here

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Owner (Name, Address, NeighborhoodId, Phone )
                                            OUTPUT INSERTED.Id
                                            VALUES (@name, @address, @neighborhoodId, @phone)";

                        cmd.Parameters.Add(new SqlParameter("@name", owner.FirstName + " " + owner.LastName));
                        cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                        cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));

                        var id = (int)cmd.ExecuteScalar();
                        owner.OwnerId = id;

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        private List<SelectListItem> GetNeighborhoodOptions()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name FROM Neighborhood";



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

        // GET: Owner/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Owner/Edit/5
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

        // GET: Owner/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Owner/Delete/5
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
    }
}