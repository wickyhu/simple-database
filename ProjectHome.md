A Simple .Net Database Library based on [IQToolkit](http://www.codeplex.com/IQToolkit).

`SimpleDatabase` uses `SimpleMapping`, which extends `BasicMapping` of `IQToolkit`, and follows [Convention over configuration](http://en.wikipedia.org/wiki/Convention_over_configuration) concept.  If your class name (entity name) and property/field name are matched with table name and column names, no mapping required. If they are not matched, you can use simple Attributes to make mapping.

Samples:
```
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
```
