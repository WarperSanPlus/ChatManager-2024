﻿using System;

namespace Attributes
{
    public sealed class ImageAssetAttribute : Attribute
    {
        private readonly string folder;
        private readonly string defaultValue;

        public ImageAssetAttribute(string folder, string defaultValue)
        {
            this.folder = folder;
            this.defaultValue = defaultValue;
        }

        public string Folder() => this.folder;

        public string DefaultValue() => this.defaultValue;
    }
}