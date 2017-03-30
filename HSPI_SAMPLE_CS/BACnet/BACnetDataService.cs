using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HSPI_SIID.General;
using Scheduler;
using HSPI_SIID_ModBusDemo;
using HomeSeerAPI;
using System.Web;
using System.Web.Script.Serialization;
using System.IO.BACnet;

namespace HSPI_SIID.BACnet
{
   public class BACnetDataService : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }
        public htmlBuilder BACnetBuilder { get; set; }

        //public List<string> DiscoveredBACnetDevices { get; set; }

        public String PageName { get; set; }


        public BACnetDataService(string pagename, InstanceHolder instance)
            : base(pagename)
        {

            Instance = instance;    //hmm.  Aren't we still accessing same network data, even if multiple instances?  Is a lock necessary?
            //BACnetBuilder = new htmlBuilder("BACnetObjectDataService" + Instance.ajaxName);
            //DiscoveredBACnetDevices = new List<string>();

            this.PageName = pagename + instance.ajaxName;

        }




        public string GetData(String data)
        {

            var jss = new JavaScriptSerializer();


            //TODO: if there are no keys supplied, just get all data.  


            System.Collections.Specialized.NameValueCollection node = null;
            node = System.Web.HttpUtility.ParseQueryString(data);


            var dataType = node["type"];

            //Get data for root node ("All networks") - to which filters are attached



            if (dataType == "root")
                return jss.Serialize(new List<BACnetTreeNode>(){BACnetGlobalNetwork.RootNode()});



            if (Instance.bacnetGlobalNetwork == null || dataType == "global_network")    //if they re-filtered
            {

                Instance.bacnetGlobalNetwork = new BACnetGlobalNetwork(
                    this.Instance,
                    node["selected_ip_address"],
                    node["udp_port"] ?? "BAC0",
                    Boolean.Parse(node["filter_device_instance"] ?? "false"),
                    Int32.Parse(node["device_instance_min"] ?? "0"),
                    Int32.Parse(node["device_instance_max"] ?? "4194303"));
            }


            if (dataType == "global_network")
                return jss.Serialize(Instance.bacnetGlobalNetwork.GetChildNodes());


            //Get data for a network node (one IP address)
            
            BACnetNetwork bacnetNetwork;
            string ipAddr = node["ip_address"];
            if (!Instance.bacnetGlobalNetwork.BacnetNetworks.TryGetValue(ipAddr, out bacnetNetwork))
                return "[]";

            if (dataType == "network")
                return jss.Serialize(bacnetNetwork.GetChildNodes());

            uint deviceInstance = uint.Parse(node["device_instance"]);
            BACnetDevice bacnetDevice;
            if (!bacnetNetwork.BacnetDevices.TryGetValue(deviceInstance, out bacnetDevice))
                return "[]";


            if (dataType == "device")
                return jss.Serialize(bacnetDevice.GetChildNodes());


            BacnetObjectTypes objType = (BacnetObjectTypes)(Int32.Parse(node["object_type"]));
            UInt32 objInstance = UInt32.Parse(node["object_instance"]);
            var bacnetObjectId = new BacnetObjectId(objType, objInstance);


            //uint deviceInstance = uint.Parse(post["device_instance"]);
            BACnetObject bacnetObject;
            if (!bacnetDevice.TryGetBacnetObject(bacnetObjectId, out bacnetObject))
                return "[]";


            if (dataType == "object")
                return jss.Serialize(bacnetObject.GetProperties());


            //return jss.Serialize(bacnetObject.GetChildNodes());


        //public BacnetObjectTypes type;
        //public UInt32 instance;


        //    //var



        //    var objType = (BacnetObjectTypes)Int32.Parse(post["object_type"]);      // /.Split('_')[1])

        //    var objInstance = UInt32.Parse(post["object_instance"]);


            

            //var bacnetObject = 



            //TODO


            //string partID = changed["id"].Split('_')[0];
            //int devId = Int32.Parse(changed["id"].Split('_')[1]);


            //var ipAddr = 



            return "";

        }







    }
}
