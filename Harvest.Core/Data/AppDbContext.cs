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
    public sealed class AppDbContextSqlite : AppDbContext
    {
        public AppDbContextSqlite(DbContextOptions<AppDbContextSqlite> options) : base(options)
        {
        }
    }

    public sealed class AppDbContextSqlServer : AppDbContext
    {
        public AppDbContextSqlServer(DbContextOptions<AppDbContextSqlServer> options) : base(options)
        {
        }
    }

    public abstract class AppDbContext: DbContext
    {
        protected AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<Field> Fields { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<ProjectAttachment> ProjectAttachments { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectHistory> ProjectHistory { get; set; }
        public virtual DbSet<Quote> Quotes { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Transfer> Transfers { get; set; }
        public virtual DbSet<Rate> Rates { get; set; }
        
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TeamDetail> TeamDetails { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketMessage> TicketMessages { get; set; }
        public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }
        public virtual DbSet<Crop> Crops { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Account.OnModelCreating(modelBuilder);
            Document.OnModelCreating(modelBuilder);
            Expense.OnModelCreating(modelBuilder);
            Field.OnModelCreating(modelBuilder);
            Invoice.OnModelCreating(modelBuilder);
            Notification.OnModelCreating(modelBuilder);
            Permission.OnModelCreating(modelBuilder);
            Project.OnModelCreating(modelBuilder);
            ProjectAttachment.OnModelCreating(modelBuilder);
            Domain.ProjectHistory.OnModelCreating(modelBuilder);
            Quote.OnModelCreating(modelBuilder);
            Role.OnModelCreating(modelBuilder);
            User.OnModelCreating(modelBuilder);
            Transfer.OnModelCreating(modelBuilder);
            Rate.OnModelCreating(modelBuilder);
            Team.OnModelCreating(modelBuilder);
            TeamDetail.OnModelCreating(modelBuilder);
            Ticket.OnModelCreating(modelBuilder);
            TicketMessage.OnModelCreating(modelBuilder);
            TicketAttachment.OnModelCreating(modelBuilder);
            Crop.OnModelCreating(modelBuilder);
        }
    }
}
