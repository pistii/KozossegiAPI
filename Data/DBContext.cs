using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Models;
using Newtonsoft.Json;
using System.Reflection.Emit;

namespace KozoskodoAPI.Data
{

    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Friendship> Friendship { get; set; } = null!;

        public virtual DbSet<Personal> Personal { get; set; }

        public virtual DbSet<Relationship> Relationship { get; set; } = null!;

        public virtual DbSet<RelationshipType> Relationshiptype { get; set; } = null!;

        public virtual DbSet<user> user { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql("server=localhost;user id=root;database=mediadb", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.20-mariadb"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_general_ci")
                .HasCharSet("utf8mb4");


            modelBuilder.Entity<RelationshipType>(entity =>
            {
                entity.HasOne(c => c.RelationshipTp)
                    .WithMany(x => x.RelationshipTypes)
                    .HasForeignKey(c => c.relationshipTypeID);
            });

            modelBuilder.Entity<user>(entity =>
            {
                entity.HasMany(x => x.Personals)
                .WithOne(x => x.personal)
                .HasForeignKey(x => x.id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_ibfk_1");
            });

            //modelBuilder.Entity<user>(entity =>
            //{
            //    entity.HasOne(x => x.personal)
            //    .WithMany( c => c.Personals)
            //    .HasForeignKey(c => c.personalID);
            //});

            modelBuilder.Entity<Personal>(entity =>
            {
                entity.HasMany(c => c.Friends)
                    .WithOne(x => x.friendships)
                    .HasForeignKey(c => c.friendshipID);
                entity.HasMany(c => c.Relationships)
                    .WithOne(x => x.relationship)
                    .HasForeignKey(c => c.relationshipID);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}