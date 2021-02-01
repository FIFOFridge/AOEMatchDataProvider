using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services
{
    public interface IStorageService
    {
        //void Set(string key, object value, bool keepForSession = false, DateTime? dateTime = null);
        void Create(string key, object value, StorageEntryExpirePolicy expirePolicy, DateTime? expires = null);

        void UpdateExpireDate(string key, DateTime expires);
        void Update(string key, object value);

        void Remove(string key);

        bool Has(string key);

        object Get(string key);
        T Get<T>(string key);

        bool TryGet(string key, out object value);
        bool TryGet<T>(string key, out T value);

        IEnumerable<string> GetKeysByPrefix(string prefix);

        StorageEntryExpirePolicy GetExpirePolicy(string key);

        void Flush();
        void Load();
        //bool TryLoad();
    }

    public enum StorageEntryExpirePolicy
    {
        AfterSession, //always expire after container GC or via data override by Load() method
        Never, //never expire
        Expire //expire according to provided date time
    }

    public class EntryExpiredException : Exception
    {
        public EntryExpiredException()
        {
        }

        public EntryExpiredException(string message) : base(message)
        {
        }

        public EntryExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntryExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
