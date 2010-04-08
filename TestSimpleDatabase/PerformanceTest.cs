using SimpleDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Text;
using System;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Reflection;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Common;

namespace TestSimpleDatabase
{

    /// <summary>
    ///This is a test class for DbTest and is intended
    ///to contain all DbTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PerformanceTest
    {
        const int idStart = 10000;
        const int entityIdStart1 = 20000;
        const int entityIdStart2 = 30000;
        const int recordsCount = 5000;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion        

        private void DeleteRecords(DbCommand cm, int idStart)
        {
            cm.CommandText = String.Format("delete from testentity where id>={0} and id<{1}", idStart, idStart + recordsCount);
            cm.ExecuteNonQuery();
        }

        [TestMethod()]
        public void AdoInsertTest()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbConnection cn = db.CreateAdoConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        DeleteRecords(cm, idStart);
                    }

                    string sql = String.Format("insert into testentity (id,loginname,createdon,createdby) values ({0},{1},{2},{3})",
                        db.GetParameterMarker("id"),
                        db.GetParameterMarker("loginname"),
                        db.GetParameterMarker("createdon"),
                        db.GetParameterMarker("createdby")
                        );
                    DateTime startTime = DateTime.Now;
                    using (DbTransaction tr = cn.BeginTransaction())
                    {
                        int count = idStart + recordsCount;
                        for (int i = idStart; i < count; i++)
                        {
                            using (DbCommand cm = cn.CreateCommand())
                            {
                                cm.CommandText = sql;
                                cm.Transaction = tr;
                                cm.Parameters.Add(db.CreateParameter("id", i));
                                cm.Parameters.Add(db.CreateParameter("loginname", "name" + i));
                                cm.Parameters.Add(db.CreateParameter("createdon", DateTime.Now));
                                cm.Parameters.Add(db.CreateParameter("createdby", "admin"));
                                cm.ExecuteNonQuery();
                            }
                        }
                        tr.Commit();

                    }
                    DateTime endTime = DateTime.Now;
                    Assert.Inconclusive("{0} seconds.", (endTime-startTime).TotalSeconds);
                }
            }
        }

        [TestMethod()]
        public void EntityInsertTest1()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbConnectionInfo cn = db.CreateConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        DeleteRecords(cm, entityIdStart1);
                    }
                }

                DbTable table = db.GetDbTable(typeof(TestEntity));
                db.SetTableAuditType(table.TableName, TableAuditTypes.None);

                int id = entityIdStart1;

                DateTime startTime = DateTime.Now;
                using (DbTransactionInfo tr = db.BeginTransaction())
                {                    
                    TestEntity te = new TestEntity();
                    te.Id = id;
                    te.LoginName = "name" + id;
                    te.CreatedOn = DateTime.Now;
                    te.CreatedBy = "admin";
                    db.Insert<TestEntity>(te);

                    tr.Commit();
                }
                DateTime endTime = DateTime.Now;                

                id++;
                db.SetTableAuditType(table.TableName, TableAuditTypes.SharedTable);
                DateTime startTime2 = DateTime.Now;
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    TestEntity te = new TestEntity();
                    te.Id = id;
                    te.LoginName = "name" + id;
                    te.CreatedOn = DateTime.Now;
                    te.CreatedBy = "admin";
                    db.Insert<TestEntity>(te);

                    tr.Commit();
                }
                DateTime endTime2 = DateTime.Now;

                id++;
                db.SetTableAuditType(table.TableName, TableAuditTypes.StandaloneTable);
                DateTime startTime3 = DateTime.Now;
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    TestEntity te = new TestEntity();
                    te.Id = id;
                    te.LoginName = "name" + id;
                    te.CreatedOn = DateTime.Now;
                    te.CreatedBy = "admin";
                    db.Insert<TestEntity>(te);

                    tr.Commit();
                }
                DateTime endTime3 = DateTime.Now;

                Assert.Inconclusive("No Audit: {0} seconds, Shared: {1} seconds, Standalone: {2} seconds",
                    (endTime - startTime).TotalSeconds,
                    (endTime2 - startTime2).TotalSeconds,
                    (endTime3 - startTime3).TotalSeconds
                    );

            }
        }

        [TestMethod()]
        public void EntityInsertTest2()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbConnectionInfo cn = db.CreateConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        DeleteRecords(cm, entityIdStart2);
                    }
                }
                
                KeyTableNextIdProvider nextId = (KeyTableNextIdProvider)db.NextIdProvider;
                nextId.CacheSize = recordsCount;

                DbTable table = db.GetDbTable(typeof(TestEntity));
                db.SetTableAuditType(table.TableName, TableAuditTypes.None);
                
                DateTime startTime = DateTime.Now;
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    int count = entityIdStart2 + recordsCount;
                    for (int i = entityIdStart2; i < count; i++)
                    {
                        TestEntity te = new TestEntity();
                        te.Id = i;
                        te.LoginName = "name" + i;
                        te.CreatedOn = DateTime.Now;
                        te.CreatedBy = "admin";
                        db.Insert<TestEntity>(te);
                    }
                    tr.Commit();
                }
                DateTime endTime = DateTime.Now;
                Assert.Inconclusive("{0} seconds.", (endTime - startTime).TotalSeconds);
                
            }
        }


    }//end of class

}
