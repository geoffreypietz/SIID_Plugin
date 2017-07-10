using System.Collections.Generic;

namespace HSPI_SIID.BACnet
{
    //maybe make this an interface instead.
    public interface IBACnetTreeDataObject
    {

        BACnetTreeNode GetTreeNode();

        List<BACnetTreeNode> GetChildNodes();

        //public abstract class TreeNode;

        //public BACnetTreeNode GetTreeNode(

    }
}
