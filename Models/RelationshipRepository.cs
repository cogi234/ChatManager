using ChatManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace MoviesDBManager.Models
{
    public class RelationshipRepository
    {
        #region "Méthodes et propritées privées"
        // Pour indiquer si une transaction est en cours
        static bool TransactionOnGoing = false;
        // Pour la gestion d'imbrications de transactions
        static int NestedTransactionsCount = 0;
        // utilisé pour prévenir des conflits entre processus
        static private readonly Mutex mutex = new Mutex();
        // cache des données du fichier JSON
        private List<Relationship> dataList = new List<Relationship>();
        // chemin d'accès absolue du fichier JSON
        private string FilePath;
        // Numéro de serie des données
        private string _SerialNumber;
        // Lecture du fichier JSON et conservation des données dans la cache dataList
        private void ReadFile()
        {
            MarkHasChanged();
            dataList.Clear();
            try
            {
                using (var sr = new StreamReader(FilePath))
                    dataList = JsonConvert.DeserializeObject<List<Relationship>>(sr.ReadToEnd());
            }
            catch (Exception e)
            {
                throw e;
            }
            if (dataList == null)
                dataList = new List<Relationship>();
        }
        // Mise à jour du fichier JSON avec les données présentes dans la cache dataList
        private void UpdateFile()
        {
            try
            {
                using (var sw = new StreamWriter(FilePath))
                    sw.WriteLine(JsonConvert.SerializeObject(dataList));
                ReadFile();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region "Méthodes publiques"
        public bool HasChanged
        {
            get
            {
                string key = this.GetType().Name;
                if (((string)HttpContext.Current.Session[key] != _SerialNumber))
                {
                    HttpContext.Current.Session[key] = _SerialNumber;
                    return true;
                }
                return false;
            }
        }
        public void BeginTransaction()
        {
            if (!TransactionOnGoing) // todo check if nested transactions still work
            {
                mutex.WaitOne();
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
        // Init : reçoit le chemin d'accès absolue du fichier JSON
        // Cette méthode doit avoir été appelée avant tout
        public virtual void Init(string filePath)
        {
            if (!TransactionOnGoing) mutex.WaitOne();
            try
            {
                FilePath = filePath;
                if (string.IsNullOrEmpty(FilePath))
                {
                    throw new Exception("FilePath not set exception");
                }
                if (!File.Exists(FilePath))
                {
                    using (StreamWriter sw = File.CreateText(FilePath)) { }
                }
                ReadFile();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (!TransactionOnGoing) mutex.ReleaseMutex();
            }
        }
        public virtual void MarkHasChanged()
        {
            _SerialNumber = Guid.NewGuid().ToString();
        }

        // Méthodes CRUD

        // Read all
        public List<Relationship> ToList() => dataList;
        // Read one
        public Relationship Get((int from, int to) id)
        {
            Relationship relationship = dataList.FirstOrDefault(d => d.Id == id);
            if (relationship == null)
            {
                relationship = new Relationship();
                relationship.Id = id;
                relationship.Status = RelationshipStatus.None;
            }
            return relationship;
        }
        public bool Exists((int from, int to) id) => dataList.FirstOrDefault(d => d.Id == id) != null;
        // Create one
        public virtual (int from, int to) Add(Relationship data)
        {
            if (!TransactionOnGoing) mutex.WaitOne(); // attendre la conclusion d'un appel concurrant
            try
            {
                //If there's already an element with this id, we throw an exception
                if (dataList.Any((element) => element.Id == data.Id))
                    throw new Exception();

                dataList.Add(data);
                UpdateFile();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (!TransactionOnGoing) mutex.ReleaseMutex();
            }
            return data.Id;
        }
        // Update one
        public virtual void Update(Relationship data)
        {
            if (!TransactionOnGoing) mutex.WaitOne();
            try
            {
                if (Exists(data.Id))
                {
                    Relationship dataToUpdate = Get(data.Id);
                    dataList[dataList.IndexOf(dataToUpdate)] = data;
                    UpdateFile();
                } else
                {
                    Add(data);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (!TransactionOnGoing) mutex.ReleaseMutex();
            }
        }
        // Delete one
        public virtual bool Delete((int from, int to) Id)
        {
            if (!TransactionOnGoing) mutex.WaitOne();
            try
            {
                Relationship dataToDelete = Get(Id);
                if (dataToDelete != null)
                {
                    dataList.RemoveAt(dataList.IndexOf(dataToDelete));
                    UpdateFile();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (!TransactionOnGoing) mutex.ReleaseMutex();
            }
            return false;
        }

        #endregion
    }
}