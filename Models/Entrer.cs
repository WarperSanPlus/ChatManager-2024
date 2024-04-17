using Newtonsoft.Json;
using System;

namespace Models
{
    public class Entrer
    {
        public int Id { get; set; } = 0;
        public int IdUser { get; set; } = 0;
        public DateTime entrer { get; set; } = DateTime.Now;
        public DateTime? sortie { get; set; } = null;

        [JsonIgnore]
        public User User { get; set; }

        /// <returns>Is this entry still active?</returns>
        public bool IsFinised() => this.sortie != null;

        public bool IsValid() => this.entrer != this.sortie;
    }

    //public class Journer
    //{
    //    public DateTime Jour { get; set; } = DateTime.Now;
    //    public List<Entrer> listeEntrer { get; set; } = new List<Entrer>();

    //}
}