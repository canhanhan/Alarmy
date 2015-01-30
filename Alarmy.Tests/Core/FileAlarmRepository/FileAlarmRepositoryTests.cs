using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alarmy.Core.FileAlarmRepository;
using NSubstitute;
using Alarmy.Common;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Alarmy.Tests.Core
{
    [TestClass]
    public class FileAlarmRepositoryTests
    {
        private class TestAlarm : IAlarm
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public AlarmStatus Status { get; set; }
            public DateTime Time { get; set; }
            public bool IsHushed { get; set; }
            
            public bool CanBeCanceled { get { return true; } }
            public bool CanBeCompleted { get { return true; } }
            public bool CanBeRinging { get { return true; } }
            public bool CanBeSet { get { return true; } }
            public bool CanBeMissed { get { return true; } }
            public bool IsRinging { get { return true; } }

            public TestAlarm()
            {
                this.Id = Guid.NewGuid();
            }

            public void SetTime(DateTime time) { }
            public void Cancel() { }
            public void Complete() { }
            public bool CheckStatusChange() { return true; }
            public bool Equals(IAlarm alarm, bool compareOnlyMetadata) { return true; }
            public void Import(IAlarm alarm) {}
        }

        private const string TEST_PATH = "TEST";
        private MemoryStream stream;
        private IFileWatcher watcher;
        private ISharedFileFactory sharedFileFactory;
        private ISharedFile sharedFile;
        private IRepositorySerializer serializer;
        private IDictionary<Guid, IAlarm> storage;
        private IDictionary<Guid, IAlarm> serializeArgs;

        private IAlarmRepository CreateBlankRepository(params IRepositoryFilter[] filters)
        {
            //var repositoryFilters = new IRepositoryFilter[0];
            return new FileAlarmRepository(this.watcher, this.sharedFileFactory, this.serializer, filters, TEST_PATH);
        }

        [TestInitialize]
        public void Setup()
        {
            this.storage = new Dictionary<Guid, IAlarm>();

            this.serializeArgs = null;
            this.serializer = Substitute.For<IRepositorySerializer>();
            this.serializer.Deserialize(null).ReturnsForAnyArgs(this.storage);
            this.serializer.WhenForAnyArgs(x => x.Serialize(null, null)).Do(x => this.serializeArgs = (IDictionary<Guid, IAlarm>)x.Args()[0]);

            this.stream = new MemoryStream();
            this.watcher = Substitute.For<IFileWatcher>();
            this.sharedFileFactory = Substitute.For<ISharedFileFactory>();
            this.sharedFile = Substitute.For<ISharedFile>();
            this.sharedFileFactory.Write(TEST_PATH).Returns(this.sharedFile);
            this.sharedFileFactory.Read(TEST_PATH).Returns(this.sharedFile);
            this.sharedFile.Stream.Returns(this.stream);
        }

        [TestMethod]
        public void Add_AddsItems()
        {
            var alarm = new TestAlarm();
            var repository = this.CreateBlankRepository();
           
            repository.Add(alarm);

            Assert.AreSame(alarm, this.serializeArgs.Values.First());
        }

        [TestMethod]
        public void Add_AddsMultipleItems()
        {            
            var alarm1= new TestAlarm();
            var alarm2 = new TestAlarm();

            var repository = this.CreateBlankRepository();

            repository.Add(alarm1);
            repository.Add(alarm2);

            Assert.AreSame(alarm1, this.serializeArgs.Values.First());
            Assert.AreSame(alarm2, this.serializeArgs.Values.Skip(1).First());
        }

        [TestMethod]
        public void Add_DoesNotOverwriteExistingAlarms()
        {
            var alarm1 = new TestAlarm();
            var alarm2 = new TestAlarm();
            this.storage.Add(alarm1.Id, alarm1);

            var repository = this.CreateBlankRepository();

            repository.Add(alarm2);

            Assert.AreSame(alarm1, this.serializeArgs.Values.First());
            Assert.AreSame(alarm2, this.serializeArgs.Values.Skip(1).First());
        }

        [TestMethod]
        public void Add_OverwritesExistingAlarmsWithSameGuid()
        {
            var alarm1 = new TestAlarm();
            var alarm2 = new TestAlarm();
            alarm2.Id = alarm1.Id;
            this.storage.Add(alarm1.Id, alarm1);

            var repository = this.CreateBlankRepository();

            repository.Add(alarm2);

            Assert.AreEqual(1, this.serializeArgs.Values.Count);
            Assert.AreSame(alarm2, this.serializeArgs.Values.First());
        }

        [TestMethod]
        public void Update_ChangesItems()
        {
            var existingAlarm = new TestAlarm { Title = "Before" };
            var duplicateAlarm = new TestAlarm { Title = "After", Id = existingAlarm.Id };
            this.storage.Add(existingAlarm.Id, existingAlarm);
            var repository = this.CreateBlankRepository();

            repository.Update(duplicateAlarm);

            Assert.AreSame(duplicateAlarm, this.serializeArgs.Values.First());
        }


        [TestMethod]
        public void List_ReturnsAlarms()
        {
            var alarm1 = new TestAlarm();
            this.storage.Add(alarm1.Id, alarm1);
            var repository = this.CreateBlankRepository();

            var result = repository.List();

            Assert.AreEqual(1, result.Count());
            Assert.AreSame(alarm1, result.First());
        }

        [TestMethod]
        public void List_DoesNotReturnFilteredOutAlarms()
        {
            var alarm1 = new TestAlarm();
            var alarm2 = new TestAlarm();
            var filter = Substitute.For<IRepositoryFilter>();
            filter.Match(alarm1).Returns(true);
            filter.Match(alarm2).Returns(false);
            this.storage.Add(alarm1.Id, alarm1);
            this.storage.Add(alarm2.Id, alarm2);

            var repository = this.CreateBlankRepository(filter);

            var result = repository.List();

            Assert.AreEqual(1, result.Count());
            Assert.AreSame(alarm1, result.First());
        }

        [TestMethod]
        public void Remove_DeletesItems()
        {
            var alarm1 = new TestAlarm();
            var alarm2 = new TestAlarm();
            this.storage.Add(alarm1.Id, alarm1);
            this.storage.Add(alarm2.Id, alarm2);
            var repository = this.CreateBlankRepository();

            repository.Remove(alarm2);

            Assert.AreEqual(1, this.serializeArgs.Values.Count);
            Assert.AreSame(alarm1, this.serializeArgs.Values.First());
        }
    }
}
