﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terminals.Data;
using Terminals.History;
using Favorite = Terminals.Data.DB.Favorite;

namespace Tests
{
    /// <summary>
    ///This is a test class for database implementation of connection history
    ///</summary>
    [TestClass]
    public class SqlConnectionsHistoryTest
    {
        private SqlTestsLab lab;
        private int historyRecordedCount;

        [TestInitialize]
        public void TestInitialize()
        {
            this.lab = new SqlTestsLab();
            this.lab.InitializeTestLab();
            historyRecordedCount = 0;
            this.lab.Persistence.ConnectionHistory.OnHistoryRecorded += new HistoryRecorded(ConnectionHistory_OnHistoryRecorded);
        }

        private void ConnectionHistory_OnHistoryRecorded(HistoryRecordedEventArgs args)
        {
            historyRecordedCount++;
        }

        [TestCleanup]
        public void TestClose()
        {
            this.lab.ClearTestLab();
        }

        [TestMethod]
        public void HistoryTest()
        {
            Favorite favorite = this.lab.AddFavoriteToPrimaryPersistence();

            IConnectionHistory history = this.lab.Persistence.ConnectionHistory;
            history.RecordHistoryItem(favorite);
            history.RecordHistoryItem(favorite);
            var expectedCount = this.GetExpectedHistoryCount();

            Assert.AreEqual(2, historyRecordedCount, "Recorded history wasnt reported");
            // to preserve duplicit times, when creating new entry in database, only one should be recorded
            Assert.AreEqual(1, expectedCount, "History wasnt stored into database");

            this.lab.Persistence.Favorites.Delete(favorite);
            var expectedCountAfter = this.GetExpectedHistoryCount();
            Assert.AreEqual(0, expectedCountAfter, "Favorite history wasnt deleted from database");
        }

        private int GetExpectedHistoryCount()
        {
            return this.lab.CheckDatabase
                .ExecuteStoreQuery<int>("select Count(FavoriteId) from History")
                .FirstOrDefault();
        }
    }
}