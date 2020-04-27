using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Afirmon_Webapp {
  public static class RedirectOnUnauthenticatedExtension {
    public static async Task RedirectToLogin(HttpContext context){
      var redirectprop = new AuthenticationProperties() {
        RedirectUri = context.Request.GetEncodedUrl()
      };
      await context.ChallengeAsync(redirectprop);
    }

    public static void RedirectOnUnauthenticated(this IApplicationBuilder app) {
      app.MapWhen(ctx=>!ctx.User.Identity.IsAuthenticated, branchApp => {
        branchApp.Run(RedirectToLogin);
      });
    }
  }
}