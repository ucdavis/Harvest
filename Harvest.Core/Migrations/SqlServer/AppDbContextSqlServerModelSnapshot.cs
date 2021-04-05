﻿// <auto-generated />
using System;
using Harvest.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace Harvest.Core.Migrations.SqlServer
{
    [DbContext(typeof(AppDbContextSqlServer))]
    partial class AppDbContextSqlServerModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Harvest.Core.Domain.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("ApprovedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("ApprovedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Number")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Percentage")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

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
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("QuoteId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("QuoteId")
                        .IsUnique();

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<bool>("Sent")
                        .HasColumnType("bit");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal>("ChargedTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Crop")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("CurrentAccountVersion")
                        .HasColumnType("int");

                    b.Property<DateTime>("End")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<Geometry>("Location")
                        .HasColumnType("geography");

                    b.Property<string>("LocationCode")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("PrincipalInvestigatorId")
                        .HasColumnType("int");

                    b.Property<int?>("QuoteId")
                        .HasColumnType("int");

                    b.Property<int?>("QuoteId1")
                        .HasColumnType("int");

                    b.Property<decimal>("QuoteTotal")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Requirements")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Start")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Name");

                    b.HasIndex("PrincipalInvestigatorId");

                    b.HasIndex("QuoteId");

                    b.HasIndex("QuoteId1");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Harvest.Core.Domain.ProjectHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Action")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("ActionDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Actor")
                        .HasMaxLength(20)
                        .HasColumnType("int");

                    b.Property<string>("ActorName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("ProjectHistory");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Quote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("ApprovedById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ApprovedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("CurrentDocumentId")
                        .HasColumnType("int");

                    b.Property<int>("InitatedById")
                        .HasColumnType("int");

                    b.Property<int?>("InitiatedById")
                        .HasColumnType("int");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Total")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("ApprovedById");

                    b.HasIndex("CurrentDocumentId")
                        .IsUnique()
                        .HasFilter("[CurrentDocumentId] IS NOT NULL");

                    b.HasIndex("InitatedById");

                    b.HasIndex("InitiatedById");

                    b.HasIndex("ProjectId");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Rate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("BillingUnit")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<DateTime?>("EffectiveOn")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<int>("UpdatedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedOn")
                        .HasColumnType("datetime2");

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
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Transfer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<decimal>("Amount")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<int>("FromAccountId")
                        .HasColumnType("int");

                    b.Property<int>("ToAccountId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FromAccountId");

                    b.HasIndex("ToAccountId");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TransferHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Action")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("ActionDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("ActorId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ActorName")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("TransferId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TransferId");

                    b.ToTable("TransferHistory");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TransferRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Description")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("KfsTrackingNumber")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<int>("RequestedById")
                        .HasColumnType("int");

                    b.Property<DateTime>("RequestedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("SlothTransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("RequestedById");

                    b.ToTable("TransferRequests");
                });

            modelBuilder.Entity("Harvest.Core.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Iam")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Kerberos")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

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
                        .HasForeignKey("ApprovedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
                    b.HasOne("Harvest.Core.Domain.User", "CreatedBy")
                        .WithMany("CreatedProjects")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.User", "PrincipalInvestigator")
                        .WithMany("PrincipalInvestigatorProjects")
                        .HasForeignKey("PrincipalInvestigatorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.Quote", "Quote")
                        .WithMany()
                        .HasForeignKey("QuoteId1");

                    b.Navigation("CreatedBy");

                    b.Navigation("PrincipalInvestigator");

                    b.Navigation("Quote");
                });

            modelBuilder.Entity("Harvest.Core.Domain.ProjectHistory", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

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
                        .HasForeignKey("InitiatedById");

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

            modelBuilder.Entity("Harvest.Core.Domain.Transfer", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Account", "FromAccount")
                        .WithMany()
                        .HasForeignKey("FromAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.Account", "ToAccount")
                        .WithMany()
                        .HasForeignKey("ToAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FromAccount");

                    b.Navigation("ToAccount");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TransferHistory", b =>
                {
                    b.HasOne("Harvest.Core.Domain.TransferRequest", null)
                        .WithMany("History")
                        .HasForeignKey("TransferId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Harvest.Core.Domain.TransferRequest", b =>
                {
                    b.HasOne("Harvest.Core.Domain.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Harvest.Core.Domain.User", "RequestedBy")
                        .WithMany()
                        .HasForeignKey("RequestedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("RequestedBy");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Document", b =>
                {
                    b.Navigation("Quote");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Project", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Quotes");
                });

            modelBuilder.Entity("Harvest.Core.Domain.Quote", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Harvest.Core.Domain.TransferRequest", b =>
                {
                    b.Navigation("History");
                });

            modelBuilder.Entity("Harvest.Core.Domain.User", b =>
                {
                    b.Navigation("CreatedProjects");

                    b.Navigation("Permissions");

                    b.Navigation("PrincipalInvestigatorProjects");
                });
#pragma warning restore 612, 618
        }
    }
}
