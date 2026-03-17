using Microsoft.EntityFrameworkCore;
using Module3_ASP_NET_RazorPage.Models;
using Module3_ASP_NET_RazorPage.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwindDB")));

builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.MapHub<ProductHub>("/productHub");
app.MapHub<ChatHub>("/chatHub");
app.Run();


