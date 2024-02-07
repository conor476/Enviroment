using Enviroment.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Enviroment.Data

{
    public class HelpdeskContext : IdentityDbContext<User>
    {
        public HelpdeskContext(DbContextOptions<HelpdeskContext> options) : base(options)
        {
        }
        //these are the tables in my database
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categorys { get; set; }

       

    }
}