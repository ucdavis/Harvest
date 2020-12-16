using System;
using System.Collections.Generic;
using System.Text;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectHistory> ProjectHistory { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Document.OnModelCreating(modelBuilder);
            Project.OnModelCreating(modelBuilder);
            Quote.OnModelCreating(modelBuilder);
            Role.OnModelCreating(modelBuilder);
            Team.OnModelCreating(modelBuilder);
            User.OnModelCreating(modelBuilder);
        }
    }
}
