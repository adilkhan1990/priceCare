using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Activation;
using Ninject.Syntax;
using Ninject.Web.Common;
using PriceCare.Web;
using PriceCare.Web.Controllers;
using PriceCare.Web.Repository;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace PriceCare.Web
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);

                // Install our Ninject-based IDependencyResolver into the Web API config
                GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IAccountRepository>().To<AccountRepository>().InRequestScope();
            kernel.Bind<ICountryRepository>().To<CountryRepository>().InRequestScope();
            kernel.Bind<IProductRepository>().To<ProductRepository>().InRequestScope();
            kernel.Bind<ILoadRepository>().To<LoadRepository>().InRequestScope();
            kernel.Bind<ISkuRepository>().To<SkuRepository>().InRequestScope();
            kernel.Bind<IRuleRepository>().To<RuleRepository>().InRequestScope();
            kernel.Bind<IPriceTypeRepository>().To<PriceTypeRepository>().InRequestScope();
            kernel.Bind<IPriceGuidanceRepository>().To<PriceGuidanceRepository>().InRequestScope();
            kernel.Bind<IListToSalesRepository>().To<ListToSalesRepository>().InRequestScope();
            kernel.Bind<InvitationRepository>().To<InvitationRepository>().InRequestScope();
            kernel.Bind<IRequestAccessRepository>().To<RequestAccessRepository>().InRequestScope();
            kernel.Bind<IEventRepository>().To<EventRepository>().InRequestScope();
            kernel.Bind<IInvitationRepository>().To<InvitationRepository>().InRequestScope();
            kernel.Bind<IPriceRepository>().To<PriceRepository>().InRequestScope();
            kernel.Bind<IListToSalesImpactRepository>().To<ListToSalesImpactRepository>().InRequestScope();
            kernel.Bind<IConsolidationRepository>().To<ConsolidationRepository>().InRequestScope();
            kernel.Bind<IVolumeRepository>().To<VolumeRepository>().InRequestScope();
       
            kernel.Bind<ICurrencyRepository>().To<CurrencyRepository>().InRequestScope();

            kernel.Bind<IFileHttpResponseCreator>().To<FileHttpResponseCreator>().InRequestScope();
            kernel.Bind<IPriceListToExcelDataTransformer>().To<PriceListToExcelDataTransformer>().InRequestScope();
            kernel.Bind<IPriceListFromExcelTransformer>().To<PriceListFromExcelTransformer>().InRequestScope();

            kernel.Bind<ApplicationUserManager>().ToMethod(GetApplicationUserManager).InRequestScope();
        }

        private static ApplicationUserManager GetApplicationUserManager(IContext context)
        {
            return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }
    }

    // Provides a Ninject implementation of IDependencyScope
    // which resolves services using the Ninject container.
    public class NinjectDependencyScope : IDependencyScope
    {
        IResolutionRoot resolver;

        public NinjectDependencyScope(IResolutionRoot resolver)
        {
            this.resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            if (resolver == null)
                throw new ObjectDisposedException("this", "This scope has been disposed");

            return resolver.TryGet(serviceType);
        }

        public System.Collections.Generic.IEnumerable<object> GetServices(Type serviceType)
        {
            if (resolver == null)
                throw new ObjectDisposedException("this", "This scope has been disposed");

            return resolver.GetAll(serviceType);
        }

        public void Dispose()
        {
            IDisposable disposable = resolver as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            resolver = null;
        }
    }

    // This class is the resolver, but it is also the global scope
    // so we derive from NinjectScope.
    public class NinjectDependencyResolver : NinjectDependencyScope, IDependencyResolver
    {
        IKernel kernel;

        public NinjectDependencyResolver(IKernel kernel)
            : base(kernel)
        {
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(kernel.BeginBlock());
        }
    }
}
