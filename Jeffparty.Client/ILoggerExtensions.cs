using Microsoft.Extensions.Logging;

namespace Jeffparty.Client
{
    public static class ILoggerExtensions
    {
        public static void Trace(this ILogger logger, [System.Runtime.CompilerServices.CallerMemberName]
            string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]
            string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber]
            int sourceLineNumber = 0)
        {
            logger.LogTrace($"{sourceFilePath} {memberName}:{sourceLineNumber}");
        }
    }
}