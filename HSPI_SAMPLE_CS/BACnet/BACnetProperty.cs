using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO.BACnet;
//using Utilities.

namespace HSPI_SIID.BACnet
{
    [DataContract]
    public class BACnetProperty
    {


    //    private string m_name = string.Empty;
    //    private bool m_readonly = false;
    //    private object m_old_value = null;
    //    private object m_value = null;
    //    private Type m_type;
    //    private object m_tag;
    //    private DynamicEnum m_options;
    //    private string m_category;
    //    // Modif FC : change type
    //    private BacnetApplicationTags? m_description;

    //    // Modif FC : constructor
    //    public BACnetProperty(string name, object value, Type type, bool read_only, string category = "", BacnetApplicationTags? description = null, DynamicEnum options = null, object tag = null)
    //    {
    //        this.m_name = name;
    //        this.m_old_value = value;
    //        this.m_value = value;
    //        this.m_type = type;
    //        this.m_readonly = read_only;
    //        this.m_tag = tag;
    //        this.m_options = options;
    //        this.m_category = "BacnetProperty";
    //        this.m_description = description;
    //    }

    //    public DynamicEnum Options
    //    {
    //        get { return m_options; }
    //    }

    //    public Type Type
    //    {
    //        get { return m_type; }
    //    }

    //    public string Category
    //    {
    //        get { return m_category; }
    //    }

    //    // Modif FC
    //    public string Description
    //    {
    //        get { return m_description == null ? null : m_description.ToString(); }
    //    }

    //    // Modif FC : added
    //    public BacnetApplicationTags? bacnetApplicationTags
    //    {
    //        get { return m_description; }
    //    }

    //    public bool ReadOnly
    //    {
    //        get
    //        {
    //            return m_readonly;
    //        }
    //    }

    //    public string Name
    //    {
    //        get
    //        {
    //            return m_name;
    //        }
    //    }

    //    public bool Visible
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }

    //    public object Value
    //    {
    //        get
    //        {
    //            return m_value;
    //        }
    //        set
    //        {
    //            m_value = value;
    //        }
    //    }

    //    public object Tag
    //    {
    //        get { return m_tag; }
    //    }

    //    public void Reset()
    //    {
    //        m_value = m_old_value;
    //    }
    //}






        public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL)
            : this(bno, property_id, new BacnetValue(null), array_index)
        {


            //if (property_value = null)
            //    ReadProperty();
        }



        public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id, BacnetValue property_value, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) //this(bno, property_id, array_index)
        {

            this.BacnetObject = bno;
            this.BacnetPropertyId = property_id;

            this.Id = (Int32)property_id;

            //this.BacnetPropertyId = 

            SetName();


            this.arrayIndex = array_index;


            if (property_value.Value == null)
                ReadProperty();
            else
                this.BacnetValue = property_value;
        }



        private void SetName()
        {

            String ts = BacnetPropertyId.ToString();    //should just get numeric value if proprietary and outside of predefined alues.

            if (ts.StartsWith("PROP_"))
                ts = ts.Substring(5);   //else - proprietary...?  No, since it was already cast into BacnetObjectTypes enum.
            //else
            //    ts = "PROPRIETARY_(" + ts + ")";   //not sure if this handles it adequately, but will see...

            String[] tw = ts.Split("_".ToCharArray());
            String tsFinal = "";
            foreach (String word in tw)
                tsFinal += word[0].ToString().ToUpper() + word.Substring(1).ToLower() + " ";

            tsFinal = tsFinal.TrimEnd();
            this.Name = tsFinal;   

        }


        private uint arrayIndex;


        public String Name;

        public Int32 Id;



        public BACnetObject BacnetObject;


        [DataMember]
        public BacnetValue BacnetValue;


        public IList<BacnetValue> BacnetValues;  //not sure how this works yet...



        //TODO: have all objects hold three required properties separately?

        public BacnetPropertyIds BacnetPropertyId;




        private bool ReadProperty()
        {


            var comm = BacnetObject.BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetObject.BacnetDevice.BacnetAddress;
            var object_id = BacnetObject.BacnetObjectId;
            var property_id = BacnetPropertyId;
            var array_index = arrayIndex;


            //BacnetPropertyValue new_entry = new BacnetPropertyValue();
            //new_entry.property = new BacnetPropertyReference((uint)property_id, array_index);   //don't need this.  Already have.
            IList<BacnetValue> value;
            try
            {
                if (!comm.ReadPropertyRequest(adr, object_id, property_id, out value, 0, array_index))
                    return false;     //ignore
            }
            catch
            {
                return false;         //ignore
            }

            if (value.Count == 1)
                this.BacnetValue = value[0];    //this should never be called on object groups or structured views, so will only contain one value....
            else
                this.BacnetValues = value;  //otherwise leave value as Null, since we can't handle multiples here...

            //this.BacnetValues = value;      //will this list always only contain one?  What about possible properties?

            //new_entry.value = value;
            //values.Add(new_entry);
            return true;
        }



        //private bool ReadProperty(ref IList<BacnetPropertyValue> values, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL)
        //{


        //    var comm = BacnetObject.  bacnetDevice.BacnetNetwork.BacnetClient;
        //    var adr = bacnetDevice.BacnetAddress;
        //    var bobj_id = BacnetObjectId;


        //    BacnetPropertyValue new_entry = new BacnetPropertyValue();
        //    new_entry.property = new BacnetPropertyReference((uint)property_id, array_index);   //don't need this.  Already have.
        //    IList<BacnetValue> value;
        //    try
        //    {
        //        if (!comm.ReadPropertyRequest(adr, object_id, property_id, out value, 0, array_index))
        //            return false;     //ignore
        //    }
        //    catch
        //    {
        //        return false;         //ignore
        //    }
        //    new_entry.value = value;
        //    values.Add(new_entry);
        //    return true;
        //}



    }
}
