using Microsoft.Data.Sqlite;

namespace Money.Services;

public static class SqliteErrorMessage
{
    public static string ToFriendly(Exception exception)
    {
        var message = exception.GetBaseException().Message;
        if (exception.GetBaseException() is SqliteException)
        {
            var marker = message.LastIndexOf(':');
            if (marker >= 0 && marker + 1 < message.Length)
                message = message[(marker + 1)..].Trim();
        }
        return message.Replace("constraint failed", "Os dados informados violam uma regra de validação.",
            StringComparison.OrdinalIgnoreCase);
    }
}
