using Microsoft.AspNetCore.Components;

namespace Document.Web.Pages
{
    public partial class Login
    {
        [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    }
}
