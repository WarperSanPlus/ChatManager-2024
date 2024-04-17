using System;

namespace Attributes
{
    public class AssetAttribute : Attribute
    {
        private readonly string folder;
        private readonly string defaultValue;

        public AssetAttribute(string folder, string defaultValue = "")
        {
            this.folder = folder;
            this.defaultValue = defaultValue;
        }

        public string Folder() => this.folder;

        public string Default() => this.defaultValue;
    }
}