namespace Hwic.Storings
{
    using System;

    using System.Collections.Specialized;

    using System.IO;


    using Hwic.Abstractings;


    public class LocalFileStorageConfig : IStorageConfig
    {
        public string RootDirectory { get; }


        public bool IsCreatePathEnabled
            => this.flags_[FLAG_CREATE_DIR];


        public bool IsFileOverwriteEnabled
            => this.flags_[FLAG_OVERWRITE];


        public bool IsKeepDirStructureEnabled
            => this.flags_[FLAG_KEEPSTRUCT];


        private BitVector32 flags_;


        private const int FLAG_CREATE_DIR = 0;
        private const int FLAG_OVERWRITE  = 1;
        private const int FLAG_KEEPSTRUCT = 2;


        public LocalFileStorageConfig(
                string rootDirectory,
                bool enableCreateDir,
                bool enableOverwrite,
                bool keepResStructure)
        {
            this.RootDirectory = rootDirectory;
            this.flags_ = new BitVector32(0);

            this.flags_[FLAG_CREATE_DIR] = enableCreateDir;
            this.flags_[FLAG_OVERWRITE] = enableOverwrite;
            this.flags_[FLAG_KEEPSTRUCT] = keepResStructure;
        }
    }



    public static class LocalFileStorageConfigStreamExtensions
    {
        public static FileStream CreateFileStream(this LocalFileStorageConfig config, Uri resourceUri)
        {
            throw new NotImplementedException();
        }
    }
}
