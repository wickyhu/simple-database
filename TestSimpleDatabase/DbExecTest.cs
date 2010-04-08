using SimpleDatabase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Configuration;

namespace TestSimpleDatabase
{
    
    
    /// <summary>
    ///This is a test class for DbTest and is intended
    ///to contain all DbTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DbExecTest
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
        

        /// <summary>
        ///A test for ExecNonQuery
        ///</summary>
        [TestMethod()]
        public void ExecNonQueryTest()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbTransactionInfo tr = db.BeginTransaction())
                {                
                    using (DbCommand cm = tr.CreateCommand())
                    {
                        string sql;
                        int expected;
                        int actual;

                        sql = String.Format(@"insert into orders (CustomerID,EmployeeID,OrderDate) 
                    values ({0},{1},{2})",
                            db.GetParameterMarker("customerID"),
                            db.GetParameterMarker("employeeID"),
                            db.GetParameterMarker("orderDate")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                        cm.Parameters.Add(db.CreateParameter("employeeID", 1));
                        cm.Parameters.Add(db.CreateParameter("orderDate", DateTime.Today.AddYears(1)));
                        expected = 1;
                        actual = db.ExecNonQuery(cm);
                        Assert.AreEqual(expected, actual);

                        sql = String.Format(@"delete from orders where OrderDate={0}",
                            db.GetParameterMarker("orderDate")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Clear();
                        cm.Parameters.Add(db.CreateParameter("orderDate", DateTime.Today.AddYears(1)));
                        expected = 1;
                        actual = db.ExecNonQuery(cm);
                        Assert.AreEqual(expected, actual);

                        tr.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///Binary test for ExecNonQuery
        ///</summary>
        [TestMethod()]
        public void BinaryTest()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbTransactionInfo tr = db.BeginTransaction())
                {
                    using (DbCommand cm = tr.CreateCommand())
                    {
                        string sql;
                        int expected;
                        int actual;

                        string notepad = Environment.ExpandEnvironmentVariables("%windir%\\notepad.exe");
                        byte[] photo = File.ReadAllBytes(notepad);

                        sql = String.Format(@"insert into employees (Photo,PhotoPath,LastName,FirstName) 
                    values ({0},{1},{2},{3})",
                            db.GetParameterMarker("photo"),
                            db.GetParameterMarker("photopath"),
                            db.GetParameterMarker("lastname"),
                            db.GetParameterMarker("firstname")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("photo", photo));
                        cm.Parameters.Add(db.CreateParameter("photopath", "binarytest"));
                        cm.Parameters.Add(db.CreateParameter("lastname", "binarytest"));
                        cm.Parameters.Add(db.CreateParameter("firstname", "binarytest"));
                        expected = 1;
                        actual = db.ExecNonQuery(cm);
                        Assert.AreEqual(expected, actual);

                        sql = String.Format(@"select Photo from employees where PhotoPath={0}",
                            db.GetParameterMarker("photopath"));
                        
                        cm.CommandText = sql;
                        cm.Parameters.Clear();
                        cm.Parameters.Add(db.CreateParameter("photopath", "binarytest"));
                        DataTable dt = db.ExecQuery(cm);
                        expected = 1;
                        Assert.AreEqual(expected, dt.Rows.Count);
                        Assert.AreEqual(photo.Length, ((byte[])dt.Rows[0][0]).Length);

                        sql = String.Format(@"delete from employees where PhotoPath={0}",
                            db.GetParameterMarker("photopath")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Clear();
                        cm.Parameters.Add(db.CreateParameter("photopath", "binarytest"));
                        expected = 1;
                        actual = db.ExecNonQuery(cm);
                        Assert.AreEqual(expected, actual);

                        tr.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///A test for ExecQuery
        ///</summary>
        [TestMethod()]
        public void ExecQueryTest()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbConnectionInfo cn = db.CreateConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        string sql;
                        int expected;
                        int actual;
                        DataTable dt;

                        sql = String.Format(@"select * from customers where CustomerID={0}",
                            db.GetParameterMarker("customerID")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                        expected = 1;
                        dt = db.ExecQuery(cm);
                        actual = dt.Rows.Count;
                        Assert.AreEqual(expected, actual);
                        Assert.AreEqual("ALFKI", dt.Rows[0]["customerID"] as string);
                    }
                }
            }
           
        }

        /// <summary>
        ///A test for ExecQueryEx
        ///This test not working for Oracle
        ///</summary>
        [TestMethod()]
        public void ExecQueryExTest()
        {
            using (Db db = Northwind.GetDb())
            {
                if (db.DbMetadata.ProviderName == "System.Data.SqlClient")
                {
                    using (DbConnectionInfo cn = db.CreateConnection())
                    {
                        using (DbCommand cm = cn.CreateCommand())
                        {
                            string sql;
                            int expected;
                            int actual;
                            DataSet ds;

                            sql = String.Format(@"select * from customers where CustomerID={0};
                        select * from employees where EmployeeID={1}",
                                db.GetParameterMarker("customerID"),
                                db.GetParameterMarker("employeeID")
                                );
                            
                            cm.CommandText = sql;
                            cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                            cm.Parameters.Add(db.CreateParameter("employeeID", 1));
                            expected = 2;
                            ds = db.ExecQueryEx(cm);
                            actual = ds.Tables.Count;
                            Assert.AreEqual(expected, actual);
                            Assert.AreEqual("ALFKI", ds.Tables[0].Rows[0]["customerID"] as string);
                            Assert.AreEqual(1, (int)ds.Tables[1].Rows[0]["employeeID"]);
                        }
                    }
                }
            }      
        }

        /// <summary>
        ///A test for ExecReader
        ///</summary>
        [TestMethod()]
        public void ExecReaderTest()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbConnectionInfo cn = db.CreateConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        string sql;
                        sql = String.Format(@"select CustomerID,CompanyName from customers where CustomerID={0}",
                            db.GetParameterMarker("customerID")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                        using (DbDataReader ddr = db.ExecReader(cm))
                        {
                            Assert.IsTrue(ddr.Read());
                            Assert.AreEqual("ALFKI", ddr.GetString(0));
                            Assert.IsFalse(ddr.Read());
                        }
                    }
                }
            }
        }

        /// <summary>
        ///A test for ExecScalar
        ///</summary>
        [TestMethod()]
        public void ExecScalarTest()
        {
            using (Db db = Northwind.GetDb())
            {
                using (DbConnectionInfo cn = db.CreateConnection())
                {
                    using (DbCommand cm = cn.CreateCommand())
                    {
                        string sql;
                        sql = String.Format(@"select CustomerID,CompanyName from customers where CustomerID={0}",
                            db.GetParameterMarker("customerID")
                            );
                        
                        cm.CommandText = sql;
                        cm.Parameters.Add(db.CreateParameter("customerID", "ALFKI"));
                        object o = db.ExecScalar(cm);
                        Assert.AreEqual("ALFKI", o as string);
                    }
                }
            }            
        }

    }//end of class
}
