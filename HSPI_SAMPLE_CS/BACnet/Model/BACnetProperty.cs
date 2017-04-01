using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO.BACnet;
using Utilities;
using System.ComponentModel;

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


        //public static readonly Dictionary<BacnetPropertyIds, BACnetPropertyDataType> PropertyDataTypes = new Dictionary<BacnetPropertyIds, BACnetPropertyDataType>(){

        //                                                                  {BacnetPropertyIds.PROP_OBJECT_IDENTIFIER,    
        //                                                                  BacnetPropertyIds.PROP_OBJECT_NAME,
        //                                                                  BacnetPropertyIds.PROP_OBJECT_TYPE, 
        //                                                                  BacnetPropertyIds.PROP_PRESENT_VALUE, 
        //                                                                  BacnetPropertyIds.PROP_DESCRIPTION, 
        //                                                                  BacnetPropertyIds.PROP_DEVICE_TYPE, 
        //                                                                  BacnetPropertyIds.PROP_STATUS_FLAGS, 
        //                                                                  BacnetPropertyIds.PROP_EVENT_STATE, 
        //                                                                  BacnetPropertyIds.PROP_OUT_OF_SERVICE, 
        //                                                                  BacnetPropertyIds.PROP_UPDATE_INTERVAL, 
        //                                                                  BacnetPropertyIds.PROP_UNITS, 
        //                                                                  BacnetPropertyIds.PROP_MIN_PRES_VALUE, 
        //                                                                  BacnetPropertyIds.PROP_MAX_PRES_VALUE, 
        //                                                                  BacnetPropertyIds.PROP_RESOLUTION, 
        //                                                                  BacnetPropertyIds.PROP_COV_INCREMENT, 
        //                                                                  BacnetPropertyIds.PROP_NOTIFICATION_CLASS,
        //                                                                  BacnetPropertyIds.PROP_HIGH_LIMIT,
        //                                                                  BacnetPropertyIds.PROP_LOW_LIMIT,
        //                                                                  BacnetPropertyIds.PROP_DEADBAND,
        //                                                                  BacnetPropertyIds.PROP_LIMIT_ENABLE,
        //                                                                  BacnetPropertyIds.PROP_EVENT_ENABLE,
        //                                                                  BacnetPropertyIds.PROP_NOTIFY_TYPE,

        //};




        private void Initialize(BACnetObject bno, BacnetPropertyIds property_id)
        {
            this.BacnetObject = bno;
            this.BacnetPropertyId = property_id;        //this is how these are indexed within the object

            this.Id = (Int32)property_id;
            SetName();



        }


        public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id)
        {
            Initialize(bno, property_id);
            ReadValue();
            ParseValue();

            //if (property_value = null)
            //    ReadProperty();
        }



        public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id, BacnetPropertyValue property_value) //, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) //this(bno, property_id, array_index)
        {
            Initialize(bno, property_id);
            this.BacnetPropertyValue = property_value;
            ParseValue();

        }








        private void ParseValue()
        {
            if (this.BacnetPropertyValue == null)
                return;

            BacnetPropertyValue p_value = (BacnetPropertyValue)this.BacnetPropertyValue;

            object value = null;
            BacnetValue[] b_values = null;
            if (p_value.value != null)
            {

                b_values = new BacnetValue[p_value.value.Count];

                p_value.value.CopyTo(b_values, 0);
                if (b_values.Length > 1)
                {
                    object[] arr = new object[b_values.Length];
                    for (int j = 0; j < arr.Length; j++)
                        arr[j] = b_values[j].Value;
                    value = arr;
                }
                else if (b_values.Length == 1)
                    value = b_values[0].Value;
            }
            else
                b_values = new BacnetValue[0];


            var propName = GetNiceName((BacnetPropertyIds)p_value.property.propertyIdentifier);
            var propRef = p_value.property;
            //var readOnly = false;   //may change...
            var appTag = b_values.Length > 0 ? b_values[0].Tag : (BacnetApplicationTags?)null;


            // Modif FC
            CustomProperty cp;

            Type propType = null;
            Boolean readOnly;

            switch ((BacnetPropertyIds)p_value.property.propertyIdentifier)
            {
                // PROP_RELINQUISH_DEFAULT can be write to null value
                case BacnetPropertyIds.PROP_PRESENT_VALUE:
                    // change to the related nullable type
                    Type t = null;
                    try
                    {
                        t = value.GetType();
                        t = Type.GetType("System.Nullable`1[" + value.GetType().FullName + "]");
                    }
                    catch { }

                    propType = t != null ? t : typeof(string);
                    readOnly = false;   //for now - in future, some properties may be
                    break;

                default:

                    propType = value != null ? value.GetType() : typeof(string);
                    readOnly = false;
                    break;
            }

            this.PropertyData = new Utilities.CustomProperty(propName, value, propType, readOnly, "", appTag, null, propRef); 

        }














        //public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL)
        //    : this(bno, property_id, new BacnetValue(null), array_index)
        //{


        //    //if (property_value = null)
        //    //    ReadProperty();
        //}



        //public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id, BacnetValue property_value, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) //this(bno, property_id, array_index)
        //{

        //    this.BacnetObject = bno;
        //    this.BacnetPropertyId = property_id;

        //    this.Id = (Int32)property_id;

        //    //this.BacnetPropertyId = 

        //    SetName();


        //    this.arrayIndex = array_index;


        //    if (property_value.Value == null)
        //        ReadProperty();
        //    else
        //        this.BacnetValue = property_value;
        //}




        //TODO: still not sure what to store in property object.




        private void SetName()          //todo: this function exists in Yabe already as GetNiceName....it's what we display as opposed to the enum value
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



        private static string GetNiceName(BacnetPropertyIds property)
        {
            string name = property.ToString();
            if (name.StartsWith("PROP_"))
            {
                name = name.Substring(5);
                name = name.Replace('_', ' ');
                name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            }
            else
                //name = "Proprietary (" + property.ToString() + ")";
                name = property.ToString() + " - Proprietary";
            return name;
        }




        private uint arrayIndex;


        public String Name;

        public Int32 Id;



        public BACnetObject BacnetObject;


        [DataMember]
        public BacnetValue BacnetValue;



        public BacnetPropertyValue? BacnetPropertyValue;



        public Utilities.CustomProperty PropertyData = null;



        public IList<BacnetValue> BacnetValues;  //not sure how this works yet...



        //TODO: have all objects hold three required properties separately?

        public BacnetPropertyIds BacnetPropertyId;






        //TODO: method to get value from HTML.  If we know what ID we assigned it to on page, will be easy....



        public bool WriteValue(object new_value)
        {
            var comm = BacnetObject.BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetObject.BacnetDevice.BacnetAddress;
            var object_id = BacnetObject.BacnetObjectId;
            var property_id = BacnetPropertyId;

            var success = false;


            var customProperty = this.PropertyData;  //customProperty

            //fetch property

            //BacnetPropertyReference property = (BacnetPropertyReference)customProperty.Tag;

            //new value
            //object new_value = gridItem.Value;
            var ty = new_value.GetType();

            //convert to bacnet
            BacnetValue[] b_value = null;
            try
            {
                if (new_value != null && new_value.GetType().IsArray)
                {
                    Array arr = (Array)new_value;
                    b_value = new BacnetValue[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                        b_value[i] = new BacnetValue(arr.GetValue(i));
                }
                else
                {
                    {
                        // Modif FC
                        b_value = new BacnetValue[1];
                        if ((BacnetApplicationTags)customProperty.bacnetApplicationTags != BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL)
                        {
                            b_value[0] = new BacnetValue((BacnetApplicationTags)customProperty.bacnetApplicationTags, new_value);
                        }
                        else
                        {
                            object o = null;
                            TypeConverter t = new TypeConverter();
                            // try to convert to the simplest type
                            String[] typelist = { "Boolean", "UInt32", "Int32", "Single", "Double" };

                            foreach (String typename in typelist)
                            {
                                try
                                {
                                    o = Convert.ChangeType(new_value, Type.GetType("System." + typename));
                                    break;
                                }
                                catch { }
                            }

                            if (o == null)
                                b_value[0] = new BacnetValue(new_value);
                            else
                                b_value[0] = new BacnetValue(o);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(this, "Couldn't convert property: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            //write
            try
            {
                comm.WritePriority = (uint)Yabe.Properties.Settings.Default.DefaultWritePriority;   //TODO: let this be changeable
                if (!comm.WritePropertyRequest(adr, object_id, BacnetPropertyId, b_value))
                {
                    success = false;
                    //MessageBox.Show(this, "Couldn't write property", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                success = false;
                //MessageBox.Show(this, "Error during write: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            success = true;

            //ReadValue();            //or do we let the webpage do this?  Once write callback has returned, read the value again?

            return success;

        }

        //instead of doing fancy HTML stuff, what about using AJAX call for each read and each write?  Generate the table row with the ability to get its initial value as well as write...
        //Yes...essentially the JS will include all of this.  The generated HTML will know to, on click, open up a dialog that, on close, will read




        public string GetHtml()     //maybe do a read to be safe?  Or, again, rather than embedding value, just give the controls the ability to get the data.
        {
            return "";


        }



        public string ValueString()
        {

            var bacnetValue = this.BacnetPropertyValue.Value;

            IList<BacnetValue> vals = bacnetValue.value;

            return vals[0].ToString();      //this gets more tricky if enum, etc....gets underlying Value (of type object) and calls ToString...
        }



        public bool ReadValue()
        {


            var comm = BacnetObject.BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetObject.BacnetDevice.BacnetAddress;
            var object_id = BacnetObject.BacnetObjectId;
            var property_id = BacnetPropertyId;
            var array_index = arrayIndex;


            BacnetPropertyValue new_entry = new BacnetPropertyValue();
            new_entry.property = new BacnetPropertyReference((uint)property_id); //, array_index);   //don't need this.  Already have.
            IList<BacnetValue> value;
            try
            {
                if (!comm.ReadPropertyRequest(adr, object_id, property_id, out value)) //, 0, array_index))      //TODO: do we ever need array index?
                    return false;     //ignore
            }
            catch
            {
                return false;         //ignore
            }
            new_entry.value = value;


            this.BacnetPropertyValue = new_entry;


            //if (value.Count == 1)
            //    this.BacnetValue = value[0];    //this should never be called on object groups or structured views, so will only contain one value....
            //else
            //    this.BacnetValues = value;  //otherwise leave value as Null, since we can't handle multiples here...

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
