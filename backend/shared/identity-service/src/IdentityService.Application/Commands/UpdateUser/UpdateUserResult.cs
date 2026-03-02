using IdentityService.Application.Models;

namespace IdentityService.Application.Commands.UpdateUser;

public sealed record UpdateUserResult(UserDetailsDto User);
