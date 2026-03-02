using IdentityService.Application.Models;

namespace IdentityService.Application.Queries.GetUserById;

public sealed record GetUserByIdResult(UserDetailsDto User);
