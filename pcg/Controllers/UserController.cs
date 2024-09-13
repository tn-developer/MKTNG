using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pcg.Models;
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
using pcg.Formula;
using System.IO;
using System.Linq;
using System.Globalization;

namespace pcg.Controllers
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
        private readonly PCGContext _pcgdb;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly FileService _fileService;

        public UserController(IConfiguration configuration, PCGContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            _pcgdb = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _fileService = new FileService(Path.Combine(_hostingEnvironment.WebRootPath, "pcgfiles"));
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
                    " u.Id AS Id_user, " +
                    " u.Name AS Name, " +
                    " u.Position FROM Tasks t " +
                    "LEFT JOIN Users u " +
                    "ON t.AssignId = u.Id " +
                    "WHERE t.SiteReqId = '" + siteId + "' AND t.Status != 'Cleared' AND (t.AddedBy = '" + sesname + "' OR t.AssignId ='" + HttpContext.Session.GetString(SessionId) + "') " +
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
        [HttpGet]
        public IActionResult AddTask(int siteid)
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
            ViewData["SiteId"] = siteid;

            cmd = new SqlCommand("SELECT SiteId, Client, Site, SiteOM FROM Sites WHERE SiteId = '" + siteid + "'", con);
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
            proce.Fill(proc, "ulist");

            ViewBag.Process = proc.Tables[0];

            var vm = new VariationModel
            {
                SiteId = siteid,
            };

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return PartialView("actionmodal/_AddTaskPartial", vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddTask(VariationModel vm, int siteid)
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
            ViewData["SiteId"] = siteid;

            cmd = new SqlCommand("SELECT SiteId, Client, Site, SiteOM FROM Sites WHERE SiteId = '" + siteid + "'", con);
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
            proce.Fill(proc, "ulist");

            ViewBag.Process = proc.Tables[0];

            if (ModelState.IsValid)
            {
                if (HttpContext.Session.GetString(SessionType) == "SUser")
                {
                    var (filePath, rawfilename, errorMessage) = await _fileService.UploadFileAsync(vm.UploadFile, "tba");
                    if (errorMessage != null)
                    {
                        ModelState.AddModelError("UploadFile", errorMessage);
                        return PartialView("actionmodal/_AddTaskPartial", vm);
                    }
                    string sesname = HttpContext.Session.GetString(SessionName);
                    string desc = vm.Description + " " + vm.Descquery + vm.Descdocreq + vm.Descvary;
                    string task = vm.Task.Replace("'", "").Replace("\"", "");
                    string details = vm.Details.Replace("'", "").Replace("\"", "");

                    DateTime cdt = DateTime.Now;
                    string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
                    using (cmd = new SqlCommand("INSERT INTO Tasks (Task, Details, Description, AddedBy, SiteReqId, AssignId, Status, DateStart, Process, Circulation, TaskType) VALUES" +
                        "(@Task, " +
                        "@Details, " +
                        "@Desc, " +
                        "@Sesname, " +
                        "@SiteReqId, " +
                        "@AssignId, " +
                        "'Approved', " +
                        "@Scdt, " +
                        "'Q1', " +
                        "'0', " +
                        "@TaskType); SELECT SCOPE_IDENTITY(), Process FROM Tasks WHERE TaskId = SCOPE_IDENTITY();", con))
                    {
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                        cmd.Parameters.AddWithValue("@Desc", desc);
                        cmd.Parameters.AddWithValue("@Sesname", sesname);
                        cmd.Parameters.AddWithValue("@SiteReqId", siteid);
                        cmd.Parameters.AddWithValue("@AssignId", "0");
                        cmd.Parameters.AddWithValue("@Scdt", scdt);
                        cmd.Parameters.AddWithValue("@TaskType", vm.Description);
                    }

                    string taskId;
                    string process;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        taskId = Convert.ToString(reader[0]);
                        process = reader["Process"].ToString();
                    }

                    int filecount = 1;
                    string addedby = HttpContext.Session.GetString(SessionId);
                    string datePart = "(" + DateTime.Now.ToString("MMddyy") + ")";
                    var fileExtension = Path.GetExtension(rawfilename);
                    string newname = "_" + taskId + "_" + addedby + "_" + filecount;

                    var renamefile = await _fileService.RenameFileAsync(rawfilename, newname);

                    if (!renamefile.Success)
                    {
                        ModelState.AddModelError("UploadFile", "Error renaming");
                        return PartialView("actionmodal/_AddTaskPartial", vm);
                    }

                    DateTime fdt = DateTime.ParseExact(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    string fname = rawfilename;

                    var pcgFile = new Files
                    {
                        FileName = datePart + newname + fileExtension,
                        FileAlias = vm.FileAlias,
                        TaskId = int.Parse(taskId),
                        Status = "Active",
                        AddedBy = int.Parse(addedby),
                        DateAdded = fdt
                    };
                    _pcgdb.Files.Add(pcgFile);
                    _pcgdb.SaveChanges();

                    using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateStart, Status, Process, Task, Details, Circulation) " +
                        "VALUES (@TaskId, " +
                        "@AssignId, " +
                        "@DateStart, " +
                        "'Pre-approve', " +
                        "@Process, " +
                        "@Task, " +
                        "@Details, " +
                        "'0')", con))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        cmd.Parameters.AddWithValue("@AssignId", "");
                        cmd.Parameters.AddWithValue("@DateStart", scdt);
                        cmd.Parameters.AddWithValue("@Process", process);
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                    }
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var (filePath, rawfilename, errorMessage) = await _fileService.UploadFileAsync(vm.UploadFile, "tba");
                    if (errorMessage != null)
                    {
                        ModelState.AddModelError("UploadFile", errorMessage);
                        return PartialView("actionmodal/_AddTaskPartial", vm);
                    }
                    string sesname = HttpContext.Session.GetString(SessionName);
                    string desc = vm.Description + " " + vm.Descquery + vm.Descdocreq + vm.Descvary;
                    string task = vm.Task.Replace("'", "").Replace("\"", "");
                    string details = vm.Details.Replace("'", "").Replace("\"", "");

                    DateTime cdt = DateTime.Now;
                    string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
                    using (cmd = new SqlCommand("INSERT INTO Tasks (Task, Details, Description, AddedBy, SiteReqId, AssignId, Status, DateStart, Process, Circulation, TaskType) VALUES" +
                        "(@Task, " +
                        "@Details, " +
                        "@Desc, " +
                        "@Sesname, " +
                        "@SiteReqId, " +
                        "@AssignId, " +
                        "'Pre-request', " +
                        "@Scdt, " +
                        "'Q1', " +
                        "'0', " +
                        "@TaskType); SELECT SCOPE_IDENTITY(), Process FROM Tasks WHERE TaskId = SCOPE_IDENTITY();", con))
                    {
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                        cmd.Parameters.AddWithValue("@Desc", desc);
                        cmd.Parameters.AddWithValue("@Sesname", sesname);
                        cmd.Parameters.AddWithValue("@SiteReqId", siteid);
                        cmd.Parameters.AddWithValue("@AssignId", "0");
                        cmd.Parameters.AddWithValue("@Scdt", scdt);
                        cmd.Parameters.AddWithValue("@TaskType", vm.Description);
                    }

                    string taskId;
                    string process;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        taskId = Convert.ToString(reader[0]);
                        process = reader["Process"].ToString();
                    }

                    int filecount = 1;
                    string addedby = HttpContext.Session.GetString(SessionId);
                    string datePart = "(" + DateTime.Now.ToString("MMddyy") + ")";
                    var fileExtension = Path.GetExtension(rawfilename);
                    string newname = "_" + taskId + "_" + addedby + "_" + filecount;

                    var renamefile = await _fileService.RenameFileAsync(rawfilename, newname);

                    if (!renamefile.Success)
                    {
                        ModelState.AddModelError("UploadFile", "Error renaming");
                        return PartialView("actionmodal/_AddTaskPartial", vm);
                    }

                    DateTime fdt = DateTime.ParseExact(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    string fname = rawfilename;

                    var pcgFile = new Files
                    {
                        FileName = datePart + newname + fileExtension,
                        FileAlias = vm.FileAlias,
                        TaskId = int.Parse(taskId),
                        Status = "Active",
                        AddedBy = int.Parse(addedby),
                        DateAdded = fdt
                    };
                    _pcgdb.Files.Add(pcgFile);
                    _pcgdb.SaveChanges();

                    using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateStart, Status, Process, Task, Details, Circulation) " +
                        "VALUES (@TaskId, " +
                        "@AssignId, " +
                        "@DateStart, " +
                        "'Pre-request', " +
                        "@Process, " +
                        "@Task, " +
                        "@Details, " +
                        "'0')", con))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        cmd.Parameters.AddWithValue("@AssignId", "");
                        cmd.Parameters.AddWithValue("@DateStart", scdt);
                        cmd.Parameters.AddWithValue("@Process", process);
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                        cmd.ExecuteNonQuery();
                    }
                }
                ViewBag.SuccessMessage = "Task added.";
                return PartialView("actionmodal/_AddTaskPartial", vm);
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return PartialView("actionmodal/_AddTaskPartial", vm);
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
                        "WHERE t.Status = 'OM-approve' AND (s.SiteSOM = '" + sesid + "') " +
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
        public IActionResult Approve(ProcessModel pm)
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

            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
            if (HttpContext.Session.GetString(SessionType) == "SOM") 
            {
                using (cmd = new SqlCommand("UPDATE Tasks SET " +
                    "Task = @Task, " +
                    "Details = @Details, " +
                    "Process = @Process, " +
                    "Status = 'SOM-approve' " +
                    "WHERE TaskId = @TaskId", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                    cmd.Parameters.AddWithValue("@Task", pm.Task);
                    cmd.Parameters.AddWithValue("@Details", pm.Details);
                    cmd.Parameters.AddWithValue("@Process", pm.Process);
                }
                cmd.ExecuteNonQuery();

                using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateFwd, Status, Process, Task, Details, Circulation) " +
                    "VALUES (@TaskId, '0', @DateFwd, 'Pre-approve', @Process, @Task, @Details, '0')", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                    cmd.Parameters.AddWithValue("@DateFwd", scdt);
                    cmd.Parameters.AddWithValue("@Task", pm.Task);
                    cmd.Parameters.AddWithValue("@Details", pm.Details);
                    cmd.Parameters.AddWithValue("@Process", pm.Process);
                }
                cmd.ExecuteNonQuery();
            }
            if (HttpContext.Session.GetString(SessionType) == "OM")
            {
                using (cmd = new SqlCommand("UPDATE Tasks SET " +
                    "Task = @Task, " +
                    "Details = @Details, " +
                    "Process = @Process, " +
                    "Status = 'OM-approve' " +
                    "WHERE TaskId = @TaskId", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                    cmd.Parameters.AddWithValue("@Task", pm.Task);
                    cmd.Parameters.AddWithValue("@Details", pm.Details);
                    cmd.Parameters.AddWithValue("@Process", pm.Process);
                }
                cmd.ExecuteNonQuery();

                using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateFwd, Status, Process, Task, Details, Circulation) " +
                    "VALUES (@TaskId, '0', @DateFwd, 'Pre-approve', @Process, @Task, @Details, '0')", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                    cmd.Parameters.AddWithValue("@DateFwd", scdt);
                    cmd.Parameters.AddWithValue("@Task", pm.Task);
                    cmd.Parameters.AddWithValue("@Details", pm.Details);
                    cmd.Parameters.AddWithValue("@Process", pm.Process);
                }
                cmd.ExecuteNonQuery();
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Pending", "User");
        }
        [HttpPost]
        public IActionResult Decline(ProcessModel pm)
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

            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");

            using (cmd = new SqlCommand("UPDATE Tasks SET " +
                "AssignId = @AssignId, " +
                "Task = @Task, " +
                "Details = @Details, " +
                "Process = @Process, " +
                "Status = 'Declined' " +
                "WHERE TaskId = @TaskId", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@AssignId", pm.AssignId);
                cmd.Parameters.AddWithValue("@Task", pm.Task);
                cmd.Parameters.AddWithValue("@Details", pm.Details);
                cmd.Parameters.AddWithValue("@Process", pm.Process);
            }
            cmd.ExecuteNonQuery();

            using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateClr, Status, Process, Task, Details, Circulation) " +
                "VALUES (@TaskId, @AssignId, @DateClr, 'Declined', @Process, @Task, @Details, '0')", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@AssignId", pm.AssignId);
                cmd.Parameters.AddWithValue("@DateClr", scdt);
                cmd.Parameters.AddWithValue("@Task", pm.Task);
                cmd.Parameters.AddWithValue("@Details", pm.Details);
                cmd.Parameters.AddWithValue("@Process", pm.Process);
            }
            cmd.ExecuteNonQuery();

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Pending", "User");
        }
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
        [HttpPost]
        public IActionResult Recieve(ProcessModel pm)
        {
            if (HttpContext.Session.GetString(SessionType) != "User" && HttpContext.Session.GetString(SessionType) != "SOM" && HttpContext.Session.GetString(SessionType) != "OM")
            {
                return RedirectToAction("Login", "Home");
            }
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
            using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, Status, Process, Task, Details, Circulation, DateClr) " +
                "VALUES (@TaskId, @AssignId, 'Cleared', 'Q8', @Task, @Details, @Circulation, @DateClr)", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@AssignId", pm.AssignId);
                cmd.Parameters.AddWithValue("@Task", pm.Task);
                cmd.Parameters.AddWithValue("@Details", pm.Details);
                cmd.Parameters.AddWithValue("@Circulation", pm.Circulation);
                cmd.Parameters.AddWithValue("@DateClr", scdt);
                cmd.ExecuteNonQuery();
            }
            using (cmd = new SqlCommand("UPDATE Tasks SET DateClr = @DateClr, Status = 'Clear' WHERE TaskId = @TaskId", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@DateClr", scdt);
                cmd.ExecuteNonQuery();
            }
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            return RedirectToAction("TaskDetail", "User", new { siteId = pm.SiteId });
        }
    }
}
