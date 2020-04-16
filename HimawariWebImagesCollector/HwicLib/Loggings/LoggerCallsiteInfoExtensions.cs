namespace Hwic.Loggings
{
    using System.IO;

    using System.Runtime.CompilerServices;


    /// <summary>
    /// https://stackoverflow.com/a/46905798/1005716
    /// </summary>
    public static class LoggerCallsiteInfoExtensions
    {
        public static Serilog.ILogger Here(this Serilog.ILogger logger,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            return logger
                .ForContext("MemberName", memberName)
                .ForContext("FilePath", Path.GetFileName(sourceFilePath))
                .ForContext("LineNumber", sourceLineNumber);
        }
    }
}
