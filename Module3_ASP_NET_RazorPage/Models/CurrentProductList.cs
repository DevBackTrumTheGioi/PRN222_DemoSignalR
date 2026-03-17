using System;
using System.Collections.Generic;

namespace Module3_ASP_NET_RazorPage.Models;

public partial class CurrentProductList
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;
}
