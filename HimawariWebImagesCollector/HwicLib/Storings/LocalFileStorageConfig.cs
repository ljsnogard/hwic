namespace Hwic.Storings
{
    using System;

    using System.Collections.Generic;
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


        public LocalFileStorageConfig(string rootDirectory) :
            this(rootDirectory, true, true, true)
        { }
    }



    public static class LocalFileStorageConfigStreamExtensions
    {
        public static FileStream CreateFileStream(
                this LocalFileStorageConfig config,
                Uri resourceUri)
        {
            DirectoryInfo rootDir;
            if (false == File.Exists(config.RootDirectory))
            {
                if (false == config.IsCreatePathEnabled)
                    throw new FileNotFoundException($"Directory {config.RootDirectory} does not exists, and creation not enabled.");
                else
                    rootDir = Directory.CreateDirectory(config.RootDirectory);
            }
            else
            {
                var pathAttr = File.GetAttributes(config.RootDirectory);
                if ((pathAttr & FileAttributes.Directory) != FileAttributes.Directory)
                    throw new DirectoryNotFoundException($"Path {config.RootDirectory} already exists and not a directory");
                else
                    rootDir = Directory.CreateDirectory(config.RootDirectory);
            }
            if (config.IsCreatePathEnabled && config.IsKeepDirStructureEnabled)
            {
                var paramsList = new List<string> { config.RootDirectory };
                foreach (var fldename in resourceUri.AbsolutePath.Split('/'))
                    paramsList.Add(fldename);

                var folderPath = Path.GetDirectoryName(Path.Combine(paramsList.ToArray()));

                var folder = Directory.CreateDirectory(folderPath);
                var fileName = Path.GetFileName(resourceUri.AbsolutePath);

                var filePath = Path.Combine(folderPath, fileName);

                if (File.Exists(filePath))
                {
                    if (false == config.IsFileOverwriteEnabled)
                        throw new IOException($"File {filePath} already exists and overwrite is not enabled.");
                    return new FileStream(filePath, FileMode.Truncate, FileAccess.Write);
                }
                else
                {
                    return new FileStream(filePath, FileMode.Create, FileAccess.Write);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
