using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Module3_ASP_NET_RazorPage.Models;
using Module3_ASP_NET_RazorPage.Hubs;

namespace Module3_ASP_NET_RazorPage.Pages.Products;

public class CreateModel : PageModel
{
    private readonly NorthwindContext _context;
    private readonly IHubContext<ProductHub> _hubContext;

    public CreateModel(NorthwindContext context, IHubContext<ProductHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [BindProperty]
    public Product Product { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        _context.Products.Add(Product);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new { 
            productId = Product.ProductId, 
            productName = Product.ProductName, 
            notificationType = "Added",
            timestamp = DateTime.Now
        });

        return RedirectToPage("Index");
    }
}

