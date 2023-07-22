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

        public virtual DbSet<Personal> Personal { get; set; } = null!;

        public virtual DbSet<Relationship> Relationship { get; set; } = null!;

        public virtual DbSet<RelationshipType> Relationshiptype { get; set; } = null!;

        public virtual DbSet<user> user { get; set; } = null!;

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


            modelBuilder.Entity<Relationship>(entity =>
            {
                entity.HasOne(c => c.RelationshipTp)
                    .WithMany(x => x.Relationshiptp)
                    .HasForeignKey(c => c.typeID)
                    .HasConstraintName("relationship_ibfk_1");
            });

            modelBuilder.Entity<user>(entity =>
            {
                entity.HasOne(c => c.friendship)
                    .WithMany(x => x.Friendships)
                    .HasForeignKey(c => c.friendshipID)
                    .HasConstraintName("user_ibfk_1");
                entity.HasOne(c => c.relationship)
                    .WithMany(x => x.Relationships)
                    .HasForeignKey(c => c.relationshipID)
                    .HasConstraintName("user_ibfk_2");
                entity.HasOne(c => c.personal)
                    .WithMany(x => x.Personals)
                    .HasForeignKey(c => c.personalID)
                    .HasConstraintName("user_ibfk_3");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}