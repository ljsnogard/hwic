namespace Hwic
{
    using CommandLine;


    [Verb("sync", HelpText = "Synchronize images from Himawari sources to remote S3")]
    public class SyncOptions
    {
        [Option('s', "src", Required = true, HelpText = "Specifiy the source type of Himawari images.")]
        public string Source { get; set; }


        [Option('d', "dst", Required = true, HelpText = "Specify the predefined destination name where the images will be stored")]
        public string Dest { get; set; }


        [Option("ak", Required = false, HelpText = "Access Key to S3")]
        public string AccessKey { get; set; }


        [Option("sk", Required = false, HelpText = "Secret key paired with access key to S3")]
        public string SecretKey { get; set; } 


        [Option("src-proxy", Required = false)]
        public string SrcProxy { get; set; }


        [Option("dst-proxy", Required = false)]
        public string DstProxy { get; set; }


        [Option("all-proxy", Required = false)]
        public string AllProxy { get; set; }


        [Option("dry-run", Default = false)]
        public bool DryRun { get; set; }


        [Option('o', "output", Required = false, HelpText = "Output the Uris needed to synchronize to a file")]
        public string Output { get; set; }
    }
}
