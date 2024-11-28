using Microsoft.AspNetCore.Authorization;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.Authorization;

/// <summary>
/// Custom authorization attribute that enforces minimum access level requirements.
/// This attribute is used to protect endpoints based on the hierarchical access level system.
/// </summary>
/// <remarks>
/// Usage examples:
/// [MinimumAccessLevel(AccessLevel.Member)] - Requires Member level or higher
/// [MinimumAccessLevel(AccessLevel.Admin)] - Requires Admin level only
///
/// The attribute works with the hierarchical access level system where:
/// Guest (0), Member (10), Owner (20), Admin (30)
/// </remarks>
public class MinimumAccessLevelAttribute : AuthorizeAttribute {
    /// <summary>
    /// Initializes a new instance of the MinimumAccessLevelAttribute.
    /// </summary>
    /// <param name="minimumLevel">The minimum AccessLevel required to access the endpoint.
    /// Users with this level or higher will be granted access.</param>
    /// <remarks>
    /// The constructor creates a policy name that matches the authorization policies
    /// defined in Program.cs (e.g., "MinimumAccessLevel_Member").
    /// </remarks>
    public MinimumAccessLevelAttribute(AccessLevel minimumLevel) {
        Policy = $"MinimumAccessLevel_{minimumLevel}";
    }
}
