﻿// <auto-generated />
using System;
using Harvest.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Migrations.Sqlite
{
    [DbContext(typeof(AppDbContextSqlite))]
    partial class AppDbContextSqliteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Harvest.Core.Domain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ApprovedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ApprovedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("Number")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Percentage")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ApprovedById");

                    b.HasIndex("Name");

                    b.HasIndex("Number");

                    b.HasIndex("ProjectId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("QuoteId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("QuoteId")
                        .IsUnique();

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Expense", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Activity")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Approved")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.Property<int?>("CreatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<int?>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("TEXT");

                    b.Property<int>("RateId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Total")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("InvoiceId");

                    b.HasIndex("ProjectId");

                    b.HasIndex("RateId");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Field", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Crop")
                        .HasColumnType("TEXT");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<Polygon>("Location")
                        .HasColumnType("POLYGON");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Crop");

                    b.HasIndex("ProjectId");

                    b.ToTable("Fields");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Invoice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("KfsTrackingNumber")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SlothTransactionId")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Total")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Invoices");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Sent")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AcreageRateId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Acres")
                        .HasColumnType("REAL");

                    b.Property<decimal>("ChargedTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.Property<int>("CreatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Crop")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT");

                    b.Property<string>("CropType")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("CurrentAccountVersion")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("End")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsApproved")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int?>("OriginalProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PrincipalInvestigatorId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("QuoteId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("QuoteTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.Property<string>("Requirements")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AcreageRateId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Name");

                    b.HasIndex("OriginalProjectId");

                    b.HasIndex("PrincipalInvestigatorId");

                    b.HasIndex("QuoteId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Harvest.Core.Domain.ProjectHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Action")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ActionDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ActorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ActorId");

                    b.HasIndex("ProjectId");

                    b.ToTable("ProjectHistory");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Quote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ApprovedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ApprovedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("CurrentDocumentId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InitiatedById")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Total")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ApprovedById");

                    b.HasIndex("CurrentDocumentId")
                        .IsUnique();

                    b.HasIndex("InitiatedById");

                    b.HasIndex("ProjectId");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Rate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("BillingUnit")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("CreatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EffectiveOn")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("TEXT");

                    b.Property<string>("Unit")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("UpdatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Description");

                    b.HasIndex("Type");

                    b.HasIndex("UpdatedById");

                    b.ToTable("Rates");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Ticket", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Completed")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CreatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Requirements")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .HasColumnType("TEXT");

                    b.Property<int>("UpdatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("WorkNotes")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Ticket");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TicketAttachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TicketId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TicketId");

                    b.ToTable("TicketAttachment");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TicketMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TicketId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TicketId");

                    b.ToTable("TicketMessage");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Transfer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("InvoiceId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Total")
                        .HasPrecision(18, 2)
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("Harvest.Core.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Iam")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("Iam");

                    b.HasIndex("Kerberos");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Account", b =>
                {
                    b.HasOne("Harvest.Core.Domain.User", "ApprovedBy")
                        .WithMany()
                        .HasForeignKey("ApprovedById");

                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany("Accounts")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ApprovedBy");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Document", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Quote", null)
                        .WithMany("Documents")
                        .HasForeignKey("QuoteId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Harvest.Core.Domain.Expense", b =>
                {
                    b.HasOne("Harvest.Core.Domain.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.HasOne("Harvest.Core.Domain.Invoice", "Invoice")
                        .WithMany("Expenses")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.Rate", "Rate")
                        .WithMany()
                        .HasForeignKey("RateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("Invoice");

                    b.Navigation("Project");

                    b.Navigation("Rate");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Field", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany("Fields")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Invoice", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Notification", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Permission", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.User", "User")
                        .WithMany("Permissions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Project", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Rate", "AcreageRate")
                        .WithMany("Projects")
                        .HasForeignKey("AcreageRateId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Harvest.Core.Domain.User", "CreatedBy")
                        .WithMany("CreatedProjects")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.Project", "OriginalProject")
                        .WithMany()
                        .HasForeignKey("OriginalProjectId");

                    b.HasOne("Harvest.Core.Domain.User", "PrincipalInvestigator")
                        .WithMany("PrincipalInvestigatorProjects")
                        .HasForeignKey("PrincipalInvestigatorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.Quote", "Quote")
                        .WithMany()
                        .HasForeignKey("QuoteId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("AcreageRate");

                    b.Navigation("CreatedBy");

                    b.Navigation("OriginalProject");

                    b.Navigation("PrincipalInvestigator");

                    b.Navigation("Quote");
                });

            modelBuilder.Entity("Harvest.Core.Domain.ProjectHistory", b =>
                {
                    b.HasOne("Harvest.Core.Domain.User", "Actor")
                        .WithMany()
                        .HasForeignKey("ActorId");

                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Actor");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Quote", b =>
                {
                    b.HasOne("Harvest.Core.Domain.User", "ApprovedBy")
                        .WithMany()
                        .HasForeignKey("ApprovedById");

                    b.HasOne("Harvest.Core.Domain.Document", "CurrentDocument")
                        .WithOne("Quote")
                        .HasForeignKey("Harvest.Core.Domain.Quote", "CurrentDocumentId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Harvest.Core.Domain.User", "InitiatedBy")
                        .WithMany()
                        .HasForeignKey("InitiatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany("Quotes")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ApprovedBy");

                    b.Navigation("CurrentDocument");

                    b.Navigation("InitiatedBy");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Rate", b =>
                {
                    b.HasOne("Harvest.Core.Domain.User", "CreatedBy")
                        .WithMany("CreatedRates")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.User", "UpdatedBy")
                        .WithMany("UpdatedRates")
                        .HasForeignKey("UpdatedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("UpdatedBy");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Ticket", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Invoice", "Invoice")
                        .WithMany("Tickets")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany("Tickets")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Invoice");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TicketAttachment", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Ticket", "Ticket")
                        .WithMany("Attachments")
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ticket");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TicketMessage", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Ticket", "Ticket")
                        .WithMany("Messages")
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ticket");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Transfer", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Invoice", "Invoice")
                        .WithMany("Transfers")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Invoice");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Document", b =>
                {
                    b.Navigation("Quote");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Invoice", b =>
                {
                    b.Navigation("Expenses");

                    b.Navigation("Tickets");

                    b.Navigation("Transfers");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Project", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Fields");

                    b.Navigation("Quotes");

                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Quote", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Rate", b =>
                {
                    b.Navigation("Projects");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Ticket", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Harvest.Core.Domain.User", b =>
                {
                    b.Navigation("CreatedProjects");

                    b.Navigation("CreatedRates");

                    b.Navigation("Permissions");

                    b.Navigation("PrincipalInvestigatorProjects");

                    b.Navigation("UpdatedRates");
                });
#pragma warning restore 612, 618
        }
    }
}
