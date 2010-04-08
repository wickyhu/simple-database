// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Configuration;
using System.Reflection;
using IQToolkit;
using IQToolkit.Data;
using IQToolkit.Data.Mapping;
using SimpleDatabase;

namespace Test
{
    class Program
    {
        static bool logEnabled = false;

        static void Main(string[] args)
        {
            string connectionStringName = ConfigurationManager.AppSettings["ConnectionStringName"];

            //string adoProviderName = "IQToolkit.Data.SqlClient";
            //string providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;
            //string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            Db db = Db.Get(connectionStringName);
            DbEntityProvider provider = (DbEntityProvider)typeof(Db).InvokeMember("DbEntityProvider",
                 BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty,
                 null,
                 db,
                 null);

            //var provider = DbEntityProvider.From(adoProviderName, connectionString, "Test.NorthwindWithAttributes");

            //var provider = DbEntityProvider.From(@"c:\data\Northwind.mdf", "Test.NorthwindWithAttributes");
            //var provider = DbEntityProvider.From(@"c:\data\Northwind.accdb", "Test.NorthwindWithAttributes");
            //var provider = DbEntityProvider.From(@"c:\data\Northwind.mdb", "Test.NorthwindWithAttributes");
            //var provider = DbEntityProvider.From(@"c:\data\Northwind.sdf", "Test.NorthwindWithAttributes");            
            //var provider = DbEntityProvider.From("IQToolkit.Data.MySqlClient", "Northwind", "Test.MySqlNorthwind");
            //var provider = DbEntityProvider.From("IQToolkit.Data.SQLite", @"c:\data\Northwind.db3", "Test.NorthwindWithAttributes");


             //TestSelectAssociations: An incorrect number of type args were specified for the declaration of a Func type.
             //TestSelectAssociationsWhereAssociations: An incorrect number of type args were specified for the declaration of a Func type.

            //provider.Log = Console.Out;
            //var policy = new EntityPolicy();
            //policy.IncludeWith<Customer>(c => c.Orders);
            //policy.IncludeWith<Order>(o => o.Details);
            //policy.IncludeWith<Order>(o => o.Customer);
            //var ndb = new Northwind(provider.New(policy));
            //NorthwindExecutionTests.Run(ndb);
            //Console.In.ReadLine();                
            //return;

            bool cont = true;
            while (cont)
            {
                Console.WriteLine();
                Console.WriteLine("Please choose test to run:");
                Console.WriteLine("0 = All");
                Console.WriteLine("1 = NorthwindTranslationTests");
                Console.WriteLine("2 = NorthwindExecutionTests");
                Console.WriteLine("3 = NorthwindCUDTests");
                Console.WriteLine("4 = MultiTableTests");
                Console.WriteLine("5 = NorthwindPerfTests");
                Console.WriteLine("log on/off = Turn on/off Log output");
                Console.WriteLine("q = Exit");
                
                string selection = Console.In.ReadLine();                
                cont = RunTest(provider, selection);

                //provider.Log = Console.Out;
                //MultiTableTests.Run(new MultiTableContext(provider.New(new AttributeMapping(typeof(MultiTableContext)))), "TestUpdate");
                //Console.ReadLine();
                //cont = false;
            }
        }

        private static bool RunTest(DbEntityProvider provider, string selection)
        {
            if (selection.Equals("q", StringComparison.OrdinalIgnoreCase)) 
                return false;

            if (selection.StartsWith("log", StringComparison.OrdinalIgnoreCase))
            {
                if (selection.Equals("log on", StringComparison.OrdinalIgnoreCase))
                {
                    logEnabled = true;
                }
                else if (selection.Equals("log off", StringComparison.OrdinalIgnoreCase))
                {
                    logEnabled = false;
                }

                Console.WriteLine(String.Format("Log is {0}.", logEnabled ? "On" : "Off"));
                return true;
            }

            if(logEnabled) 
                provider.Log = Console.Out;
            else 
                provider.Log = null;

            provider.Connection.Open();
            //provider.Cache = new QueryCache(5);
            try
            {
                var db = new Northwind(provider);         

                if (selection.Contains("0") || selection.Contains("1"))
                    NorthwindTranslationTests.Run(db, true);
                if (selection.Contains("0") || selection.Contains("2"))
                    NorthwindExecutionTests.Run(db);
                if (selection.Contains("0") || selection.Contains("3"))
                    NorthwindCUDTests.Run(db);
                if (selection.Contains("0") || selection.Contains("4"))
                    MultiTableTests.Run(new MultiTableContext(provider.New(new AttributeMapping(typeof(MultiTableContext)))));
                if (selection.Contains("0") || selection.Contains("5"))
                    NorthwindPerfTests.Run(db, "TestStandardQuery");                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                provider.Connection.Close();
            }

            return true;
        }

    }
}
