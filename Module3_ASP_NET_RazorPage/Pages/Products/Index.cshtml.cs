﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Module3_ASP_NET_RazorPage.Hubs;
using Module3_ASP_NET_RazorPage.Models;

namespace Module3_ASP_NET_RazorPage.Pages.Products;

public class IndexModel : PageModel
{
    private readonly NorthwindContext _context;
    private readonly IHubContext<ProductHub> _hubContext;

    public IndexModel(NorthwindContext context, IHubContext<ProductHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public List<Product> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _context.Products.ToListAsync();
    }

    public async Task<IActionResult> OnGetGetProductsAsync()
    {
        var products = await _context.Products.ToListAsync();
        return new JsonResult(products);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new { 
                productId = product.ProductId, 
                productName = product.ProductName, 
                notificationType = "Deleted",
                timestamp = DateTime.Now
            });
        }
        return RedirectToPage();
    }
}
