using AOEMatchDataProvider.Extensions;
using AOEMatchDataProvider.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Services.Default
{
    internal class StorageService : IStorageService
    {
        public const string fileStorageExtension = ".json";

        readonly Dictionary<string, ValueWrapper> storageEntries = new Dictionary<string, ValueWrapper>();

        IAppConfigurationService AppConfigurationService { get; }
        ILogService LogService { get; }

        public StorageService(IAppConfigurationService appConfigurationService, ILogService logService)
        {
            AppConfigurationService = appConfigurationService;
            LogService = logService;
        }

        public bool TryGet(string key, out object value)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                if (storageEntries.ContainsKey(key))
                {
                    value = storageEntries[key].Value;
                    return true;
                }

                value = null;
                
                return false;
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                if (storageEntries.ContainsKey(key))
                {
                    value = (T)storageEntries[key].Value;
                    return true;
                }

                value = default(T);
                return false;
            }
        }

        public void Flush()
        {
            if (!Directory.Exists(AppConfigurationService.StorageDirectory))
                Directory.CreateDirectory(AppConfigurationService.StorageDirectory);

            lock(((ICollection)storageEntries).SyncRoot)
            {
                var storageCopy = new Dictionary<string, ValueWrapper>(storageEntries);

                foreach (var keyValuePair in storageCopy) //make sure to use a copy of original
                {
                    string serialized = null;

                    if (
                        //skip if entry is only for current app session
                        keyValuePair.Value.StorageEntryExpirePolicy == StorageEntryExpirePolicy.AfterSession ||
                        //skip if entry is expired
                        (keyValuePair.Value.StorageEntryExpirePolicy == StorageEntryExpirePolicy.Expire && keyValuePair.Value.Expired)
                        )
                        continue;

                    serialized = JsonConvert.SerializeObject(keyValuePair.Value);
                    File.WriteAllText(
                        Path.Combine( //${storage}/{key}.{storageExtension}
                            AppConfigurationService.StorageDirectory,
                            keyValuePair.Key + fileStorageExtension),
                        serialized);
                }
            }
        }

        public void Load()
        {
            var files = Directory.GetFiles(AppConfigurationService.StorageDirectory, "*" + fileStorageExtension);

            var deserializerSettings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            lock (((ICollection)storageEntries).SyncRoot)
            {
                foreach (var filePath in files)
                {
                    try
                    {
                        var keyName = Path.GetFileNameWithoutExtension(filePath);
                        var value = JsonConvert.DeserializeObject<ValueWrapper>(File.ReadAllText(filePath), deserializerSettings);

                        storageEntries.Add(keyName, value);
                    }
                    catch (Exception e)
                    {
                        if (
                            e is StackOverflowException ||
                            e is ThreadAbortException ||
                            e is AccessViolationException
                           )
                            throw e; //rethrow if not related with file operation

                        var logProperties = new Dictionary<string, object>();
                        logProperties.Add("Exception", e.ToString());
                        logProperties.Add("Stack", e.StackTrace);
                        LogService.Warning($"Unable to load storage entity: {filePath}", logProperties);

                        //try to delete file
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception de)
                        {
                            if (
                                de is StackOverflowException ||
                                de is ThreadAbortException ||
                                de is AccessViolationException
                               )
                                throw de; //rethrow if not related with file operation

                            var fileDeleteLogProperties = new Dictionary<string, object>();
                            fileDeleteLogProperties.Add("Exception", de.ToString());
                            fileDeleteLogProperties.Add("Stack", de.StackTrace);
                            LogService.Warning($"Unable to delete saved storage entity: {filePath}", fileDeleteLogProperties);
                        }
                    }
                }
            }
        }

        public object Get(string key)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                var entry = storageEntries[key];

                if (entry.Expired)
                    throw new EntryExpiredException("Entry has expired");

                return entry.Value;
            }
        }

        public T Get<T>(string key)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                var value = Get(key);
                return (T)value;
            }
        }

        public IEnumerable<string> GetKeysByPrefix(string prefix)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                List<string> keys = new List<string>();

                foreach (var entryKey in storageEntries.Keys)
                {
                    if (entryKey.StartsWith(prefix))
                    {
                        keys.Add(entryKey);
                    }
                }

                return keys;
            }
        }

        public StorageEntryExpirePolicy GetExpirePolicy(string key)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                if (!Has(key))
                    throw new InvalidOperationException($"Unable to find entry with key: {key}");

                return storageEntries[key].StorageEntryExpirePolicy;
            }
        }

        public bool Has(string key)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                return storageEntries.ContainsKey(key);
            }
        }

        public void Create(string key, object value, StorageEntryExpirePolicy expirePolicy, DateTime? expires = null)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                //if entry exists and its valid (non expired) we have to use Update
                if (Has(key) && !(storageEntries[key].Expired))
                    throw new InvalidOperationException($"Entry with key: {key} already exists and not expired, use Update(...) instead");

                //if entry exists but expired, we can override it
                if (Has(key))
                    storageEntries.Remove(key);

                //make sure 'expires' value is valid for specified expire policy
                if (
                    (expirePolicy == StorageEntryExpirePolicy.AfterSession || expirePolicy == StorageEntryExpirePolicy.Never) &&
                    expires != null
                    )
                    throw new ArgumentException("expires argument have to be null if expirePlicy is other then 'Expire'");

                //validate expire parmater
                if (expirePolicy == StorageEntryExpirePolicy.Expire)
                {
                    if (expires == null)
                        throw new ArgumentException("expirePolicy is set to expire but 'expires' parameter is null");

                    var notNullexpires = (DateTime)expires;

                    if (!notNullexpires.IsUTC())
                        throw new ArgumentException("'expire' have to be provided as UTC");
                }

                storageEntries.Add(key, new ValueWrapper(value, expirePolicy, expires));
            }
        }

        public void UpdateExpireDate(string key, DateTime expires)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                //only valid policy is 'Expire'
                if (!(GetExpirePolicy(key) == StorageEntryExpirePolicy.Expire))
                    throw new InvalidOperationException("Current entry policy don't allow to modify it's expire time");

                if (!expires.IsUTC())
                    throw new ArgumentException("'expire' have to be provided as UTC");

                storageEntries[key].ChangeExpireDate(expires);
            }
        }

        public void Update(string key, object value)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                var entry = storageEntries[key];

                if (entry.Expired)
                    throw new EntryExpiredException("Entry has expired");

                if (entry.SerializationTypeMaping != value.GetType().FullName)
                    throw new InvalidOperationException("Value type is different then previously assigned");

                entry.Value = value;
            }
        }

        public void Remove(string key)
        {
            lock (((ICollection)storageEntries).SyncRoot)
            {
                if (!Has(key))
                    throw new InvalidOperationException($"Entry with key: ${key} don't exists");

                storageEntries.Remove(key);
            }
        }

        //public void Set(string key, object value, bool keepForSession = false, DateTime? expires = null)
        //{
        //    //update or set if expried
        //    if(storageEntries.ContainsKey(key))
        //    {
        //        var storageValue = storageEntries[key];

        //        //update if not expired
        //        storageValue.Value = value;
        //        storageValue.Expires = expires;
        //    }
        //    //create new if not exists
        //    else
        //    {
        //        var storageValue = new ValueWrapper(value, keepForSession, expires);
        //        storageEntries.Add(key, storageValue);
        //    }
        //}

        internal class ValueWrapper
        {
            internal ValueWrapper(object value, StorageEntryExpirePolicy expirePolicy, DateTime? expires = null)
            {
                //this.keepForSession = keepForSession;
                this.StorageEntryExpirePolicy = expirePolicy;
                this.Expires = expires;
                this.Value = value;
                this.SerializationTypeMaping = value.GetType().FullName;

                flushed = false;
            }

            [JsonConstructor]
            internal ValueWrapper(DateTime? Expires, object EntryValue, StorageEntryExpirePolicy StorageEntryExpirePolicy, string SerializationTypeMaping)
            {
                //keepForSession = false;
                this.StorageEntryExpirePolicy = StorageEntryExpirePolicy;
                this.Expires = Expires;
                this.SerializationTypeMaping = SerializationTypeMaping;
                //this.Value = EntryValue;

                var recreatedModel = ModelDeserializationHelper.ConstcructModel(JsonConvert.SerializeObject(EntryValue), SerializationTypeMaping);
                Value = recreatedModel;
            }

            //[NonSerialized]
            //internal readonly bool keepForSession;
            [NonSerialized]
            internal bool flushed;

            //DateTime updated;
            public DateTime? Expires { get; internal set; }

            public StorageEntryExpirePolicy StorageEntryExpirePolicy { get; internal set; }

            //bool CanExpire => Expires != null;

            //For serialization purposes
            public object EntryValue { get; internal set; }

            public string SerializationTypeMaping { get; internal set; }

            [NonSerialized]
            bool expired;
            public bool Expired
            {
                get
                {
                    UpdateExpireStatus();
                    return expired;
                }

                protected set
                {
                    expired = value;
                }
            }

            internal void ChangeExpireDate(DateTime dateTime)
            {
                if (!dateTime.IsUTC())
                    throw new ArgumentException("DateTime have to be provided as UTC");

                Expires = dateTime;
            }

            void UpdateExpireStatus()
            {
                //check if expires
                if (Expires != null)
                {
                    Expired = DateTime.Compare(DateTime.UtcNow, (DateTime)Expires) > 0;
                }
            }

            internal void ValidateExpire()
            {
                if(Expired)
                    throw new EntryExpiredException("Entry has been expired");
            }

            internal object Value
            {
                get
                {
                    UpdateExpireStatus();
                    ValidateExpire();
                    return EntryValue;
                }

                set
                {
                    //update access time
                    //updated = DateTime.UtcNow;

                    UpdateExpireStatus();
                    ValidateExpire();

                    //reset flushed
                    flushed = false;
                    SerializationTypeMaping = value.GetType().ToString();

                    this.EntryValue = value;
                }
            }
        }
    }
}
