using Alarmy.Common;
using Alarmy.Services;
using Alarmy.Views;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Infrastructure
{
    public class WindsorInstaller : IWindsorInstaller 
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                .For<IAlarmRepository>()
                .ImplementedBy<FileAlarmRepository>()
                .LifestyleSingleton()
                .DependsOn(Property.ForKey("path").Eq(Environment.GetCommandLineArgs()[1]))    
            );

            container.Register(Component
                .For<IAlarmService>()
                .ImplementedBy<AlarmService>()
                .LifestyleSingleton()
            );

            container.Register(Component
                .For<ITimerService>()
                .ImplementedBy<TimerService>()
                .LifestyleSingleton()
            );

            container.Register(Component.For<MainForm>());
        }
    }
}
