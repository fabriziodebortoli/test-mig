using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
    [Flags]
    public enum IMagoModulesConfiguration
    {
        NONE = 0,
        DMS = 1,
        SALES = 2,
        MARKETING = 4
    }

    public class LogInfo
    {
        public IProviderLogWriter ProviderLogWriter { get; set; }
        public string CompanyName { get; set; }
        public string ProviderName { get; set; }
    }

    internal static class Utilities
    {
        public const string IMAGO_DMS_CONF = "DMS";
        public const string IMAGO_SALES_CONF = "SALES";
        public const string IMAGO_MARKETING_CONF = "MARKETING";

        public const string PROVIDER_IAF_CONFIGURATION_DMS = "IAFDMS";
        public const string PROVIDER_IAF_CONFIGURATION_SALES = "IAFCRMSALES";
        public const string PROVIDER_IAF_CONFIGURATION_MARKETING = "IAFCRMARKETING";
        public const string PROVIDER_IAF_CONFIGURATION_SALES_AND_MARKETING = "IAFCRMSALESMARKETING";


        ///<summary>
        /// Restituisce la maschera di bit rappresentante i Moduli configurati di IMago
        ///</summary>
        ///<param name="semicolonSeparatedModules">Lista dei Moduli di IMago configurati e separati da ";". L'insieme previsto è [DMS, SALES, MARKETING]</param>
        //--------------------------------------------------------------------------------
        public static IMagoModulesConfiguration GetIMagoCoonfigurationModules(string semicolonSeparatedModules, LogInfo logInfo)
        {
            IMagoModulesConfiguration result = IMagoModulesConfiguration.NONE;

            try
            {
                if (string.IsNullOrEmpty(semicolonSeparatedModules) || string.IsNullOrWhiteSpace(semicolonSeparatedModules))
                {

                    logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, "No IMago configuration Modules...", "GetIMagoCoonfigurationModules");
                    return result;
                }

                string[] splitted = semicolonSeparatedModules.Split(';');
                var badConfigurations = splitted.Where(conf => !conf.Equals(IMAGO_DMS_CONF, StringComparison.CurrentCultureIgnoreCase)
                                                            && !conf.Equals(IMAGO_SALES_CONF, StringComparison.CurrentCultureIgnoreCase)
                                                            && !conf.Equals(IMAGO_MARKETING_CONF, StringComparison.CurrentCultureIgnoreCase));

                if (badConfigurations != null && badConfigurations.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Found bad IMago Configuration Modules: ");

                    foreach (var item in badConfigurations)
                    {
                        sb.Append($" {item} ");
                    }

                    logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, sb.ToString(), "GetIMagoCoonfigurationModules");
                    return result;
                }


                foreach (var item in splitted)
                {
                    if (item.Equals(IMAGO_DMS_CONF, StringComparison.CurrentCultureIgnoreCase))
                        result |= IMagoModulesConfiguration.DMS;
                    else if (item.Equals(IMAGO_SALES_CONF, StringComparison.CurrentCultureIgnoreCase))
                        result |= IMagoModulesConfiguration.SALES;
                    else if (item.Equals(IMAGO_MARKETING_CONF, StringComparison.CurrentCultureIgnoreCase))
                        result |= IMagoModulesConfiguration.MARKETING;
                }
            }
            catch (Exception e)
            {
                logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, e.Message, "GetIMagoCoonfigurationModules");
            }

            return result;
        }

        public static bool GetCRMEnabledForAction(string actionName, IMagoModulesConfiguration iMagoModulesConfiguration, string iafModule, LogInfo logInfo, bool bIMagoActivated)
        {
            if (!bIMagoActivated) // quando non e' attivo il modulo IMAGO non devo valorizzare lo iafmodule ma devo poter sincronizzare tutto
                return true;

            try
            {
                switch (iafModule)
                {
                    case PROVIDER_IAF_CONFIGURATION_DMS:
                        return (IMagoModulesConfiguration.DMS & iMagoModulesConfiguration) == IMagoModulesConfiguration.DMS;

                    case PROVIDER_IAF_CONFIGURATION_SALES:
                        return (IMagoModulesConfiguration.SALES & iMagoModulesConfiguration) == IMagoModulesConfiguration.SALES;

                    case PROVIDER_IAF_CONFIGURATION_MARKETING:
                        return (IMagoModulesConfiguration.MARKETING & iMagoModulesConfiguration) == IMagoModulesConfiguration.MARKETING;

                    case PROVIDER_IAF_CONFIGURATION_SALES_AND_MARKETING:
                        return ((IMagoModulesConfiguration.MARKETING & iMagoModulesConfiguration) == IMagoModulesConfiguration.MARKETING) &&
                               ((IMagoModulesConfiguration.SALES & iMagoModulesConfiguration) == IMagoModulesConfiguration.SALES);
                    default:
                        logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, "Unknown IAFModule non ", $"GetCRMEnabledForAction(Action: {actionName}, IAFModule: {iafModule}, [...])");
                        return false;
                }
            }
            catch (Exception e)
            {
                logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, e.Message, $"GetCRMEnabledForAction(Action: {actionName}, IAFModule: {iafModule}, [...])");
                return false;
            }
        }

                
        public static bool GetCRMEnabledForAction(ActionToMassiveSync action, string iafModule, LogInfo logInfo, bool bIMagoActivated)
        {
            if (action == null)
            {
                logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, "Action cannot be null", "GetIMagoCoonfigurationModules");
                return false;
            }

            return GetCRMEnabledForAction(action?.Name, action.IMagoModulesConfiguration, iafModule, logInfo, bIMagoActivated);
        }

    }
}
