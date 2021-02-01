using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services.Test
{
    public class StorageTestingService : IStorageService
    {
        public void Create(string key, object value, StorageEntryExpirePolicy expirePolicy, DateTime? expires = null)
        {
            //throw new NotImplementedException();
        }

        public void Flush()
        {
            //throw new NotImplementedException();
        }

        public object Get(string key)
        {
            //throw new NotImplementedException();
            return null;
        }

        public T Get<T>(string key)
        {
            return default(T);
        }

        public StorageEntryExpirePolicy GetExpirePolicy(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetKeysByPrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        public bool Has(string key)
        {
            //throw new NotImplementedException();
            return false;
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string key, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string key, out T value)
        {
            throw new NotImplementedException();
        }

        public void Update(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void UpdateExpireDate(string key, DateTime expires)
        {
            throw new NotImplementedException();
        }
    }
}
