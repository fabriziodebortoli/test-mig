using System;

namespace Microarea.Library.Internet.BitsWrap
{
    public class ProxySettings
	{
        private BG_JOB_PROXY_USAGE m_Usage;
        private string m_sProxyList;
        private string m_sProxyBypassList;

        internal ProxySettings(BG_JOB_PROXY_USAGE usage, string proxyList, string bypassList)
		{
            this.m_Usage = usage;
            this.m_sProxyList = proxyList;
            this.m_sProxyBypassList = bypassList;
        }

        public ProxyUsage Usage
		{
            get
			{
                return (ProxyUsage)m_Usage;
            }
            set
			{
                m_Usage = (BG_JOB_PROXY_USAGE)value;
            }
        }

        public string ProxyList
		{
            get
			{
                return m_sProxyList;
            }
            set
			{
                m_sProxyList = value;
            }
        }

        public string ProxyBypassList
		{
            get
			{
                return m_sProxyBypassList;
            }
            set
			{
                m_sProxyBypassList = value;
            }
        }

    }
}