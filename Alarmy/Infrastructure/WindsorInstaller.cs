﻿using Alarmy.Common;
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
            container.AddFacility<LoggingFacility>(f => f.UseNLog().WithAppConfig());

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

            container.Register(Component.For<MainForm>());
        }
    }
}
