using Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Repositories
{
    ///////////////////////////////////////////////////////////////
    // Ce patron de classe permet de stocker dans un fichier JSON
    // une collection d'objects. Ces derniers doivent posséder
    // la propriété int Id {get; set;}
    // Après l'instanciation il faut invoquer la méthode Init
    // pour fournir le chemin d'accès du fichier JSON.
    //
    // Tous les membres annotés avec [asset] seront traités
    // en tant que données hors BD
    //
    // Voir dans global.asax
    //
    // Author : Nicolas Chourot
    // date: Janvier 2024
    ///////////////////////////////////////////////////////////////
    public class Repository<T> : Repository
    {
        #region Private

        /// <summary>
        /// Représente le nombre de transactions imbriquées
        /// </summary>
        private static int NestedTransactionsCount = 0;

        // cache des données du fichier JSON
        private List<T> dataList;

        // retourne la valeur de l'attribut attributeName de l'intance data de classe T
        private object GetAttributeValue(T data, string attributeName) => data.GetType().GetProperty(attributeName).GetValue(data, null);

        // affecter la valeur de l'attribut attributeName de l'intance data de classe T
        private void SetAttributeValue(T data, string attributeName, object value) => data.GetType().GetProperty(attributeName).SetValue(data, value, null);

        // Vérifier si l'attribut attributeName est présent dans la classe T
        private bool AttributeNameExist(string attributeName) => Activator.CreateInstance(typeof(T)).GetType().GetProperty(attributeName) != null;

        private void DeleteAssets(T data)
        {
            Type type = data.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                Attribute attribute = property.GetCustomAttribute(typeof(ImageAssetAttribute));
                if (attribute == null)
                    continue;

                var value = this.GetAttributeValue(data, property.Name).ToString();

                var assetsFolder = ((ImageAssetAttribute)attribute).Folder();
                var masterPath = HostingEnvironment.MapPath("~/" + assetsFolder);

                File.Delete(masterPath + "/" + value);
            }
        }

        private void HandleAssetMembers(T data)
        {
            // Get previous item
            T prevData = this.Get(this.Id(data));

            Type type = data.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                Attribute attribute = property.GetCustomAttribute(typeof(ImageAssetAttribute));

                // If not ImageAssetAttribute, skip
                if (!(attribute is ImageAssetAttribute asset))
                    continue;

                byte[] bytes;
                string extension;

                var propertyValue = property.GetValue(data).ToString();

                // If the given value is base64
                if (propertyValue.Contains("base64"))
                {
                    // Remove beginning tag
                    propertyValue = propertyValue.Replace("data:image/", "");

                    // Get bytes
                    bytes = Convert.FromBase64String(propertyValue.Split(',')[1]);

                    // Get extension
                    extension = propertyValue.Split(';')[0];
                }
                // Null value
                else
                {
                    var path = HostingEnvironment.MapPath("~/" + asset.DefaultValue());

                    // Get bytes
                    bytes = File.Exists(path)
                        ? File.ReadAllBytes(path)
                        : throw new ArgumentNullException($"No valid image was provided for the new item of type \'{type.Name}\'.");

                    // Get extension
                    extension = path.Split('.').Last();
                }

                // Get path
                var masterPath = HostingEnvironment.MapPath("~/" + asset.Folder());

                // Delete previous asset
                if (prevData != null)
                    File.Delete(masterPath + "/" + property.GetValue(prevData));

                var fileName = Guid.NewGuid().ToString() + "." + extension;

                // Update path
                property.SetValue(data, fileName, null);

                // Create image
                File.WriteAllBytes(masterPath + "/" + fileName, bytes);
            }
        }

        // retourne la valeur de l'attribut Id d'une instance de classe T
        private int Id(T data) => (int)this.GetAttributeValue(data, "Id");

        // Mise à jour du fichier JSON avec les données présentes dans la cache dataList
        private void UpdateFile()
        {
            if (this.dataList == null)
                return;

            try
            {
                using (var sw = new StreamWriter(this.FilePath))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(this.dataList));
                }

                this.ReadFile();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Private

        #region Protected

        protected override void ReadFile()
        {
            this.GetNewSerialNumber();
            this.dataList?.Clear();
            try
            {
                using (var sr = new StreamReader(this.FilePath))
                {
                    this.dataList = JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd());
                }

                this.dataList = this.dataList ?? new List<T>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override int NextId() => this.dataList == null || this.dataList.Count == 0 ? 1 : this.dataList.Max(this.Id) + 1;

        #endregion Protected

        #region Public

        // constructeur
        public Repository()
        {
            this.dataList = new List<T>();
            try
            {
                // s'assurer que la propriété int Id {get; set;} est belle et bien dans la classe T
                var idExist = this.AttributeNameExist("Id");
                if (!idExist)
                    throw new Exception("The class Repository cannot work with types that does not contain an attribute named Id of type int.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // Méthodes CRUD
        // Read
        public T Get(int Id) => this.dataList.FirstOrDefault(d => this.Id(d) == Id);

        public List<T> ToList() => this.dataList;

        public int Count => this.dataList.Count;

        // Update
        public virtual bool Update(T data)
        {
            var success = false;
            if (!TransactionOnGoing)
                _ = mutex.WaitOne();
            try
            {
                T dataToUpdate = this.Get(this.Id(data));
                if (dataToUpdate != null)
                {
                    var index = this.dataList.IndexOf(dataToUpdate);
                    this.HandleAssetMembers(data);
                    this.dataList[index] = data;
                    this.UpdateFile();
                    success = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing)
                    mutex.ReleaseMutex();
            }

            return success;
        }

        // Delete
        public virtual bool Delete(int Id)
        {
            var success = false;
            if (!TransactionOnGoing)
                _ = mutex.WaitOne();
            try
            {
                T dataToDelete = this.Get(Id);

                if (dataToDelete != null)
                {
                    this.DeleteAssets(dataToDelete);
                    var index = this.dataList.IndexOf(dataToDelete);
                    this.dataList.RemoveAt(index);
                    this.UpdateFile();
                    success = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing)
                    mutex.ReleaseMutex();
            }

            return success;
        }

        public override int Add(object data)
        {
            var newData = (T)data;

            var newId = 0;
            if (!TransactionOnGoing)
                _ = mutex.WaitOne(); // attendre la conclusion d'un appel concurrant
            try
            {
                newId = this.NextId();
                this.SetAttributeValue(newData, "Id", newId);
                this.HandleAssetMembers(newData);
                this.dataList.Add(newData);
                this.UpdateFile();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing)
                    mutex.ReleaseMutex();
            }

            return newId;
        }

        public void BeginTransaction()
        {
            if (!TransactionOnGoing) // todo check if nested transactions still work
            {
                _ = mutex.WaitOne();
                TransactionOnGoing = true;
            }
            else
            {
                NestedTransactionsCount++;
            }
        }

        public void EndTransaction()
        {
            if (NestedTransactionsCount <= 0)
            {
                TransactionOnGoing = false;
                mutex.ReleaseMutex();
            }
            else
            {
                if (NestedTransactionsCount > 0)
                    NestedTransactionsCount--;
            }
        }

        #endregion Public
    }

    public abstract class Repository
    {
        #region Private

        /// <summary>
        /// Numéro de série des données
        /// </summary>
        private string _SerialNumber;

        #endregion Private

        #region Protected

        /// <summary>
        /// Chemin d'accès absolu du fichier
        /// </summary>
        protected string FilePath;

        /// <summary>
        /// Détermine si une transaction est en cours
        /// </summary>
        protected static bool TransactionOnGoing = false;

        /// <summary>
        /// Utilisé pour prévenir des conflits entre processus
        /// </summary>
        protected static readonly Mutex mutex = new Mutex();

        protected abstract void ReadFile();

        protected abstract int NextId();

        protected void GetNewSerialNumber() => this._SerialNumber = Guid.NewGuid().ToString();

        #endregion Protected

        #region Public

        public bool HasChanged
        {
            get
            {
                var key = this.GetType().Name;
                if ((string)HttpContext.Current.Session[key] != this._SerialNumber)
                {
                    HttpContext.Current.Session[key] = this._SerialNumber;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initialise le <see cref="Repository"/> à partir du chemin donné
        /// </summary>
        public void Init(string filePath)
        {
            if (!TransactionOnGoing)
                _ = mutex.WaitOne();

            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException("FilePath not set exception");

                this.FilePath = filePath;

                if (!File.Exists(this.FilePath))
                {
                    using (StreamWriter sw = File.CreateText(this.FilePath))
                    {
                        sw.Close();
                    }
                }

                this.ReadFile();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing)
                    mutex.ReleaseMutex();
            }
        }

        // Create
        public abstract int Add(object data);

        #endregion Public
    }
}