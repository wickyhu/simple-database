using System;
using System.Text;
using System.Configuration;
using System.Collections.Generic;
using SimpleDatabase;
using IQToolkit;
using IQToolkit.Data;

namespace TestSimpleDatabase
{

    [DbTable("Customers")]
    public class Customer
    {
        public string CustomerID;
        public string ContactName;
        public string CompanyName;
        public string Phone;
        public string City;
        public string Country;

        [DbAssociation()]
        public IList<Order> Orders;
    }

    [DbTable("Orders")]
    public class Order
    {
        public int OrderID;
        public string CustomerID;
        public DateTime OrderDate;
        public Customer Customer;

        [DbAssociation()]
        public List<OrderDetail> Details;
    }

    [DbTable("Order Details")]
    public class OrderDetail
    {
        public int? OrderID { get; set; }
        public int ProductID { get; set; }
        public Product Product;
    }

    public interface IEntity
    {
        int ID { get; }
    }

    [DbTable("Products")]
    public class Product : IEntity
    {
        [DbField("ProductID")]
        public int ID;
        public string ProductName;
        public bool Discontinued;

        int IEntity.ID
        {
            get { return this.ID; }
        }
    }

    [DbTable("Employees")]
    public class Employee
    {
        public int EmployeeID;
        public string LastName;
        public string FirstName;
        public string Title;
        public Address Address;
    }

    public class Address
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string Region { get; private set; }
        public string PostalCode { get; private set; }

        public Address(string street, string city, string region, string postalCode)
        {
            this.Street = street;
            this.City = city;
            this.Region = region;
            this.PostalCode = postalCode;
        }
    }

    [DbTable("v_customers")]
    public class CustomerView
    {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
    }


    public enum Status
    {
        Approved,
        NotApproved,
        Cancelled
    }

    public class DbObject
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    //[DbTable(AuditType = TableAuditTypes.SharedTable)]
    public class TestEntity : DbObject
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string Passwd { get; set; }
        public Status Status { get; set; }
        public byte[] Photo { get; set; }
        public string Text { get; set; }
        public Guid Guid { get; set; }
        [DbEmbedded]
        public Address Address { get; set; }

        public string Dummy { get; set; }
    }

    public class Northwind
    {
        public static Db GetDb()
        {
            string name = ConfigurationManager.AppSettings["ConnectionStringName"];
            Db db = Db.Get(name);
            db.UserInfoProvider = new UserInfo();
            return db;
        }
    }

    public class UserInfo : IUserInfoProvider
    {
        public string UserName { get { return "admin"; } }
        public string HostName { get { return "TestMachine"; } }
        public string IPAddress { get { return "127.0.0.1"; } }
        public string OSUser { get { return "Administrator"; } }
    }
}
