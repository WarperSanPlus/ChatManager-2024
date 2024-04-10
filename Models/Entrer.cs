using System;

namespace Models
{
    public class Entrer
    {

        public int Id { get; set; } = 0;
        public int IdUser { get; set; } = 0;
        public DateTime entrer { get; set; } = DateTime.Now;

        public DateTime? sortie { get; set; } = null;
    }

    //public class Journer
    //{
    //    public DateTime Jour { get; set; } = DateTime.Now;
    //    public List<Entrer> listeEntrer { get; set; } = new List<Entrer>();

    //}
}