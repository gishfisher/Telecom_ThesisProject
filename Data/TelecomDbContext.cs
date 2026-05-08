using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Apartment> Apartments { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Connection> Connections { get; set; }

    public virtual DbSet<DevicePort> DevicePorts { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<MountingPoint> MountingPoints { get; set; }

    public virtual DbSet<MountingPointType> MountingPointTypes { get; set; }

    public virtual DbSet<MountingSpotType> MountingSpotTypes { get; set; }

    public virtual DbSet<NetworkDevice> NetworkDevices { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<RequestComment> RequestComments { get; set; }

    public virtual DbSet<RequestStatus> RequestStatuses { get; set; }

    public virtual DbSet<RequestsType> RequestsTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<SnmpProfile> SnmpProfiles { get; set; }

    public virtual DbSet<Street> Streets { get; set; }

    public virtual DbSet<Tariff> Tariffs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Addresses");

            entity.Property(e => e.HouseNumber).HasMaxLength(10);

            entity.HasOne(d => d.Street).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.StreetId)
                .HasConstraintName("FK__Addresses__Stree__5070F446");
        });

        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.Property(e => e.Number).HasMaxLength(50);

            entity.HasOne(d => d.Address).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Apartments_Addresses");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cities");

            entity.HasIndex(e => e.Name, "UQ__Cities__Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clients");

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Connections");

            entity.HasIndex(e => e.PortId, "UQ__Connections_Ports").IsUnique();

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StaticIp).HasMaxLength(45);

            entity.HasOne(d => d.Apartment).WithMany(p => p.Connections)
                .HasForeignKey(d => d.ApartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Connections_Apartments");

            entity.HasOne(d => d.Client).WithMany(p => p.Connections)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Connectio__Clien__7F2BE32F");

            entity.HasOne(d => d.Port).WithOne(p => p.Connection)
                .HasForeignKey<Connection>(d => d.PortId)
                .HasConstraintName("FK__Connectio__PortI__00200768");

            entity.HasOne(d => d.Tariff).WithMany(p => p.Connections)
                .HasForeignKey(d => d.TariffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Connectio__Tarif__01142BA1");
        });

        modelBuilder.Entity<DevicePort>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DevicePorts");

            entity.Property(e => e.PortName).HasMaxLength(50);

            entity.HasOne(d => d.Device).WithMany(p => p.DevicePorts)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__DevicePor__Devic__656C112C");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceType");

            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Employees");

            entity.HasIndex(e => e.UserId, "UQ__Employees__User").IsUnique();

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Employees__UserI__71D1E811");
        });

        modelBuilder.Entity<MountingPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MountingPoints");

            entity.Property(e => e.LocationDescription).HasMaxLength(255);
            entity.Property(e => e.MountingDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Address).WithMany(p => p.MountingPoints)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK__MountingP__Addre__5629CD9C");

            entity.HasOne(d => d.PointType).WithMany(p => p.MountingPoints)
                .HasForeignKey(d => d.PointTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MountingP__Point__571DF1D5");

            entity.HasOne(d => d.SpotType).WithMany(p => p.MountingPoints)
                .HasForeignKey(d => d.SpotTypeId)
                .HasConstraintName("FK_MountingPoints_MountingSpotTypes");
        });

        modelBuilder.Entity<MountingPointType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MountingPointTypes");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<MountingSpotType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<NetworkDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NetworkDevices");

            entity.HasIndex(e => e.IpAddress, "UQ__NetworkDevice_IpAddress").IsUnique();

            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.IsMonitored).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.DeviceType).WithMany(p => p.NetworkDevices)
                .HasForeignKey(d => d.DeviceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NetworkDe__Devic__5FB337D6");

            entity.HasOne(d => d.MountingPoint).WithMany(p => p.NetworkDevices)
                .HasForeignKey(d => d.MountingPointId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__NetworkDe__Mount__60A75C0F");

            entity.HasOne(d => d.ParentDevice).WithMany(p => p.InverseParentDevice)
                .HasForeignKey(d => d.ParentDeviceId)
                .HasConstraintName("FK__NetworkDe__Paren__619B8048");

            entity.HasOne(d => d.SnmpProfile).WithMany(p => p.NetworkDevices)
                .HasForeignKey(d => d.SnmpProfileId)
                .HasConstraintName("FK_NetworkDevices_SnmpProfiles");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Requests");

            entity.Property(e => e.ClosedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.Client).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK__Requests__Client__0C85DE4D");

            entity.HasOne(d => d.Employee).WithMany(p => p.Requests)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Requests__Employ__0E6E26BF");

            entity.HasOne(d => d.Status).WithMany(p => p.Requests)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK__Requests__Status__0B91BA14");

            entity.HasOne(d => d.Type).WithMany(p => p.Requests)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requests_RequestsTypes");
        });

        modelBuilder.Entity<RequestComment>(entity =>
        {
            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RequestComments)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequestComments_Employees");

            entity.HasOne(d => d.Request).WithMany(p => p.RequestComments)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("FK_RequestComments_Requests");
        });

        modelBuilder.Entity<RequestStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestStatuses");

            entity.HasIndex(e => e.Name, "UQ__RequestStatuses__Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<RequestsType>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles");

            entity.HasIndex(e => e.Name, "UQ__Roles__Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SnmpProfile>(entity =>
        {
            entity.Property(e => e.Community).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Version).HasMaxLength(3);
        });

        modelBuilder.Entity<Street>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Streets");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.City).WithMany(p => p.Streets)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK__Streets__CityId__4D94879B");
        });

        modelBuilder.Entity<Tariff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tariffs");

            entity.Property(e => e.MonthlyFee)
                .HasDefaultValue(0m)
                .HasColumnType("numeric(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasMany(d => d.Services).WithMany(p => p.Tariffs)
                .UsingEntity<Dictionary<string, object>>(
                    "TariffService",
                    r => r.HasOne<Service>().WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TariffServices_Services"),
                    l => l.HasOne<Tariff>().WithMany()
                        .HasForeignKey("TariffId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TariffServices_Tariffs"),
                    j =>
                    {
                        j.HasKey("TariffId", "ServiceId");
                        j.ToTable("TariffServices");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users");

            entity.HasIndex(e => e.Login, "UQ__Users__5E55825BDB28843E").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__6E01572D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
