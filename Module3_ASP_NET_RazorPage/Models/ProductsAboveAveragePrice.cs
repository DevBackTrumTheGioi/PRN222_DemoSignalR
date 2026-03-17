using System;
using System.Collections.Generic;

namespace Module3_ASP_NET_RazorPage.Models;

public partial class ProductsAboveAveragePrice
{
    public string ProductName { get; set; } = null!;

    public decimal? UnitPrice { get; set; }
}
