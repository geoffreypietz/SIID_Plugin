using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.BACnet
{
    [Serializable]
    public class BACnetTreeNode
    {
        public bool lazy = false;

        public bool folder = false;

        public String title;

        //public abstract Dictionary<String, Object> data();

        public Dictionary<String, Object> data = new Dictionary<String, Object>();

        public List<BACnetTreeNode> children = new List<BACnetTreeNode>();

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
