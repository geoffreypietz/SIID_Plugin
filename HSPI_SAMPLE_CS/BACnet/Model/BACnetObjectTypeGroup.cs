using System;
using System.Collections.Generic;
using System.IO.BACnet;

namespace HSPI_Utilities_Plugin.BACnet
{
    public class BACnetObjectTypeGroup : IBACnetTreeDataObject
    {
        //public BacnetObjectId boi;

        public String TypeString;

        private List<BACnetObject> bacnetObjects = new List<BACnetObject>();

        public BACnetObjectTypeGroup(String typeString) //, List<BACnetObject> bacnetObjects)
        {
            this.TypeString = typeString;
            //this.BacnetObjects = bacnetObjects;
        }


        public void AddObject(BACnetObject bo)
        {
            this.bacnetObjects.Add(bo);
        }



        public static BACnetObjectTypeGroup GetByTypeString(List<BACnetObjectTypeGroup> bacnetObjectTypeGroups, String typeString)
        {
            foreach (BACnetObjectTypeGroup bacnetObjectTypeGroup in bacnetObjectTypeGroups)
            {
                if (bacnetObjectTypeGroup.TypeString == typeString)
                    return bacnetObjectTypeGroup;
            }
            return null;
        }


        public static List<BACnetObjectTypeGroup> OrganizeBacnetObjects(List<KeyValuePair<BacnetObjectId, BACnetObject>> bacnetObjects) //used from parent BACnet device
        {

            List<String> uniqueTypeStrings = new List<String>();
            List<BACnetObjectTypeGroup> bacnetObjectTypeGroups = new List<BACnetObjectTypeGroup>();


            foreach (var kvp in bacnetObjects)
            {
                String objTypeString = kvp.Key.typeString;
                if (!uniqueTypeStrings.Contains(objTypeString))
                {
                    uniqueTypeStrings.Add(objTypeString);        //should be in order already since we sorted BacnetObjects by type
                    bacnetObjectTypeGroups.Add(new BACnetObjectTypeGroup(objTypeString));
                }
            }


            foreach (var kvp in bacnetObjects)
            {
                var typeString = kvp.Key.typeString;
                var bacnetObject = kvp.Value;
                var bacnetObjectTypeGroup = BACnetObjectTypeGroup.GetByTypeString(bacnetObjectTypeGroups, typeString);
                bacnetObjectTypeGroup.AddObject(bacnetObject);
            }

            return bacnetObjectTypeGroups;

        }







        public BACnetTreeNode GetTreeNode()
        {
            var tn = new BACnetTreeNode();
            tn.title = this.TypeString;
            tn.children = this.GetChildNodes();
            tn.data["node_type"] = "object_type";
            return tn;
        }

        //[Serializable]
        //public class TreeNode : BACnetTreeNode
        //{
        //    public TreeNode(BACnetObjectTypeGroup bacnetObjectTypeGroup)  //: base(parent)
        //    {
        //        title = bacnetObjectTypeGroup.TypeString;

        //        lazy = false;
        //        children = bacnetObjectTypeGroup.GetChildNodes();
        //        data["type"] = "object_type";
        //    }
        //}



        public List<BACnetTreeNode> GetChildNodes()     //this one only works from within the application...or on discover/refresh button?
        {
            var childNodes = new List<BACnetTreeNode>();
            foreach (var bacnetObject in bacnetObjects)   //normally would leave children blank, but this is a lazy node
                childNodes.Add(bacnetObject.GetTreeNode());

            return childNodes;
        }




        //[Serializable]
        //public struct TreeNodeData //: BACnet.
        //{
        //    //public bool lazy = true;    //don't get child objects until this node is selected
        //    public string title;
        //    //public string key;    //just let these be auto-generated; we need more than this to uniquely identify node anyway


        //    //public uint object_type;    //or some type...

        //    public List<BACnetObject.TreeNodeData> children = new List<BACnetObject.TreeNodeData>();



        //    //public TreeNodeData(BACnetDevice.TreeNodeData bacnetDeviceNode)


        //    //since not lazy, don't need all this stuff in here...

        //    //public string ip_address;
        //    //public uint device_instance;


        //    //public string data_url;
        //}


        //public TreeNodeData GetTreeNodeData()
        //{
        //    //TODO: this would be where we call Discover.  Right?  Or just get properties of network itself....


        //    var node = new TreeNodeData();

        //    node.title = TypeString;

        //    foreach (var bacnetObject in BacnetObjects)
        //        node.children.Add(bacnetObject.GetTreeNodeData());




        //    //TODO: node.children?





        //    //node.title = "Device " + InstanceNumber + " - " + BacnetAddress.ToString();



        //    ////Data needed to fetch node's children through post request
        //    //node.ip_address = BacnetNetwork.IpAddress;
        //    //node.device_instance = InstanceNumber;


        //    return node;

        //    //node.data_url = BAC


        //    //return new JavaScriptSerializer().Serialize(node);    //the parent can do the serialization....
        //}




        }
}
