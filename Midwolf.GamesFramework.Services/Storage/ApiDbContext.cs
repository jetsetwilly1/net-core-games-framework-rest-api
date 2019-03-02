using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Models.Db;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Midwolf.GamesFramework.Services.Storage
{
    public class ApiDbContext : IdentityDbContext<ApiUser>
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<GameEntity> Games { get; set; }

        public DbSet<EventEntity> Events { get; set; }

        public DbSet<PlayerEntity> Players { get; set; }

        public DbSet<EntryEntity> Entries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>(entity => {
                entity.Property(m => m.LoginProvider).HasMaxLength(85);
                entity.Property(m => m.ProviderKey).HasMaxLength(85);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity => {
                entity.Property(m => m.RoleId).HasMaxLength(85);
                entity.Property(m => m.UserId).HasMaxLength(85);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity => {
                entity.Property(m => m.UserId).HasMaxLength(85);
                entity.Property(m => m.LoginProvider).HasMaxLength(85);
                entity.Property(m => m.Name).HasMaxLength(85);
            });

            modelBuilder.Entity<ApiUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<EntryEntity>(entity =>
            {
                entity.HasOne<GameEntity>()
                .WithMany(b => b.Entries)
                .HasForeignKey(p => p.GameId);

                entity.HasOne<PlayerEntity>()
                .WithMany(b => b.Entries)
                .HasForeignKey(p => p.PlayerId);

                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlayerId).IsRequired();
                entity.Property(e => e.Metadata).HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<object, object>>(v));
            });

            modelBuilder.Entity<PlayerEntity>(entity =>
            {
                entity.HasOne<GameEntity>()
                .WithMany(b => b.Players)
                .HasForeignKey(p => p.GameId);

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Metadata).HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<object, object>>(v));
            });

            modelBuilder.Entity<GameEntity>(entity =>
            {
                entity.HasOne(p => p.User)
                .WithMany(b => b.Games);
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Created).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
                entity.Property(e => e.Metadata).HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<JObject>(v));
                entity.Property(e => e.Flow).HasConversion(v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<FlowEntity>>(v));
            });

            modelBuilder.Entity<EventEntity>(entity =>
            {
                entity.HasOne(p => p.Game)
                .WithMany(b => b.Events)
                .HasForeignKey(p => p.GameId);

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.ManualAdvance).HasColumnType("TINYINT(1)");
                //entity.Property(e => e.Rules).HasConversion(v => JsonConvert.SerializeObject(v),
                //    v => JsonConvert.DeserializeObject<Dictionary<object, object>>(v)).IsRequired();
                //entity.Property(e => e.RuleSet).HasConversion(v => JsonConvert.SerializeObject(v),
                //    v => JsonConvert.DeserializeObject<EventRules>(v));
            });
            

            modelBuilder.Entity<ApiUser>().HasData(new ApiUser
            {
                Id = "1",
                Email = "stuart.elcocks@gmail.com",
                EmailConfirmed = true,
                NormalizedEmail = "STUART.ELCOCKS@GMAIL.COM",
                UserName = "stuart.elcocks@gmail.com",
                FirstName = "Stuart",
                LastName = "Elcocks"
            });

            modelBuilder.Entity<GameEntity>().HasData(new GameEntity
            {
                Id = 1,
                Created = DateTime.UtcNow,
                Title = "Test Game",
                UserId = "1",
                LastUpdated = DateTime.UtcNow,
                Flow = new List<FlowEntity>()
                {
                    new FlowEntity{ Id = 100, IsStart = true, SuccessEvent = 101 },
                    new FlowEntity{ Id = 101, SuccessEvent = 102 },
                    new FlowEntity{ Id = 102 },
                }
            });

            modelBuilder.Entity<EventEntity>().HasData(new EventEntity
            {
                Id = 100,
                GameId = 1,
                Name = "Submission event",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                Type = "submission",
                RuleSet = JsonConvert.SerializeObject(new Submission { Interval = "hour", NumberEntries = 10 })
            });

            modelBuilder.Entity<EventEntity>().HasData(new EventEntity
            {
                Id = 101,
                GameId = 1,
                Name = "Moderate event",
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(3),
                Type = "moderate",
                ManualAdvance = true
            });

            modelBuilder.Entity<EventEntity>().HasData(new EventEntity
            {
                Id = 102,
                GameId = 1,
                Name = "Random draw event",
                StartDate = DateTime.UtcNow.AddDays(4),
                EndDate = DateTime.UtcNow.AddDays(5),
                Type = "randomdraw",
                RuleSet = JsonConvert.SerializeObject(new RandomDraw { Winners = 1 })
            });

            modelBuilder.Entity<PlayerEntity>().HasData(new PlayerEntity
            {
                Id = 200,
                Email = "test@test.com",
                GameId = 1
            });

            modelBuilder.Entity<PlayerEntity>().HasData(new PlayerEntity
            {
                Id = 201,
                Email = "stuart@test.com",
                GameId = 1
            });

            modelBuilder.Entity<PlayerEntity>().HasData(new PlayerEntity
            {
                Id = 202,
                Email = "craig@test.com",
                GameId = 1
            });

            modelBuilder.Entity<EntryEntity>().HasData(new EntryEntity
            {
                Id = 300,
                GameId = 1,
                PlayerId = 200,
                State = 101,
                CreatedAt = DateTime.UtcNow.AddMinutes(8)
            });
            modelBuilder.Entity<EntryEntity>().HasData(new EntryEntity
            {
                Id = 301,
                GameId = 1,
                PlayerId = 200,
                State = 101,
                CreatedAt = DateTime.UtcNow.AddMinutes(2)
            });
            modelBuilder.Entity<EntryEntity>().HasData(new EntryEntity
            {
                Id = 302,
                GameId = 1,
                PlayerId = 201,
                State = 101,
                CreatedAt = DateTime.UtcNow.AddMinutes(4)
            });
            modelBuilder.Entity<EntryEntity>().HasData(new EntryEntity
            {
                Id = 303,
                GameId = 1,
                PlayerId = 200,
                State = 101,
                CreatedAt = DateTime.UtcNow.AddMinutes(6)
            });
        }

    }
}

