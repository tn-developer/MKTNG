using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace mktng.Models
{
    public class mktngContext : DbContext
    {
        public mktngContext(DbContextOptions<mktngContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        // DbSet for Clients table
        public DbSet<ClientModel> Clients { get; set; }
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
}