using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Data;

public partial class TelecomDbContext : DbContext
{
    public TelecomDbContext()
    {
        Database.EnsureCreated();
    }

    public TelecomDbContext(DbContextOptions<TelecomDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .Build();

        optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Connection> Connections { get; set; }

    public virtual DbSet<DevicePort> DevicePorts { get; set; }

    public virtual DbSet<DeviceStatusHistory> DeviceStatusHistories { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<NetworkDevice> NetworkDevices { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<RequestStatus> RequestStatuses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Tariff> Tariffs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clients__3214EC0707183C6C");

            entity.HasIndex(e => e.ContractNumber, "UQ__Clients__C51D43DA5819838C").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ContractNumber).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Connecti__3214EC07272DA0B8");

            entity.HasIndex(e => e.ClientId, "IX_Connections_ClientId");

            entity.HasIndex(e => e.PortId, "IX_Connections_PortId");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Client).WithMany(p => p.Connections)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conn_Client");

            entity.HasOne(d => d.Port).WithMany(p => p.Connections)
                .HasForeignKey(d => d.PortId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conn_Port");

            entity.HasOne(d => d.Tariff).WithMany(p => p.Connections)
                .HasForeignKey(d => d.TariffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conn_Tariff");
        });

        modelBuilder.Entity<DevicePort>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DevicePo__3214EC076D2D0623");

            entity.Property(e => e.IsUplink).HasDefaultValue(false);

            entity.HasOne(d => d.Device).WithMany(p => p.DevicePorts)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK_Ports_Device");
        });

        modelBuilder.Entity<DeviceStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceSt__3214EC074974D312");

            entity.ToTable("DeviceStatusHistory");

            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employee__3214EC0774BDDE87");

            entity.HasIndex(e => e.UserId, "IX_Employees").IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Users");
        });

        modelBuilder.Entity<NetworkDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NetworkD__3214EC079C4BA55D");

            entity.HasIndex(e => e.IpAddress, "UQ__NetworkD__30C707A30F2384ED").IsUnique();

            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.LastUpTime).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.ParentDevice).WithMany(p => p.InverseParentDevice)
                .HasForeignKey(d => d.ParentDeviceId)
                .HasConstraintName("FK_Devices_Hierarchy");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Requests__3214EC070C63E1FB");

            entity.HasIndex(e => e.ClientId, "IX_Requests_ClientId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK_Req_Client");

            entity.HasOne(d => d.Device).WithMany(p => p.Requests)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK_Req_Device");

            entity.HasOne(d => d.Employee).WithMany(p => p.Requests)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Req_Employee");

            entity.HasOne(d => d.Status).WithMany(p => p.Requests)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Req_Status");
        });

        modelBuilder.Entity<RequestStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestS__3214EC0774B5262A");

            entity.HasIndex(e => e.Name, "UQ__RequestS__737584F68334DEE7").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07BBCCA337");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F65CF7AC9F").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasMany(d => d.Tariffs).WithMany(p => p.Services)
                .UsingEntity<Dictionary<string, object>>(
                    "TariffService",
                    r => r.HasOne<Tariff>().WithMany()
                        .HasForeignKey("TariffId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TariffServices_Tariffs"),
                    l => l.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TariffServices_Services"),
                    j =>
                    {
                        j.HasKey("ServiceId", "TariffId");
                        j.ToTable("TariffServices");
                    });
        });

        modelBuilder.Entity<Tariff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tariffs__3214EC075B64839A");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
