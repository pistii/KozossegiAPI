using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Models;
using Newtonsoft.Json;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KozoskodoAPI.Auth;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;
using System.Data;

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
        public virtual DbSet<Post>? Post { get; set; }
        public virtual DbSet<Comment>? Comment { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<PersonalChatRoom> PersonalChatRoom { get; set; }
        public virtual DbSet<ChatRoom> ChatRoom { get; set; }
        public virtual DbSet<ChatContent> ChatContent { get; set; }

        protected override async void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("server=localhost;user id=root;database=mediadb", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.20-mariadb"));
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_general_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(x => x.Comments)
                .WithMany(x => x.comments)
                .HasForeignKey(x => x.CommentId);
            });

            modelBuilder.Entity<RelationshipType>(entity =>
            {
                entity.HasOne(c => c.RelationshipTp)
                    .WithMany(x => x.RelationshipTypes)
                    .HasForeignKey(c => c.relationshipTypeID);
            });

            modelBuilder.Entity<Personal>(entity =>
            {
                entity.HasMany(x => x.personals)
                .WithOne(x => x.personal)
                .HasForeignKey(x => x.personalID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_ibfk_1");

                entity.HasMany(c => c.Friends)
                    .WithOne(x => x.friendships)
                    .HasForeignKey(c => c.friendshipID);

                entity.HasMany(c => c.Relationships)
                    .WithOne(x => x.relationship)
                    .HasForeignKey(c => c.relationshipID);

                entity.HasMany(c => c.Notifications)
                    .WithOne(x => x.notification)
                    .HasForeignKey(c => c.personId);

                entity.HasMany(_ => _.PersonalChatRooms)
                    .WithOne(_ => _.PersonalRoom)
                    .HasForeignKey(_ => _.FK_PersonalId);
            });

            modelBuilder.Entity<ChatRoom>()
                .HasMany(x => x.ChatContents)
                .WithOne(x => x.ChatRooms)
                .HasForeignKey(x => x.chatContentId);
            modelBuilder.Entity<ChatContent>()
                 .Property(e => e.status)
                 .HasConversion(
                     c => c.ToString(),
                     v => (Status)Enum.Parse(typeof(Status), v));

            //Junction table 
            modelBuilder.Entity<PersonalChatRoom>()
                .HasKey(x => new { x.FK_PersonalId, x.FK_ChatRoomId });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}