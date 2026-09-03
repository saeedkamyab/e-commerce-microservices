using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Authentication;

public interface IAccessTokenProvider
{
    string Create(User user);
}
