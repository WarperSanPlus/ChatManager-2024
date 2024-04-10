using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;

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

    public class ImageAsset<T>
    {
        private object GetAttributeValue(T data, string attributeName) => data.GetType().GetProperty(attributeName).GetValue(data, null);
        // affecter la valeur de l'attribut attributeName de l'intance data de classe T
        private void SetAttributeValue(T data, string attributeName, object value) => data.GetType().GetProperty(attributeName).SetValue(data, value, null);
        private bool IsBase64Value(string value)
        {
            var isBase64 = value.Contains("data:") && value.Contains(";base64,");
            return isBase64;
        }
        public void Delete(T data)
        {
            Type type = data.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                Attribute attribute = property.GetCustomAttribute(typeof(ImageAssetAttribute));

                if (attribute != null)
                {
                    var assetsFolder = ((ImageAssetAttribute)attribute).Folder();
                    var defaultValue = ((ImageAssetAttribute)attribute).DefaultValue();
                    var propName = property.Name;
                    var value = this.GetAttributeValue(data, propName).ToString();
                    if (value != null && value != assetsFolder + defaultValue)
                        File.Delete(HostingEnvironment.MapPath(value).ToString());
                }
            }
        }
        public void Update(T data)
        {
            Type type = data.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                var assetAttribute = (ImageAssetAttribute)property.GetCustomAttribute(typeof(ImageAssetAttribute), true);

                if (assetAttribute != null)
                {
                    var propName = property.Name;
                    var propValue = this.GetAttributeValue(data, propName).ToString() ?? "";
                    if (this.IsBase64Value(propValue)) // new image
                    {
                        var assetsFolder = assetAttribute.Folder();
                        var defaultValue = assetAttribute.DefaultValue();
                        var previousAssetURL = propValue.Split('|')[0];
                        var imagePart = propValue.Split('|')[1];
                        if (previousAssetURL != "" && previousAssetURL != assetsFolder + defaultValue)
                            File.Delete(HostingEnvironment.MapPath(previousAssetURL));
                        var base64Data = imagePart.Split(',');
                        var extension = base64Data[0].Replace(";base64", "").Split('/')[1];
                        // IIS mime patch : does not serve webp and avif mimes
                        if (extension.ToLower() == "webp")
                            extension = "png";
                        if (extension.ToLower() == "avif")
                            extension = "png";
                        var assetData = base64Data[1];
                        string assetUrl;
                        string newAssetServerPath;
                        do
                        {
                            var key = Guid.NewGuid().ToString();
                            assetUrl = assetsFolder + key + "." + extension;
                            newAssetServerPath = HostingEnvironment.MapPath(assetUrl);
                            // make sure new file does not already exists 
                        } while (File.Exists(newAssetServerPath));
                        this.SetAttributeValue(data, propName, assetUrl);
                        using (var stream = new MemoryStream(Convert.FromBase64String(assetData)))
                        {
                            using (var file = new FileStream(newAssetServerPath, FileMode.Create, FileAccess.Write))
                                stream.WriteTo(file);
                        }
                    }
                }
            }
        }
    }
}