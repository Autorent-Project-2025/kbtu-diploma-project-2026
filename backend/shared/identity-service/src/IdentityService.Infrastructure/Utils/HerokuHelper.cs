namespace IdentityService.Infrastructure.Utils;

public static class HerokuHelper
{
    public static string BuildConnectionString(string databaseUrl)
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo[1];
        var host = databaseUri.Host;
        var port = databaseUri.Port;
        var database = databaseUri.AbsolutePath.Trim('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
