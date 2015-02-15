using Alarmy.Common;
using Alarmy.Core.FileAlarmRepository;
using Alarmy.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private IList<IAlarm> storage;
        private IEnumerable<IAlarm> serializeArgs;

        private IAlarmRepository CreateBlankRepository(params IRepositoryFilter[] filters)
        {
            var repository = new FileAlarmRepository(this.watcher, this.sharedFileFactory, this.serializer, filters, TEST_PATH);
            repository.Start();

            return repository;
        }

        [TestInitialize]
        public void Setup()
        {
            this.storage = new List<IAlarm>();

            this.serializeArgs = null;
            this.serializer = Substitute.For<IRepositorySerializer>();
            this.serializer.Deserialize(null).ReturnsForAnyArgs(this.storage);
            this.serializer.WhenForAnyArgs(x => x.Serialize(null, null)).Do(x => this.serializeArgs = (IEnumerable<IAlarm>)x.Args()[0]);

            this.stream = new MemoryStream();
            this.watcher = Substitute.For<IFileWatcher>();
            this.sharedFileFactory = Substitute.For<ISharedFileFactory>();
            this.sharedFile = Substitute.For<ISharedFile>();
            this.sharedFileFactory.Write(TEST_PATH).Returns(this.sharedFile);
            this.sharedFileFactory.Read(TEST_PATH).Returns(this.sharedFile);
            this.sharedFile.Stream.Returns(this.stream);
        }

        [TestMethod]
        public void FileAlarmRepository_Load_ReadsAlarmsFromDisk()
        {
            var alarm = FakeAlarm.GetAlarm();
            var repository = this.CreateBlankRepository();

            repository.Load();

            this.sharedFileFactory.ReceivedWithAnyArgs().Read(null);
        }

        [TestMethod]
        public void FileAlarmRepository_Add_AddsItems()
        {
            var alarm = FakeAlarm.GetAlarm();
            var repository = this.CreateBlankRepository();
           
            repository.Save(new [] { alarm });

            Assert.AreSame(alarm, this.serializeArgs.First());
        }

        [TestMethod]
        public void FileAlarmRepository_Add_AddsMultipleItems()
        {            
            var alarm1= FakeAlarm.GetAlarm();
            var alarm2 = FakeAlarm.GetAlarm();

            var repository = this.CreateBlankRepository();

            repository.Save(new [] { alarm1, alarm2 });

            Assert.AreSame(alarm1, this.serializeArgs.First());
            Assert.AreSame(alarm2, this.serializeArgs.Skip(1).First());
        }

        [TestMethod]
        public void FileAlarmRepository_List_ReturnsAlarms()
        {
            var alarm1 = FakeAlarm.GetAlarm();
            this.storage.Add(alarm1);
            var repository = this.CreateBlankRepository();

            var result = repository.Load();

            Assert.AreEqual(1, result.Count());
            Assert.AreSame(alarm1, result.First());
        }

        [TestMethod]
        public void FileAlarmRepository_List_DoesNotReturnFilteredOutAlarms()
        {
            var alarm1 = FakeAlarm.GetAlarm();
            var alarm2 = FakeAlarm.GetAlarm();
            var filter = Substitute.For<IRepositoryFilter>();
            filter.Match(alarm1).Returns(true);
            filter.Match(alarm2).Returns(false);
            this.storage.Add(alarm1);
            this.storage.Add(alarm2);

            var repository = this.CreateBlankRepository(filter);

            var result = repository.Load();

            Assert.AreEqual(1, result.Count());
            Assert.AreSame(alarm1, result.First());
        }
    }
}
