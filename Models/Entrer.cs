using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public class Entrer
    {
       
        public int Id { get; set; } = 0;
        public DateTime entrer { get; set; } = DateTime.Now;

        public DateTime? sortie { get; set; } = null;
    }



    //public class Journer
    //{
    //    public DateTime Jour { get; set; } = DateTime.Now;
    //    public List<Entrer> listeEntrer { get; set; } = new List<Entrer>();
 
    //}
}