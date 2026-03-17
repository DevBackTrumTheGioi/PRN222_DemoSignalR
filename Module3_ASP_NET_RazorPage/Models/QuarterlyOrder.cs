using System;
using System.Collections.Generic;

namespace Module3_ASP_NET_RazorPage.Models;

public partial class QuarterlyOrder
{
    public string? CustomerId { get; set; }

    public string? CompanyName { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}
