using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Authentication;

public interface IAccessTokenProvider
{
   public string Create(User user);
}
