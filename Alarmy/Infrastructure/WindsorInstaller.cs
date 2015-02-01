﻿using Alarmy.Common;
using Alarmy.Core;
using Alarmy.Core.FileAlarmRepository;
using Alarmy.ViewModels;
using Alarmy.Views;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Alarmy.Infrastructure
{
    internal static class WindsorHelper
    {
        public static ComponentRegistration<TService> Settings<TService>(this ComponentRegistration<TService> config, params Expression<Func<Settings, object>>[] usedSettings)
             where TService : class
        {
            return config.DynamicParameters((k, d) =>
            {
                var settings = k.Resolve<Settings>();
                var serviceType = config.Implementation ?? typeof(TService);
                var ctorArguments = serviceType.GetConstructors().Single().GetParameters();

                foreach (var setting in usedSettings)
                {
                    var memberExpression = (MemberExpression)(setting.Body is UnaryExpression ? ((UnaryExpression)setting.Body).Operand : setting.Body);
                    var target = ctorArguments.SingleOrDefault(x => x.ParameterType == ((PropertyInfo)memberExpression.Member).PropertyType);
                    string targetName;
                    if (target == null)
                    {
                        targetName = memberExpression.Member.Name;
                        targetName = targetName.Substring(0, 1).ToLowerInvariant() + targetName.Substring(1);
                    }
                    else
                    {
                        targetName = target.Name;
                    }

                    d[targetName] = setting.Compile().Invoke(settings);
                }
            });
        }
    }

    public class WindsorInstaller : IWindsorInstaller
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {            
            NLog.Config.ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("databasePath", typeof(DatabasePathLayoutRenderer));
            container.AddFacility<LoggingFacility>(f => f.UseNLog().WithAppConfig());
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));   

            container.Register(Component.For<Settings>().ImplementedBy<CommandLineArgsSettings>());

            container.Register(Component
                .For<IAlarmRepository>()
                .ImplementedBy<FileAlarmRepository>()
                .LifestyleSingleton()
                .Settings(x => x.AlarmDatabasePath)
            );

            container.Register(Component
                .For<IAlarmService>()
                .ImplementedBy<AlarmService>()
                .LifestyleSingleton()
                .Settings(x => x.RepositoryRefreshIntervalInSeconds)
            );

            container.Register(Component
                .For<ITimer>()
                .ImplementedBy<Timer>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<IAlarmManager>()
                .ImplementedBy<SessionStateBasedAlarmManager>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<IMainView, MainForm>()
                .ImplementedBy<MainForm>()
                .LifestyleSingleton()
                .Settings(x => x.AlarmListGroupInterval)
            );

            container.Register(Component
                .For<IAlarmFactory>()
                .ImplementedBy<AlarmFactory>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<IDateTimeProvider>()
                .ImplementedBy<DateTimeProvider>()
                .LifestyleSingleton()
            );
            
            container.Register(
                Classes.FromThisAssembly()
                .IncludeNonPublicTypes()
                .BasedOn<IRepositoryFilter>()
                .WithServiceFirstInterface()
                .Configure(x => x.Settings(y => y.FreshnessInMinutes))
            );

            container.Register(Component
                .For<IFileWatcher>()
                .ImplementedBy<ReliableFileSystemWatcher>()
                .LifestyleSingleton()
                .Settings(x => x.AlarmDatabasePath)
            );

            container.Register(Component
                .For<IRepositorySerializer>()
                .ImplementedBy<JsonRepositorySerializer>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<ISharedFileFactory>()
                .ImplementedBy<SharedFileFactory>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<SoundPlayer>()
                .LifestyleSingleton()
                .Settings(x => x.AlarmSoundFile)
            );

            container.Register(Component
                .For<ISessionStateProvider>()
                .ImplementedBy<SessionStateProvider>()
                .LifestyleSingleton()
            );

            container.Register(Component.For<MainViewModel>().LifestyleSingleton());

            container.Register(Component.For<Program.MainFormContext>().LifestyleSingleton());
        }
    }
}
