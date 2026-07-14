using Chingoo.Models;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts => Set<Post>();

        public DbSet<Notice> Notices => Set<Notice>();

        public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();

        public DbSet<Comment> Comments => Set<Comment>();

        public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();

        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>().HasIndex(x => new { x.BoardType, x.BoardId });

            modelBuilder.Entity<ChatRoom>()
                .HasIndex(x => new { x.PostId, x.PostOwnerId, x.ParticipantId })
                .IsUnique();

            modelBuilder.Entity<ChatRoom>()
                .HasOne(x => x.Post)
                .WithMany()
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatRoom>()
                .HasOne(x => x.PostOwner)
                .WithMany()
                .HasForeignKey(x => x.PostOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatRoom>()
                .HasOne(x => x.Participant)
                .WithMany()
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(x => new { x.ChatRoomId, x.CreatedAt });

            modelBuilder.Entity<ChatMessage>()
                .HasOne(x => x.ChatRoom)
                .WithMany()
                .HasForeignKey(x => x.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
