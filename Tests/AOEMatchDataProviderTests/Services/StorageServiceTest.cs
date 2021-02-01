using AOEMatchDataProvider.Models;
using AOEMatchDataProvider.Services;
using AOEMatchDataProviderTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOEMatchDataProviderTests.Services
{
    [TestClass]
    [TestCategory("Service")]
    [TestCategory("IStorageService")]
    public class StorageServiceTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            ServiceResolver.SetupServicesForIStorageService(TestContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceResolver.DisposeServices();
        }

        [TestMethod]
        public void CommonRW()
        {
            var storageService = ServiceResolver.GetService<IStorageService>();

            storageService.Create("infiniteKey", StorageValueWrapper.FromValue(1), StorageEntryExpirePolicy.Never);

            Assert.IsTrue(storageService.Has("infiniteKey"), "Invalid value");
            Assert.AreEqual(storageService.Get<StorageValueWrapper>("infiniteKey").value, 1);

            storageService.Remove("infiniteKey");

            Assert.IsFalse(storageService.Has("infiniteKey"));

            //storageService.Set("sampleKey", null);
            //Assert.IsNotNull(storageService.Has("sampleKey"));
        }

        [TestMethod]
        public void FlushLoad()
        {
            //make sure all entries will be deleted
            TestingResourcesHelper.PreapareTestingDirectory(ServiceResolver.GetService<IAppConfigurationService>());

            var storageService = ServiceResolver.GetService<IStorageService>();

            storageService.Create("infiniteKey", StorageValueWrapper.FromValue(1), StorageEntryExpirePolicy.Never);

            //save data
            storageService.Flush();

            //reset service(es)
            Cleanup();
            Init();

            storageService = ServiceResolver.GetService<IStorageService>();
            storageService.Load();

            Assert.AreEqual(storageService.Get<StorageValueWrapper>("infiniteKey").value, 1, "Inifinite key should be aviable");
        }

        [TestMethod]
        public void ExpireTime()
        {
            //make sure all entries will be deleted
            TestingResourcesHelper.PreapareTestingDirectory(ServiceResolver.GetService<IAppConfigurationService>());

            var storageService = ServiceResolver.GetService<IStorageService>();

            storageService.Create("sessionKey", StorageValueWrapper.FromValue(1), StorageEntryExpirePolicy.AfterSession);
            storageService.Create("infiniteKey", StorageValueWrapper.FromValue(1), StorageEntryExpirePolicy.Never);
            storageService.Create("nonExpiredKey", StorageValueWrapper.FromValue(1), StorageEntryExpirePolicy.Expire, DateTime.UtcNow + new TimeSpan(24, 0, 0));

            //make sure entry will be expired
            storageService.Create("expiredKey", StorageValueWrapper.FromValue(1), StorageEntryExpirePolicy.Expire, DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, 5));
            Thread.Sleep(10);

            //test expierd entry
            Assert.ThrowsException<EntryExpiredException>(() =>
            {
                storageService.Get("expiredKey");
            }, "Expired entry have to throw exception during R/W operations");

            Assert.ThrowsException<EntryExpiredException>(() =>
            {
                storageService.Update("expiredKey", StorageValueWrapper.FromValue(2));
            }, "Expired entry have to throw exception during R/W operations");

            storageService.UpdateExpireDate("expiredKey", DateTime.UtcNow + new TimeSpan(24, 0, 0));
            storageService.Update("expiredKey", StorageValueWrapper.FromValue(5));

            //test if reseted entry will be changed as expected
            Assert.AreEqual(storageService.Get<StorageValueWrapper>("expiredKey").value, 5);

            //save data
            storageService.Flush();

            //reset service(es)
            Cleanup();
            Init();

            storageService = ServiceResolver.GetService<IStorageService>();
            storageService.Load();

            Assert.IsFalse(storageService.Has("sessionKey"), "Session key shouldn't be saved and loaded");
            Assert.AreEqual(storageService.Get<StorageValueWrapper>("infiniteKey").value, 1, "Inifinite key should be aviable");
        }
    }

    public class StorageValueWrapper : ISerializableModel
    {
        public int value;

        public StorageValueWrapper() { }

        public StorageValueWrapper(int value)
        {
            this.value = value;
        }

        static public StorageValueWrapper FromValue(int value)
        {
            return new StorageValueWrapper(value);
        }

        public object FromJSON(string serialized)
        {
            return JsonConvert.DeserializeObject<StorageValueWrapper>(serialized);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
