using IdentityService.Application.Models;

namespace IdentityService.Application.Queries.GetUsers;

public sealed record GetUsersResult(IReadOnlyCollection<UserDetailsDto> Users);
