using ChatManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
namespace MoviesDBManager.Models
{
    public class EntrerRepository: Repository<Entrer>
    {




        public Entrer Create(Entrer entrer)
        {
            try
            {
                entrer.Id = base.Add(entrer);
                return entrer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add user failed : Message - {ex.Message}");
            }
            return null;
        }
        public override bool Update(Entrer entrer)
        {
            try
            {
                return base.Update(entrer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update user failed : Message - {ex.Message}");
            }
            return false;
        }
        public override bool Delete(int EntrerId)
        {
            try
            {
                Entrer EntrerToDelete = DB.Entrers.Get(EntrerId);
                if (EntrerToDelete != null)
                {
                    BeginTransaction();
                  
                    base.Delete(EntrerId);
                    EndTransaction();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remove user failed : Message - {ex.Message}");
                EndTransaction();
                return false;
            }
        }

        public IEnumerable<Entrer> SortedUsers()
        {
            return ToList().OrderBy(u => u.entrer);
        }

    }
}