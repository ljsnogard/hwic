namespace Hwic.Storings
{
    public readonly struct S3Provider
    {
        public string Name { get; }


        public S3Provider(string name)
            => this.Name = name;
    }
}
