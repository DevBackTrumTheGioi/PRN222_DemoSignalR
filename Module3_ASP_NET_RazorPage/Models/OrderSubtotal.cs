using System;
using System.Collections.Generic;

namespace Module3_ASP_NET_RazorPage.Models;

public partial class OrderSubtotal
{
    public int OrderId { get; set; }

    public decimal? Subtotal { get; set; }
}
