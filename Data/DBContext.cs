﻿using Microsoft.EntityFrameworkCore;
using KozossegiAPI.Models;

namespace KozossegiAPI.Data
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

        public virtual DbSet<Friend> Friendship { get; set; } = null!;
        public virtual DbSet<FriendshipStatus> FriendshipStatus { get; set; }
        public virtual DbSet<Personal> Personal { get; set; }
        public virtual DbSet<Relationship> Relationship { get; set; } = null!;
        public virtual DbSet<RelationshipType> Relationshiptype { get; set; } = null!;
        public virtual DbSet<user> user { get; set; }
        public virtual DbSet<UserStatus> UserStatus { get; set; }
        public virtual DbSet<UserRestriction> UserRestriction { get; set; }
        public virtual DbSet<Restriction> Restriction { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<PersonalPost> PersonalPost { get; set; }
        public virtual DbSet<Comment>? Comment { get; set; }
        public virtual DbSet<CommentReaction>? CommentReaction { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<UserNotification> UserNotification { get; set; }
        public virtual DbSet<PersonalChatRoom> PersonalChatRoom { get; set; }
        public virtual DbSet<ChatRoom> ChatRoom { get; set; }
        public virtual DbSet<ChatContent> ChatContent { get; set; }
        public virtual DbSet<ChatFile> ChatFile { get; set; }
        public virtual DbSet<MediaContent> MediaContent { get; set; }
        public virtual DbSet<PostReaction> PostReaction { get; set; }
        public virtual DbSet<Study> Study { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        
        
        protected override async void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("server=localhost;user id=root;database=mediadb;Convert Zero Datetime=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.20-mariadb"));
            }
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
                entity.HasOne(p => p.personal)
                .WithOne(p => p.users)
                .IsRequired();

                entity.HasMany(p => p.Studies)
                .WithOne(u => u.user)
                .HasForeignKey(p => p.FK_UserId);
            });

            modelBuilder.Entity<Restriction>(entity =>
            {
                entity.HasMany(s => s.UserRestriction)
               .WithOne(g => g.restriction)
               .HasForeignKey(s => s.RestrictionId);

                entity.HasOne(s => s.UserStatus)
                  .WithMany(g => g.restrictions)
                  .HasForeignKey(s => s.FK_StatusId);
            });

            modelBuilder.Entity<UserRestriction>( entity => 
            {
                entity.HasOne(p => p.user)
                .WithMany(p => p.UserRestriction)
                .HasForeignKey(p => p.UserId);

                entity.HasOne(p => p.restriction)
               .WithMany(p => p.UserRestriction)
               .HasForeignKey(p => p.RestrictionId);

                entity.HasKey(x => new { x.UserId, x.RestrictionId });
            });

            modelBuilder.Entity<Personal>(entity =>
            {
                entity.HasMany(c => c.Relationships)
                    .WithOne(x => x.relationship)
                    .HasForeignKey(c => c.relationshipID);


                entity.HasMany(_ => _.PersonalChatRooms)
                    .WithOne(_ => _.PersonalRoom)
                    .HasForeignKey(_ => _.FK_PersonalId);

                entity.HasOne(p => p.Settings)
                    .WithOne(p => p.personal)
                    .HasForeignKey<Settings>(s => s.FK_UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.friends)
                    .WithMany(p => p.GetPersonals)
                    .HasForeignKey(p => p.id);

            });

            //1-1 kapcsolat a friend-friendshipStatus táblák között.
            modelBuilder.Entity<Friend>(entity =>
            {
                entity.HasOne(x => x.friendship_status)
                    .WithOne(x => x.friendship);
            });

            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasMany(x => x.ChatContents)
                    .WithOne(x => x.ChatRooms)
                    .HasForeignKey(x => x.chatContentId);
            });

            modelBuilder.Entity<ChatContent>(entity =>
            {
                entity.Property(e => e.status)
                    .HasConversion(
                    c => c.ToString(),
                    v => (Status)Enum.Parse(typeof(Status), v));

                entity.HasIndex(e => e.chatContentId).IsUnique();


                entity.HasOne(e => e.ChatFile)
                    .WithOne(e => e.ChatContent)
                    .HasForeignKey<ChatFile>(e => e.ChatContentId);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasMany(r => r.PostReactions)
                    .WithOne(p => p.post)
                    .HasForeignKey(p => p.PostId);

                entity.HasOne(p => p.MediaContent)
                .WithOne(p => p.Post)
                .HasForeignKey<MediaContent>(p => p.FK_PostId);
            });

            modelBuilder.Entity<PostReaction>(entity =>
            {
                entity.HasKey(p => p.Pk_Id);

                entity.HasMany(r => r.ReactionTypes)
                .WithOne(p => p.PostReaction)
                .HasForeignKey(p => p.Id);                
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(p => p.commentId);

                entity.HasOne(_ => _.Post)
                    .WithMany(x => x.PostComments)
                    .HasForeignKey(i => i.PostId);

                entity.HasMany(p => p.CommentReactions)
                    .WithOne(p => p.Comment)
                    .HasForeignKey(x => x.FK_CommentId);
            });

            //https://www.learnentityframeworkcore.com/configuration/many-to-many-relationship-configuration
            modelBuilder.Entity<PersonalPost>(entity =>
            {
                modelBuilder.Entity<PersonalPost>()
                .HasOne(p => p.Author)
                .WithMany(a => a.SentPosts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<PersonalPost>()
                .HasOne(p => p.Receiver)
                .WithMany(p => p.ReceivedPosts)
                .HasForeignKey(p => p.PostedToId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Posts)
               .WithMany(p => p.PersonalPosts)
               .HasForeignKey(p => p.PostId);

                entity.HasKey(x => new { x.PersonalPostId });
            });


            //Junction table 
            modelBuilder.Entity<PersonalChatRoom>()
                .HasKey(x => new { x.FK_PersonalId, x.FK_ChatRoomId });

            modelBuilder.Entity<UserNotification>()
                .HasKey(x => new { x.UserId, x.NotificationId });

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.personal)
                .WithMany(p => p.UserNotification)
                .HasForeignKey(un => un.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.notification)
                .WithMany(p => p.UserNotification)
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.NotificationType)
                         .HasConversion(
                         c => c.ToString(),
                         type => (NotificationType)Enum.Parse(typeof(NotificationType), type));
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}