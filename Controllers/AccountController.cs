using Microsoft.AspNetCore.Mvc;

namespace Afirmon_Webapp.Controllers {
  public class AccountController : Controller {
    // GET
    public IActionResult Index() {
      return Ok(new {username = User.Identity.Name});
    }
  }
}