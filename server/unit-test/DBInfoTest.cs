using System;
using TaskBuilderNetCore.Data;
using Xunit;

namespace Tests
{
    public class DBInfoTest
    {
        [Fact]
        public void ExistTableTest() 
        {
            string connStr = "Server=USR-SARMANTANA1;Database=Company_ERP;User Id=sa;Password = ;";
            Assert.True(DBInfo.ExistTable(connStr,"ma_items"));
            Assert.False(DBInfo.ExistTable(connStr, "ma_itemsss"));
        }

        [Fact]
        public void ExistColumnTest()
        {
            string connStr = "Server=USR-SARMANTANA1;Database=Company_ERP;User Id=sa;Password = ;";
            Assert.True(DBInfo.ExistColumn(connStr, "ma_items", "Item"));
            Assert.False(DBInfo.ExistColumn(connStr, "ma_items", "Itemmm"));
            Assert.False(DBInfo.ExistColumn(connStr, "ma_itemsss","vv"));
        }

        [Fact]
        public void GetColumnTypeTest()
        {
            string connStr = "Server=USR-SARMANTANA1;Database=Company_ERP;User Id=sa;Password = ;";
            Assert.True(DBInfo.GetColumnType(connStr, "ma_items", "Item").CompareTo("String")==0);
            Assert.True(DBInfo.GetColumnType(connStr, "ma_items", "baseprice").CompareTo("Double") == 0);        
        }

        [Fact]
        public void GetColumnCollationTest()
        {
            string connStr = "Server=USR-SARMANTANA1;Database=Company_ERP;User Id=sa;Password = ;";
            Assert.True(DBInfo.GetColumnCollation(connStr, "ma_items", "Item").CompareTo("Latin1_General_CI_AS") == 0);
            Assert.True(string.IsNullOrEmpty(DBInfo.GetColumnCollation(connStr, "ma_items", "baseprice")));
        }
    }
}
