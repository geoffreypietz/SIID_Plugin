﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO.BACnet;
using System.IO.BACnet.Storage;


namespace HSPI_Utilities_Plugin.BACnet
{
    [DataContract]
    public class BACnetGlobalNetwork : IBACnetTreeDataObject
    {

        public BACnetGlobalNetwork(InstanceHolder instance, Boolean filterIpAddress = false, String selectedIpAddress = "", Int32 udpPort = 47808,
            Boolean filterDeviceInstance = false, Int32 deviceInstanceMin = 0, Int32 deviceInstanceMax = 4194303)
        {
            this.Instance = instance;

            this.FilterIpAddress = filterIpAddress;
            this.SelectedIpAddress = selectedIpAddress;
            this.UdpPort = udpPort;
            this.FilterDeviceInstance = filterDeviceInstance;
            this.DeviceInstanceMin = deviceInstanceMin;
            this.DeviceInstanceMax = deviceInstanceMax;

        }


        public InstanceHolder Instance { get; set; }


        public Boolean FilterIpAddress = false;

        public String SelectedIpAddress = null;


        //public int PortValue { get; set; }    //now udpPort

        public int RetriesValue = 2;

        public int TimeoutValue = 1000;



        //public String UdpPort = "BAC0";

        public Int32 UdpPort = 47808;


        public Boolean FilterDeviceInstance = false;

        public Int32 DeviceInstanceMin = 0;

        public Int32 DeviceInstanceMax = 99999;



        public static DeviceStorage m_storage;     //one per application.

        public static List<BacnetObjectDescription> objectsDescriptionExternal, objectsDescriptionDefault;


        public Boolean NetworksDiscovered = false;



        private string[] ipAddresses;



        [DataMember]
        public Dictionary<string, BACnetNetwork> BacnetNetworks;



        //Can call this before any filters set...good for getting initial tree data.
        public BACnetTreeNode GetTreeNode()
        {
            return BACnetGlobalNetwork.RootNode();

        }



        public static BACnetTreeNode RootNode()
        {
            var tn = new BACnetTreeNode();
            tn.title = "All networks";
            tn.folder = true;
            tn.lazy = true;
            //tn.children = null;
            //tn.data["is_global_network"] = true;
            tn.data["node_type"] = "global_network";
            return tn;
        }





        public List<BACnetTreeNode> GetChildNodes()     //this one only works from within the application...or on discover/refresh button?
        {
            Discover();     //don't need to get child networks until data requested?  This is a general pattern for these objects.


            var childNodes = new List<BACnetTreeNode>();


            foreach (var bn in BacnetNetworks.Values)
                childNodes.Add(bn.GetTreeNode());


            return childNodes;
        }






        public static string[] GetAvailableIps()
        {
            List<string> ips = new List<string>();
            System.Net.NetworkInformation.NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            Console.WriteLine("Available IPS:"+ interfaces.Length);

            foreach (System.Net.NetworkInformation.NetworkInterface inf in interfaces)
            {

                Console.WriteLine(inf.Name);

                if (!inf.IsReceiveOnly && inf.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && inf.SupportsMulticast && inf.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                {
                    System.Net.NetworkInformation.IPInterfaceProperties ipinfo = inf.GetIPProperties();
                    //if (ipinfo.GatewayAddresses == null || ipinfo.GatewayAddresses.Count == 0 || (ipinfo.GatewayAddresses.Count == 1 && ipinfo.GatewayAddresses[0].Address.ToString() == "0.0.0.0")) continue;
                    foreach (System.Net.NetworkInformation.UnicastIPAddressInformation addr in ipinfo.UnicastAddresses)
                    {
                        Console.WriteLine(addr.Address);

                        if ((addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))  // ||
                        // ((addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) && Yabe.Properties.Settings.Default.IPv6_Support))   //for now, no IPv6...
                        {
                            ips.Add(addr.Address.ToString());
                        }
                    }
                }
            }

            return ips.ToArray();
        }


        //Each BACnet discovery should first produce a list of IPs (>= 1, depending if they select the filter.  Each IP should spawn off a BACnetNetwork, which initiates its own device discovery
        //based on the filter settings in the discovery window.




        public void Discover()

        {

            NetworksDiscovered = false;


            ipAddresses = BACnetGlobalNetwork.GetAvailableIps();    //This doesn't depend on user filters, so don't need to call this method per instance.

            BacnetNetworks = new Dictionary<string, BACnetNetwork>();

            foreach (String ipAddress in ipAddresses)
            {
                if (!FilterIpAddress || (ipAddress == SelectedIpAddress))
                    BacnetNetworks.Add(ipAddress, new BACnetNetwork(this, ipAddress,Instance));
            }


            NetworksDiscovered = true;

        }


        public BACnetNetwork GetBacnetNetwork(String ipAddress)
        {
            //foreach (var kvp in BacnetObjects)
            //{
            //    if (kvp.Key.Equals(boi))
            //        return kvp.Value;

            //}
            //return null;


            if (!NetworksDiscovered)
                Discover();

       
            foreach (var kvp in BacnetNetworks)
            {
                
                String thisIpAddress = kvp.Key;
                if (thisIpAddress.Equals(ipAddress)) 
                    return kvp.Value;

            
            }
            //If we get here, then the ipAddress is not one of the BACnet networks. Return a 0 network to stop a crash


            return new BACnetNetwork(new BACnetGlobalNetwork(Instance), "0", Instance);

        


        }




    }
}
