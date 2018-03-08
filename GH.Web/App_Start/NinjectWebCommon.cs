using System.Web.Http;
using Microsoft.AspNet.SignalR;
using GH.Web.DependencyResolvers;
using SignalR.EventAggregatorProxy.EventAggregation;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(GH.Web.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(GH.Web.NinjectWebCommon), "Stop")]

namespace GH.Web
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    //using SignalR.EventProxy;
    using Core.SignalR.EventProxy;

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

                GlobalHost.DependencyResolver = new SignalRNinjectDependencyResolver(kernel);
                GlobalConfiguration.Configuration.DependencyResolver = new WebApiDependencyResolver(kernel);

                RegisterServices(kernel);
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
            kernel.Bind<Caliburn.Micro.IEventAggregator>().To<Caliburn.Micro.EventAggregator>().InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregatorProxy>().InSingletonScope();
        }
    }
}
