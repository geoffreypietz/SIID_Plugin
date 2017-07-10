using System.Collections.Generic;

namespace System.IO.BACnet
{
    public class BacnetDeviceLine
    {
        public BacnetClient Line;
        public List<KeyValuePair<BacnetAddress, uint>> Devices = new List<KeyValuePair<BacnetAddress, uint>>();
        public HashSet<byte> mstp_sources_seen = new HashSet<byte>();
        public HashSet<byte> mstp_pfm_destinations_seen = new HashSet<byte>();
        public BacnetDeviceLine(BacnetClient bacnetClient)
        {
            Line = bacnetClient;
        }
    }
}
