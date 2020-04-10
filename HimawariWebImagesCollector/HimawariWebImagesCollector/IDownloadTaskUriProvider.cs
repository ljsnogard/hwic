namespace Hwic
{
    using System;
    using System.Collections.Generic;


    public interface IDownloadTaskUriProvider
    {
        IReadOnlyCollection<Uri> GetUri<TConfig>(
            in TConfig config,
            in DateTimeOffset time
        );
    }
}
