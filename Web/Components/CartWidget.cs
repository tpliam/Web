using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.Infrastructure;
using Web.Models;

namespace Web.Components
{
    public class CartWidget : ViewComponent
    {
       
        public IViewComponentResult Invoke()
        {
            return View(HttpContext.Session.GetJson<Cart>("cart"));
        }
    }
}
