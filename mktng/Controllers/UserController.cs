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
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Globalization;

namespace mktng.Controllers
{
    public class UserController : Controller
    {
        const string SessionName = "_Name";
        const string SessionLayout = "_Layout";
        const string SessionType = "_Type";
        const string SessionId = "_Id";

        public SqlConnection con;
        public SqlCommand cmd;
        private readonly IConfiguration _configuration;
        private readonly mktngContext _mktngdb;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserController(IConfiguration configuration, mktngContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _mktngdb = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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

            if (HttpContext.Session.GetString(SessionType) == "SOM" || HttpContext.Session.GetString(SessionType) == "OM") 
            {
                string sesid = HttpContext.Session.GetString(SessionId);
                cmd = new SqlCommand("SELECT t.TaskId, " +
                        " t.Task," +
                        " t.Description," +
                        " t.Details," +
                        " t.AddedBy," +
                        " t.SiteReqId," +
                        " t.DateStart," +
                        " t.DateFwd," +
                        " t.DateRcv," +
                        " t.DateClr," +
                        " t.AssignId," +
                        " t.ForwardId," +
                        " t.Status," +
                        " p.Code," +
                        " s.Client," +
                        " s.Site," +
                        " s.SiteOM," +
                        " s.SiteSOM," +
                        " s.SiteSC," +
                        " s.SiteTK," +
                        " uf.Id AS Idf," +
                        " uf.Name AS Namef, " +
                        " uf.Position As Positionf, " +
                        " ua.Id AS Ida, " +
                        " ua.Name AS Namea, " +
                        " ua.Position AS Positiona, " +
                        " pf.UserType AS UserTypepf, " +
                        " pa.UserType As UserTypepa " +
                        "FROM Tasks t " +
                        "LEFT JOIN Users uf " +
                        "ON t.ForwardId = uf.Id " +
                        "LEFT JOIN Positions pf " +
                        "ON uf.Position = pf.Position " +
                        "LEFT JOIN Users ua " +
                        "ON t.AssignId = ua.Id " +
                        "LEFT JOIN Positions pa " +
                        "ON ua.Position = pa.Position " +
                        "LEFT JOIN Sites s " +
                        "ON t.SiteReqId = s.SiteId " +
                        "LEFT JOIN Taskprocess p " +
                        "ON t.Process = p.Code " +
                        "INNER JOIN (SELECT MAX(DateFwd) AS LatestDateFwd FROM Tasks) AS latest ON t.DateFwd = latest.LatestDateFwd " +
                        "WHERE t.Status = 'Pre-request' AND (s.SiteOM = '" + sesid + "' OR s.SiteSOM = '" + sesid + "') " +
                        "ORDER BY t.TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "tlist");

                ViewBag.Pending = task.Tables[0];
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult ChangeInfo()
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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
            string sesId = HttpContext.Session.GetString(SessionId);
            using (cmd = new SqlCommand("SELECT Name, Email, ContactNo FROM Users WHERE Id = '" + sesId + "'", con))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows && reader.Read())
                {
                    var model = new ChangeInfo
                    {
                        CurName = reader["Name"].ToString(),
                        CurEmail = reader["Email"].ToString(),
                        CurContact = reader["ContactNo"].ToString()
                    };
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    return View(model);
                }
                else
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    return View();
                }
            }

        }
        [HttpPost]
        public IActionResult ChangeInfo(ChangeInfo info)
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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
                    ViewBag.SuccessName = "Name updated";
                }
                if (info.Password != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET Password = '" + info.Password + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                    ViewBag.SuccessPass = "Password updated";
                }
                if (info.Email != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET Email = '" + info.Email + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                    ViewBag.SuccessEmail = "Email updated";
                }
                if (info.ContactNo != null)
                {
                    cmd = new SqlCommand("UPDATE Users SET ContactNo = '" + info.ContactNo + "' WHERE Id = '" + sesId + "'", con);
                    cmd.ExecuteNonQuery();
                    ViewBag.SuccessContactNo = "Contact# updated";
                }                
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View(info);
        }
        public IActionResult Sites()
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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
            string sesname = HttpContext.Session.GetString(SessionName);
            cmd = new SqlCommand("SELECT s.SiteId, s.Client, s.Site, s.DateAdded, s.Status, s.SiteOM, uom.Name AS OMname, s.SiteSOM, usom.Name SOMname, s.SiteSC, usc.Name AS SCname, s.SiteTK, utk.Name AS TKname FROM Sites s LEFT JOIN Users uom ON s.SiteOM = uom.Id " +
                                "LEFT JOIN Users usom ON s.SiteSOM = usom.Id " +
                                "LEFT JOIN Users usc ON s.SiteSC = usc.Id " +
                                "LEFT JOIN Users utk ON s.SiteTK = utk.Id WHERE (uom.Name = '" + sesname + "' OR usom.Name = '" + sesname + "' OR usc.Name = '" + sesname + "' OR utk.Name = '" + sesname + "') AND s.Status = 'Active'", con);
            DataSet sites = new DataSet();
            SqlDataAdapter ssite = new SqlDataAdapter(cmd);
            ssite.Fill(sites, "slist");

            ViewBag.Sitelist = sites.Tables[0];

            cmd = new SqlCommand("SELECT u.Id, u.Name, p.Usertype, u.Position FROM Users u LEFT JOIN Positions p ON u.Position = p.Position", con);
            DataSet user = new DataSet();
            SqlDataAdapter users = new SqlDataAdapter(cmd);
            users.Fill(user, "ulist");

            ViewBag.Userlist = user.Tables[0];

            cmd = new SqlCommand("SELECT * FROM Taskprocess", con);
            DataSet proc = new DataSet();
            SqlDataAdapter proce = new SqlDataAdapter(cmd);
            proce.Fill(proc, "plist");

            ViewBag.Process = proc.Tables[0];
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult TaskDetail(string siteId)
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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
            string sesname = HttpContext.Session.GetString(SessionName);
            string sesid = HttpContext.Session.GetString(SessionId);

            if (HttpContext.Session.GetString(SessionType) == "SOM") {
                cmd = new SqlCommand("SELECT t.TaskId," +
                    " t.Task," +
                    " t.Description," +
                    " t.Details," +
                    " t.AddedBy," +
                    " t.SiteReqId AS SiteId," +
                    " t.DateStart," +
                    " t.DateFwd," +
                    " t.DateRcv," +
                    " t.DateClr," +
                    " t.AssignId," +
                    " t.ForwardId," +
                    " t.Status," +
                    " u.Id AS Id_user, " +
                    " u.Name AS Name, " +
                    " u.Position FROM Tasks t " +
                    "LEFT JOIN Users u " +
                    "ON t.AssignId = u.Id " +
                    "WHERE t.SiteReqId = '" + siteId + "' " +
                    "ORDER BY TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Tasklog = task.Tables[0];
            }

            if (HttpContext.Session.GetString(SessionType) == "OM")
            {
                cmd = new SqlCommand("SELECT t.TaskId," +
                    " t.Task," +
                    " t.Description," +
                    " t.Details," +
                    " t.AddedBy," +
                    " t.SiteReqId AS SiteId," +
                    " t.DateStart," +
                    " t.DateFwd," +
                    " t.DateRcv," +
                    " t.DateClr," +
                    " t.AssignId," +
                    " t.ForwardId," +
                    " t.Status," +
                    " u.Id AS Id_user, " +
                    " u.Name AS Name, " +
                    " u.Position FROM Tasks t " +
                    "LEFT JOIN Users u " +
                    "ON t.AssignId = u.Id " +
                    "WHERE t.SiteReqId = '" + siteId + "' " +
                    "ORDER BY TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Tasklog = task.Tables[0];
            }
            if (HttpContext.Session.GetString(SessionType) == "User")
            {
                cmd = new SqlCommand("SELECT t.TaskId," +
                    " t.Task," +
                    " t.Description," +
                    " t.Details," +
                    " t.AddedBy," +
                    " t.SiteReqId AS SiteId," +
                    " t.DateStart," +
                    " t.DateFwd," +
                    " t.DateRcv," +
                    " t.DateClr," +
                    " t.AssignId," +
                    " t.ForwardId," +
                    " t.Circulation," +
                    " t.Status," +
                    " s.Client," +
                    " s.SiteSOM," +
                    " usom.Name AS SOM," +
                    " s.SiteOM," +
                    " uom.Name AS OM," +
                    " s.SiteSC," +
                    " usc.Name AS SC," +
                    " s.SiteTK," +
                    " utk.Name AS TK," +
                    " u.Id AS Id_user, " +
                    " u.Name AS Name, " +
                    " u.Position FROM Tasks t " +
                    "LEFT JOIN Users u " +
                    "ON t.AssignId = u.Id " +
                    "LEFT JOIN Sites s on t.SiteReqId = s.SiteId " +
                    "LEFT JOIN Users usom on s.SiteSOM = usom.Id " +
                    "LEFT JOIN Users uom on s.SiteOM = uom.Id " +
                    "LEFT JOIN Users usc on s.SiteSC = usc.Id " +
                    "LEFT JOIN Users utk on s.SiteTK = utk.Id " +
                    "WHERE t.SiteReqId = '" + siteId + "' AND t.Status NOT IN ('Cleared', 'Declined') AND (t.AddedBy = '" + sesname + "' OR t.AssignId = '" + sesid + "' " +
                    "OR s.SiteSC = " + sesid + " OR s.SiteTK = '" + sesid + "') " +
                    "ORDER BY TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Tasklog = task.Tables[0];
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        
        
        public IActionResult Pending()
        {
            if (HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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
            string sesid = HttpContext.Session.GetString(SessionId);
            if (HttpContext.Session.GetString(SessionType) == "OM")
            {
                cmd = new SqlCommand("SELECT t.TaskId, " +
                        " t.Task," +
                        " t.Description," +
                        " t.Details," +
                        " t.AddedBy," +
                        " t.SiteReqId," +
                        " t.DateStart," +
                        " t.DateFwd," +
                        " t.DateRcv," +
                        " t.DateClr," +
                        " t.AssignId," +
                        " t.ForwardId," +
                        " t.Status," +
                        " p.Code," +
                        " s.Client," +
                        " s.Site," +
                        " s.SiteOM," +
                        " s.SiteSOM," +
                        " s.SiteSC," +
                        " s.SiteTK," +
                        " uf.Id AS Idf," +
                        " uf.Name AS Namef, " +
                        " uf.Position As Positionf, " +
                        " ua.Id AS Ida, " +
                        " ua.Name AS Namea, " +
                        " ua.Position AS Positiona, " +
                        " pf.UserType AS UserTypepf, " +
                        " pa.UserType As UserTypepa " +
                        "FROM Tasks t " +
                        "LEFT JOIN Users uf " +
                        "ON t.ForwardId = uf.Id " +
                        "LEFT JOIN Positions pf " +
                        "ON uf.Position = pf.Position " +
                        "LEFT JOIN Users ua " +
                        "ON t.AssignId = ua.Id " +
                        "LEFT JOIN Positions pa " +
                        "ON ua.Position = pa.Position " +
                        "LEFT JOIN Sites s " +
                        "ON t.SiteReqId = s.SiteId " +
                        "LEFT JOIN Taskprocess p " +
                        "ON t.Process = p.Code " +
                        "WHERE t.Status = 'Pre-request' AND (s.SiteOM = '" + sesid + "') " +
                        "ORDER BY t.TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "tlist");

                ViewBag.Pending = task.Tables[0];
            }
            if (HttpContext.Session.GetString(SessionType) == "SOM")
            {
                cmd = new SqlCommand("SELECT t.TaskId, " +
                        " t.Task," +
                        " t.Description," +
                        " t.Details," +
                        " t.AddedBy," +
                        " t.SiteReqId," +
                        " t.DateStart," +
                        " t.DateFwd," +
                        " t.DateRcv," +
                        " t.DateClr," +
                        " t.AssignId," +
                        " t.ForwardId," +
                        " t.Status," +
                        " p.Code," +
                        " s.Client," +
                        " s.Site," +
                        " s.SiteOM," +
                        " s.SiteSOM," +
                        " s.SiteSC," +
                        " s.SiteTK," +
                        " uf.Id AS Idf," +
                        " uf.Name AS Namef, " +
                        " uf.Position As Positionf, " +
                        " ua.Id AS Ida, " +
                        " ua.Name AS Namea, " +
                        " ua.Position AS Positiona, " +
                        " pf.UserType AS UserTypepf, " +
                        " pa.UserType As UserTypepa " +
                        "FROM Tasks t " +
                        "LEFT JOIN Users uf " +
                        "ON t.ForwardId = uf.Id " +
                        "LEFT JOIN Positions pf " +
                        "ON uf.Position = pf.Position " +
                        "LEFT JOIN Users ua " +
                        "ON t.AssignId = ua.Id " +
                        "LEFT JOIN Positions pa " +
                        "ON ua.Position = pa.Position " +
                        "LEFT JOIN Sites s " +
                        "ON t.SiteReqId = s.SiteId " +
                        "LEFT JOIN Taskprocess p " +
                        "ON t.Process = p.Code " +
                        "WHERE t.Status = 'OM-approved' AND (s.SiteSOM = '" + sesid + "') " +
                        "ORDER BY t.TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "tlist");

                ViewBag.Pending = task.Tables[0];
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
       
        public IActionResult MyTask()
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
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

            string sesname = HttpContext.Session.GetString(SessionName);
            cmd = new SqlCommand("SELECT t.TaskId," +
                " t.Task," +
                " t.Description," +
                " t.Details," +
                " t.AddedBy," +
                " t.SiteReqId," +
                " t.DateStart," +
                " t.DateFwd," +
                " t.DateRcv," +
                " t.DateClr," +
                " t.AssignId," +
                " t.ForwardId," +
                " t.Status," +
                " t.Process," +
                " t.Circulation," +
                " p.Code, " +
                " s.Client, " +
                " s.Site, " +
                " u.Id AS Id_user, " +
                " u.Name AS Name, " +
                " u.Position FROM Tasks t " +
                "LEFT JOIN Users u " +
                "ON t.AssignId = u.Id " +
                "LEFT JOIN Sites s " +
                "ON t.SiteReqId = s.SiteId " +
                "LEFT JOIN Taskprocess p " +
                "ON t.Process = p.Code " +
                "WHERE t.Status = 'Approved' AND u.Name = '" + sesname + "' ", con);
            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "tlist");

            ViewBag.MyTasks = task.Tables[0];

            cmd = new SqlCommand("SELECT Code, Process FROM Taskprocess WHERE Code != 'Q8'", con);
            DataSet proc = new DataSet();
            SqlDataAdapter procs = new SqlDataAdapter(cmd);
            procs.Fill(proc, "proc");

            ViewBag.Process = proc.Tables[0];

            cmd = new SqlCommand("SELECT * FROM Users WHERE Name != '" + sesname + "' AND Status = 'Active' AND Position = 'Site Coordinator'", con);
            DataSet coor = new DataSet();
            SqlDataAdapter coord = new SqlDataAdapter(cmd);
            coord.Fill(coor, "clist");

            ViewBag.Coord = coor.Tables[0];

            cmd = new SqlCommand("SELECT * FROM Users WHERE Name != '" + sesname + "' AND Status = 'Active'", con);
            DataSet user = new DataSet();
            SqlDataAdapter users = new SqlDataAdapter(cmd);
            users.Fill(user, "ulist");

            ViewBag.Users = user.Tables[0];
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        
    }
}
