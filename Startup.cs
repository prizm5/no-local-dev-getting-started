using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Afirmon_Webapp {
  public class Startup {
    private ILogger<Startup> logger;
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }
    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.Configure<ForwardedHeadersOptions>(options => {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
      });
      services.AddControllersWithViews();
      services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
      services
        .AddAuthentication(options => {
          options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(cookie => { cookie.Cookie.Name = Constants.AuthCookieName; })
        .AddOpenIdConnect(options => {
          options.Events.OnRemoteFailure = context => {
            context.HandleResponse();
            logger.LogError(context.Failure.Message);
            return ErrorResponse.SendErrorResponse(context.Response, context.Failure.Message);
          };
          IdentityModelEventSource.ShowPII = true; 
          options.SignInScheme = "Cookies";
          options.ClientId = Configuration["Identity:clientId"];
          options.ClientSecret = Configuration["Identity:clientSecret"];
          options.Authority = Configuration["Identity:authority"];
          options.ResponseType = OpenIdConnectResponseType.Code;
          options.Scope.Add("openid");
          options.Scope.Add("profile");
          options.Scope.Add("id");
          options.Scope.Add("api");
          options.GetClaimsFromUserInfoEndpoint = true;
          options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true
          };
          options.TokenValidationParameters.NameClaimType = "name";
          options.SaveTokens = true;
        })
        .AddJwtBearer()
        .AddIdentityCookies();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger) {
      this.logger = logger;
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }
      else {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseAuthentication();
      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseSpaStaticFiles();
      app.RedirectOnUnauthenticated();
      app.UseRouting();
      app.Use(ClaimSession.ApplyCookies);
      app.UseEndpoints(endpoints => {
        endpoints.MapControllerRoute(
          name: "default",
          pattern: "{controller}/{action=Index}/{id?}");
      });
      app.UseSpa(spa => {
        spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment()) {
          spa.UseReactDevelopmentServer(npmScript: "start");
        }
      });
    }
  }
}