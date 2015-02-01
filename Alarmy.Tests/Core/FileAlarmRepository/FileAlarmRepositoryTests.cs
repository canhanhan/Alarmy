using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alarmy.Core.FileAlarmRepository;
using NSubstitute;
using Alarmy.Common;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Alarmy.Tests.Utils;

namespace Alarmy.Tests.Core
{
    [TestClass]
    public class FileAlarmRepositoryTests
    {
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
        public void FileAlarmRepository_Start_ReadsAlarmsFromDisk()
        {
            var alarm = new FakeAlarm();
            var repository = this.CreateBlankRepository();

            this.sharedFileFactory.ReceivedWithAnyArgs().Read(null);
        }

        [TestMethod]
        public void FileAlarmRepository_Add_AddsItems()
        {
            var alarm = new FakeAlarm();
            var repository = this.CreateBlankRepository();
           
            repository.Add(alarm);

            Assert.AreSame(alarm, this.serializeArgs.Values.First());
        }

        [TestMethod]
        public void FileAlarmRepository_Add_AddsMultipleItems()
        {            
            var alarm1= new FakeAlarm();
            var alarm2 = new FakeAlarm();

            var repository = this.CreateBlankRepository();

            repository.Add(alarm1);
            repository.Add(alarm2);

            Assert.AreSame(alarm1, this.serializeArgs.Values.First());
            Assert.AreSame(alarm2, this.serializeArgs.Values.Skip(1).First());
        }

        [TestMethod]
        public void FileAlarmRepository_Add_DoesNotOverwriteExistingAlarms()
        {
            var alarm1 = new FakeAlarm();
            var alarm2 = new FakeAlarm();
            this.storage.Add(alarm1.Id, alarm1);

            var repository = this.CreateBlankRepository();

            repository.Add(alarm2);

            Assert.AreSame(alarm1, this.serializeArgs.Values.First());
            Assert.AreSame(alarm2, this.serializeArgs.Values.Skip(1).First());
        }

        [TestMethod]
        public void FileAlarmRepository_Add_OverwritesExistingAlarmsWithSameGuid()
        {
            var alarm1 = new FakeAlarm();
            var alarm2 = new FakeAlarm();
            alarm2.Id = alarm1.Id;
            this.storage.Add(alarm1.Id, alarm1);

            var repository = this.CreateBlankRepository();

            repository.Add(alarm2);

            Assert.AreEqual(1, this.serializeArgs.Values.Count);
            Assert.AreSame(alarm2, this.serializeArgs.Values.First());
        }

        [TestMethod]
        public void FileAlarmRepository_Update_ChangesItems()
        {
            var existingAlarm = new FakeAlarm { Title = "Before" };
            var duplicateAlarm = new FakeAlarm { Title = "After", Id = existingAlarm.Id };
            this.storage.Add(existingAlarm.Id, existingAlarm);
            var repository = this.CreateBlankRepository();

            repository.Update(duplicateAlarm);

            Assert.AreSame(duplicateAlarm, this.serializeArgs.Values.First());
        }


        [TestMethod]
        public void FileAlarmRepository_List_ReturnsAlarms()
        {
            var alarm1 = new FakeAlarm();
            this.storage.Add(alarm1.Id, alarm1);
            var repository = this.CreateBlankRepository();

            var result = repository.List();

            Assert.AreEqual(1, result.Count());
            Assert.AreSame(alarm1, result.First());
        }

        [TestMethod]
        public void FileAlarmRepository_List_DoesNotReturnFilteredOutAlarms()
        {
            var alarm1 = new FakeAlarm();
            var alarm2 = new FakeAlarm();
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
        public void FileAlarmRepository_Remove_DeletesItems()
        {
            var alarm1 = new FakeAlarm();
            var alarm2 = new FakeAlarm();
            this.storage.Add(alarm1.Id, alarm1);
            this.storage.Add(alarm2.Id, alarm2);
            var repository = this.CreateBlankRepository();

            repository.Remove(alarm2);

            Assert.AreEqual(1, this.serializeArgs.Values.Count);
            Assert.AreSame(alarm1, this.serializeArgs.Values.First());
        }
    }
}
