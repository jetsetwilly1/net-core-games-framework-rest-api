﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Midwolf.GamesFramework.Services.Storage;

namespace Midwolf.GamesFramework.Services.Migrations
{
    [DbContext(typeof(ApiDbContext))]
    partial class ApiDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(85);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(85);

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(85);

                    b.Property<string>("RoleId")
                        .HasMaxLength(85);

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasMaxLength(85);

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(85);

                    b.Property<string>("Name")
                        .HasMaxLength(85);

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.ApiUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");

                    b.HasData(
                        new
                        {
                            Id = "1",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "b0d08f60-4275-45ae-98a6-9ba236b9b18b",
                            Email = "stuart.elcocks@gmail.com",
                            EmailConfirmed = true,
                            FirstName = "Stuart",
                            LastName = "Elcocks",
                            LockoutEnabled = false,
                            NormalizedEmail = "STUART.ELCOCKS@GMAIL.COM",
                            PhoneNumberConfirmed = false,
                            TwoFactorEnabled = false,
                            UserName = "stuart.elcocks@gmail.com"
                        });
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.EntryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<int>("GameId");

                    b.Property<string>("Metadata");

                    b.Property<int>("PlayerId");

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("PlayerId");

                    b.ToTable("Entries");

                    b.HasData(
                        new
                        {
                            Id = 300,
                            CreatedAt = new DateTime(2019, 3, 1, 22, 1, 6, 999, DateTimeKind.Utc).AddTicks(679),
                            GameId = 1,
                            PlayerId = 200,
                            State = 101
                        },
                        new
                        {
                            Id = 301,
                            CreatedAt = new DateTime(2019, 3, 1, 21, 55, 6, 999, DateTimeKind.Utc).AddTicks(1381),
                            GameId = 1,
                            PlayerId = 200,
                            State = 101
                        },
                        new
                        {
                            Id = 302,
                            CreatedAt = new DateTime(2019, 3, 1, 21, 57, 6, 999, DateTimeKind.Utc).AddTicks(1398),
                            GameId = 1,
                            PlayerId = 201,
                            State = 101
                        },
                        new
                        {
                            Id = 303,
                            CreatedAt = new DateTime(2019, 3, 1, 21, 59, 6, 999, DateTimeKind.Utc).AddTicks(1409),
                            GameId = 1,
                            PlayerId = 200,
                            State = 101
                        });
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.EventEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EndDate");

                    b.Property<int>("GameId");

                    b.Property<sbyte>("ManualAdvance")
                        .HasColumnType("TINYINT(1)");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("RuleSet");

                    b.Property<DateTime>("StartDate");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Events");

                    b.HasData(
                        new
                        {
                            Id = 100,
                            EndDate = new DateTime(2019, 3, 2, 21, 53, 6, 964, DateTimeKind.Utc).AddTicks(8786),
                            GameId = 1,
                            ManualAdvance = (sbyte)0,
                            Name = "Submission event",
                            RuleSet = "{\"Interval\":\"hour\",\"NumberEntries\":10,\"NumberRefferals\":0}",
                            StartDate = new DateTime(2019, 3, 1, 21, 53, 6, 964, DateTimeKind.Utc).AddTicks(8032),
                            Type = "submission"
                        },
                        new
                        {
                            Id = 101,
                            EndDate = new DateTime(2019, 3, 4, 21, 53, 6, 997, DateTimeKind.Utc).AddTicks(6126),
                            GameId = 1,
                            ManualAdvance = (sbyte)1,
                            Name = "Moderate event",
                            StartDate = new DateTime(2019, 3, 3, 21, 53, 6, 997, DateTimeKind.Utc).AddTicks(6119),
                            Type = "moderate"
                        },
                        new
                        {
                            Id = 102,
                            EndDate = new DateTime(2019, 3, 6, 21, 53, 6, 997, DateTimeKind.Utc).AddTicks(6729),
                            GameId = 1,
                            ManualAdvance = (sbyte)0,
                            Name = "Random draw event",
                            RuleSet = "{\"Winners\":1}",
                            StartDate = new DateTime(2019, 3, 5, 21, 53, 6, 997, DateTimeKind.Utc).AddTicks(6729),
                            Type = "randomdraw"
                        });
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.GameEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Flow");

                    b.Property<DateTime>("LastUpdated");

                    b.Property<string>("Metadata");

                    b.Property<string>("Title");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Games");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Created = new DateTime(2019, 3, 1, 21, 53, 6, 963, DateTimeKind.Utc).AddTicks(9672),
                            Flow = "[{\"Id\":100,\"SuccessEvent\":101,\"FailEvent\":null,\"IsStart\":true},{\"Id\":101,\"SuccessEvent\":102,\"FailEvent\":null,\"IsStart\":false},{\"Id\":102,\"SuccessEvent\":null,\"FailEvent\":null,\"IsStart\":false}]",
                            LastUpdated = new DateTime(2019, 3, 1, 21, 53, 6, 964, DateTimeKind.Utc).AddTicks(1435),
                            Title = "Test Game",
                            UserId = "1"
                        });
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.PlayerEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<int>("GameId");

                    b.Property<string>("Metadata");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Players");

                    b.HasData(
                        new
                        {
                            Id = 200,
                            Email = "test@test.com",
                            GameId = 1
                        },
                        new
                        {
                            Id = 201,
                            Email = "stuart@test.com",
                            GameId = 1
                        },
                        new
                        {
                            Id = 202,
                            Email = "craig@test.com",
                            GameId = 1
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.ApiUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.ApiUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Midwolf.GamesFramework.Services.Models.ApiUser")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.ApiUser")
                        .WithMany("Tokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.EntryEntity", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.Db.GameEntity")
                        .WithMany("Entries")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Midwolf.GamesFramework.Services.Models.Db.PlayerEntity")
                        .WithMany("Entries")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.EventEntity", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.Db.GameEntity", "Game")
                        .WithMany("Events")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.GameEntity", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.ApiUser", "User")
                        .WithMany("Games")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Midwolf.GamesFramework.Services.Models.Db.PlayerEntity", b =>
                {
                    b.HasOne("Midwolf.GamesFramework.Services.Models.Db.GameEntity")
                        .WithMany("Players")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
