using AOEMatchDataProvider.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AOEMatchDataProviderTests.Services.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;

namespace AOEMatchDataProviderTests.Helpers
{
    //helper class for testing services stand alone from rest of the app itself
    public static class ServiceResolver
    {
        static IUnityContainer Container { get; set; }

        //preapare services for testing IUserRankDataProcessingService
        public static void SetupServicesForIUserRankDataProcessingService(TestContext testContext)
        {
            Container = new UnityContainer();

            Container.RegisterInstance<IAppConfigurationService>(
                new AppConfigurationTestingService());
            Container.RegisterInstance<IAppCriticalExceptionHandlerService>(
                new AppCriticalExceptionHandlerTestingService());
            Container.RegisterInstance<ILogService>(
                new LogTestingService(testContext));
            Container.RegisterInstance<IStorageService>(
                new StorageTestingService());
            Container.RegisterInstance<IRequestService>(
                new RequestTestingService(Container.Resolve<IAppConfigurationService>()));
            Container.RegisterInstance<IQueryCacheService>(
                new AOEMatchDataProvider.Services.Default.QueryCacheService(
                        Container.Resolve<IStorageService>(),
                        Container.Resolve<ILogService>(),
                        Container.Resolve<IRequestService>()
                    )
                );

            Container.RegisterFactory<IUserRankDataProcessingService>(
                f => new AOEMatchDataProvider.Services.Default.UserRankService(
                        f.Resolve<ILogService>(),
                        f.Resolve<IStorageService>(),
                        f.Resolve<IQueryCacheService>(),
                        f.Resolve<IRequestService>()
                    )
                );
        }

        public static void SetupServicesForIStorageService(TestContext testContext)
        {
            Container = new UnityContainer();

            Container.RegisterInstance<IAppConfigurationService>(
                new AppConfigurationTestingService());
            Container.RegisterInstance<IAppCriticalExceptionHandlerService>(
                new AppCriticalExceptionHandlerTestingService());
            Container.RegisterInstance<ILogService>(
                new LogTestingService(testContext));
            Container.RegisterInstance<IStorageService>(
                new AOEMatchDataProvider.Services.Default.StorageService(
                        Container.Resolve<IAppConfigurationService>(),
                        Container.Resolve<ILogService>()
                    )
                );
        }

        public static void SetupServicesForeIQueryCacheService(TestContext testContext)
        {
            Container = new UnityContainer();

            Container.RegisterInstance<IAppConfigurationService>(
                new AppConfigurationTestingService());
            Container.RegisterInstance<IAppCriticalExceptionHandlerService>(
                new AppCriticalExceptionHandlerTestingService());
            Container.RegisterInstance<ILogService>(
                new LogTestingService(testContext));
            Container.RegisterInstance<IRequestService>(
                new RequestTestingService(Container.Resolve<IAppConfigurationService>()));
            Container.RegisterInstance<IStorageService>(
                new AOEMatchDataProvider.Services.Default.StorageService(
                        Container.Resolve<IAppConfigurationService>(),
                        Container.Resolve<ILogService>()
                    )
                );
            Container.RegisterInstance<IQueryCacheService>(
                new AOEMatchDataProvider.Services.Default.QueryCacheService(
                        Container.Resolve<IStorageService>(),
                        Container.Resolve<ILogService>(),
                        Container.Resolve<IRequestService>()
                    )
                );
        }

        //yo dude apply patch before commit
        public static void SetupServicesForIMatchProcessingService(TestContext testContext)
        {
            Container = new UnityContainer();

            Container.RegisterInstance<IAppConfigurationService>(
                new AppConfigurationTestingService());
            Container.RegisterInstance<IAppCriticalExceptionHandlerService>(
                new AppCriticalExceptionHandlerTestingService());
            Container.RegisterInstance<ILogService>(
                new LogTestingService(testContext));
            Container.RegisterInstance<IRequestService>(
                new RequestTestingService(Container.Resolve<IAppConfigurationService>()));
            Container.RegisterInstance<IStorageService>(
                new AOEMatchDataProvider.Services.Default.StorageService(
                        Container.Resolve<IAppConfigurationService>(),
                        Container.Resolve<ILogService>()
                    )
                );

            Container.RegisterInstance<IQueryCacheService>(
                new AOEMatchDataProvider.Services.Default.QueryCacheService(
                    Container.Resolve<IStorageService>(),
                    Container.Resolve<ILogService>(),
                    Container.Resolve<IRequestService>()
                )
            );

            //InjectionFactory is obsolete
            //Container.RegisterType<IUserRankDataProcessingService>(
            //    new InjectionFactory(
            //        f => new AOEMatchDataProvider.Services.Default.UserRankService(
            //            f.Resolve<ILogService>(),
            //            f.Resolve<IStorageService>(),
            //            f.Resolve<IQueryCacheService>()
            //            )
            //        )
            //    );

            Container.RegisterFactory<IUserRankService>(
                f => new AOEMatchDataProvider.Services.Default.UserRankService(
                        f.Resolve<ILogService>(),
                        f.Resolve<IStorageService>(),
                        f.Resolve<IQueryCacheService>(),
                        f.Resolve<IRequestService>()
                    )
                );

            Container.RegisterFactory<IMatchProcessingService>(
                f => new AOEMatchDataProvider.Services.Default.MatchProcessingService(
                    f.Resolve<ILogService>(),
                    f.Resolve<IStorageService>(),
                    f.Resolve<IQueryCacheService>(),
                    f.Resolve<IUserRankService>(),
                    f.Resolve<IAppConfigurationService>()
            ));
        }

        public static void DisposeServices()
        {
            foreach(var service in Container.Registrations)
            {
                if(service is IDisposable)
                {
                    try
                    {
                        (service as IDisposable).Dispose();
                    }
                    finally { /* supress diposing exceptions */ }
                }
            }
        }

        public static TService GetService<TService>()
        {
            return Container.Resolve<TService>();
        }
    }
}
