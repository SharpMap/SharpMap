namespace SharpMap.Demo.Wms.Models
{
    using System;

    public class DemoItem : IEquatable<DemoItem>
    {
        public DemoItem(string name, string url)
        {
            if (name == null) 
                throw new ArgumentNullException("name");
            if (url == null) 
                throw new ArgumentNullException("url");
            this.Name = name;
            this.Url = url;
        }

        public string Name { get; set; }

        public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Url: {1}", this.Name, this.Url);
        }

        public bool Equals(DemoItem other)
        {
            if (ReferenceEquals(null, other)) 
                return false;
            if (ReferenceEquals(this, other)) 
                return true;
            return Equals(other.Name, this.Name) && Equals(other.Url, this.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            if (obj.GetType() != typeof(DemoItem)) 
                return false;
            return this.Equals((DemoItem)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Name.GetHashCode() * 397) ^ this.Url.GetHashCode();
            }
        }    
    }
}