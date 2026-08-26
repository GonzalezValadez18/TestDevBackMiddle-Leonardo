using Microsoft.EntityFrameworkCore;
using TestDevBackMiddle.Models;

namespace TestDevBackMiddle.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Login> Logins { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Area> Areas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Login>(entity =>
        {
            entity.ToTable("ccloglogin");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                .HasColumnName("User_id")
                .ValueGeneratedNever();

            entity.Property(e => e.Extension)
                .HasColumnName("Extension");

            entity.Property(e => e.TipoMov)
                .HasColumnName("TipoMov");

            entity.Property(e => e.Fecha)
                .HasColumnName("fecha");
        });

       modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("ccUsers");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("User_id")
                .ValueGeneratedNever();

            entity.Property(e => e.Login)
                .HasColumnName("Login");

            entity.Property(e => e.Nombres)
                .HasColumnName("Nombres");

            entity.Property(e => e.ApellidoPaterno)
                .HasColumnName("ApellidoPaterno");

            entity.Property(e => e.ApellidoMaterno)
                .HasColumnName("ApellidoMaterno");

            entity.Property(e => e.IDArea)
                .HasColumnName("IDArea");
        });
        modelBuilder.Entity<Area>(entity =>
        {
            entity.ToTable("ccRIACat_Areas");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IdArea)
                .HasColumnName("IDArea");

            entity.Property(e => e.AreaName)
                .HasColumnName("AreaName");

            entity.Property(e => e.StatusArea)
                .HasColumnName("StatusArea");

            entity.Property(e => e.CreateDate)
                .HasColumnName("CreateDate");
        });
    }
}