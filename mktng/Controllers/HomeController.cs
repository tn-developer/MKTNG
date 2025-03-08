using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using mktng.Models;
using System.Data.SqlClient;
using System.Data;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace mktng.Controllers
{
    public class HomeController : BaseController
    {
        const string SessionName = "_Name";
        const string SessionType = "_Type";
        const string SessionLayout = "_Layout";
        const string SessionId = "_Id";
        const string SessionPos = "_Position";

        public SqlConnection con;
        public SqlCommand cmd;
        private readonly IConfiguration _configuration;
        private readonly mktngContext _dbContext;

        public HomeController(IConfiguration configuration, mktngContext dbContext)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _dbContext = dbContext;
        }
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString(SessionType) == null)
            {
                return RedirectToAction("Login", "Home");
            }           
            return View();
        }
        public IActionResult Register()
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            if (ModelState.IsValid)
            {
                cmd = new SqlCommand("SELECT Position FROM Positions WHERE Position != 'ADMIN'", con);
                DataSet pos = new DataSet();
                SqlDataAdapter post = new SqlDataAdapter(cmd);
                post.Fill(pos, "slist");

                ViewBag.Poslist = pos.Tables[0];

                cmd = new SqlCommand("SELECT Office FROM Office WHERE Status = 'Active'", con);
                DataSet office = new DataSet();
                SqlDataAdapter offices = new SqlDataAdapter(cmd);
                offices.Fill(office, "slist");

                ViewBag.Officelist = office.Tables[0];
            }
            return View();        
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store";
            HttpContext.Response.Headers["Pragma"] = "no-cache";
            HttpContext.Response.Headers["Expires"] = "-1";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Register(UsersModel reg)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            if (ModelState.IsValid)
            {
                cmd = new SqlCommand("SELECT COUNT(Username) FROM Users WHERE Username = '" + reg.Username + "'", con);
                int UserCount = Convert.ToInt32(cmd.ExecuteScalar());
                if (UserCount > 0)
                {
                    reg.Usernamecheck = reg.Username;
                    ModelState.AddModelError("Username", "Username is already taken.");

                    cmd = new SqlCommand("SELECT Position FROM Positions WHERE Position != 'ADMIN'", con);
                    DataSet pos = new DataSet();
                    SqlDataAdapter post = new SqlDataAdapter(cmd);
                    post.Fill(pos, "slist");

                    ViewBag.Poslist = pos.Tables[0];

                    return View(reg);
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Users (Username, Password, Name, Email, ContactNo, Position,Office, Status) " +
                            "VALUES ('" + reg.Username + "', '" +
                            reg.Password + "', '" +
                            reg.Name + "', '" +
                            reg.Email + "', '" +
                            reg.ContactNo + "', '" +
                            reg.Position + "', '" +
                            reg.Office + "', '" +
                            "Pending')", con);
                    cmd.ExecuteNonQuery();
                    Notify("Your account is now for approval!", "Account created successfully!", NotificationType.success);

                }
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }           
                return RedirectToAction("Login","Home");           
        }
        public IActionResult Login()
        {
            string session = HttpContext.Session.GetString(SessionType);

            if (!string.IsNullOrEmpty(session))
            {
                switch (session)
                {
                    case "SAdmin":
                        return RedirectToAction("Index", "Admin");
                    case "Admin":
                        return RedirectToAction("Index", "User");
                    default:
                        return RedirectToAction("Index", session);
                }
            }
            return View();
        }


        [HttpPost]
        public IActionResult Login(Login log)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            if (ModelState.IsValid)
            {
                cmd = new SqlCommand("SELECT COUNT(Id) FROM Users WHERE Username ='" + log.Username + "' AND Password ='" +
                      log.Password + "' AND Status = 'Active'", con);
                int UserCount = Convert.ToInt32(cmd.ExecuteScalar());
                if(UserCount > 0)
                {
                    cmd = new SqlCommand("SELECT u.Name From Users u WHERE Username = '" + log.Username + "' AND Password = '" + log.Password + "'", con);
                    string name = cmd.ExecuteScalar().ToString();
                    HttpContext.Session.SetString(SessionName, name);
                    
                    cmd = new SqlCommand("SELECT p.Usertype From Users u LEFT JOIN Positions p ON u.Position = p.Position WHERE Username = '" + log.Username + "' AND Password = '" + log.Password + "'", con);
                    string type = cmd.ExecuteScalar().ToString();                   
                    HttpContext.Session.SetString(SessionType, type);
                    HttpContext.Session.SetString(SessionLayout, type);
                  
                    cmd = new SqlCommand("SELECT u.Id From Users u WHERE Username = '" + log.Username + "' AND Password = '" + log.Password + "'", con);
                    string SesId = cmd.ExecuteScalar().ToString();
                    HttpContext.Session.SetString(SessionId, SesId);
                    cmd = new SqlCommand("SELECT Position From Users WHERE Username = '" + log.Username + "' AND Password = '" + log.Password + "'", con);
                    string SesPos = cmd.ExecuteScalar().ToString();
                    HttpContext.Session.SetString(SessionPos, SesPos);

                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    Notify("Login successfully!", "Welcome!", NotificationType.success);
                    
                            return RedirectToAction("Index", "Admin");
                    
                }
                else
                {
                    Notify("Wrong credentials | Account for Approval.", "Error", NotificationType.error);
                }
            }
            return View();
        }
    }
}
