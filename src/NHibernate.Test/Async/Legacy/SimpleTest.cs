﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using NHibernate.DomainModel;
using NUnit.Framework;

namespace NHibernate.Test.Legacy
{
	using System.Threading.Tasks;
	[TestFixture]
	public class SimpleTestAsync : TestCase
	{
		private DateTime testDateTime = new DateTime(2003, 8, 16);
		private DateTime updateDateTime = new DateTime(2003, 8, 17);

		protected override string[] Mappings
		{
			get { return new string[] {"Simple.hbm.xml"}; }
		}

		[Test]
		public async Task TestCRUDAsync()
		{
			long key = 10;
			long otherKey = 9;

			using (ISession s1 = OpenSession())
			using (ITransaction t1 = s1.BeginTransaction())
			{
				// create a new
				Simple simple1 = new Simple();
				Simple otherSimple1 = new Simple();

				simple1.Name = "Simple 1";
				simple1.Address = "Street 12";
				simple1.Date = testDateTime;
				simple1.Count = 99;

				otherSimple1.Name = "Other Simple 1";
				otherSimple1.Address = "Other Street 12";
				otherSimple1.Date = testDateTime;
				otherSimple1.Count = 98;

				simple1.Other = otherSimple1;

				await (s1.SaveAsync(otherSimple1, otherKey));
				await (s1.SaveAsync(simple1, key));

				await (t1.CommitAsync());
			}

			// try to Load the object to make sure the save worked
			ISession s2 = OpenSession();
			ITransaction t2 = s2.BeginTransaction();

			Simple simple2 = (Simple) await (s2.LoadAsync(typeof(Simple), key));
			Simple otherSimple2 = (Simple) await (s2.LoadAsync(typeof(Simple), otherKey));

			// verify each property was saved as expected
			Assert.IsNotNull(simple2, "Unable to load object");
			Assert.IsNotNull(otherSimple2);
			Assert.AreSame(simple2.Other, otherSimple2);

			// update
			simple2.Count = 999;
			simple2.Name = "Simple 1 (Update)";
			simple2.Address = "Street 123";
			simple2.Date = updateDateTime;

			await (s2.UpdateAsync(simple2, key));

			await (t2.CommitAsync());
			s2.Close();

			// lets verify that the update worked 
			ISession s3 = OpenSession();
			ITransaction t3 = s3.BeginTransaction();

//			Simple simple3 = (Simple)s3.Load(typeof(Simple), key);
			Simple simple3 = (Simple) (await (s3.CreateQuery("from Simple as s where s.id = ? and '?'='?'").SetInt64(0, key).ListAsync()))[0];
			Simple otherSimple3;

			Assert.AreEqual(simple2.Count, simple3.Count);
			Assert.AreEqual(simple2.Name, simple3.Name);
			Assert.AreEqual(simple2.Address, simple3.Address);
			Assert.AreEqual(simple2.Date, simple3.Date);

			// note that the Other will not be the same object because
			// they were loaded in 2 different sessions
			otherSimple3 = simple3.Other;

			// the update worked - lets clear out the table
			await (s3.DeleteAsync(simple3));
			await (s3.DeleteAsync(otherSimple3));

			await (t3.CommitAsync());
			s3.Close();

			// verify there is no other Simple objects in the db
			ISession s4 = OpenSession();
			Assert.AreEqual(0, (await (s4.CreateCriteria(typeof(Simple)).ListAsync())).Count);
			s4.Close();
		}

		[Test]
		public async Task SetPropertiesOnQueryAsync()
		{
			DateTime now = DateTime.Now;

			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();

			// create a new
			long key = 10;
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			simple.Address = "Street 12";
			simple.Date = now;
			simple.Count = 99;

			await (s.SaveAsync(simple, key));

			await (t.CommitAsync());

			t = s.BeginTransaction();

			IQuery q = s.CreateQuery("from s in class Simple where s.Name=:Name and s.Count=:Count");
			q.SetProperties(simple);

			Simple loadedSimple = (Simple) (await (q.ListAsync()))[0];
			Assert.AreEqual(99, loadedSimple.Count);
			Assert.AreEqual("Simple 1", loadedSimple.Name);
			Assert.AreEqual("Street 12", loadedSimple.Address);
			Assert.AreEqual(now.ToString(), loadedSimple.Date.ToString());

			await (s.DeleteAsync(simple));

			await (t.CommitAsync());
			s.Close();
		}
	}
}
