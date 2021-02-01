using AOEMatchDataProvider.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services.Test
{
    //public class VoidStorageTestingService : IStorageService
    //{
    //    Dictionary<string, object> items = new Dictionary<string, object>();

    //    public VoidStorageTestingService() { }

    //    public void Flush()
    //    {
    //        // supress
    //    }

    //    public object Get(string key)
    //    {
    //        return items[key];
    //    }

    //    public T Get<T>(string key)
    //    {
    //        return (T)items[key];
    //    }

    //    public IEnumerable<string> GetKeysByPrefix(string prefix)
    //    {
    //        return items.Keys.Where((k, v) => k.StartsWith(prefix));
    //    }

    //    public bool Has(string key)
    //    {
    //        return items.ContainsKey(key);
    //    }

    //    public void Load()
    //    {
    //        //supress
    //    }

    //    public void Set(string key, object value, bool keepForSession = false, DateTime? dateTime = null)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool TryGet(string key, out object value)
    //    {
    //        try
    //        {
    //            value = items[key];
    //            return true; 
    //        }
    //        catch (Exception e) //not nice
    //        {
    //            value = null;
    //            return false;
    //        }
    //    }

    //    public bool TryGet<T>(string key, out object value)
    //    {
    //        try
    //        {
    //            value = items[key];
    //            return true;
    //        }
    //        catch (Exception e) //not nice
    //        {
    //            value = null;
    //            return false;
    //        }
    //    }
    //}
}
