using Alarmy.Common;
using Alarmy.Controllers;
using Alarmy.Services;
using Alarmy.Views;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alarmy.Infrastructure
{
    public class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            NLog.Config.ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("databasePath", typeof(DatabasePathLayoutRenderer));

            container.Register(Component.For<Settings>());

            container.Register(Component
                .For<IAlarmRepository>()
                .ImplementedBy<FileAlarmRepository>()
                .LifestyleSingleton()
                .DynamicParameters((k, d) => d["path"] = k.Resolve<Settings>().AlarmDatabasePath)
            );

            container.Register(Component
                .For<IAlarmService>()
                .ImplementedBy<AlarmService>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<ITimer>()
                .ImplementedBy<Timer>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<ISmartAlarmController>()
                .ImplementedBy<SmartAlarmController>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<IMainView, MainForm>()
                .ImplementedBy<MainForm>()
                .LifestyleSingleton()
            );

            container.Register(Component.For<MainViewController>().LifestyleSingleton());

            container.Register(Component.For<Program.MainFormContext>().LifestyleSingleton());
        }
    }
}
