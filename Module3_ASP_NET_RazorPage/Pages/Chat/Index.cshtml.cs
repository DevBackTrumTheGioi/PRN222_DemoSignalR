using Microsoft.AspNetCore.Mvc.RazorPages;
using Module3_ASP_NET_RazorPage.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace Module3_ASP_NET_RazorPage.Pages.Chat;

public class IndexModel : PageModel
{
    public IActionResult OnGetGetMessages()
    {
        var messages = ChatMessageStore.GetMessages();
        return new JsonResult(messages);
    }
}

