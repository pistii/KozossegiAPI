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


        protected override async void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                ////initialize
                //var credential = GoogleCredential.FromFile("./firebase_key.json");
                //FirebaseApp app = FirebaseApp.Create(new AppOptions
                //{
                //    Credential = credential,
                //});
                ////Connection string
                //FirestoreDb db = FirestoreDb.Create("socialmedia-397719");

                ////Collection refs
                //CollectionReference collection = db.Collection("media");
                //DocumentSnapshot snapshot = await collection.Document("c356SK6AVIPWCksrg9DW").GetSnapshotAsync();
                //if (snapshot.Exists)
                //{
                //    Dictionary<string, object> data = snapshot.ToDictionary();
                //    //Vannak adatok.....
                //}

                //DocumentReference doc = db.Collection("media").Document("new-doc-id");
                //Dictionary<string, object> newData = new Dictionary<string, object>
                //{
                //    { "testName", "John Test" },
                //    { "testAdat", "Test" }
                //};
                //await doc.SetAsync(newData);

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
                    .HasForeignKey(c => c.personId)
                    .HasPrincipalKey(_ => _.notificationId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}