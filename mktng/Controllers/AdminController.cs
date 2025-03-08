using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using mktng.Models;
using System.Data.SqlClient;
using System.Data;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Globalization;


namespace mktng.Controllers
{
    public class AdminController : BaseController
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

        public AdminController(IConfiguration configuration, mktngContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _mktngdb = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString(SessionType) != "Admin" && HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);




            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult ChangeInfo()
        {
            if (HttpContext.Session.GetString(SessionType) != "Admin" && HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            string sesId = HttpContext.Session.GetString(SessionId);
            using (cmd = new SqlCommand("SELECT Name, Password, Email, ContactNo FROM Users WHERE Id = '" + sesId + "'", con))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    var model = new ChangeInfo
                    {
                        CurName = reader["Name"].ToString(),
                        CurPass = reader["Password"].ToString(),
                        CurEmail = reader["Email"].ToString(),
                        CurContact = reader["ContactNo"].ToString()
                    };

                    return View(model);
                }
                else
                {
                    return View();
                }

            }
        }
        public IActionResult UserAccount()
        {
            if (HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            cmd = new SqlCommand("SELECT Id, Name, Position, Office FROM Users WHERE Status = 'Pending'", con);

            DataSet ulist = new DataSet();
            SqlDataAdapter userList = new SqlDataAdapter(cmd);
            userList.Fill(ulist, "userlist");

            ViewBag.users = ulist.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
        public IActionResult UserAccount(string userId, string stat)
        {
            if (HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            using (cmd = new SqlCommand("Update Users SET Status = @Status WHERE Id = @UserId", con))
            {
                cmd.Parameters.AddWithValue("@Status", stat);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
            Notify("Approved Successfully!", "Success", NotificationType.success);
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("UserAccount", "Admin");
        }
        [HttpPost]
        public IActionResult ChangeInfo(ChangeInfo info)
        {
            if (HttpContext.Session.GetString(SessionType) != "Admin" && HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);
            string sesId = HttpContext.Session.GetString(SessionId);
            using (cmd = new SqlCommand("SELECT Name, Email, ContactNo FROM Users WHERE Id = '" + sesId + "'", con))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    {
                        info.CurName = reader["Name"].ToString();
                        info.CurEmail = reader["Email"].ToString();
                        info.CurContact = reader["ContactNo"].ToString();
                    }
                }
            }
            if (ModelState.IsValid)
            {
                if (info.Name != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET Name = '" + info.Name + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                }
                if (info.Password != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET Password = '" + info.Password + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                }
                if (info.Email != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET Email = '" + info.Email + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                }
                if (info.ContactNo != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET ContactNo = '" + info.ContactNo + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View(info);
        }
        public IActionResult AccountDeactivate()
        {
            if (HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            var ActiveUsers = _mktngdb.Users
                .FromSqlRaw("SELECT * FROM Users WHERE Status = 'Active'")
                .ToList();

            ViewBag.Users = ActiveUsers;

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
        public IActionResult DeactivateAcc(int id)
        {
            if (HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionPos"] = HttpContext.Session.GetString(SessionPos);

            var user = _mktngdb.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Status = "Deactivated";
                _mktngdb.SaveChanges();
            }

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("AccountDeactivate", "Admin");
        }
        
       
    }    
}       