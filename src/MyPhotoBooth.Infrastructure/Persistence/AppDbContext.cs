using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PhotoTag> PhotoTags => Set<PhotoTag>();
    public DbSet<AlbumPhoto> AlbumPhotos => Set<AlbumPhoto>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupSharedContent> GroupSharedContents => Set<GroupSharedContent>();
    public DbSet<FavoritePhoto> FavoritePhotos => Set<FavoritePhoto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Photo configuration
        builder.Entity<Photo>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(p => p.StorageKey).IsRequired().HasMaxLength(255);
            entity.Property(p => p.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(p => p.ThumbnailPath).IsRequired().HasMaxLength(500);
            entity.Property(p => p.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(p => p.UserId).IsRequired();
            entity.Property(p => p.ExifDataJson).HasColumnType("jsonb");

            // Indexes
            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => new { p.UserId, p.CapturedAt });
            entity.HasIndex(p => new { p.UserId, p.UploadedAt }); // For recently added queries
        });

        // Album configuration
        builder.Entity<Album>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(1000);
            entity.Property(a => a.UserId).IsRequired();
            
            entity.HasIndex(a => a.UserId);
        });

        // Tag configuration
        builder.Entity<Tag>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(50);
            entity.Property(t => t.UserId).IsRequired();
            
            // Unique constraint: one user cannot have duplicate tag names
            entity.HasIndex(t => new { t.Name, t.UserId }).IsUnique();
        });

        // PhotoTag many-to-many configuration
        builder.Entity<PhotoTag>(entity =>
        {
            entity.HasKey(pt => new { pt.PhotoId, pt.TagId });
            
            entity.HasOne(pt => pt.Photo)
                .WithMany(p => p.PhotoTags)
                .HasForeignKey(pt => pt.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(pt => pt.Tag)
                .WithMany(t => t.PhotoTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AlbumPhoto many-to-many configuration
        builder.Entity<AlbumPhoto>(entity =>
        {
            entity.HasKey(ap => new { ap.AlbumId, ap.PhotoId });
            
            entity.HasOne(ap => ap.Album)
                .WithMany(a => a.AlbumPhotos)
                .HasForeignKey(ap => ap.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ap => ap.Photo)
                .WithMany(p => p.AlbumPhotos)
                .HasForeignKey(ap => ap.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(ap => new { ap.AlbumId, ap.SortOrder });
        });

        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.Property(rt => rt.UserId).IsRequired();

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index on token for fast lookup
            entity.HasIndex(rt => rt.Token);
        });

        // ShareLink configuration
        builder.Entity<ShareLink>(entity =>
        {
            entity.HasKey(sl => sl.Id);
            entity.Property(sl => sl.Token).IsRequired().HasMaxLength(50);
            entity.Property(sl => sl.UserId).IsRequired();
            entity.Property(sl => sl.PasswordHash).HasMaxLength(500);

            entity.HasIndex(sl => sl.Token).IsUnique();
            entity.HasIndex(sl => sl.UserId);

            entity.HasOne(sl => sl.Photo)
                .WithMany()
                .HasForeignKey(sl => sl.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sl => sl.Album)
                .WithMany()
                .HasForeignKey(sl => sl.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(sl => sl.IsExpired);
            entity.Ignore(sl => sl.IsRevoked);
            entity.Ignore(sl => sl.IsActive);
        });

        // Group configuration
        builder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(200);
            entity.Property(g => g.Description).HasMaxLength(1000);
            entity.Property(g => g.OwnerId).IsRequired();

            entity.HasIndex(g => g.OwnerId);
            entity.HasIndex(g => new { g.OwnerId, g.DeletedAt });

            entity.Ignore(g => g.IsDeleted);
            entity.Ignore(g => g.IsDeletionScheduled);
            entity.Ignore(g => g.DaysUntilDeletion);
        });

        // GroupMember configuration
        builder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(gm => gm.Id);
            entity.Property(gm => gm.GroupId).IsRequired();
            entity.Property(gm => gm.UserId).IsRequired();

            entity.HasIndex(gm => new { gm.GroupId, gm.UserId, gm.LeftAt });

            entity.HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gm => gm.User)
                .WithMany()
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(gm => gm.IsActive);
            entity.Ignore(gm => gm.IsInGracePeriod);
        });

        // GroupSharedContent configuration
        builder.Entity<GroupSharedContent>(entity =>
        {
            entity.HasKey(gsc => gsc.Id);
            entity.Property(gsc => gsc.GroupId).IsRequired();
            entity.Property(gsc => gsc.SharedByUserId).IsRequired();

            entity.HasIndex(gsc => new { gsc.GroupId, gsc.ContentType, gsc.RemovedAt });
            entity.HasIndex(gsc => gsc.PhotoId);
            entity.HasIndex(gsc => gsc.AlbumId);

            entity.HasOne(gsc => gsc.Group)
                .WithMany(g => g.SharedContent)
                .HasForeignKey(gsc => gsc.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gsc => gsc.Photo)
                .WithMany()
                .HasForeignKey(gsc => gsc.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gsc => gsc.Album)
                .WithMany()
                .HasForeignKey(gsc => gsc.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(gsc => gsc.IsActive);
        });

        // FavoritePhoto configuration
        builder.Entity<FavoritePhoto>(entity =>
        {
            entity.HasKey(fp => fp.Id);
            entity.Property(fp => fp.UserId).IsRequired();
            entity.Property(fp => fp.CreatedAt).IsRequired();

            // One user per photo (prevent duplicates)
            entity.HasIndex(fp => new { fp.UserId, fp.PhotoId }).IsUnique();
            entity.HasIndex(fp => fp.UserId);

            entity.HasOne(fp => fp.Photo)
                .WithMany()
                .HasForeignKey(fp => fp.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fp => fp.User)
                .WithMany()
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
