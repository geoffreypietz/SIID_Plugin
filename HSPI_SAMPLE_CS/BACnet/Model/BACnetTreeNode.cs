using System;
using System.Collections.Generic;

namespace HSPI_Utilities_Plugin.BACnet
{
    [Serializable]
    public class BACnetTreeNode
    {

       public static readonly String[] DeviceNodeProperties = new String[] { "ip_address", "device_instance" };

       public static readonly String[] ObjectNodeProperties = new String[] { "ip_address", "device_instance", "object_type", "object_instance" };

        private bool _mLazy = false;

        public bool lazy {
            get { return _mLazy; }
            set {
                if (value)
                    children = null;
                _mLazy = value;
        } }

        public bool folder { get; set; }

        public String title { get; set; }

        //public abstract Dictionary<String, Object> data();

        public Dictionary<String, Object> data { get; set; }

        public List<BACnetTreeNode> children { get; set; }


        public BACnetTreeNode()
        {
            lazy = false;
            folder = false;
            data = new Dictionary<String, Object>();
            children = new List<BACnetTreeNode>();
        }


        //[NonSerialized]
        //public BACnetTreeNode Parent;


        //public BACnetTreeNode(BACnetTreeNode parentNode)
        //{
        //    this.Parent = parentNode;
        //    //CopyParentData();
        //}

        //public void CopyParentData()
        //{
        //    foreach (var kvp in Parent.data)
        //        this.data.Add(kvp.Key, kvp.Value);
        //}



        public void CopyNodeData(BACnetTreeNode otherNode)
        {
            foreach (var kvp in otherNode.data)
                this.data.Add(kvp.Key, kvp.Value);
        }


    }
}
