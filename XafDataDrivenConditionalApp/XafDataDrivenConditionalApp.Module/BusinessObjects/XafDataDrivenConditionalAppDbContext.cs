using System.Drawing;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.ExpressApp.EFCore.Updating;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace XafDataDrivenConditionalApp.Module.BusinessObjects
{
    public class NullableColorToInt32Converter : ValueConverter<Color?, int?>
    {
        public NullableColorToInt32Converter()
            : base(
                c => c.HasValue ? c.Value.ToArgb() : null,
                v => v.HasValue ? Color.FromArgb(v.Value) : null)
        { }
    }

    [TypesInfoInitializer(typeof(DbContextTypesInfoInitializer<XafDataDrivenConditionalAppEFCoreDbContext>))]
    public class XafDataDrivenConditionalAppEFCoreDbContext : DbContext
    {
        public XafDataDrivenConditionalAppEFCoreDbContext(DbContextOptions<XafDataDrivenConditionalAppEFCoreDbContext> options) : base(options)
        {
        }
        public DbSet<AppearanceRuleData> AppearanceRules { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseDeferredDeletion(this);
            modelBuilder.UseOptimisticLock();
            modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

            modelBuilder.Entity<AppearanceRuleData>(entity =>
            {
                entity.Property(e => e.BackColor)
                    .HasConversion<NullableColorToInt32Converter>()
                    .HasColumnName("BackColorValue");
                entity.Property(e => e.FontColor)
                    .HasConversion<NullableColorToInt32Converter>()
                    .HasColumnName("FontColorValue");
                entity.Property(e => e.Visibility).HasConversion<int?>().HasColumnName("Visibility");
                entity.Property(e => e.FontStyle).HasConversion<int?>().HasColumnName("FontStyle");
            });
        }
    }
}
