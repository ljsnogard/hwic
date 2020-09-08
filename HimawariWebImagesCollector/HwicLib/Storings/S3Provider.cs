namespace Hwic.Storings
{
    public sealed class S3Provider
    {
        public string Name { get; }


        public string EndPoint { get; }


        public S3Provider(string name, string endpoint)
        {
            this.Name = name;
            this.EndPoint = endpoint;
        }
    }
}
