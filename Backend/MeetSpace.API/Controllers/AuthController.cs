using System.Collections.Immutable;
using System.Security.Claims;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MeetSpace.API.Controllers;

/// <summary>
/// OAuth2 token endpoint — issues access tokens via the Resource Owner Password Credentials grant.
/// POST /connect/token
///   Content-Type: application/x-www-form-urlencoded
///   grant_type=password&amp;client_id=MeetSpace-angular&amp;username={email}&amp;password={pass}&amp;scope=openid profile roles
/// </summary>
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("~/connect/token")]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (!request.IsPasswordGrantType())
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(
                    new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.UnsupportedGrantType,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Only the password grant type is supported."
                    }));

        if (string.IsNullOrEmpty(request.ClientId))
            return InvalidRequest("The client_id parameter is required.");

        if (string.IsNullOrEmpty(request.Username))
            return InvalidRequest("The username parameter is required.");

        if (string.IsNullOrEmpty(request.Password))
            return InvalidRequest("The password parameter is required.");

        // Resolve user by email or username
        var user = await _userManager.FindByEmailAsync(request.Username)
                   ?? await _userManager.FindByNameAsync(request.Username);

        if (user is null || !user.IsActive)
            return InvalidGrant("The email/password combination is invalid.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return InvalidGrant("The email/password combination is invalid.");

        // Build claims identity
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        var roles = await _userManager.GetRolesAsync(user);

        identity
            .SetClaim(Claims.Subject, user.Id)
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(Claims.Name, user.UserName)
            .SetClaim(Claims.GivenName, user.FirstName)
            .SetClaim(Claims.FamilyName, user.LastName)
            .SetClaims(Claims.Role, roles.ToImmutableArray());

        // Every claim must declare where it is included (access token and/or id token)
        identity.SetDestinations(claim => claim.Type switch
        {
            Claims.Subject or Claims.Name or Claims.Email
                or Claims.GivenName or Claims.FamilyName
                => [Destinations.AccessToken, Destinations.IdentityToken],

            Claims.Role => [Destinations.AccessToken, Destinations.IdentityToken],

            _ => [Destinations.AccessToken]
        });

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private IActionResult InvalidRequest(string description) =>
        Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(
                new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
                }));

    private IActionResult InvalidGrant(string description) =>
        Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties(
                new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
                }));
}
