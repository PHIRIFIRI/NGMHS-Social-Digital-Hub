using Microsoft.EntityFrameworkCore;
using NGMHS.Models;

namespace NGMHS.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SocialWorkForm> SocialWorkForms => Set<SocialWorkForm>();
    public DbSet<FormAttachment> FormAttachments => Set<FormAttachment>();
    public DbSet<ExternalQuery> ExternalQueries => Set<ExternalQuery>();
    public DbSet<OutreachLetter> OutreachLetters => Set<OutreachLetter>();
    public DbSet<OutreachAttachment> OutreachAttachments => Set<OutreachAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<SocialWorkForm>()
            .HasOne(f => f.CreatedByUser)
            .WithMany(u => u.CreatedForms)
            .HasForeignKey(f => f.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FormAttachment>()
            .HasOne(a => a.SocialWorkForm)
            .WithMany(f => f.Attachments)
            .HasForeignKey(a => a.SocialWorkFormId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FormAttachment>()
            .HasOne(a => a.UploadedByUser)
            .WithMany(u => u.UploadedFormAttachments)
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExternalQuery>()
            .HasOne(q => q.AssignedToUser)
            .WithMany()
            .HasForeignKey(q => q.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OutreachLetter>()
            .HasOne(o => o.CreatedByUser)
            .WithMany(u => u.CreatedOutreachLetters)
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OutreachAttachment>()
            .HasOne(a => a.OutreachLetter)
            .WithMany(o => o.Attachments)
            .HasForeignKey(a => a.OutreachLetterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
