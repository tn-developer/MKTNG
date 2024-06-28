using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pcg.Models;
using System.Data.SqlClient;
using System.Data;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace pcg.Controllers
{
    public class HomeController : Controller
    {
        const string SessionName = "_Name";
        const string SessionType = "_Type";
        const string SessionLayout = "_Layout";
        const string SessionId = "_Id";

        public SqlConnection con;
        public SqlCommand cmd;
        private readonly IConfiguration _configuration;
        private readonly PCGContext _dbContext;

        public HomeController(IConfiguration configuration, PCGContext dbContext)
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
                cmd = new SqlCommand("SELECT Position FROM Positions", con);
                DataSet pos = new DataSet();
                SqlDataAdapter post = new SqlDataAdapter(cmd);
                post.Fill(pos, "slist");

                ViewBag.Poslist = pos.Tables[0];
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

                    cmd = new SqlCommand("SELECT Position FROM Positions", con);
                    DataSet pos = new DataSet();
                    SqlDataAdapter post = new SqlDataAdapter(cmd);
                    post.Fill(pos, "slist");

                    ViewBag.Poslist = pos.Tables[0];

                    return View(reg);
                }
                else
                {
                    cmd = new SqlCommand("INSERT INTO Users (Username, Password, Name, Email, ContactNo, Position, Status) " +
                            "VALUES ('" + reg.Username + "', '" +
                            reg.Password + "', '" +
                            reg.Name + "', '" +
                            reg.Email + "', '" +
                            reg.ContactNo + "', '" +
                            reg.Position + "', '" +
                            "Pending')", con);
                    cmd.ExecuteNonQuery();
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

            if (session != null)
            {
                if (session == "SAdmin")
                {                   
                    return RedirectToAction("Index", "Admin");
                }
                if (session == "SUser")
                {
                    return RedirectToAction("Index", "User");
                }
                else 
                {
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

                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    if (type == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (type == "User")
                    {
                        return RedirectToAction("Index", "User");
                    }
                    if (type == "SUser")
                    {
                        return RedirectToAction("Index", "User");
                    }
                    if (type == "SAdmin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }
                else
                {                   
                    ModelState.AddModelError("Username", "Username / Password is incorrect.");                    
                }
            }
            return View();
        }
    }
}
