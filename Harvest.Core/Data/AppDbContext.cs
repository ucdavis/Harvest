using System;
using System.Collections.Generic;
using System.Text;
using Harvest.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Core.Data
{
    // Subclasses are a MS-condoned hack to allow defining provider-specific migrations
    // Commands to add migrations are (from within Harvest.Core directory)...
    // dotnet ef migrations add Initial --context AppDbContextSqlite --output-dir Migrations/Sqlite --startup-project ../Harvest.Web/Harvest.Web.csproj -- --provider Sqlite
    // dotnet ef migrations add Initial --context AppDbContextSqlServer --output-dir Migrations/SqlServer --startup-project ../Harvest.Web/Harvest.Web.csproj -- --provider SqlServer
    public sealed class AppDbContextSqlite : AppDbContext<AppDbContextSqlite>
    {
        public AppDbContextSqlite(DbContextOptions<AppDbContextSqlite> options) : base(options)
        {
        }
    }

    public sealed class AppDbContextSqlServer : AppDbContext<AppDbContextSqlServer>
    {
        public AppDbContextSqlServer(DbContextOptions<AppDbContextSqlServer> options) : base(options)
        {
        }
    }

    public abstract class AppDbContext<T> : AppDbContext
        where T : AppDbContext<T>
    {
        protected AppDbContext(DbContextOptions<T> options) : base(options)
        {
        }
    }

    public abstract class AppDbContext: DbContext
    {
        protected AppDbContext(DbContextOptions options) : base(options)
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
            Account.OnModelCreating(modelBuilder);
            Document.OnModelCreating(modelBuilder);
            Notification.OnModelCreating(modelBuilder);
            Permission.OnModelCreating(modelBuilder);
            Project.OnModelCreating(modelBuilder);
            Domain.ProjectHistory.OnModelCreating(modelBuilder);
            Quote.OnModelCreating(modelBuilder);
            Role.OnModelCreating(modelBuilder);
            Team.OnModelCreating(modelBuilder);
            User.OnModelCreating(modelBuilder);
        }
    }
}
