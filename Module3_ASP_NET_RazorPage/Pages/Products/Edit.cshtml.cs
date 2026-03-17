using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Module3_ASP_NET_RazorPage.Models;
using Module3_ASP_NET_RazorPage.Hubs;

namespace Module3_ASP_NET_RazorPage.Pages.Products;

public class EditModel : PageModel
{
    private readonly NorthwindContext _context;
    private readonly IHubContext<ProductHub> _hubContext;

    public EditModel(NorthwindContext context, IHubContext<ProductHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [BindProperty]
    public Product Product { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return RedirectToPage("Index");

        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var product = await _context.Products.FindAsync(Product.ProductId);
        if (product == null)
            return RedirectToPage("Index");

        product.ProductName = Product.ProductName;
        product.UnitPrice = Product.UnitPrice;
        product.UnitsInStock = Product.UnitsInStock;
        product.UnitsOnOrder = Product.UnitsOnOrder;
        product.ReorderLevel = Product.ReorderLevel;
        product.QuantityPerUnit = Product.QuantityPerUnit;
        product.Discontinued = Product.Discontinued;

        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new { 
            productId = product.ProductId, 
            productName = product.ProductName, 
            notificationType = "Updated",
            timestamp = DateTime.Now
        });
        
        return RedirectToPage("Index");
    }
}

