using SimpleDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Text;
using System;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.Common;

namespace TestSimpleDatabase
{


    /// <summary>
    ///This is a test class for DbTest and is intended
    ///to contain all DbTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DbQueryTest
    {
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

        [TestMethod()]
        public void ViewTest()
        {
            using (Db db = Northwind.GetDb())
            {
                var q = from c in db.Query<CustomerView>()
                        where !String.IsNullOrEmpty(c.CompanyName)
                        select c;
                CustomerView vc1 = q.FirstOrDefault();
                Assert.IsNotNull(vc1);
            }
        }

        [TestMethod()]
        public void UpdateTest()
        {
            using (Db db = Northwind.GetDb())
            {
                Db.AuditType = DbAuditTypes.Explicit;

                string id = "TstID";
                Customer c1 = new Customer()
                {
                    CustomerID = id,
                    CompanyName = "WiCKY",
                    City = "GuangZhou"
                };

                int r;

                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    r = db.Delete<Customer>(c1);
                    r = db.Insert<Customer>(c1);
                    Assert.AreEqual(1, r);
                    tr.Commit();
                }


                var q = from c in db.Query<Customer>()
                        where c.CustomerID == id
                        select c;
                Customer c2 = q.SingleOrDefault();
                Assert.IsNotNull(c2);
                Assert.AreEqual(c1.City, c2.City);


                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    r = db.Delete<Customer>(c2);
                    Assert.AreEqual(1, r);
                    tr.Commit();
                }

                Customer c3 = q.SingleOrDefault();
                Assert.IsNull(c3);
            }
        }


        /// <summary>
        ///A test for Insert
        ///</summary>
        [TestMethod()]
        public void InsertTest()
        {
            int r;

            using (Db db = Northwind.GetDb())
            {
                Db.AuditType = DbAuditTypes.Implicit;

                TestEntity u = new TestEntity() { Id = 1, LoginName = "wickyhu", Passwd = "888", Status = Status.Cancelled };
                u.Photo = File.ReadAllBytes(Path.Combine(Environment.SystemDirectory, "user32.dll"));
                u.Text = File.ReadAllText(Path.Combine(Environment.SystemDirectory, "eula.txt"));//.Substring(0, 4000);
                u.Guid = Guid.NewGuid();

                Address a = new Address("street", "city", "region", "postcode");
                u.Address = a;

                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    r = db.Delete<TestEntity>(u);
                    //r = db.Delete<TestEntity>(q => q.Id == 1 && q.LoginName == "wickyhu");
                    r = db.Insert<TestEntity>(u);
                    Assert.AreEqual(r, 1);
                    u.Passwd = "999";
                    u.Status = Status.NotApproved;
                    r = db.Update<TestEntity>(u);
                    Assert.AreEqual(r, 1);
                    tr.Commit();
                }

                TestEntity u3 = new TestEntity() { Id = 2, LoginName = "blabla2", Passwd = "9992" };
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    r = db.Delete<TestEntity>(q => q.Id == 2 && q.LoginName == "blabla2");
                    r = db.Insert<TestEntity>(u3);
                    Assert.AreEqual(r, 1);
                    tr.Commit();
                }

                u3 = new TestEntity() { Id = 3, LoginName = "blabla3", Passwd = "9993" };
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    r = db.Insert<TestEntity>(u3);
                    Assert.AreEqual(r, 1);
                    tr.Rollback();
                }

                TestEntity u2 = db.GetById<TestEntity>(1);
                Assert.AreEqual(u.Photo.Length, u2.Photo.Length);
                Assert.AreEqual(u.Text, u2.Text);
                //Assert.AreEqual(u.Guid, u2.Guid);

                Assert.AreEqual("wickyhu", u2.LoginName);
                Assert.AreEqual(Status.NotApproved, u2.Status);
                Assert.AreEqual("999", u2.Passwd);

                Assert.IsNotNull(u2.CreatedOn);
                Assert.IsNotNull(u2.CreatedBy);
                Assert.IsNotNull(u2.UpdatedBy);
                Assert.IsNotNull(u2.UpdatedOn);

                u3 = db.GetById<TestEntity>(3);
                Assert.IsNull(u3);
            }
        }

        [TestMethod()]
        public void QueryTest1()
        {
            using (Db db = Northwind.GetDb())
            {
                var q = from c in db.Query<Customer>()
                        where !String.IsNullOrEmpty(c.City)
                        select c;
                Customer c1 = q.FirstOrDefault();

                Assert.IsNotNull(c1);
                Assert.IsNotNull(c1.Orders);

                var oq = from o in db.Query<Order>()
                         where o.OrderID == 10248
                         select o;
                Order o1 = oq.FirstOrDefault();
                Assert.IsNotNull(o1);
                Assert.IsNotNull(o1.Details);
            }
        }

        [TestMethod()]
        public void QueryTest2()
        {
            using (Db db = Northwind.GetDb())
            {
                UserInfo ui = new UserInfo();
                var q = from t in db.Query<TestEntity>()
                        where t.CreatedBy == ui.UserName && t.CreatedOn < DateTime.Now.AddSeconds(1) 
                        where t.LoginName == "wickyhu" || t.LoginName == "blabla2" 
                        select t;
                int i = q.Count();
                Assert.AreEqual(2, i);

                TestEntity t1;

                i = q.OrderBy(u => u.LoginName).Select(u => u.LoginName).Skip(2).Take(1).Distinct().Count();
                Assert.AreEqual(0, i);

                t1 = q.First();
                Assert.AreEqual("wickyhu", t1.LoginName);
                Assert.IsNotNull(t1.Address);
                Assert.AreEqual("city", t1.Address.City);
                Assert.IsNotNull(t1.Text);
            }
        }

        
        [TestMethod()]
        public void DbAuditTest1()
        {
            using (Db db = Northwind.GetDb())
            {
                var q = from ad in db.Query<DbAuditDetail>()
                        select ad;
                DbAuditDetail auditDetail = q.First();

                DbAuditDetail auditDetail2 = db.GetById<DbAuditDetail>(auditDetail.Id);

                Assert.AreEqual(auditDetail.AuditId, auditDetail2.AuditId);
            }
        }

        [TestMethod()]
        public void DbAuditTest2()
        {
            using (Db db = Northwind.GetDb())
            {
                var q = from q1 in db.Query<DbAudit>()
                        join q2 in db.Query<DbAuditDetail>() on q1.Id equals q2.AuditId
                        //from q2 in db.Query<DbAuditDetail>()
                        where q1.Id > 1
                        select new
                        {
                            q1.Id,
                            q1.Action,
                            q1.UserName,
                            q1.CreatedOn,
                            DetailId = q2.Id,
                            q2.AuditId,
                            q2.OldValue,
                            q2.NewValue
                        };
                var o = q.FirstOrDefault();
                Assert.IsNotNull(o);
                Assert.AreEqual(o.Id, o.AuditId);
            }
        }


        public void SampleCodes()
        {
            //entity samples
            using (Db db = Northwind.GetDb())
            {
                //insert 
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    TestEntity te = new TestEntity();
                    te.LoginName = "Admin";
                    db.Insert<TestEntity>(te);
                    tr.Commit();
                }

                //update
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    var q = from e in db.Query<TestEntity>()
                            where e.LoginName == "Admin"
                            select e;
                    TestEntity te = q.First();
                    te.LoginName = "Admin2";
                    db.Update<TestEntity>(te);
                    //db.Delete<TestEntity>(te);
                    tr.Commit();
                }

                //delete
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    db.Delete<TestEntity>(e => e.LoginName == "Admin2");
                    tr.Commit();
                }
            }

            //ado samples 
            using (Db db = Northwind.GetDb())
            {
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    using (DbCommand cm = tr.CreateCommand())
                    {
                        string sql = String.Format(@"insert into orders (CustomerID,EmployeeID,OrderDate) 
                            values ({0},{1},{2})",
                            db.GetParameterMarker("customerID"),
                            db.GetParameterMarker("employeeID"),
                            db.GetParameterMarker("orderDate")
                            );

                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                        cm.Parameters.Add(db.CreateParameter("employeeID", 1));
                        cm.Parameters.Add(db.CreateParameter("orderDate", DateTime.Today.AddYears(1)));
                        db.ExecNonQuery(cm);

                        tr.Commit();
                    }

                    using (DbCommand cm = tr.CreateCommand())
                    {
                        string sql = String.Format(@"select * from customers where CustomerID={0}",
                                    db.GetParameterMarker("customerID")
                                    );
                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                        DataTable dt = db.ExecQuery(cm);
                    }
                }
            }

        }

    }
}
