namespace Hwic.Loggings
{
    using System;


    using Serilog;
    using Serilog.Events;


    internal static class GetLoggerExtensions
    {
        private static readonly Lazy<ILogger> lazyGlobalLogger_ =
            new Lazy<ILogger>(GetLoggerExtensions.InitLogger_);


        public static ILogger GetLogger<T>(this T _)
            => lazyGlobalLogger_.Value.ForContext<T>();


        private static ILogger InitLogger_()
        {
            var outputTemplate = 

"{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u4} [{SourceContext} {MemberName}] at {FilePath} #{LineNumber}:{NewLine}{Message}{NewLine}{Exception}{NewLine}";

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(
                    "serilog/HwicLib-{Date}.log",
                    LogEventLevel.Verbose,
                    outputTemplate
                )
                .CreateLogger();
        }
    }
}
