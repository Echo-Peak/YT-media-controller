using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv
{
    internal class FirewallManager
    {
        // Constants for Windows Firewall COM interface
        private const int NET_FW_RULE_DIR_IN = 1;
        private const int NET_FW_ACTION_ALLOW = 1;
        private const int NET_FW_IP_PROTOCOL_TCP = 6;
        private const int NET_FW_PROFILE2_ALL = 0x7FFFFFFF;

        private dynamic Policy => Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        private readonly ILogger logger;
        
        public FirewallManager(ILogger Logger) {
               logger = Logger;
        }
        
        private dynamic FindExact(string name, int protocol)
        {
            foreach (dynamic r in Policy.Rules)
                if (string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)
                    && r.Direction == NET_FW_RULE_DIR_IN
                    && r.Protocol == protocol)
                    return r;
            return null;
        }
        
        public void Update(string ruleName, int port)
        {
            var rule = FindExact(ruleName, NET_FW_IP_PROTOCOL_TCP);
            if (rule == null)
            {
                logger.Info($"Creating inbound FW rule \"{ruleName}\" to use port {port}");
                rule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                rule.Name = ruleName;
                rule.Direction = NET_FW_RULE_DIR_IN;
                rule.Action = NET_FW_ACTION_ALLOW;
                rule.Enabled = true;
                rule.Profiles = NET_FW_PROFILE2_ALL;
                rule.Protocol = NET_FW_IP_PROTOCOL_TCP;
                rule.LocalPorts = port.ToString();
                Policy.Rules.Add(rule);
            }
            else
            {
                logger.Info($"Updating FW rule \"{ruleName}\" to use port {port}");
                rule.Protocol = NET_FW_IP_PROTOCOL_TCP;
                rule.LocalPorts = port.ToString();
                rule.Enabled = true;
            }
        }

        public void Remove(string ruleName)
        {
            var rule = FindExact(ruleName, NET_FW_IP_PROTOCOL_TCP);
            if(rule != null)
            {
                logger.Info($"Removing FW rule \"{ruleName}\"");
                try { Policy.Rules.Remove(rule.Name); } catch (Exception err) {
                    logger.Error($"Unable to remove FW rule ({ruleName})", err);
                }
            }
            else
            {
                logger.Debug($"Did not find FW rule \"{ruleName}\" to remove");
            }
        }
    }
}
