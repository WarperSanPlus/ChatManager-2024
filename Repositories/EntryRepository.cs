using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class EntryRepository : Repository<Entry>
    {
        public static EntryRepository Instance => (EntryRepository)DB.GetRepo<Entry>();

        public Entry Create(Entry entrer)
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
                var EntrerToDelete = this.Get(EntrerId);
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

        public Entry GetEntrer(int IDUser)
        {
            var entrerList = Instance.ToList().Where(e => e.IdUser == IDUser);

            // Trier les entrées par ordre décroissant de dateTime d'entrée
            var latestEntrer = entrerList.OrderByDescending(e => e.Start).FirstOrDefault();

            return latestEntrer;
        }

        public IEnumerable<Entry> GetEntrers() => Instance.ToList().OrderByDescending(e => e.Start);

        public void SupprimerJour(DateTime date)
        {
            try
            {
                this.BeginTransaction();

                var entrersToDelete = Instance.ToList().Where(e => e.Start.Date == date.Date).ToList();

                foreach (var entrer in entrersToDelete)
                {
                    _ = this.Delete(entrer.Id);
                }

                this.EndTransaction();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete les entrerr echouer - {ex.Message}");
                this.EndTransaction();
            }
        }

        public void UtilisateurEntrerSupprimer(int userId)
        {
            try
            {
                this.BeginTransaction();

                var entrersToDelete = Instance.ToList().Where(e => e.IdUser == userId).ToList();

                foreach (var entrer in entrersToDelete)
                {
                    _ = this.Delete(entrer.Id);
                }

                this.EndTransaction();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete usager pas bien fait - {ex.Message}");
                this.EndTransaction();
            }
        }
    }
}