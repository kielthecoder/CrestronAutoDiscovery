using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDiscoveryLib4
{
    public class DiscoveryAgent
    {
        private bool _active;
        public ushort Active
        {
            get
            {
                // Translate bool to ushort for SIMPL+
                if (_active)
                    return 1;
                else
                    return 0;
            }
        }

        public string IPv4 { get; set; }
        public string Hostname { get; set; }
        public string Description { get; set; }
        public int PollingPeriod { get; set; }

        public event EventHandler OnStarted;
        public event EventHandler OnStopping;
    }
}
