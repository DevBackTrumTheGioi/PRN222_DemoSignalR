using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Module3_ASP_NET_RazorPage.Models;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Product Name is required")]
    [StringLength(40, ErrorMessage = "Product Name cannot exceed 40 characters")]
    public string ProductName { get; set; } = null!;

    public int? SupplierId { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(20, ErrorMessage = "Quantity Per Unit cannot exceed 20 characters")]
    public string? QuantityPerUnit { get; set; }

    [Range(0, 99999, ErrorMessage = "Unit Price must be between 0 and 99999")]
    public decimal? UnitPrice { get; set; }

    [Range(0, 32767, ErrorMessage = "Units In Stock must be between 0 and 32767")]
    public short? UnitsInStock { get; set; }

    [Range(0, 32767, ErrorMessage = "Units On Order must be between 0 and 32767")]
    public short? UnitsOnOrder { get; set; }

    [Range(0, 32767, ErrorMessage = "Reorder Level must be between 0 and 32767")]
    public short? ReorderLevel { get; set; }

    public bool Discontinued { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Supplier? Supplier { get; set; }
}
