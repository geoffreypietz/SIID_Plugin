using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using HSPI_SIID.General;
using Scheduler;
using HSPI_SIID;
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

        private static readonly String emptyResult = "[]";

        //private JavaScriptSerializer jss = new JavaScriptSerializer();

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




       //TODO: private methods for getting diff. objects....because we will need these




        public BACnetGlobalNetwork GetBacnetGlobalNetwork(NameValueCollection nodeData, Boolean refresh = false)     //any time this is called, shouldn't we refresh filters?  Maybe not.....
        {
            //always refresh if calling from data service, but if calling internally to create Homeseer object, no need to refresh everything


            if (Instance.bacnetGlobalNetwork == null || refresh)    //if they re-filtered
            {
                Instance.bacnetGlobalNetwork = new BACnetGlobalNetwork(
                    this.Instance,
                    Boolean.Parse(nodeData["filter_ip_address"] ?? "false"),
                    nodeData["selected_ip_address"],
                    nodeData["udp_port"] ?? "BAC0",
                    Boolean.Parse(nodeData["filter_device_instance"] ?? "false"),
                    Int32.Parse(nodeData["device_instance_min"] ?? "0"),
                    Int32.Parse(nodeData["device_instance_max"] ?? "4194303"));
            }

            return Instance.bacnetGlobalNetwork;

        }




        public BACnetNetwork GetBacnetNetwork(NameValueCollection nodeData, Boolean refresh = false)
        {
            var bacnetGlobalNetwork = GetBacnetGlobalNetwork(nodeData, refresh);
            if (refresh)    //if calling from API, children won't exist yet.
                bacnetGlobalNetwork.Discover();

            BACnetNetwork bacnetNetwork; // = null;
            bacnetGlobalNetwork.BacnetNetworks.TryGetValue(nodeData["ip_address"], out bacnetNetwork);  //sometimes BacnetNetwork can be null, if discovery not initiated.
            return bacnetNetwork;
        }




        public BACnetDevice GetBacnetDevice(NameValueCollection nodeData, Boolean refresh = false)
        {

            var bacnetNetwork = GetBacnetNetwork(nodeData, refresh);
            if (refresh)
                bacnetNetwork.Discover();

            BACnetDevice bacnetDevice;
            bacnetNetwork.BacnetDevices.TryGetValue(uint.Parse(nodeData["device_instance"]), out bacnetDevice);
            return bacnetDevice;

        }



        public BACnetObject GetBacnetObject(NameValueCollection nodeData, Boolean refresh = false)
        {
            var bacnetDevice = GetBacnetDevice(nodeData, refresh);
            if (refresh)
                bacnetDevice.GetObjects();


            BacnetObjectTypes objType = (BacnetObjectTypes)(Int32.Parse(nodeData["object_type"]));
            UInt32 objInstance = UInt32.Parse(nodeData["object_instance"]);
            var bacnetObjectId = new BacnetObjectId(objType, objInstance);

            var bacnetObject = bacnetDevice.GetBacnetObject(bacnetObjectId);
            return bacnetObject;

        }



        //public BACnetObject GetBacnetProperty(NameValueCollection nodeData)
        //{

        //    var bacnetDevice = GetBacnetDevice(nodeData, refresh);

        //    BacnetObjectTypes objType = (BacnetObjectTypes)(Int32.Parse(nodeData["object_type"]));
        //    UInt32 objInstance = UInt32.Parse(nodeData["object_instance"]);
        //    var bacnetObjectId = new BacnetObjectId(objType, objInstance);

        //    var bacnetObject = bacnetDevice.GetBacnetObject(bacnetObjectId);
        //    return bacnetObject;

        //}







        //private String SerializedResult(List<BACnetTreeNode> nodes)
        //{
        //    return jss.Serialize(nodes);
        //}



       //not just tree data...


       //GetBacnetNode(string nodeData)             //GetNodeData(Bacnet node)


       //public void Get(String data, Boolean treeData = false)
       //{




       //}






        public string GetTreeData(String data)      //this is called from discover tree, but also
        {

            var jss = new JavaScriptSerializer();
            var emptyResult = jss.Serialize(null);

            //TODO: if there are no keys supplied, just get all data.  


            System.Collections.Specialized.NameValueCollection nodeData = null;
            nodeData = System.Web.HttpUtility.ParseQueryString(data);

            var dataType = nodeData["node_type"];

            //IBACnetTreeDataObject node;

            try
            {



                //switch (dataType)
                //{
                //    case "root":    //only happens for getNodeData - this is a construct used for treeview, has no underlying bacnet data structure
                //        return jss.Serialize(new List<BACnetTreeNode>() { BACnetGlobalNetwork.RootNode() });
                //        //node = BACnetGlobalNetwork.RootNode();
                //        //return jss.Serialize(new List<BACnetTreeNode>() { BACnetGlobalNetwork.RootNode() });
                //    //break;
                //    case "global_network":
                //        node = GetBacnetGlobalNetwork(nodeData, true);
                //        break;
                //        //return jss.Serialize(GetBacnetGlobalNetwork(nodeData, true).GetChildNodes());
                //    //break;
                //    case "network":
                //        node = GetBacnetNetwork(nodeData);
                //        break;
                //        //return jss.Serialize(GetBacnetNetwork(nodeData).GetChildNodes());
                //    //break;
                //    case "device":
                //        node = GetBacnetDevice(nodeData);
                //        break;
                //        //return jss.Serialize(GetBacnetDevice(nodeData).GetChildNodes());
                //    //break;
                //    case "object":
                //        node = GetBacnetObject(nodeData);
                //        return jss.Serialize(GetBacnetObject(nodeData).GetProperties());
                //    //break;
                //    //case "property":
                //    //    return jss.Serialize(GetBacnetProperty(nodeData, true));    //not node data, just list of id/value/names
                //    //    break;
                //    default:
                //        //return emptyResult;
                //        break;
                //}



                

                switch (dataType)
                {
                    case "root":
                        return jss.Serialize(new List<BACnetTreeNode>() { BACnetGlobalNetwork.RootNode() });
                        //break;
                    case "global_network":
                        return jss.Serialize(GetBacnetGlobalNetwork(nodeData, true).GetChildNodes());
                        //break;
                    case "network":
                        return jss.Serialize(GetBacnetNetwork(nodeData).GetChildNodes());
                        //break;
                    case "device":
                        return jss.Serialize(GetBacnetDevice(nodeData).GetChildNodes());
                        //break;
                    case "object":
                        return jss.Serialize(GetBacnetObject(nodeData).GetProperties());
                        //break;
                    //case "property":
                    //    return jss.Serialize(GetBacnetProperty(nodeData, true));    //not node data, just list of id/value/names
                    //    break;
                    default:
                        //return emptyResult;
                        break;
                }
            }
            catch (Exception ex)
            {
                //return emptyResult;
            }

            return emptyResult;

        }

    }
}
