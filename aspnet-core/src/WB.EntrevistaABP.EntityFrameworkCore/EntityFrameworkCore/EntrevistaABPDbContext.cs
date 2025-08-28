using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using WB.EntrevistaABP.Domain.Entidades;
using System.Collections.Generic;
namespace WB.EntrevistaABP.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class EntrevistaABPDbContext :
    AbpDbContext<EntrevistaABPDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<Pasajero> Pasajeros { get; set; }
    public DbSet<Viaje> Viajes { get; set; }



    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public EntrevistaABPDbContext(DbContextOptions<EntrevistaABPDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        
        builder.Entity<Pasajero>(b =>
        {
            b.ToTable("Pasajeros");

            b.Property(x => x.Nombre).IsRequired().HasMaxLength(128);
            b.Property(x => x.Apellido).IsRequired().HasMaxLength(128);
            b.Property(x => x.DNI).IsRequired();

            // Un DNI por pasajero
            b.HasIndex(x => x.DNI).IsUnique();

            // 1:1 con IdentityUser
            b.HasOne(x => x.User)
             .WithOne()
             .HasForeignKey<Pasajero>(x => x.UserId)
             .IsRequired();

            
        });

        
        builder.Entity<Viaje>(b =>
        {
            b.ToTable("Viajes");

            b.Property(x => x.Origen).IsRequired().HasMaxLength(128);
            b.Property(x => x.Destino).IsRequired().HasMaxLength(128);
            b.Property(x => x.MedioDeTransporte).IsRequired() .HasConversion<string>().HasMaxLength(64);
            b.Property(x => x.FechaSalida).IsRequired();
            b.Property(x => x.FechaLlegada).IsRequired();

            // Coordinador 1:1 (FK a Pasajero)
            b.HasOne(x => x.Coordinador)
             .WithMany()
             .HasForeignKey(x => x.CoordinadorId)
             .OnDelete(DeleteBehavior.Restrict)  // evita borrar un Pasajero si es Coordinador
             .IsRequired();
        });

        // N:N automática con tabla intermedia personalizada
        builder.Entity<Viaje>()
            .HasMany(v => v.Pasajeros)
            .WithMany(p => p.Viajes)
            .UsingEntity<Dictionary<string, object>>(
                "PasajerosViajes",
                // lado Pasajero
                right => right
                    .HasOne<Pasajero>()
                    .WithMany()
                    .HasForeignKey("PasajeroId")
                    .OnDelete(DeleteBehavior.Restrict), // Impide borrar Pasajero si está asignado a un viaje
                                                        
                left => left
                    .HasOne<Viaje>()
                    .WithMany()
                    .HasForeignKey("ViajeId")
                    .OnDelete(DeleteBehavior.Restrict), // Impide borrar Viaje si tiene pasajeros
                                                        // configuración de la tabla intermedia
                join =>
                {
                    join.ToTable("PasajerosViajes");
                    join.HasKey("PasajeroId", "ViajeId"); // PK compuesta
                    join.HasIndex("ViajeId");
                    join.HasIndex("PasajeroId");
                }
            );

        builder.Entity<Pasajero>()
           .HasIndex(x => x.UserId)
           .IsUnique();


        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(EntrevistaABPConsts.DbTablePrefix + "YourEntities", EntrevistaABPConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
