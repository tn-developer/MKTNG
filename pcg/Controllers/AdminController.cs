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
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.AspNetCore.Hosting;
using pcg.Formula;
using System.IO;
using System.Globalization;


namespace pcg.Controllers
{
    public class AdminController : Controller
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

        public AdminController(IConfiguration configuration, PCGContext dbContext, IWebHostEnvironment hostingEnvironment)
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

            cmd = new SqlCommand("WITH LatestDateStart AS (" +
                "SELECT MAX(TaskLog.DateStart) AS MaxDateStart " +
                "FROM TaskLog LEFT JOIN Tasks on Tasks.TaskId = TaskLog.TaskId " +
                "WHERE TaskLog.Status NOT IN ('Pre-approve', 'Pre-request') " +
                "AND (Tasks.TaskType = 'Variation')) " +
                "SELECT l.LogId, l.TaskId, l.AssignId, l.Status, l.DateStart, l.DateFwd, l.DateRcv, l.DateClr, " +
                "t.Task, t.TaskType, t.Details, t.Description, t.SiteReqId, t.AddedBy, s.Client, s.Site, " +
                "u.Name, u.Position, l.LogId, l.TaskId, l.AssignId, l.Status, l.DateStart, l.DateFwd, l.DateRcv, " +
                "l.DateClr, t.Task, t.TaskType, t.Details, t.Description, " +
                "t.SiteReqId, t.AddedBy, s.Client, s.Site, u.Name, u.Position " +
                "FROM TaskLog l " +
                "LEFT JOIN Tasks t ON t.TaskId = l.TaskId " +
                "LEFT JOIN Sites s ON t.SiteReqId = s.SiteId " +
                "LEFT JOIN Users u ON l.AssignId = u.Id " +
                "CROSS JOIN LatestDateStart " +
                "WHERE l.DateStart = LatestDateStart.MaxDateStart " +
                "AND l.Status NOT IN('Complete', 'Pre-approve', 'Pre-request', 'Declined')", con);

            DataSet stask = new DataSet();
            SqlDataAdapter stasks = new SqlDataAdapter(cmd);
            stasks.Fill(stask, "slist");
            ViewBag.variation = stask.Tables[0];

            cmd = new SqlCommand("WITH LatestDateStart AS (" +
                "SELECT MAX(TaskLog.DateStart) AS MaxDateStart " +
                "FROM TaskLog LEFT JOIN Tasks on Tasks.TaskId = TaskLog.TaskId " +
                "WHERE TaskLog.Status NOT IN ('Pre-approve', 'Pre-request') " +
                "AND (Tasks.TaskType = 'Query')) " +
                "SELECT l.LogId, l.TaskId, l.AssignId, l.Status, l.DateStart, l.DateFwd, l.DateRcv, l.DateClr, " +
                "t.Task, t.TaskType, t.Details, t.Description, t.SiteReqId, t.AddedBy, s.Client, s.Site, " +
                "u.Name, u.Position, l.LogId, l.TaskId, l.AssignId, l.Status, l.DateStart, l.DateFwd, l.DateRcv, " +
                "l.DateClr, t.Task, t.TaskType, t.Details, t.Description, " +
                "t.SiteReqId, t.AddedBy, s.Client, s.Site, u.Name, u.Position " +
                "FROM TaskLog l " +
                "LEFT JOIN Tasks t ON t.TaskId = l.TaskId " +
                "LEFT JOIN Sites s ON t.SiteReqId = s.SiteId " +
                "LEFT JOIN Users u ON l.AssignId = u.Id " +
                "CROSS JOIN LatestDateStart " +
                "WHERE l.DateStart = LatestDateStart.MaxDateStart " +
                "AND l.Status NOT IN('Complete', 'Pre-approve', 'Pre-request', 'Declined')", con);

            DataSet ftask = new DataSet();
            SqlDataAdapter ftasks = new SqlDataAdapter(cmd);
            ftasks.Fill(ftask, "slist");
            ViewBag.Query = ftask.Tables[0];

            cmd = new SqlCommand("WITH LatestDateStart AS (" +
                "SELECT MAX(TaskLog.DateStart) AS MaxDateStart " +
                "FROM TaskLog LEFT JOIN Tasks on Tasks.TaskId = TaskLog.TaskId " +
                "WHERE TaskLog.Status NOT IN ('Pre-approve', 'Pre-request') " +
                "AND (Tasks.TaskType = 'Document Request')) " +
                "SELECT l.LogId, l.TaskId, l.AssignId, l.Status, l.DateStart, l.DateFwd, l.DateRcv, l.DateClr, " +
                "t.Task, t.TaskType, t.Details, t.Description, t.SiteReqId, t.AddedBy, s.Client, s.Site, " +
                "u.Name, u.Position, l.LogId, l.TaskId, l.AssignId, l.Status, l.DateStart, l.DateFwd, l.DateRcv, " +
                "l.DateClr, t.Task, t.TaskType, t.Details, t.Description, " +
                "t.SiteReqId, t.AddedBy, s.Client, s.Site, u.Name, u.Position " +
                "FROM TaskLog l " +
                "LEFT JOIN Tasks t ON t.TaskId = l.TaskId " +
                "LEFT JOIN Sites s ON t.SiteReqId = s.SiteId " +
                "LEFT JOIN Users u ON l.AssignId = u.Id " +
                "CROSS JOIN LatestDateStart " +
                "WHERE l.DateStart = LatestDateStart.MaxDateStart " +
                "AND l.Status NOT IN('Complete', 'Pre-approve', 'Pre-request', 'Declined')", con);
            DataSet rtask = new DataSet();
            SqlDataAdapter rtasks = new SqlDataAdapter(cmd);
            rtasks.Fill(rtask, "slist");
            ViewBag.docreq = rtask.Tables[0];


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
        public IActionResult Sites()
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
            cmd = new SqlCommand("SELECT s.SiteId, s.Client, s.Site, s.DateAdded, s.Status, s.SiteOM, om.Name AS OM, som.Name AS SOM, sc.Name AS SC, tk.Name AS TK " +
                "FROM Sites s " +
                "LEFT JOIN Users om " +
                "ON s.SiteOM = om.Id " +
                "LEFT JOIN Users som " +
                "ON s.SiteSOM = som.Id " +
                "LEFT JOIN Users sc " +
                "ON s.SiteSC = sc.Id " +
                "LEFT JOIN Users tk " +
                "ON s.SiteTK = tk.Id " +
                "WHERE s.Status = 'Active'", con);
            DataSet sites = new DataSet();
            SqlDataAdapter ssite = new SqlDataAdapter(cmd);
            ssite.Fill(sites, "slist");

            ViewBag.Sitelist = sites.Tables[0];

            cmd = new SqlCommand("SELECT TaskId, Task, Details, Description, SiteReqId FROM Tasks", con);
            DataSet vars = new DataSet();
            SqlDataAdapter vvars = new SqlDataAdapter(cmd);
            vvars.Fill(vars, "varie");

            ViewBag.Tasklist = vars.Tables[0];

            cmd = new SqlCommand("SELECT u.Id, u.Name, u.Position, p.Usertype FROM Users u LEFT JOIN Positions p ON u.Position = p.Position WHERE u.Status = 'Active'", con);
            DataSet user = new DataSet();
            SqlDataAdapter uuser = new SqlDataAdapter(cmd);
            uuser.Fill(user, "ulist");

            ViewBag.Userlist = user.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult AddSite()
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

            cmd = new SqlCommand("SELECT u.Id, u.Name, u.Position, p.Usertype FROM Users u LEFT JOIN Positions p ON u.Position = p.Position WHERE u.Status = 'Active'", con);
            DataSet user = new DataSet();
            SqlDataAdapter uuser = new SqlDataAdapter(cmd);
            uuser.Fill(user, "ulist");

            ViewBag.Userlist = user.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
        public IActionResult AddSite(SitesModel sm)
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
            if (ModelState.IsValid)
            {
                cmd = new SqlCommand("SELECT COUNT(Client) FROM Sites WHERE Client = '" + sm.Client + "' AND Status != 'Declined'", con);
                int ccount = Convert.ToInt32(cmd.ExecuteScalar());
                if (ccount > 0)
                {
                    sm.Clientcheck = sm.Client;
                    ModelState.AddModelError("Client", "Client is already registered");

                    cmd = new SqlCommand("SELECT u.Id, u.Name, u.Position, p.Usertype FROM Users u LEFT JOIN Positions p ON u.Position = p.Position", con);
                    DataSet user = new DataSet();
                    SqlDataAdapter uuser = new SqlDataAdapter(cmd);
                    uuser.Fill(user, "ulist");

                    ViewBag.Userlist = user.Tables[0];

                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, errors = errors });
                    //return RedirectToAction("Sites", "Admin", sm);
                }
                else
                {
                    DateTime cdt = DateTime.Now;
                    string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
                    cmd = new SqlCommand("INSERT INTO Sites (Client, Site, SiteOM, DateAdded, Status, SiteSOM, SiteSC, SiteTK) " +
                        "VALUES('" + sm.Client + "', " +
                        "'" + sm.Site + "', " +
                        "'" + sm.SiteOM + "', " +
                        "'" + scdt + "', " +
                        "'Pending', " +
                        "'" + sm.SiteSOM + "', " +
                        "'" + sm.SiteSC + "', " +
                        "'" + sm.SiteTK + "')", con);
                    cmd.ExecuteNonQuery();
                }
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return Json(new { success = ModelState.IsValid });
            //return RedirectToAction("Sites", "Admin");
        }
        public IActionResult AddTask(string siteId)
        {
            if (HttpContext.Session.GetString(SessionType) != "Admin" && HttpContext.Session.GetString(SessionType) != "SAdmin")
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Layout = HttpContext.Session.GetString(SessionLayout);
            ViewData["SessionName"] = HttpContext.Session.GetString(SessionName);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            cmd = new SqlCommand("SELECT SiteId, Client, Site, SiteOM FROM Sites WHERE SiteId = '" + siteId + "'", con);
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

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddTask(VariationModel vars, string siteId)
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

            cmd = new SqlCommand("SELECT SiteId, Client, Site, SiteOM FROM Sites WHERE SiteId = '" + siteId + "'", con);
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

            if (HttpContext.Session.GetString(SessionType) == "SAdmin")
            {
                var (filePath, rawfilename, errorMessage) = await _fileService.UploadFileAsync(vars.UploadFile, "tba");
                if (errorMessage != null)
                {
                    ModelState.AddModelError("UploadFile", errorMessage);
                    return View(vars);
                }

                if (ModelState.IsValid)
                {
                    string sesname = HttpContext.Session.GetString(SessionName);
                    string desc = vars.Description + " " + vars.Descquery + vars.Descdocreq + vars.Descvary;
                    string task = vars.Task.Replace("'", "").Replace("\"", "");
                    string details = vars.Details.Replace("'", "").Replace("\"", "");

                    DateTime cdt = DateTime.Now;
                    string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
                    using (cmd = new SqlCommand("INSERT INTO Tasks (Task, Details, Description, AddedBy, SiteReqId, AssignId, Status, DateStart, DateFwd, DateRcv, Process, Circulation, TaskType) VALUES" +
                        "(@Task, " +
                        "@Details, " +
                        "@Desc, " +
                        "@Sesname, " +
                        "@SiteReqId, " +
                        "@AssignId, " +
                        "'Approved', " +
                        "@Scdt, " +
                        "@Scdt, " +
                        "@Scdt, " +
                        "'Q1', " +
                        "'0', " +
                        "@TaskType); SELECT SCOPE_IDENTITY(), Process FROM Tasks WHERE TaskId = SCOPE_IDENTITY();", con))
                    {
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                        cmd.Parameters.AddWithValue("@Desc", desc);
                        cmd.Parameters.AddWithValue("@Sesname", sesname);
                        cmd.Parameters.AddWithValue("@SiteReqId", vars.SiteReqId);
                        cmd.Parameters.AddWithValue("@AssignId", vars.AssignId);
                        cmd.Parameters.AddWithValue("@Scdt", scdt);
                        cmd.Parameters.AddWithValue("@TaskType", vars.Description);
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
                        return View(vars);
                    }

                    DateTime fdt = DateTime.ParseExact(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    string fname = rawfilename;

                    var pcgFile = new Files
                    {
                        FileName = datePart + newname + fileExtension,
                        FileAlias = vars.FileAlias,
                        TaskId = int.Parse(taskId),
                        Status = "Active",
                        AddedBy = int.Parse(addedby),
                        DateAdded = fdt
                    };
                    _pcgdb.Files.Add(pcgFile);
                    _pcgdb.SaveChanges();

                    using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateStart, DateFwd, DateRcv, Status, Process, Task, Details, Circulation) " +
                        "VALUES (@TaskId, " +
                        "@AssignId, " +
                        "@DateStart, " +
                        "@DateFwd, " +
                        "@DateRcv, " +
                        "'Approved', " +
                        "@Process, " +
                        "@Task, " +
                        "@Details, " +
                        "'0')", con))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        cmd.Parameters.AddWithValue("@AssignId", vars.AssignId);
                        cmd.Parameters.AddWithValue("@DateStart", scdt);
                        cmd.Parameters.AddWithValue("@DateFwd", scdt);
                        cmd.Parameters.AddWithValue("@DateRcv", scdt);
                        cmd.Parameters.AddWithValue("@Process", process);
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                    }
                    cmd.ExecuteNonQuery();
                    return Json(new { success = true, redirectUrl = Url.Action("Sites", "Admin") });
                }
            }
            else
            {
                var (filePath, rawfilename, errorMessage) = await _fileService.UploadFileAsync(vars.UploadFile, "tba");
                if (errorMessage != null)
                {
                    ModelState.AddModelError("UploadFile", errorMessage);
                    return View(vars);
                }

                if (ModelState.IsValid)
                {
                    string sesname = HttpContext.Session.GetString(SessionName);
                    string desc = vars.Description + " " + vars.Descquery + vars.Descdocreq + vars.Descvary;
                    string task = vars.Task.Replace("'", "").Replace("\"", "");
                    string details = vars.Details.Replace("'", "").Replace("\"", "");

                    DateTime cdt = DateTime.Now;
                    string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
                    using (cmd = new SqlCommand("INSERT INTO Tasks (Task, Details, Description, AddedBy, SiteReqId, AssignId, Status, DateStart, Process, Circulation, TaskType) VALUES " +
                        "(@Task, " +
                        "@Details, " +
                        "@Desc, " +
                        "@Sesname, " +
                        "@SiteReqId, " +
                        "@AssignId, " +
                        "'Waiting', " +
                        "@Scdt, " +
                        "'Q1', " +
                        "'0', " +
                        "@TaskType ); SELECT SCOPE_IDENTITY(), Process FROM Tasks WHERE TaskId = SCOPE_IDENTITY();", con))
                    {
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                        cmd.Parameters.AddWithValue("@Desc", desc);
                        cmd.Parameters.AddWithValue("@Sesname", sesname);
                        cmd.Parameters.AddWithValue("@SiteReqId", vars.SiteReqId);
                        cmd.Parameters.AddWithValue("@AssignId", vars.AssignId);
                        cmd.Parameters.AddWithValue("@Scdt", scdt);
                        cmd.Parameters.AddWithValue("@TaskType", vars.Description);
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
                        return View(vars);
                    }

                    DateTime fdt = DateTime.ParseExact(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    string fname = rawfilename;

                    var pcgFile = new Files
                    {
                        FileName = datePart + newname + fileExtension,
                        FileAlias = vars.FileAlias,
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
                        "'Waiting', " +
                        "@Process, " +
                        "@Task, " +
                        "@Details, " +
                        "'0')", con))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        cmd.Parameters.AddWithValue("@AssignId", vars.AssignId);
                        cmd.Parameters.AddWithValue("@DateStart", scdt);
                        cmd.Parameters.AddWithValue("@Process", process);
                        cmd.Parameters.AddWithValue("@Task", task);
                        cmd.Parameters.AddWithValue("@Details", details);
                    }
                    cmd.ExecuteNonQuery();
                    return Json(new { success = true, redirectUrl = Url.Action("Sites", "Admin") });
                }
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Sites", "Admin");
        }
        public IActionResult TaskDetail(SitesModel s)
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
            string sesname = HttpContext.Session.GetString(SessionName);
            cmd = new SqlCommand("SELECT t.TaskId AS TaskID_Tasks," +
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
                " s.Status AS SiteStatus," +
                " u.Id AS Id_user, " +
                " u.Name AS Name, " +
                " u.Position FROM Tasks t " +
                "LEFT JOIN Users u " +
                "ON t.AssignId = u.Id " +
                "LEFT JOIN Sites s " +
                "ON t.SiteReqId = s.SiteId " +
                "WHERE t.SiteReqId = '" + s.SiteId + "' AND s.Status = 'Active' " +
                "ORDER BY t.TaskId DESC", con);
            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "slist");

            ViewBag.Tasklog = task.Tables[0];

            cmd = new SqlCommand("SELECT Client, Site FROM Sites WHERE SiteId = '" + s.SiteId + "'", con);

            DataSet site = new DataSet();
            SqlDataAdapter sites = new SqlDataAdapter(cmd);
            sites.Fill(site, "sitelist");

            ViewBag.Sitename = site.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult TaskForward(string taskId)
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
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            cmd = new SqlCommand("SELECT t.TaskId, t.Task, t.Details, t.Description, t.AssignId, t.DateFwd, u.Name, u.Position, s.SiteSC , s.SiteTK " +
                "FROM Tasks t " +
                "LEFT JOIN Sites s on t.SiteReqId = s.SiteId " +
                "LEFT JOIN Users u " +
                "ON t.AssignId = u.Id " +
                "WHERE t.TaskId = '" + taskId + "'", con);
            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "slist");
            ViewBag.Tasklog = task.Tables[0];

            string sesname = HttpContext.Session.GetString(SessionName);
            cmd = new SqlCommand("SELECT * FROM Users WHERE Name != '" + sesname + "' AND Status = 'Active'", con);
            DataSet user = new DataSet();
            SqlDataAdapter users = new SqlDataAdapter(cmd);
            users.Fill(user, "ulist");

            ViewBag.Userlist = user.Tables[0];

            cmd = new SqlCommand("SELECT * FROM Taskprocess WHERE Code NOT IN ('Q8', 'Q9');", con);
            DataSet proc = new DataSet();
            SqlDataAdapter proce = new SqlDataAdapter(cmd);
            proce.Fill(proc, "ulist");

            ViewBag.Process = proc.Tables[0];
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
        public IActionResult TaskForward(ProcessModel vm, string taskId)
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
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);

            cmd = new SqlCommand("SELECT Circulation FROM Tasks WHERE TaskId = '" + vm.TaskId + "'", con);
            int count = (int)cmd.ExecuteScalar();
            string task = vm.Task.Replace("'", "").Replace("\"", "");
            string details = vm.Details.Replace("'", "").Replace("\"", "");
            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
            string sc;
            string tk;

            if (vm.Code == "Q7" || vm.Code == "Q8")
            {
                string assignId = vm.AssignId;

                if (vm.Code == "Q8")
                {
                    assignId = !string.IsNullOrWhiteSpace(vm.SiteSC) ? vm.SiteSC : vm.SiteTK;
                }
                using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateFwd, Status, Process, Task, Details, Circulation) VALUES " +
                                            "(@TaskId, @AssignId, @Scdt, 'Approved', @Code, @Task, @Details, @Count)", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", vm.TaskId);
                    cmd.Parameters.AddWithValue("@AssignId", assignId);
                    cmd.Parameters.AddWithValue("@Scdt", scdt);
                    cmd.Parameters.AddWithValue("@Code", vm.Code);
                    cmd.Parameters.AddWithValue("@Task", task);
                    cmd.Parameters.AddWithValue("@Details", details);
                    cmd.Parameters.AddWithValue("@Count", count);
                    cmd.ExecuteNonQuery();
                }
                using (cmd = new SqlCommand("UPDATE Tasks SET Status = 'Approved', DateFwd = @Scdt, AssignId = @AssignId, Process = @Code WHERE TaskId = @TaskId", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", vm.TaskId);
                    cmd.Parameters.AddWithValue("@AssignId", assignId);
                    cmd.Parameters.AddWithValue("@Scdt", scdt);
                    cmd.Parameters.AddWithValue("@Code", vm.Code);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateFwd, Status, Process, Task, Details, Circulation) VALUES " +
                                            "(@TaskId, @AssignId, @Scdt, 'Pending', @Code, @Task, @Details, @Count)", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", vm.TaskId);
                    cmd.Parameters.AddWithValue("@AssignId", vm.FwdId);
                    cmd.Parameters.AddWithValue("@Scdt", scdt);
                    cmd.Parameters.AddWithValue("@Code", vm.Code);
                    cmd.Parameters.AddWithValue("@Task", task);
                    cmd.Parameters.AddWithValue("@Details", details);
                    cmd.Parameters.AddWithValue("@Count", count);
                    cmd.ExecuteNonQuery();
                }

                using (cmd = new SqlCommand("UPDATE Tasks SET Status = 'Pending', DateFwd = @Scdt, ForwardId = @FwdId, Process = @Code WHERE TaskId = @TaskId", con))
                {
                    cmd.Parameters.AddWithValue("@TaskId", vm.TaskId);
                    cmd.Parameters.AddWithValue("@FwdId", vm.FwdId);
                    cmd.Parameters.AddWithValue("@Scdt", scdt);
                    cmd.Parameters.AddWithValue("@Code", vm.Code);
                    cmd.ExecuteNonQuery();
                }
            }


            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("MyTask", "Admin");
        }
        public IActionResult Pending()
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
            string sesname = HttpContext.Session.GetString(SessionName);
            if (HttpContext.Session.GetString(SessionType) == "SAdmin")
            {
                cmd = new SqlCommand("SELECT t.TaskId AS TaskID_Tasks, " +
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
                    " uf.Id AS Idf, " +
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
                    "WHERE t.Status = 'Waiting' OR t.Status = 'SOM-approve' " +
                    "ORDER BY TaskId DESC", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Tasklog = task.Tables[0];

                cmd = new SqlCommand("SELECT u.Id, u.Name, u.Position,u.Status, p.UserType FROM Users u LEFT JOIN Positions p ON p.Position = u.Position WHERE p.UserType = 'SAdmin' OR p.UserType = 'Admin' AND u.Status = 'Active'", con);

                DataSet user = new DataSet();
                SqlDataAdapter users = new SqlDataAdapter(cmd);
                users.Fill(user, "slist");

                ViewBag.Users = user.Tables[0];
            }
            else
            {
                cmd = new SqlCommand("SELECT t.TaskId AS TaskID_Tasks, " +
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
                    " uf.Id AS Idf, " +
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
                    "WHERE t.Status = 'Waiting'" +
                    "AND " +
                    "ua.Name = '" + sesname + "'", con);
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
        public IActionResult Forwarded()
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
            string sesname = HttpContext.Session.GetString(SessionName);
            if (HttpContext.Session.GetString(SessionType) == "SAdmin")
            {
                cmd = new SqlCommand("SELECT" +
                        " t.TaskId," +
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
                        " (SELECT Task FROM Tasklog tl WHERE tl.TaskId = t.TaskId AND tl.DateFwd = (SELECT MAX(DateFwd) FROM Tasklog WHERE TaskId = t.TaskId)) AS TaskRename," +
                        " (SELECT Details FROM Tasklog tl WHERE tl.TaskId = t.TaskId AND tl.DateFwd = (SELECT MAX(DateFwd) FROM Tasklog WHERE TaskId = t.TaskId)) AS DetailsRename," +
                        " s.Client," +
                        " s.Site," +
                        " uf.Id AS Idf," +
                        " uf.Name AS Namef," +
                        " uf.Position AS Positionf," +
                        " ua.Id AS Ida," +
                        " ua.Name AS Namea," +
                        " ua.Position AS Positiona," +
                        " pf.UserType AS UserTypepf," +
                        " pa.UserType AS UserTypepa " +
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
                        "WHERE t.Status = 'Pending'", con);

                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Tasklog = task.Tables[0];
            }
            else
            {
                cmd = new SqlCommand("SELECT" +
                        " t.TaskId," +
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
                        " (SELECT Task FROM Tasklog tl WHERE tl.TaskId = t.TaskId AND tl.DateFwd = (SELECT MAX(DateFwd) FROM Tasklog WHERE TaskId = t.TaskId)) AS TaskRename," +
                        " (SELECT Details FROM Tasklog tl WHERE tl.TaskId = t.TaskId AND tl.DateFwd = (SELECT MAX(DateFwd) FROM Tasklog WHERE TaskId = t.TaskId)) AS DetailsRename," +
                        " s.Client," +
                        " s.Site," +
                        " uf.Id AS Idf," +
                        " uf.Name AS Namef," +
                        " uf.Position AS Positionf," +
                        " ua.Id AS Ida," +
                        " ua.Name AS Namea," +
                        " ua.Position AS Positiona," +
                        " pf.UserType AS UserTypepf," +
                        " pa.UserType AS UserTypepa " +
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
                        "WHERE t.Status = 'Pending' " +
                        "AND " +
                        "uf.Name = '" + sesname + "'", con);
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
        [HttpPost]
        public IActionResult Approve(ProcessModel pm)
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

            cmd = new SqlCommand("SELECT Circulation FROM Tasks WHERE TaskId = '" + pm.TaskId + "'", con);
            int count = (int)cmd.ExecuteScalar();
            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");

            if (int.Parse(pm.FwdId) == 0)
            {
                if (pm.Status == "Pre-approve")
                {
                    cmd = new SqlCommand("UPDATE Tasks SET Status = 'Approved', AssignId = '" + pm.AssignId + "', DateStart = '" + scdt + "',DateFwd = '" + scdt + "', DateRcv = '" + scdt + "', Process = '" + pm.Code + "', Circulation = '" + count + "' WHERE TaskId = " + pm.TaskId, con);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateStart, DateFwd, DateRcv, Status, Process, Task, Details, Circulation) VALUES ('" + pm.TaskId + "', '" + pm.AssignId + "', '" + scdt + "', '" + scdt + "', '" + scdt + "', 'Approved', '" + pm.Code + "', '" + pm.Task + "', '" + pm.Details + "', '" + count + "')", con);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd = new SqlCommand("UPDATE Tasks SET Status = 'Approved', AssignId = '" + pm.AssignId + "',  DateRcv = '" + scdt + "', Process = '" + pm.Code + "', Circulation = '" + count + "' WHERE TaskId = " + pm.TaskId, con);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateRcv, Status, Process, Task, Details, Circulation) VALUES ('" + pm.TaskId + "', '" + pm.AssignId + "', '" + scdt + "', 'Approved', '" + pm.Code + "', '" + pm.Task + "', '" + pm.Details + "', '" + count + "')", con);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                cmd = new SqlCommand("UPDATE Tasks SET Status = 'Approved', DateRcv = '" + scdt + "', AssignId = '" + pm.FwdId + "', Process = '" + pm.Code + "', Task = '" + pm.Task + "', Details = '" + pm.Details + "', Circulation = '" + count + "' WHERE TaskId = " + pm.TaskId, con);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateRcv, Status, Process, Task, Details, Circulation) VALUES ('" + pm.TaskId + "', '" + pm.FwdId + "', '" + scdt + "', 'Approved', '" + pm.Code + "', '" + pm.Task + "', '" + pm.Details + "', '" + count + "')", con);
                cmd.ExecuteNonQuery();
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Mytask", "Admin");
        }
        public IActionResult Decline(ProcessModel pm, string taskId)
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
            try
            {
                cmd = new SqlCommand("SELECT t.TaskId AS TaskID_Tasks, " +
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
                        " uf.Id AS Idf, " +
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
                        "WHERE t.Status IN ('Waiting', 'SOM-approve', 'Pending') AND t.TaskId = " + taskId + " " +
                        "ORDER BY TaskId DESC", con);

                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Taskdecline = task.Tables[0];

                return PartialView("actionmodal/_Decline", pm);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Forward action: " + ex.Message);
                return StatusCode(500);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        [HttpPost]
        public IActionResult Decline(ProcessModel pm)
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
            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
            cmd = new SqlCommand("UPDATE Tasks SET DateClr = '" + scdt + "', Status = 'Declined', Process = '" + pm.Code + "', Comment = '" + pm.Comment + "' WHERE TaskId = " + pm.TaskId, con);
            cmd.ExecuteNonQuery();

            using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateClr, Status, Process, Task, Details, Circulation) " +
                "VALUES (@TaskId, " +
                "@AssignId, " +
                "@Scdt, " +
                "'Declined', " +
                "@Code, " +
                "@Task, " +
                "@Details, " +
                "'0')", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@AssignId", pm.AssignId);
                cmd.Parameters.AddWithValue("@Scdt", scdt);
                cmd.Parameters.AddWithValue("@Task", pm.Task);
                cmd.Parameters.AddWithValue("@Code", pm.Code);
                cmd.Parameters.AddWithValue("@Details", pm.Details);
            }
            cmd.ExecuteNonQuery();

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Pending", "Admin");
        }
        [HttpPost]
        public IActionResult Complete(ProcessModel pm)
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
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            string task = pm.Task.Replace("'", "").Replace("\"", "");
            string details = pm.Details.Replace("'", "").Replace("\"", "");
            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");

            cmd = new SqlCommand("UPDATE Tasks SET Status = 'Complete', DateClr = '" + scdt + "' WHERE TaskId = " + pm.TaskId, con);
            cmd.ExecuteNonQuery();

            using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, Task, Details, AssignId, DateClr, Status, Process) " +
                "VALUES (@TaskId, " +
                "@Task, " +
                "@Details, " +
                "@AssignId, " +
                "@Scdt, " +
                "'Complete', " +
                "'Q7');", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@Task", task);
                cmd.Parameters.AddWithValue("@Details", details);
                cmd.Parameters.AddWithValue("@AssignId", pm.AssignId);
                cmd.Parameters.AddWithValue("@Scdt", scdt);
            }
            cmd.ExecuteNonQuery();

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Pending", "Admin");
        }
        public IActionResult SendToSite(ProcessModel pm)
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
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
            ViewData["SessionType"] = HttpContext.Session.GetString(SessionType);
            string task = pm.Task.Replace("'", "").Replace("\"", "");
            string details = pm.Details.Replace("'", "").Replace("\"", "");
            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");

            cmd = new SqlCommand("UPDATE Tasks SET Status = 'Complete', DateClr = '" + scdt + "' WHERE TaskId = " + pm.TaskId, con);
            cmd.ExecuteNonQuery();

            using (cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, Task, Details, AssignId, DateClr, Status, Process) " +
                "VALUES (@TaskId, " +
                "@Task, " +
                "@Details, " +
                "@AssignId, " +
                "@Scdt, " +
                "'Complete', " +
                "'Q7');", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", pm.TaskId);
                cmd.Parameters.AddWithValue("@Task", task);
                cmd.Parameters.AddWithValue("@Details", details);
                cmd.Parameters.AddWithValue("@AssignId", pm.AssignId);
                cmd.Parameters.AddWithValue("@Scdt", scdt);
            }
            cmd.ExecuteNonQuery();

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Pending", "Admin");
        }
        [HttpPost]
        public IActionResult Revise(ProcessModel pm)
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

            DateTime cdt = DateTime.Now;
            string scdt = cdt.ToString("yyyy-MM-dd HH:mm:ss");
            cmd = new SqlCommand("SELECT Circulation FROM Tasks WHERE TaskId = '" + pm.TaskId + "'", con);
            int count = (int)cmd.ExecuteScalar() + 1;

            cmd = new SqlCommand("INSERT INTO Tasklog (TaskId, AssignId, DateFwd, Status, Process, Task, Details, Circulation) VALUES ('" + pm.TaskId + "', '" + pm.FwdId + "', '" + scdt + "', 'Pending', '" + pm.Code + "', '" + pm.Task + "', '" + pm.Details + "', '" + count + "')", con);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand("UPDATE Tasks SET Status = 'Pending', ForwardId = '" + pm.FwdId + "', Task = '" + pm.Task + "', Details = '" + pm.Details + "', DateRcv = '" + scdt + "', Process = '" + pm.Code + "', Circulation = '" + count + "' WHERE TaskId = " + pm.TaskId, con);
            cmd.ExecuteNonQuery();

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("MyTask", "Admin");
        }
        public IActionResult History()
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

            cmd = new SqlCommand("SELECT l.LogId, " +
                "l.TaskId, " +
                "l.AssignId, " +
                "l.Status, " +
                "l.Task, " +
                "l.DateStart, " +
                "l.DateFwd, " +
                "l.DateRcv, " +
                "l.DateClr, " +
                "p.Process, " +
                "t.SiteReqId, " +
                "t.AddedBy, " +
                "s.Client, " +
                "s.Site, " +
                "u.Name, " +
                "u.Position " +
                "FROM Tasklog l " +
                "LEFT JOIN Tasks t ON t.TaskId = l.TaskId " +
                "LEFT JOIN Sites s ON t.SiteReqId = s.SiteId " +
                "LEFT JOIN Users u ON l.AssignId = u.Id " +
                "LEFT JOIN Taskprocess p ON l.Process = p.Code " +
                "ORDER BY LogId DESC", con);
            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "slist");
            ViewBag.Tasklog = task.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult MyTask()
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
            ViewData["SessionId"] = HttpContext.Session.GetString(SessionId);
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
                " p.Code," +
                " s.Client," +
                " s.Site," +
                " s.SiteSC," +
                " s.SiteTK," +
                " u.Id AS Id_user, " +
                " u.Name AS Name, " +
                " u.Position FROM Tasks t " +
                "LEFT JOIN Users u " +
                "ON t.AssignId = u.Id " +
                "LEFT JOIN Sites s " +
                "ON t.SiteReqId = s.SiteId " +
                "LEFT JOIN Taskprocess p " +
                "ON t.Process = p.Code " +
                "WHERE t.Status = 'Approved' AND u.Name = '" + sesname + "' " +
                "ORDER BY t.TaskId DESC", con);
            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "slist");

            ViewBag.Tasklog = task.Tables[0];

            cmd = new SqlCommand("SELECT Code, Process FROM Taskprocess WHERE Code NOT IN ('Q8', 'Q9')", con);
            DataSet proc = new DataSet();
            SqlDataAdapter procs = new SqlDataAdapter(cmd);
            procs.Fill(proc, "proc");

            ViewBag.Process = proc.Tables[0];

            cmd = new SqlCommand("SELECT * FROM Users WHERE Name != '" + sesname + "' AND Status = 'Active' AND Position = 'Site Coordinator'", con);
            DataSet coor = new DataSet();
            SqlDataAdapter coord = new SqlDataAdapter(cmd);
            coord.Fill(coor, "clist");

            ViewBag.Coord = coor.Tables[0];

            cmd = new SqlCommand("SELECT u.Id, u.Name, u.Position,u.Status, p.UserType FROM Users u LEFT JOIN Positions p ON p.Position = u.Position WHERE p.UserType = 'SAdmin' OR p.UserType = 'Admin' AND u.Status = 'Active' AND u.Name != '" + sesname + "'", con);
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
        public IActionResult SiteStatus()
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
            cmd = new SqlCommand("SELECT s.SiteId, s.Client, s.Site, s.DateAdded, s.Status, s.SiteOM, u.Name FROM Sites s LEFT JOIN Users u ON s.SiteOM = u.Id " +
                "WHERE s.Status = 'Active' OR s.Status = 'Inactive'", con);
            DataSet sites = new DataSet();
            SqlDataAdapter ssite = new SqlDataAdapter(cmd);
            ssite.Fill(sites, "slist");

            ViewBag.Sitelist = sites.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
        public IActionResult SiteActivate(SitesModel sm)
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
            cmd = new SqlCommand("UPDATE Sites SET Status = 'Active' WHERE SiteId = '" + sm.SiteId + "'", con);
            cmd.ExecuteNonQuery();
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("SiteStatus", "Admin");
        }
        [HttpPost]
        public IActionResult SiteDeactivate(SitesModel sm)
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
            cmd = new SqlCommand("UPDATE Sites SET Status = 'Inactive' WHERE SiteId = '" + sm.SiteId + "'", con);
            cmd.ExecuteNonQuery();
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("SiteStatus", "Admin");
        }
        public IActionResult SitePending()
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
            cmd = new SqlCommand("SELECT s.SiteId, s.Client, s.Site, s.DateAdded, s.Status, s.SiteOM, u.Name FROM Sites s LEFT JOIN Users u ON s.SiteOM = u.Id " +
                "WHERE s.Status = 'Pending'", con);
            DataSet sites = new DataSet();
            SqlDataAdapter ssite = new SqlDataAdapter(cmd);
            ssite.Fill(sites, "slist");

            ViewBag.Sitelist = sites.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        [HttpPost]
        public IActionResult SiteApproval(SitesModel sm, int status)
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
            if (status == 0)
            {
                cmd = new SqlCommand("UPDATE Sites SET Status = 'Active'" +
                    "WHERE SiteId = '" + sm.SiteId + "'", con);
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = new SqlCommand("UPDATE Sites SET Status = 'Declined'" +
                    "WHERE SiteId = '" + sm.SiteId + "'", con);
                cmd.ExecuteNonQuery();
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return RedirectToAction("Sites", "Admin");
        }
        public IActionResult VarTask()
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

            cmd = new SqlCommand("SELECT" +
                    " t.TaskId," +
                    " CONCAT(t.Task, ' ', t.Description) AS TaskInfo, " +
                    " t.DateStart," +
                    " t.DateRcv," +
                    " t.Circulation," +
                    " t.TaskType," +
                    " (SELECT MAX(l1.DateRcv) FROM Tasklog l1 WHERE l1.Process = 'Q1' AND l1.TaskId = t.TaskId ) AS DateRcvQ1," +
                    " (SELECT MAX(l2.DateFwd) FROM Tasklog l2 WHERE l2.Process = 'Q2' AND l2.TaskId = t.TaskId) AS DateFwdQ2," +
                    " (SELECT MAX(l2.DateRcv) FROM Tasklog l2 WHERE l2.Process = 'Q2' AND l2.TaskId = t.TaskId) AS DateRcvQ2," +
                    " (SELECT MAX(l3.DateFwd) FROM Tasklog l3 WHERE l3.Process = 'Q3' AND l3.TaskId = t.TaskId) AS DateFwdQ3," +
                    " (SELECT MAX(l3.DateRcv) FROM Tasklog l3 WHERE l3.Process = 'Q3' AND l3.TaskId = t.TaskId) AS DateRcvQ3," +
                    " (SELECT MAX(l4.DateFwd) FROM Tasklog l4 WHERE l4.Process = 'Q4' AND l4.TaskId = t.TaskId) AS DateFwdQ4," +
                    " (SELECT MAX(l4.DateRcv) FROM Tasklog l4 WHERE l4.Process = 'Q4' AND l4.TaskId = t.TaskId) AS DateRcvQ4," +
                    " (SELECT MAX(l5.DateFwd) FROM Tasklog l5 WHERE l5.Process = 'Q5' AND l5.TaskId = t.TaskId) AS DateFwdQ5," +
                    " (SELECT MAX(l5.DateRcv) FROM Tasklog l5 WHERE l5.Process = 'Q5' AND l5.TaskId = t.TaskId) AS DateRcvQ5," +
                    " (SELECT MAX(l6.DateFwd) FROM Tasklog l6 WHERE l6.Process = 'Q6' AND l6.TaskId = t.TaskId) AS DateFwdQ6," +
                    " (SELECT MAX(l6.DateRcv) FROM Tasklog l6 WHERE l6.Process = 'Q6' AND l6.TaskId = t.TaskId) AS DateRcvQ6," +
                    " (SELECT MAX(l7.DateFwd) FROM Tasklog l7 WHERE l7.Process = 'Q7' AND l7.TaskId = t.TaskId) AS DateFwdQ7," +
                    " (SELECT MAX(l7.DateRcv) FROM Tasklog l7 WHERE l7.Process = 'Q7' AND l7.TaskId = t.TaskId) AS DateRcvQ7," +
                    " (SELECT MAX(l8.DateFwd) FROM Tasklog l8 WHERE l8.Process = 'Q8' AND l8.TaskId = t.TaskId) AS DateFwdQ8," +
                    " (SELECT MAX(l8.DateRcv) FROM Tasklog l8 WHERE l8.Process = 'Q8' AND l8.TaskId = t.TaskId) AS DateRcvQ8," +
                    " u.Name," +
                    " s.Client," +
                    " s.Site," +
                    " a.Name," +
                    " a.Position," +
                    " p.UserType " +
                    " FROM" +
                    " Tasks t" +
                    " LEFT JOIN" +
                    " Users u ON u.Id = t.AssignId" +
                    " LEFT JOIN" +
                    " Sites s ON s.SiteId = t.SiteReqId" +
                    " LEFT JOIN" +
                    " Users a ON a.Name = t.AddedBy" +
                    " LEFT JOIN" +
                    " Positions p ON p.Position = a.Position" +
                    " WHERE t.Status NOT IN ('Complete', 'Pre-approve', 'Pre-request', 'Declined') AND (t.TaskType = 'Variation' OR t.TaskType = 'Document Request')" +
                    " ORDER BY" +
                    " t.TaskId DESC; ", con);

            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "slist");
            ViewBag.tlist = task.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }

        public IActionResult QueryTask()
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

            cmd = new SqlCommand("SELECT" +
                    " t.TaskId," +
                    " CONCAT(t.Task, ' ', t.Description) AS TaskInfo, " +
                    " t.DateStart," +
                    " t.DateRcv," +
                    " t.Circulation," +
                    " t.TaskType," +
                    " (SELECT MAX(l1.DateRcv) FROM Tasklog l1 WHERE l1.Process = 'Q1' AND l1.TaskId = t.TaskId ) AS DateRcvQ1," +
                    " (SELECT MAX(l2.DateFwd) FROM Tasklog l2 WHERE l2.Process = 'Q2' AND l2.TaskId = t.TaskId) AS DateFwdQ2," +
                    " (SELECT MAX(l2.DateRcv) FROM Tasklog l2 WHERE l2.Process = 'Q2' AND l2.TaskId = t.TaskId) AS DateRcvQ2," +
                    " (SELECT MAX(l3.DateFwd) FROM Tasklog l3 WHERE l3.Process = 'Q3' AND l3.TaskId = t.TaskId) AS DateFwdQ3," +
                    " (SELECT MAX(l3.DateRcv) FROM Tasklog l3 WHERE l3.Process = 'Q3' AND l3.TaskId = t.TaskId) AS DateRcvQ3," +
                    " (SELECT MAX(l4.DateFwd) FROM Tasklog l4 WHERE l4.Process = 'Q4' AND l4.TaskId = t.TaskId) AS DateFwdQ4," +
                    " (SELECT MAX(l4.DateRcv) FROM Tasklog l4 WHERE l4.Process = 'Q4' AND l4.TaskId = t.TaskId) AS DateRcvQ4," +
                    " (SELECT MAX(l5.DateFwd) FROM Tasklog l5 WHERE l5.Process = 'Q5' AND l5.TaskId = t.TaskId) AS DateFwdQ5," +
                    " (SELECT MAX(l5.DateRcv) FROM Tasklog l5 WHERE l5.Process = 'Q5' AND l5.TaskId = t.TaskId) AS DateRcvQ5," +
                    " (SELECT MAX(l6.DateFwd) FROM Tasklog l6 WHERE l6.Process = 'Q6' AND l6.TaskId = t.TaskId) AS DateFwdQ6," +
                    " (SELECT MAX(l6.DateRcv) FROM Tasklog l6 WHERE l6.Process = 'Q6' AND l6.TaskId = t.TaskId) AS DateRcvQ6," +
                    " (SELECT MAX(l7.DateFwd) FROM Tasklog l7 WHERE l7.Process = 'Q7' AND l7.TaskId = t.TaskId) AS DateFwdQ7," +
                    " (SELECT MAX(l7.DateRcv) FROM Tasklog l7 WHERE l7.Process = 'Q7' AND l7.TaskId = t.TaskId) AS DateRcvQ7," +
                    " (SELECT MAX(l8.DateFwd) FROM Tasklog l8 WHERE l8.Process = 'Q8' AND l8.TaskId = t.TaskId) AS DateFwdQ8," +
                    " (SELECT MAX(l8.DateRcv) FROM Tasklog l8 WHERE l8.Process = 'Q8' AND l8.TaskId = t.TaskId) AS DateRcvQ8," +
                    " u.Name," +
                    " s.Client," +
                    " s.Site," +
                    " a.Name," +
                    " a.Position," +
                    " p.UserType " +
                    " FROM" +
                    " Tasks t" +
                    " LEFT JOIN" +
                    " Users u ON u.Id = t.AssignId" +
                    " LEFT JOIN" +
                    " Sites s ON s.SiteId = t.SiteReqId" +
                    " LEFT JOIN" +
                    " Users a ON a.Name = t.AddedBy" +
                    " LEFT JOIN" +
                    " Positions p ON p.Position = a.Position" +
                    " WHERE t.Status NOT IN ('Complete', 'Pre-approve', 'Pre-request', 'Declined') AND (t.TaskType = 'Query')" +
                    " ORDER BY" +
                    " t.TaskId DESC; ", con);

            DataSet task = new DataSet();
            SqlDataAdapter tasks = new SqlDataAdapter(cmd);
            tasks.Fill(task, "slist");
            ViewBag.tlist = task.Tables[0];

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View();
        }
        public IActionResult TaskDetails(string taskId)
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

            using (cmd = new SqlCommand("SELECT t.TaskId, t.Task, t.Description, t.Details, t.AddedBy, t.SiteReqId, t.Status, " +
                      "t.AssignId, t.ForwardId, p.Process, t.Circulation, t.TaskType, s.Site, s.Client, " +
                      "s.SiteOM, s.SiteSOM, s.SiteSC, s.SiteTK, ua.Name AS Assignee, uf.Name AS Forwarded, om.Name AS OMName, som.Name AS SOMName, " +
                      "sc.Name AS SCName, tk.Name AS TKName " +
                      "FROM Tasks t " +
                      "LEFT JOIN Sites s ON s.SiteId = t.SiteReqId " +
                      "LEFT JOIN Taskprocess p ON t.Process = p.Code " +
                      "LEFT JOIN Users ua ON ua.Id = t.AssignId " +
                      "LEFT JOIN Users uf ON uf.Id = t.ForwardId " +
                      "LEFT JOIN Users om ON om.Id = s.SiteOM " +
                      "LEFT JOIN Users som ON som.Id = s.SiteSOM " +
                      "LEFT JOIN Users sc ON sc.Id = s.SiteSC " +
                      "LEFT JOIN Users tk ON tk.Id = s.SiteTK " +
                      "WHERE t.TaskId = @TaskId", con))
            {
                cmd.Parameters.AddWithValue("@TaskId", taskId);

                try
                {
                    var adapter = new SqlDataAdapter(cmd);
                    var dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                    {
                        var row = dataSet.Tables[0].Rows[0];
                        var detailsModel = new DetailsModel
                        {
                            TaskId = Convert.ToString(row["TaskId"]),
                            Task = Convert.ToString(row["Task"]),
                            Type = Convert.ToString(row["TaskType"]),
                            Description = Convert.ToString(row["Description"]),
                            Details = Convert.ToString(row["Details"]),
                            AddedBy = Convert.ToString(row["AddedBy"]),
                            SiteId = Convert.ToString(row["SiteReqId"]),
                            Site = Convert.ToString(row["Client"]) + " " + Convert.ToString(row["Site"]),
                            Status = Convert.ToString(row["Status"]),
                            Process = Convert.ToString(row["Process"]),
                            AssignId = Convert.ToString(row["Assignee"]),
                            ForwardId = Convert.ToString(row["Forwarded"]),
                            SiteOM = Convert.ToString(row["OMName"]),
                            SiteSOM = Convert.ToString(row["SOMName"]),
                            SiteSC = Convert.ToString(row["SCName"]),
                            SiteTK = Convert.ToString(row["TKName"])
                        };

                        return PartialView("moredetails/_TaskDetails", detailsModel);
                    }
                    else
                    {
                        return Content("Task not found.");
                    }
                }
                catch (Exception ex)
                {
                    return Content($"Error: {ex.Message}");
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
        }

        public IActionResult Forward(ProcessModel pm, string taskId)
        {
            try
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

                cmd = new SqlCommand("SELECT t.TaskId, t.Task, t.Details, t.Description, t.AssignId, t.DateFwd, u.Name, u.Position, s.SiteSC , s.SiteTK " +
                                "FROM Tasks t " +
                                "LEFT JOIN Sites s on t.SiteReqId = s.SiteId " +
                                "LEFT JOIN Users u " +
                                "ON t.AssignId = u.Id " +
                                "WHERE t.TaskId = '" + taskId + "'", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");
                ViewBag.Taskfwd = task.Tables[0];

                string sesname = HttpContext.Session.GetString(SessionName);
                cmd = new SqlCommand("SELECT * FROM Users WHERE Name != '" + sesname + "' AND Status = 'Active'", con);
                DataSet user = new DataSet();
                SqlDataAdapter users = new SqlDataAdapter(cmd);
                users.Fill(user, "ulist");

                ViewBag.Users = user.Tables[0];

                cmd = new SqlCommand("SELECT * FROM Taskprocess WHERE Code != 'Q9'", con);
                DataSet proc = new DataSet();
                SqlDataAdapter proce = new SqlDataAdapter(cmd);
                proce.Fill(proc, "ulist");

                ViewBag.Process = proc.Tables[0];
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                return PartialView("actionmodal/_Forward", pm);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in controller action: " + ex.Message);
                return StatusCode(500);
            }
        }
        public IActionResult Revise(ProcessModel pm, string taskId)
        {
            try
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
                cmd = new SqlCommand("SELECT t.TaskId, t.Task, t.Details, t.Description, t.AssignId, t.DateFwd, u.Name, u.Position " +
                    "FROM Tasks t " +
                    "LEFT JOIN Users u " +
                    "ON t.AssignId = u.Id " +
                    "WHERE t.TaskId = '" + taskId + "'", con);
                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");
                ViewBag.Taskfwd = task.Tables[0];

                string sesname = HttpContext.Session.GetString(SessionName);
                cmd = new SqlCommand("SELECT * FROM Users WHERE Name != '" + sesname + "' AND Status = 'Active'", con);
                DataSet user = new DataSet();
                SqlDataAdapter users = new SqlDataAdapter(cmd);
                users.Fill(user, "ulist");

                ViewBag.Users = user.Tables[0];

                cmd = new SqlCommand("SELECT * FROM Taskprocess WHERE Code != 'Q9'", con);
                DataSet proc = new DataSet();
                SqlDataAdapter proce = new SqlDataAdapter(cmd);
                proce.Fill(proc, "ulist");

                ViewBag.Process = proc.Tables[0];
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                return PartialView("actionmodal/_Revise", pm);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in controller action: " + ex.Message);
                return StatusCode(500);
            }
        }
        public IActionResult DeclinedTask()
        {
            try
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
                string sesid = HttpContext.Session.GetString(SessionId);

                cmd = new SqlCommand("SELECT t.TaskId, t.Task, t.Details, t.Description, s.Client, s.Site, t.AssignId, u.Name, u.Position, t.Comment " +
                            "FROM Tasks t " +
                            "LEFT JOIN Users u ON t.AssignId = u.Id " +
                            "LEFT JOIN Sites s ON t.SiteReqId = s.SiteId " +
                            "WHERE (t.AssignId = '" + sesid + "' AND t.Status = 'Declined') OR (t.AssignId = 0 AND t.Status = 'Declined')" +
                            "ORDER BY t.TaskId DESC", con);

                DataSet task = new DataSet();
                SqlDataAdapter tasks = new SqlDataAdapter(cmd);
                tasks.Fill(task, "slist");

                ViewBag.Declined = task.Tables[0];

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in controller action: " + ex.Message);
                return StatusCode(500);
            }
        }
        [HttpGet]
        public IActionResult Upload(string taskid)
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

            if (string.IsNullOrEmpty(taskid))
            {
                return RedirectToAction("Index", "Admin");
            }

            var task = _pcgdb.Tasks
                        .Where(a => a.TaskId == int.Parse(taskid))
                        .Select(a => new
                        {
                            a.TaskId,
                            a.Task
                        });
            var taskresult = task.FirstOrDefault();

            var fm = new FileModel
            {
                TaskId = taskresult.TaskId,
                Task = taskresult.Task
            };
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return PartialView("actionmodal/_UploadPartial", fm);
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile(FileModel pm, string taskid)
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

            if (pm.FileAlias == null || pm.FileAlias == "")
            {
                ModelState.AddModelError("FileAlias", "Invalid File name input.");
                return PartialView("actionmodal/_UploadPartial", pm);
            }
            var task = _pcgdb.Tasks
                        .Where(a => a.TaskId == int.Parse(taskid))
                        .Select(a => new
                        {
                            a.TaskId
                        });
            var taskresult = task.FirstOrDefault();

            if (taskresult == null)
                {
                    ModelState.AddModelError("TaskId", "Invalid site ID.");
                    return PartialView("actionmodal/_UploadPartial", pm);
                }

            var count = _pcgdb.Files
                        .Where(z => z.TaskId == int.Parse(taskid))
                        .Count();
            int filecount = count + 1;

            string addedby = HttpContext.Session.GetString(SessionId);
            string aaa = "_" + pm.TaskId + "_" + addedby + "_" + filecount;
            var (filePath, rawfilename, errorMessage) = await _fileService.UploadFileAsync(pm.UploadFile, aaa);
            if (errorMessage != null)
            {
                ModelState.AddModelError("UploadFile", errorMessage);
                return PartialView("actionmodal/_UploadPartial", pm);
            }

            DateTime fdt = DateTime.ParseExact(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            string fname = rawfilename;

            var pcgFile = new Files
            {
                FileName = fname,
                FileAlias = pm.FileAlias,
                TaskId = taskresult.TaskId,
                Status = "Active",
                AddedBy = int.Parse(addedby),
                DateAdded = fdt
            };
            _pcgdb.Files.Add(pcgFile);
            _pcgdb.SaveChanges();

            if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            ViewBag.SuccessMessage = "File uploaded successfully.";
            return PartialView("actionmodal/_UploadPartial", pm);
        }
        public IActionResult Downloads(int taskId)
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

            var dlFiles = from f in _pcgdb.Files
                            join t in _pcgdb.Tasks on f.TaskId equals t.TaskId into taskGroup
                            from tg in taskGroup.DefaultIfEmpty() 
                            join u in _pcgdb.Users on f.AddedBy equals u.Id into userGroup
                            from ug in userGroup.DefaultIfEmpty() 
                            where f.TaskId == taskId && f.Status == "Active"
                            select new Downloads
                            {
                                FileId = f.FileId,
                                FileName = f.FileName,
                                FileAlias = f.FileAlias,
                                Status = f.Status,
                                Name = ug.Name,
                                DateAdded = f.DateAdded,
                                Task = tg.Task,
                                TaskId = tg.TaskId
                            };

            ViewBag.Taskid = taskId;

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return View(dlFiles.ToList());
        }
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var fileResult = await _fileService.DownloadFileAsync(fileName);
                return fileResult;
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        public async Task<IActionResult> DeleteFile(string fileName, int fileId, int taskid)
        {
            try
            {
                var fileResult = await _fileService.DeleteFileAsync(fileName);

                var filestatus = _pcgdb.Files.SingleOrDefault(f => f.FileId == fileId);

                if (filestatus != null)
                {
                    filestatus.Status = "Deleted";
                    _pcgdb.SaveChanges();
                }

                return RedirectToAction("Downloads", "Admin", new { taskId = taskid });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
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

            cmd = new SqlCommand("SELECT Id, Name, Position FROM Users WHERE Status = 'Pending'", con);
                
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
            using (cmd = new SqlCommand("Update Users SET Status = @Status WHERE Id = @UserId", con))
            {
                cmd.Parameters.AddWithValue("@Status", stat);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }

            cmd = new SqlCommand("SELECT Id, Name, Position FROM Users WHERE Status = 'Pending'", con);

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
    }    
}       