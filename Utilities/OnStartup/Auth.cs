using GrantTracker.Dal.Repositories.UserRepository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace GrantTracker.Utilities.OnStartup
{
	#region Startup Initialization

	public static class Auth
	{
		public static string GetBadgeNumber(ClaimsIdentity identity)
		{
			if (identity.Name == null)
				return "";

			string badgeNumber = Regex.Replace(identity.Name, "[^0-9]", ""); //Remove string "TUSD\\" from "TUSD\\######"

			if (!int.TryParse(badgeNumber, out int _))
			{
				var tusdContext = new PrincipalContext(ContextType.Domain, "10.15.64.11");
				var user = UserPrincipal.FindByIdentity(tusdContext, IdentityType.SamAccountName, identity.Name);
				badgeNumber = user.EmployeeId;
			}

			return badgeNumber;
		}

		public static void Setup(WebApplicationBuilder builder)
		{
			builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme).AddNegotiate();
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("Administrator", policy => policy.RequireClaim("Administrator"));
				options.AddPolicy("AnyAuthorizedUser",
					policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.IsInRole("Coordinator"))
				);
			});

			builder.Services.AddScoped<IClaimsTransformation, RoleAuthorizationTransform>();
			builder.Services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();
		}

		public static void Configure(WebApplication app)
		{
			app.UseAuthentication();
			app.UseAuthorization();
		}
	}

	#endregion Startup Initialization

	#region Authorization Transform

	//Authenticate and assign authorization level from database repository.

	public class RoleAuthorizationTransform : IClaimsTransformation
	{
		private readonly IRoleProvider _roleProvider;

		public RoleAuthorizationTransform(IRoleProvider roleProvider)
		{
			_roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
		}

		private async Task AddUserClaimAsync(ClaimsIdentity identity)
		{
			string userRole = await _roleProvider.GetUserRoleAsync(Auth.GetBadgeNumber(identity));

			var claim = new Claim(identity.RoleClaimType, "Administrator");
			identity.AddClaim(claim);
		}

		//Called first to determine authorization level
		public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		{
			if (principal.Identity is null) return principal;

			ClaimsPrincipal clone = principal.Clone();

			await AddUserClaimAsync((ClaimsIdentity)clone.Identity);

			return clone;
		}
	}

	#endregion Authorization Transform

	#region Handle Authorization Requirements

	//[Authorize] controllers call this
	//Assess policy-based authorization requirements for a given controller and approve or deny access.

	public class AuthorizationHandler : IAuthorizationHandler
	{
		//Called when policies are evaluated, after user claims are evaluated and set
		public async Task HandleAsync(AuthorizationHandlerContext context)
		{
			var user = context.User;
			foreach (var requirement in context.PendingRequirements)
			{
				//context.Succeed(requirement);
				//continue;
				if (requirement is ClaimsAuthorizationRequirement claimsRequirement)
				{
					var requiredClaimType = claimsRequirement.ClaimType;
					if (user.IsInRole(requiredClaimType)) context.Succeed(requirement);
					else context.Fail(new AuthorizationFailureReason(this, $"User does not have the required claims. Required claim: {requiredClaimType}"));
				}
				else if (requirement is AssertionRequirement assertRequirement)
				{
					if (await assertRequirement.Handler(context)) context.Succeed(requirement);
					else context.Fail(new AuthorizationFailureReason(this, $"User does not have the required claims. Required claim: [One of X, can't tell yet]"));
				}
			}
		}
	}

	#endregion Handle Authorization Requirements
}