using System;
using System.Collections;

namespace Microarea.TaskBuilderNet.Core.SecurityLayer
{

    //============================================================================
    public enum DefaultBaseRoles
    {
        Purchases = 0,
        Configuration = 1,
        Inventory = 2,
        Manufacturing = 3,
        Sales = 4,
        Accounting = 5
    }

    //============================================================================
    public enum DefaultSecurityRoles
    {
        Purchases = 0,
        Configuration = 1,
        Inventory = 2,
        Manufacturing = 4,
        Sales = 16,
        Accounting = 32
    }

    //============================================================================
    public enum DefaultAdvancedRolesType
    {
        Director = 0,
        Assistant = 1,
        Employee = 2,
        Base = 3,
        FromFile = 4
    }

    //===========================================================================
    public static class DefaultSecurityRolesEngine
    {
        //-----------------------------------------------------------------------
        public static bool AreValidRoles(string rolesString)
        {
            if (string.IsNullOrEmpty(rolesString))
                return true;

            return AreValidRoles(rolesString.Split(','));
        }

        //-----------------------------------------------------------------------
        public static bool AreValidRoles(string[] roles)
        {
            if (roles == null)
                return true;

            foreach (string role in roles)
            {
                if (!Enum.IsDefined(typeof(DefaultSecurityRoles), role.Trim()) && !DefaultAdvancedRoles.IsAdvancedRole(role))
                    return false;
            }

            return true;
        }

        //---------------------------------------------------------------------
        public static string GetTypeFromNewObject(Microarea.TaskBuilderNet.Core.NameSolver.NewObject obj)
        {
            if (obj.IsBatch)
                return "Batch";

            if (obj.IsFinder)
                return "Finder";

            if (obj.IsReport)
                return "Report";

            return "Data Entry";
        }
    }

    //=========================================================================
    public class DefaultAdvancedRoles
    {
        public const string aManufacturing = "aManufacturing Manager";
        public const string bManufacturing = "bManufacturing Chief";
        public const string cManufacturing = "cManufacturing Employee";

        public const string aSales = "aSales Manager";
        public const string bSales = "bSales Employee";
        public const string cSales = "cSales Employee";

        public const string aInventory = "aInventory Manager";
        public const string bWarehouseman = "bChief Warehouseman";
        public const string cWarehouseman = "cWarehouseman";

        public const string aAccounting = "aAccounting Director";
        public const string bAccounting = "bAccounting Adviser";
        public const string cAccounting = "cAccounting Operator";

        public const string aPurchases = "aPurchases Director";
        public const string bPurchases = "bPurchases Assistant";
        public const string cPurchases = "cPurchases Employee";

        public const string aMailConnector      = "aMailConnector Parameter Manager";
        public const string aXtech              = "aXtech Parameter Manager";
        public const string aDMSManager         = "aDMSManager";
        public const string aUnprotected        = "aUnprotected Report Manager";
        public const string aResourceManager    = "aResource Manager";


        //---------------------------------------------------------------------
        public static ArrayList GetAllAdvancedRoles()
        {
            ArrayList advancedRoles = new ArrayList();

            advancedRoles.Add(new string[2] { aManufacturing, GetRoleDescriptionFromRoleName(aManufacturing) });
            advancedRoles.Add(new string[2] { bManufacturing, GetRoleDescriptionFromRoleName(bManufacturing) });
            advancedRoles.Add(new string[2] { cManufacturing, GetRoleDescriptionFromRoleName(cManufacturing) });

            advancedRoles.Add(new string[2] { aSales, GetRoleDescriptionFromRoleName(aSales) });
            advancedRoles.Add(new string[2] { bSales, GetRoleDescriptionFromRoleName(bSales) });
            advancedRoles.Add(new string[2] { cSales, GetRoleDescriptionFromRoleName(cSales) });

            advancedRoles.Add(new string[2] { aInventory, GetRoleDescriptionFromRoleName(aInventory) });
            advancedRoles.Add(new string[2] { bWarehouseman, GetRoleDescriptionFromRoleName(bWarehouseman) });
            advancedRoles.Add(new string[2] { cWarehouseman, GetRoleDescriptionFromRoleName(cWarehouseman) });

            advancedRoles.Add(new string[2] { aAccounting, GetRoleDescriptionFromRoleName(aAccounting) });
            advancedRoles.Add(new string[2] { bAccounting, GetRoleDescriptionFromRoleName(bAccounting) });
            advancedRoles.Add(new string[2] { cAccounting, GetRoleDescriptionFromRoleName(cAccounting) });

            advancedRoles.Add(new string[2] { aPurchases, GetRoleDescriptionFromRoleName(aPurchases) });
            advancedRoles.Add(new string[2] { bPurchases, GetRoleDescriptionFromRoleName(bPurchases) });
            advancedRoles.Add(new string[2] { cPurchases, GetRoleDescriptionFromRoleName(cPurchases) });
            advancedRoles.Add(new string[2] { aMailConnector, GetRoleDescriptionFromRoleName(aMailConnector) });
            advancedRoles.Add(new string[2] { aXtech, GetRoleDescriptionFromRoleName(aXtech) });
            advancedRoles.Add(new string[2] { aUnprotected, GetRoleDescriptionFromRoleName(aUnprotected) });
            advancedRoles.Add(new string[2] { aDMSManager,  GetRoleDescriptionFromRoleName(aDMSManager) });
            advancedRoles.Add(new string[2] { aResourceManager, GetRoleDescriptionFromRoleName(aResourceManager) });
            return advancedRoles;
        }


        //---------------------------------------------------------------------
        public static DefaultAdvancedRolesType GetDefaultAdvancedRolesTypeFromRoleName(string roleName)
        {
            if (string.Compare(roleName, aManufacturing) == 0 || string.Compare(roleName, aSales) == 0 ||
                    string.Compare(roleName, aInventory) == 0 || string.Compare(roleName, aAccounting) == 0 ||
                    string.Compare(roleName, aPurchases) == 0 || string.Compare(roleName, aMailConnector) == 0 ||
                    string.Compare(roleName, aXtech) == 0 || string.Compare(roleName, aUnprotected) == 0 ||
                    string.Compare(roleName, aDMSManager) == 0 || string.Compare(roleName, aResourceManager) == 0)
                return DefaultAdvancedRolesType.Director;


            if (string.Compare(roleName, bManufacturing) == 0 || string.Compare(roleName, bSales) == 0 ||
                    string.Compare(roleName, bWarehouseman) == 0 || string.Compare(roleName, bAccounting) == 0 ||
                    string.Compare(roleName, bPurchases) == 0)
                return DefaultAdvancedRolesType.Assistant;


            if (string.Compare(roleName, cManufacturing) == 0 || string.Compare(roleName, cSales) == 0 ||
                string.Compare(roleName, cWarehouseman) == 0 || string.Compare(roleName, cAccounting) == 0 ||
                string.Compare(roleName, cPurchases) == 0)
                return DefaultAdvancedRolesType.Employee;

            return DefaultAdvancedRolesType.FromFile;


        }
        //---------------------------------------------------------------------
        public static string GetBaseRoleFromAdvancedRole(string advancedRole)
        {

            if (string.Compare(advancedRole, aAccounting) == 0 ||
                string.Compare(advancedRole, bAccounting) == 0)
                return DefaultBaseRoles.Accounting.ToString();

            if (string.Compare(advancedRole, aInventory) == 0 ||
                string.Compare(advancedRole, bWarehouseman) == 0)
                return DefaultBaseRoles.Inventory.ToString();


            if (string.Compare(advancedRole, aManufacturing) == 0 ||
                string.Compare(advancedRole, bManufacturing) == 0)
                return DefaultBaseRoles.Manufacturing.ToString();


            if (string.Compare(advancedRole, aPurchases) == 0 ||
                string.Compare(advancedRole, bPurchases) == 0)
                return DefaultBaseRoles.Purchases.ToString();



            if (string.Compare(advancedRole, aSales) == 0 ||
                string.Compare(advancedRole, bSales) == 0)
                return DefaultBaseRoles.Sales.ToString();

            return string.Empty;
        }

        //---------------------------------------------------------------------
        public static ArrayList GetAdvancedRolesFromBaseRole(DefaultBaseRoles baseRole)
        {
            ArrayList roles = new ArrayList();

            if (baseRole == DefaultBaseRoles.Accounting)
            {
                roles.Add(aAccounting);
                roles.Add(bAccounting);
            }

            if (baseRole == DefaultBaseRoles.Inventory)
            {
                roles.Add(aInventory);
                roles.Add(bWarehouseman);
            }

            if (baseRole == DefaultBaseRoles.Manufacturing)
            {
                roles.Add(aManufacturing);
                roles.Add(bManufacturing);
            }

            if (baseRole == DefaultBaseRoles.Purchases)
            {
                roles.Add(aPurchases);
                roles.Add(bPurchases);
            }

            if (baseRole == DefaultBaseRoles.Sales)
            {
                roles.Add(aSales);
                roles.Add(bSales);
            }

            return roles;
        }

        //---------------------------------------------------------------------
        public static string GetRoleDescriptionFromRoleName(string roleName)
        {
            if (string.Compare(roleName, aAccounting) == 0)
                return Strings.aAccountingDescription;

            if (string.Compare(roleName, bAccounting) == 0)
                return Strings.bAccountingDescription;

            if (string.Compare(roleName, cAccounting) == 0)
                return Strings.cAccountingDescription;

            if (string.Compare(roleName, aSales) == 0)
                return Strings.aSalesDescription;

            if (string.Compare(roleName, bSales) == 0)
                return Strings.bSalesDescription;

            if (string.Compare(roleName, cSales) == 0)
                return Strings.cSalesDescription;

            if (string.Compare(roleName, aInventory) == 0)
                return Strings.aInventoryDescription;

            if (string.Compare(roleName, bWarehouseman) == 0)
                return Strings.bWarehousemanDescription;

            if (string.Compare(roleName, cWarehouseman) == 0)
                return Strings.cWarehousemanDescription;

            if (string.Compare(roleName, aManufacturing) == 0)
                return Strings.aManufacturingDescription;

            if (string.Compare(roleName, bManufacturing) == 0)
                return Strings.bManufacturingDescription;

            if (string.Compare(roleName, cManufacturing) == 0)
                return Strings.cManufacturingDescription;

            if (string.Compare(roleName, aPurchases) == 0)
                return Strings.aPurchasesDescription;

            if (string.Compare(roleName, bPurchases) == 0)
                return Strings.bPurchasesDescription;

            if (string.Compare(roleName, cPurchases) == 0)
                return Strings.cPurchasesDescription;

            if (string.Compare(roleName, aMailConnector) == 0)
                return Strings.aMailConnectorDescription;

            if (string.Compare(roleName, aXtech) == 0)
                return Strings.aXtechDescription;

            if (string.Compare(roleName, aUnprotected) == 0)
                return Strings.aUnprotectedDescription;

            if (string.Compare(roleName, DefaultBaseRoles.Purchases.ToString()) == 0)
                return Strings.PurchasesDescription;

            if (string.Compare(roleName, DefaultBaseRoles.Accounting.ToString()) == 0)
                return Strings.AccountingDescription;

            if (string.Compare(roleName, DefaultBaseRoles.Configuration.ToString()) == 0)
                return Strings.ConfigurationDescription;

            if (string.Compare(roleName, DefaultBaseRoles.Inventory.ToString()) == 0)
                return Strings.InventoryDescription;

            if (string.Compare(roleName, DefaultBaseRoles.Manufacturing.ToString()) == 0)
                return Strings.ManufacturingDescription;

            if (string.Compare(roleName, DefaultBaseRoles.Sales.ToString()) == 0)
                return Strings.SalesDescription;

            if (string.Compare(roleName, aDMSManager) == 0)
                return Strings.aDMSManager;

            if (string.Compare(roleName, aResourceManager) == 0)
                return Strings.aResourceManager;
            return string.Empty;

        }

        //---------------------------------------------------------------------
        public static bool IsAdvancedRole(string role)
        {
            ArrayList advancedRoles = GetAllAdvancedRoles();
            foreach (string[] adRole in advancedRoles)
            {
                if (string.Compare(adRole[0], role, true) == 0)
                    return true;
            }
            return false;

        }

    }
}
