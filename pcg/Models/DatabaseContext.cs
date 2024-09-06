using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace pcg.Models
{
    public class PCGContext : DbContext
    {
        public PCGContext(DbContextOptions<PCGContext> options) : base(options)
        {
        }
        public DbSet<Files> Files { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Taskprocess> Taskprocesses { get; set; }
        public DbSet<Sites> Sites { get; set; }
        public DbSet<Tasklog> Tasklog { get; set; }
    }
    public class Files 
    {
        [Key]
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FileAlias { get; set; }
        public int TaskId { get; set; }
        public string? Status { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
    }
    public class Tasks
    {
        [Key]
        public int TaskId { get; set; }
        public string Task { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string AddedBy { get; set; }
        public int SiteReqId { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateFwd { get; set; }
        public DateTime? DateRcv { get; set; }
        public DateTime? DateClr { get; set; }
        public string Status { get; set; }
        public int AssignId { get; set; }
        public int ForwardId { get; set; }
        public string Process { get; set; }
        public int? Circulation { get; set; }
        public string TaskType { get; set; }
        public string? Comment { get; set; }
    }
    public class Tasklog
    {
        [Key]
        public int LogId { get; set; }
        public int TaskId { get; set; }
        public int AssignId { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateFwd { get; set; }
        public DateTime? DateRcv { get; set; }
        public DateTime? DateClr { get; set; }
        public string Status { get; set; }
        public string Process { get; set; }
        public string Task { get; set; }
        public string Remarks { get; set; }
        public int? Circulation { get; set; }
        public string Details { get; set; }
    }
    public class Taskprocess
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Process { get; set; }
    }
    public class Users 
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }
    }
    public class Sites
    {
        [Key]
        public int SiteId { get; set; }
        public string Client { get; set; }
        public string Site { get; set; }
        public DateTime? DateAdded { get; set; }
        public string Status { get; set; }
        public string? SiteOM { get; set; }
        public string? SiteSOM { get; set; }
        public string? SiteSC { get; set; }
        public string? SiteTK { get; set; }
    }
}