using DevExpress.Persistent.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XafDataDrivenConditionalApp.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(Name))]
public class Customer
{
    [Key]
    public virtual int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public virtual string Email { get; set; } = string.Empty;

    [InverseProperty(nameof(Order.Customer))]
    public virtual IList<Order> Orders { get; set; } = new ObservableCollection<Order>();
}
