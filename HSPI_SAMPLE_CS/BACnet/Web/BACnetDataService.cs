using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Scheduler;
using System.Web.Script.Serialization;
using System.IO.BACnet;

namespace HSPI_SIID.BACnet
{
    public class BACnetDataService : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }
        //public htmlBuilder BACnetBuilder { get; set; }

        private static readonly String emptyResult = "[]";

        private BACnetGlobalNetwork bacnetGlobalNetwork = null;     //still only one per instance

        //private JavaScriptSerializer jss = new JavaScriptSerializer();

        //public List<string> DiscoveredBACnetDevices { get; set; }

        //public String PageName { get; set; }


        public static String BaseUrl = "BACnetDataService";

        public BACnetDataService(string pagename, InstanceHolder instance)
            : base(pagename)
        {

            Instance = instance;    //hmm.  Aren't we still accessing same network data, even if multiple instances?  Is a lock necessary?
            //BACnetBuilder = new htmlBuilder("BACnetObjectDataService" + Instance.ajaxName);
            //DiscoveredBACnetDevices = new List<string>();


            this.PageName = pagename + instance.ajaxName;

            //this.PageName = 

            //this.PageName = BaseUrl + instance.ajaxName.Replace(":", "_");

        }




       //TODO: private methods for getting diff. objects....because we will need these




        private BACnetGlobalNetwork GetBacnetGlobalNetwork(NameValueCollection nodeData, Boolean refresh = false)     //any time this is called, shouldn't we refresh filters?  Maybe not.....
        {
            //always refresh if calling from data service, but if calling internally to create Homeseer object, no need to refresh everything


            if (bacnetGlobalNetwork == null || refresh)    //if they re-filtered
            {
                bacnetGlobalNetwork = new BACnetGlobalNetwork(
                    this.Instance,
                    Boolean.Parse(nodeData["filter_ip_address"] ?? "false"),
                    nodeData["selected_ip_address"],
                    Int32.Parse(nodeData["udp_port"] ?? "47808"),
                    Boolean.Parse(nodeData["filter_device_instance"] ?? "false"),
                    Int32.Parse(nodeData["device_instance_min"] ?? "0"),
                    Int32.Parse(nodeData["device_instance_max"] ?? "4194303"));
                bacnetGlobalNetwork.Discover();
            }

            return bacnetGlobalNetwork;

        }




        public BACnetNetwork GetBacnetNetwork(NameValueCollection nodeData, Boolean refresh = false)
        {
            var bacnetGlobalNetwork = GetBacnetGlobalNetwork(nodeData, refresh);
    


            if (refresh)    //if calling from API, children won't exist yet.
                bacnetGlobalNetwork.Discover();

            //BACnetNetwork bacnetNetwork; // = null;
            //bacnetGlobalNetwork.BacnetNetworks.TryGetValue(nodeData["ip_address"], out bacnetNetwork);  //sometimes BacnetNetwork can be null, if discovery not initiated.
            //return bacnetNetwork;
            return bacnetGlobalNetwork.GetBacnetNetwork(nodeData["ip_address"]);
        
        }




        public BACnetDevice GetBacnetDevice(NameValueCollection nodeData, Boolean refresh = false)
        {

            var bacnetNetwork = GetBacnetNetwork(nodeData, refresh);
            if (refresh)
                bacnetNetwork.Discover();

            if (bacnetNetwork == null)
            {
                return null;
            }
            //BACnetDevice bacnetDevice;
            //bacnetNetwork.BacnetDevices.TryGetValue(uint.Parse(nodeData["device_instance"]), out bacnetDevice);
            //return bacnetDevice;
            return bacnetNetwork.GetBacnetDevice(uint.Parse(nodeData["device_instance"]));
        }



        public BACnetObject GetBacnetObject(NameValueCollection nodeData, Boolean refresh = false)
        {
            var bacnetDevice = GetBacnetDevice(nodeData, refresh);
            if (refresh)
                bacnetDevice.GetObjects();

            if (bacnetDevice != null)
            {


                BacnetObjectTypes objType = (BacnetObjectTypes)(Int32.Parse(nodeData["object_type"]));
                UInt32 objInstance = UInt32.Parse(nodeData["object_instance"]);
                var bacnetObjectId = new BacnetObjectId(objType, objInstance);

                var bacnetObject = bacnetDevice.GetBacnetObject(bacnetObjectId);
                return bacnetObject;
            }
            return null;

        }




        public BACnetObject GetBacnetObjectOrDeviceObject(NameValueCollection bacnetNodeData)
        {

            try
            {
                BACnetObject bno = null;
                if (bacnetNodeData["node_type"] == "device")
                {
                    var d = GetBacnetDevice(bacnetNodeData);//, true);  //still bug in this, somehow...
                    if (d != null)
                    {
                        bno = d.DeviceObject;
                    }


                    //for now, just have to refresh when getting anything device or above...since network, globalnetwork may be null
                }
                else
                {
                    bno = GetBacnetObject(bacnetNodeData);//, true);
                }

                return bno;


            }

            catch (Exception ex)
            {
                Instance.hspi.Log("BACnetDevice Exception " + ex.Message, 2);
                Console.WriteLine("Exception in GetBacnetObject: " + ex.StackTrace);
                return null;

                //ModbusConfHtml.add("Error: ", "Could not connect to device to retrieve object properties.  Please make sure that the BACnet device is present on the network.");
            }

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
                Instance.hspi.Log("BACnetDevice Exception " + ex.Message, 2);
                Console.WriteLine("exception in GetPagePlugin");
                Console.WriteLine(ex.StackTrace);
                //return emptyResult;
            }

            return emptyResult;

        }

    }
}
