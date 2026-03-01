using Npgsql;

namespace BookingService.Infrastructure.Utils
{
    public static class HerokuHelper
    {
        public static string BuildConnectionString(string databaseUrl)
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new ArgumentNullException(nameof(databaseUrl));

            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
            };

            return builder.ToString();
        }
    }
}
