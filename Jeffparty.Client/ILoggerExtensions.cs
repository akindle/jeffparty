using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client
{
    public static class ILoggerExtensions
    {
        public static void Trace(this ILogger logger,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogTrace($"{sourceFilePath} {memberName}:{sourceLineNumber}");
        }

        public static void Trace(this ILogger logger, string message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            logger.LogTrace($"{sourceFilePath} {memberName}:{sourceLineNumber}: {message}");
        }
    }
}