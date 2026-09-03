namespace Identity.Application.Abstractions.Authentication;

public interface ICurrentUser
{
   public Guid UserId { get; }
}
