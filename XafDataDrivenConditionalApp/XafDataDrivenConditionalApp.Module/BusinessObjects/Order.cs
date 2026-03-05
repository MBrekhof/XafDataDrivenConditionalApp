using DevExpress.Persistent.Base;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace XafDataDrivenConditionalApp.Module.BusinessObjects;

[DefaultClassOptions]
public class Order
{
    [Key]
    public virtual int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public virtual string OrderNumber { get; set; } = string.Empty;

    public virtual DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Precision(18, 2)]
    public virtual decimal TotalAmount { get; set; }

    public virtual int CustomerId { get; set; }

    [Required]
    [InverseProperty(nameof(Customer.Orders))]
    public virtual Customer Customer { get; set; } = null!;
}
