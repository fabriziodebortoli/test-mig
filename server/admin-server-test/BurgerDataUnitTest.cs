using Microarea.AdminServer.Services.BurgerData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microarea.AdminServerTest
{
	/// <summary>
	/// This class contains unit-test for SelectScript class
	/// </summary>
    [TestClass]
	//================================================================================
	public class SelectScriptTest
    {
		//--------------------------------------------------------------------------------
		[TestMethod]
        public void TestMethod1()
        {
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddSelectField("Age");
			selectScript.AddSelectField("Job");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			string test = selectScript.ToString();
			Assert.AreEqual(test, "SELECT Age,Job FROM Persons WHERE Name = 'Steve'");
        }

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod2()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddSelectField("Age");
			selectScript.AddSelectField("Job");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			string test = selectScript.GetParameterizedQuery();
			Assert.AreEqual(test, "SELECT Age,Job FROM Persons WHERE Name = @Name");
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod3()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddSelectField("Age");
			selectScript.AddSelectField("Job");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			selectScript.AddWhereParameter("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			string test = selectScript.GetParameterizedQuery();
			Assert.AreEqual(test, "SELECT Age,Job FROM Persons WHERE Name = @Name AND LastName = @LastName");
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod4()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddSelectField("Age");
			selectScript.AddSelectField("Job");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			selectScript.AddWhereParameter("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			string test = selectScript.ToString();
			Assert.AreEqual(test, "SELECT Age,Job FROM Persons WHERE Name = 'Steve' AND LastName = 'Jobs'");
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod5()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			selectScript.AddWhereParameter("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			Assert.IsNotNull(selectScript.SqlParameterList);
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod6()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			selectScript.AddWhereParameter("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual(selectScript.SqlParameterList.Count, 2);
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod7()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			selectScript.AddWhereParameter("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("@Name", selectScript.SqlParameterList[0].ParameterName);
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod8()
		{
			SelectScript selectScript = new SelectScript("Persons");
			selectScript.AddWhereParameter("Name", "Steve", QueryComparingOperators.IsEqual, false);
			selectScript.AddWhereParameter("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("Steve", selectScript.SqlParameterList[0].Value);
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod9()
		{
			DeleteScript deleteScript = new DeleteScript("Persons");
			deleteScript.Add("Name", "Steve", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("DELETE FROM Persons WHERE Name = 'Steve'", deleteScript.ToString());
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod10()
		{
			DeleteScript deleteScript = new DeleteScript("Persons");
			deleteScript.Add("Name", "Steve", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("DELETE FROM Persons WHERE Name = @Name", deleteScript.GetParameterizedQuery());
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod11()
		{
			DeleteScript deleteScript = new DeleteScript("Persons");
			deleteScript.Add("Name", "Steve", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("Company", "Apple", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("DELETE FROM Persons WHERE Name = @Name AND Company = @Company", deleteScript.GetParameterizedQuery());
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod12()
		{
			DeleteScript deleteScript = new DeleteScript("Persons");
			deleteScript.LogicOperatorForAllParameters = SqlLogicOperators.OR;
			deleteScript.Add("Name", "Steve", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("DELETE FROM Persons WHERE Name = @Name OR LastName = @LastName", deleteScript.GetParameterizedQuery());
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod13()
		{
			DeleteScript deleteScript = new DeleteScript("Persons");
			deleteScript.Add("Name", "Steve", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("City", "Palo Alto", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("Country", "CA", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual(4, deleteScript.SqlParameterList.Count);
		}

		//--------------------------------------------------------------------------------
		[TestMethod]
		public void TestMethod14()
		{
			DeleteScript deleteScript = new DeleteScript("Persons");
			deleteScript.Add("Name", "Steve", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("LastName", "Jobs", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("City", "Palo Alto", QueryComparingOperators.IsEqual, false);
			deleteScript.Add("Country", "CA", QueryComparingOperators.IsEqual, false);
			Assert.AreEqual("@Country", deleteScript.SqlParameterList[3].ParameterName);
		}
	}
}
