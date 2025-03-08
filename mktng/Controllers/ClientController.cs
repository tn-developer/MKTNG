using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mktng.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace mktng.Controllers
{
    public class ClientController : BaseController
    {
        const string SessionName = "_Name";
        const string SessionLayout = "_Layout";
        const string SessionType = "_Type";
        const string SessionId = "_Id";
        const string SessionPos = "_Position";

        public SqlConnection con;
        public SqlCommand cmd;
        private readonly IConfiguration _configuration;
        private readonly mktngContext _mktngdb;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IConfiguration configuration, mktngContext dbContext, IWebHostEnvironment hostingEnvironment, ILogger<ClientController> logger)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _mktngdb = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }
        public IActionResult Index(string assessment, string searchTerm = "", int pageNumber = 1)
        {
            // Store the current search term in ViewData
            ViewData["CurrentSearch"] = searchTerm;

            // Retrieve session data for personalization
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            // Pass filter values to the view
            ViewBag.Assessment = assessment;

            string position = ViewData["SessionPos"]?.ToString();
            string name = ViewData["SessionName"]?.ToString();

            // Pagination settings
            int pageSize = 10;
            int offset = (pageNumber - 1) * pageSize;

            // Initialize query and parameters
            List<ClientModel> clients = new();
            int totalRecords = 0;

            try
            {
                // Base query with filtering by FlowStatus
                string baseQuery = @"
        SELECT Id, Prospect, Designation, Company, Phone, Email, Status, Notes, DateAdded, AddedBy, Type, Assessment, InquiryType 
        FROM Clients 
        WHERE FlowStatus = 'F1'";
                string countQuery = "SELECT COUNT(*) FROM Clients WHERE FlowStatus = 'F1'";

                // Add role-based filter if applicable
                if (position == "SALES STAFF" || position == "ACCOUNT EXECUTIVE")
                {
                    baseQuery += " AND AddedBy = @AddedBy";
                    countQuery += " AND AddedBy = @AddedBy";
                }

                // Add Assessment filter based on the assessment parameter
                if (!string.IsNullOrEmpty(assessment))
                {
                    if (assessment == "Positive")
                    {
                        baseQuery += " AND Assessment = 'Positive'";
                        countQuery += " AND Assessment = 'Positive'";
                    }
                    else
                    {
                        baseQuery += " AND Assessment != 'Positive'";
                        countQuery += " AND Assessment != 'Positive'";
                    }
                }

                // Add search condition if a search term is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string searchCondition = @"
            AND (Prospect LIKE @SearchTerm OR 
                 Designation LIKE @SearchTerm OR 
                 Company LIKE @SearchTerm OR 
                 Phone LIKE @SearchTerm OR 
                 Email LIKE @SearchTerm OR 
                 Status LIKE @SearchTerm OR 
                 Notes LIKE @SearchTerm OR 
                 AddedBy LIKE @SearchTerm OR 
                 Type LIKE @SearchTerm OR 
                 Assessment LIKE @SearchTerm OR 
                 InquiryType LIKE @SearchTerm)";

                    baseQuery += searchCondition;
                    countQuery += searchCondition;
                }

                // Add pagination to the main query
                baseQuery += " ORDER BY DateAdded DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                // Fetch data from the database
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Count total records
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                    {
                        if (position == "SALES STAFF" || position == "ACCOUNT EXECUTIVE")
                        {
                            countCommand.Parameters.AddWithValue("@AddedBy", name);
                        }

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            countCommand.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        }

                        totalRecords = (int)countCommand.ExecuteScalar();
                    }

                    // Fetch paginated records
                    using (SqlCommand command = new SqlCommand(baseQuery, connection))
                    {
                        if (position == "SALES STAFF" || position == "ACCOUNT EXECUTIVE")
                        {
                            command.Parameters.AddWithValue("@AddedBy", name);
                        }

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        }

                        command.Parameters.AddWithValue("@Offset", offset);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clients.Add(new ClientModel
                                {
                                    Id = reader.GetInt32(0),
                                    Prospect = reader.GetString(1),
                                    Designation = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Company = reader.GetString(3),
                                    Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    Status = reader.GetString(6),
                                    Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    DateAdded = reader.GetDateTime(8),
                                    AddedBy = reader.GetString(9),
                                    Type = reader.GetString(10),
                                    Assessment = reader.GetString(11),
                                    InquiryType = reader.IsDBNull(12) ? null : reader.GetString(12)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                _logger.LogError(ex, "An error occurred while fetching clients.");
            }

            // Pass pagination data to the view
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;

            return View(Tuple.Create(clients, totalRecords));
        }
        public IActionResult InquiredClients(string inqType, string searchTerm = "", int pageNumber = 1)
        {
            // Store the current search term in ViewData
            ViewData["CurrentSearch"] = searchTerm;

            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            ViewBag.InqType = inqType; // Pass the inquiry type to the view
            string name = ViewData["SessionName"]?.ToString();

            // Pagination settings
            int pageSize = 10;
            int offset = (pageNumber - 1) * pageSize;

            // Initialize query and parameters
            List<ClientModel> clients = new();
            int totalRecords = 0;

            try
            {
                // Base query with filtering by FlowStatus and AddedBy
                string baseQuery = @"
        SELECT Id, Prospect, Designation, Company, Phone, Email, Status, Notes, DateAdded, AddedBy, Type, Assessment, InquiryType
        FROM Clients 
        WHERE FlowStatus = 'F1' AND AddedBy = @AddedBy";

                string countQuery = "SELECT COUNT(*) FROM Clients WHERE FlowStatus = 'F1' AND AddedBy = @AddedBy";

                // Add InquiryType filter based on inqType
                if (inqType == "CC")
                {
                    baseQuery += " AND InquiryType = 'Cold Call'";
                    countQuery += " AND InquiryType = 'Cold Call'";
                }
                else
                {
                    baseQuery += " AND InquiryType != 'Cold Call'";
                    countQuery += " AND InquiryType != 'Cold Call'";
                }

                // Add search condition if a search term is provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string searchCondition = @"
            AND (Prospect LIKE @SearchTerm OR 
                 Designation LIKE @SearchTerm OR 
                 Company LIKE @SearchTerm OR 
                 Phone LIKE @SearchTerm OR 
                 Email LIKE @SearchTerm OR 
                 Status LIKE @SearchTerm OR 
                 Notes LIKE @SearchTerm OR 
                 AddedBy LIKE @SearchTerm OR 
                 Type LIKE @SearchTerm OR 
                 Assessment LIKE @SearchTerm)";

                    baseQuery += searchCondition;
                    countQuery += searchCondition;
                }

                // Add pagination to the main query
                baseQuery += " ORDER BY DateAdded DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                // Fetch data from the database
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Count total records
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                    {
                        countCommand.Parameters.AddWithValue("@AddedBy", name);

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            countCommand.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        }

                        totalRecords = (int)countCommand.ExecuteScalar();
                    }

                    // Fetch paginated records
                    using (SqlCommand command = new SqlCommand(baseQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AddedBy", name);

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        }

                        command.Parameters.AddWithValue("@Offset", offset);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clients.Add(new ClientModel
                                {
                                    Id = reader.GetInt32(0),
                                    Prospect = reader.GetString(1),
                                    Designation = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Company = reader.GetString(3),
                                    Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    Status = reader.GetString(6),
                                    Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    DateAdded = reader.GetDateTime(8),
                                    AddedBy = reader.GetString(9),
                                    Type = reader.GetString(10),
                                    Assessment = reader.GetString(11),
                                    InquiryType = reader.IsDBNull(12) ? null : reader.GetString(12) // Map InquiryType
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                _logger.LogError(ex, "An error occurred while fetching clients.");
            }

            // Pass pagination data to the view
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;

            return View(Tuple.Create(clients, totalRecords));
        }
        // Helper method to add parameters to SQL commands
        private void AddParameters(SqlCommand command, string position, string name, string searchTerm)
        {
            if (position == "SALES STAFF" || position == "ACCOUNT EXECUTIVE")
            {
                command.Parameters.AddWithValue("@AddedBy", name);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
        }
        // GET: AddClients
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult AddClients()
        {
            // Check session for authorization
            if (HttpContext.Session.GetString(SessionType) != "Admin" && HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }

            // Pass session data to the view
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            return View();
        }
        // POST: Client/AddClients
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClients(ClientModel client)
        {
            // Pass session data to the view
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);
            string name = ViewData["SessionName"]?.ToString();

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values for fields not provided by the user
                    client.DateAdded = DateTime.Now;
                    client.AddedBy = name ?? "System"; // Default to "System" if no user is authenticated
                    client.FlowStatus = "F1"; // Default flow status


                    await _mktngdb.Clients.AddAsync(client);
                    int rowsAffected = await _mktngdb.SaveChangesAsync();

                    if (rowsAffected > 0)
                        {
                            // Notify success
                            Notify("Client added successfully!", "Success", NotificationType.success);

                            // Redirect to a success page or list of clients
                            return RedirectToAction("Index", "Client"); // Replace "Index" with your desired action
                        }
                    
                }
                catch (Exception ex)
                {
                    // Log the error (optional)
                    ModelState.AddModelError("", "An error occurred while saving the client.");
                }
            }

            Notify("Saaaad!", "Error", NotificationType.error);
            // If the model state is invalid, return the view with validation errors
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClient([FromBody] ClientModel client)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid || client == null)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                // Fetch the existing client record from the database
                var existingClient = _mktngdb.Clients.FirstOrDefault(c => c.Id == client.Id);

                if (existingClient == null)
                {
                    return Json(new { success = false, message = "Client not found." });
                }

                // Update the existing client record with the new values
                existingClient.Prospect = client.Prospect;
                existingClient.Designation = client.Designation ?? (string?)null; // Handle nullable fields
                existingClient.Company = client.Company;
                existingClient.Phone = client.Phone ?? (string?)null;
                existingClient.Email = client.Email ?? (string?)null;
                existingClient.Status = client.Status;
                existingClient.Type = client.Type;
                existingClient.Notes = client.Notes ?? (string?)null;
                existingClient.Assessment = client.Assessment;

                // Save changes to the database
                int rowsAffected = _mktngdb.SaveChanges();

                // Check if the update was successful
                if (rowsAffected == 0)
                {
                    return Json(new { success = false, message = "No changes were made." });
                }

                // Fetch the updated record from the database
                var updatedClient = _mktngdb.Clients
                    .Where(c => c.Id == client.Id)
                    .Select(c => new
                    {
                        id = c.Id,
                        prospect = c.Prospect,
                        type = c.Type,
                        designation = c.Designation,
                        company = c.Company,
                        phone = c.Phone,
                        email = c.Email,
                        status = c.Status,
                        notes = c.Notes,
                        assessment = c.Assessment
                    })
                    .FirstOrDefault();

                if (updatedClient == null)
                {
                    return Json(new { success = false, message = "Failed to fetch updated client data." });
                }

                // Return success response with updated client data
                return Json(new { success = true, client = updatedClient });
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Return a detailed error message
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost]
        public JsonResult UpdateField([FromBody] UpdateFieldModel model)
        {
            try
            {
                // Update the database based on the provided field
                var client = _mktngdb.Clients.Find(model.Id);
                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found." });
                }

                switch (model.Field.ToLower())
                {
                    case "prospect":
                        client.Prospect = model.Value;
                        break;
                    case "type":
                        client.Type = model.Value;
                        break;
                    case "designation":
                        client.Designation = model.Value;
                        break;
                    case "company":
                        client.Company = model.Value;
                        break;
                    case "phone":
                        client.Phone = model.Value;
                        break;
                    case "email":
                        client.Email = model.Value;
                        break;
                    case "status":
                        client.Status = model.Value;
                        break;
                    case "notes":
                        client.Notes = model.Value;
                        break;
                    case "assessment":
                        client.Assessment = model.Value;
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid field." });
                }

                _mktngdb.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateFieldModel
        {
            public int Id { get; set; }
            public string Field { get; set; }
            public string Value { get; set; }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClient(int id, [FromBody] DeleteRequest request)
        {
            // Pass session data to the view
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);
            string name = ViewData["SessionName"]?.ToString();

            try
            {
                // Validate input
                if (request == null || string.IsNullOrWhiteSpace(request.Reason))
                {
                    return Json(new { success = false, message = "Reason for deletion is required." });
                }

                // Get the current user from the session
                var deletedBy = name ?? "Unknown User";

                // Fetch the client record to be updated
                var client = _mktngdb.Clients.FirstOrDefault(c => c.Id == id);

                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found or already marked as deleted." });
                }

                // Update the client record
                client.FlowStatus = "D1"; // Mark as deleted
                client.DeletedBy = deletedBy;
                client.DateDeleted = DateTime.Now;
                client.ReasonDel = request.Reason;

                // Save changes to the database
                int rowsAffected = _mktngdb.SaveChanges();

                // Check if the update was successful
                if (rowsAffected == 0)
                {
                    return Json(new { success = false, message = "Failed to mark client as deleted." });
                }

                // Return success response
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Return a detailed error message
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // Helper class for the delete request
        public class DeleteRequest
        {
            public string Reason { get; set; }
        }


    }
}
