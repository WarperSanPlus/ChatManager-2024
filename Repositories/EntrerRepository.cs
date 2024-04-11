using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class EntrerRepository : Repository<Entrer>
    {
        public static EntrerRepository Instance => (EntrerRepository)DB.GetRepo<Entrer>();
        public  Entrer Create(Entrer entrer)
        {
            try
            {
                entrer.Id = this.Add(entrer);
                return entrer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Add user failed : Message - {ex.Message}");
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool Delete(int EntrerId)
        {
            try
            {
                Entrer EntrerToDelete = this.Get(EntrerId);
                if (EntrerToDelete != null)
                {
                    this.BeginTransaction();

                    _ = base.Delete(EntrerId);
                    this.EndTransaction();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remove user failed : Message - {ex.Message}");
                this.EndTransaction();
                return false;
            }
        }

        public IEnumerable<Entrer> SortedUsers() => this.ToList().OrderBy(u => u.entrer);
    }
}