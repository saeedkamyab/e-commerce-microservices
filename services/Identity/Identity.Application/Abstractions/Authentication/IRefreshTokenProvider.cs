namespace Identity.Application.Abstractions.Authentication;

public interface IRefreshTokenProvider
{
    public string Generate();

    public string Hash(string token);
}
