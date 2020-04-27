using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Afirmon_Webapp {
    internal class Constants {
      public const string UsernameCookieName = "openidconnect.identity";
      public const string AuthCookieName = "openidconnect.aspnet";
    }

    public class CookieSession {
      private readonly IRequestCookieCollection requestCookies;
      private readonly IResponseCookies responseCookies;

      public CookieSession(IRequestCookieCollection requestCookies, IResponseCookies responseCookies) {
        this.requestCookies = requestCookies;
        this.responseCookies = responseCookies;
      }

      private bool IsIdentityCookieAlreadySetForUser(string name) {
        return (requestCookies.ContainsKey(Constants.UsernameCookieName)
                && (requestCookies[Constants.UsernameCookieName] == name));
      }

      private void AssignIdentityCookieForUser(string name) {
        responseCookies.Delete(Constants.UsernameCookieName);
        responseCookies.Append(
          Constants.UsernameCookieName,
          name,
          new CookieOptions {Expires = DateTimeOffset.Now.AddHours(10)});
      }

      public void SetUserCookie(string name) {
        if (!IsIdentityCookieAlreadySetForUser(name)) {
          AssignIdentityCookieForUser(name);
        }
      }
    }
  public static class ClaimSession {
    private static string ReadIdentitiyFromOIDCToken(ClaimsPrincipal claimsPrincipal) {
      string name = string.Empty;
      if (claimsPrincipal.HasClaim(c => c.Type == "nickname")) {
        name = claimsPrincipal.FindFirst("nickname").Value;
      }
      else if (claimsPrincipal.HasClaim(c => c.Type == "name")) {
        name = claimsPrincipal.FindFirst("name").Value;
      }
      else if (claimsPrincipal.HasClaim(c => c.Type == "preferred_username")) {
        // KeyCloak
        name = claimsPrincipal.FindFirst("preferred_username").Value;
      }
      else {
        name = claimsPrincipal.Identity.Name; // AD FS server
      }

      return name;
    }

    public static async Task ApplyCookies(HttpContext context, Func<Task> next) {
      new CookieSession(context.Request.Cookies, context.Response.Cookies)
        .SetUserCookie(ReadIdentitiyFromOIDCToken(context.User));
      await next.Invoke();
    }
  }
}