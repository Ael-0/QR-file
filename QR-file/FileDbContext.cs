using Microsoft.EntityFrameworkCore;
using QRFileManager.Models;

namespace QRFileManager.Data
{
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
        {
        }

        public DbSet<FileModel> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StoredName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UploaderIP).HasMaxLength(45);
                entity.Property(e => e.QRCodePath).HasMaxLength(500);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
            });
        }
    }
}