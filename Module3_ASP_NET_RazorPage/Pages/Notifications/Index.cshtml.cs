using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Module3_ASP_NET_RazorPage.Hubs;

namespace Module3_ASP_NET_RazorPage.Pages.Notifications;

public class IndexModel : PageModel
{
    public IActionResult OnGetGetNotifications()
    {
        var notifications = NotificationStore.GetNotifications();
        return new JsonResult(notifications);
    }
}

