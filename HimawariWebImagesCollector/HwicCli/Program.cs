namespace Hwic
{
    using System;

    using System.Collections.Generic;

    using System.Threading;
    using System.Threading.Tasks;


    using CommandLine;


    public class Program
    {
        public const string K_CONFIG_FILE = "config-file";


        public static Task Main(string[] args)
        {
            return CommandLine.Parser.Default
                .ParseArguments<SyncOptions>(args)
                .MapResult(
                    RunWithSyncOptionsAsync_,
                    HandleParseErrorsAsync_
                );
        }


        private static async Task<int> RunWithSyncOptionsAsync_(SyncOptions syncOptions)
        {
            await Task.CompletedTask;
            return 0;
        }


        private static Task<int> HandleParseErrorsAsync_(IEnumerable<CommandLine.Error> errors)
        {
            return Task.FromResult(-1);
        }
    }
}
