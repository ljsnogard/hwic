namespace Hwic.Storings
{
    using System;


    public sealed class S3Provider : IEquatable<S3Provider>
    {
        public string Name { get; }


        public string EndPoint { get; }


        public S3Provider(string name, string endpoint)
        {
            this.Name = name;
            this.EndPoint = endpoint;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return
                    (this.Name.GetHashCode() * 13137) ^
                    (this.EndPoint.GetHashCode() * 3731) ^
                    typeof(S3Provider).GetHashCode();
            }
        }


        public bool Equals(S3Provider other)
        {
            return
                this.Name.Equals(other.Name, StringComparison.Ordinal) &&
                this.EndPoint.Equals(other.EndPoint, StringComparison.Ordinal);
        }
    }
}
