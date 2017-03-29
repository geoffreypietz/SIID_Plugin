using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.IO.BACnet;
using System.Diagnostics;
using System.Xml.Serialization;


namespace HSPI_SIID.BACnet
{
    [DataContract]
    public class BACnetObject : IBACnetTreeDataObject
    {

        public BACnetObject(BACnetDevice bnd, BacnetObjectId boi)
        {
            this.BacnetDevice = bnd;        //need this for communication - i.e. get sub-objects.
            this.BacnetObjectId = boi;

            //GetName();  //or just get all properties....


            FetchRequiredProperties();    //obj type, obj instance (already known), name


            //may already know this from device...



            //Changed this.  Now treating structured views and groups just like any other type........
            //if (boi.type == BacnetObjectTypes.OBJECT_GROUP)
            //    FetchGroupProperties();     //should obj_id be a param?  Isn't it just this?
            //else if ((boi.type == BacnetObjectTypes.OBJECT_STRUCTURED_VIEW)) // && Yabe.Properties.Settings.Default.DefaultPreferStructuredView)
            //    FetchViewObjects();
            //else


            //FetchProperties();  //yes, even if device, I guess...



            //if (!(this is BACnetDevice))
            //{
                
            //    GetObjectDetails();
            //}

        }




        //public struct TreeNodeData
        //{
        //    //public bool lazy = true;    //don't get child objects until this node is selected
        //    public string title;
        //    //public string key;    //just let these be auto-generated; we need more than this to uniquely identify node anyway


        //    public Int32 object_type;
        //    public UInt32 object_instance;


        //    //public uint object_type;    //or some type...

        //    //public BACnetObject.



        //    //since not lazy, don't need all this stuff in here...

        //    //public string ip_address;
        //    //public uint device_instance;


        //    //public string data_url;
        //}


        //public TreeNodeData GetTreeNode()
        //{
        //    //TODO: this would be where we call Discover.  Right?  Or just get properties of network itself....


        //    FetchRequiredProperties();

        //    var node = new TreeNodeData();


        //    node.title = RequiredProperties[BacnetPropertyIds.PROP_OBJECT_NAME].BacnetValue.Value.ToString();


        //    node.object_

        //    //node.title = TypeString;



        //    //TODO: node.children?





        //    //node.title = "Device " + InstanceNumber + " - " + BacnetAddress.ToString();



        //    ////Data needed to fetch node's children through post request
        //    //node.ip_address = BacnetNetwork.IpAddress;
        //    //node.device_instance = InstanceNumber;


        //    return node;

        //    //node.data_url = BAC


        //    //return new JavaScriptSerializer().Serialize(node);    //the parent can do the serialization....
        //}


        //public string GetChildNodes()
        //{



        //}





        //public BACnetObject(BACnetDevice bnd, uint instanceNumber)
        //{
        //    this.BacnetDevice = bnd;        //need this for communication - i.e. get sub-objects.
        //    //this.BacnetObjectId = boi;

        //    //GetName();  //or just get all properties....


        //    FetchRequiredProperties();


        //    //may already know this from device...



        //    if (!(this is BACnetDevice))
        //    {

        //        GetObjectDetails();
        //    }

        //}





        public BACnetTreeNode GetTreeNode()
        {
            return new TreeNode(this);
        }



        [Serializable]
        public class TreeNode : BACnetTreeNode
        {
            public TreeNode(BACnetObject bacnetObject) //: base(parent)
            {
                title = bacnetObject.Name;   //should already be fetched by this point

                folder = true;

                //lazy = true;      //For now, no object sub-nodes.

                children = null;

                CopyNodeData(bacnetObject.BacnetDevice.GetTreeNode());

                data["object_type"] = (Int32)bacnetObject.BacnetObjectId.Type;      //could have done string, probably...but Int is more consistent (it's the underlying type)
                data["object_instance"] = bacnetObject.BacnetObjectId.Instance;


            }
        }


        public List<BACnetTreeNode> GetChildNodes()     //this one only works from within the application...or on discover/refresh button?
        {
            var childNodes = new List<BACnetTreeNode>();

            //TODO: for now, objects don't get child nodes

            return childNodes;
        }




        //TODO: shouldn't be adding objects to objects anymore, with current structure.

        public void AddObject(BacnetObjectId boi, BACnetObject bo)
        {
            var kvp = new KeyValuePair<BacnetObjectId, BACnetObject>(boi, bo);
            Objects.Add(kvp);
        }


        //TODO: might need function to get a certain object as well...




        //public Dictionary<BacnetObjectId, BACnetObject> Objects = new Dictionary<BacnetObjectId, BACnetObject>();







        //public OrderedDictionary<BacnetObjectId, BACnetObject> Objects = new OrderedDictionary<BacnetObjectId, BACnetObject>();

        [DataMember]
        public List<KeyValuePair<BacnetObjectId, BACnetObject>> Objects = new List<KeyValuePair<BacnetObjectId, BACnetObject>>();
        //we do want them to be ordered appropriately...otherwise could just do dictionary...but ordered dictionary too complicated.


        //public List<Object> Properties;



        [DataMember]
        public List<KeyValuePair<BacnetPropertyIds, BACnetProperty>> Properties = new List<KeyValuePair<BacnetPropertyIds, BACnetProperty>>();




        //public List<KeyValuePair<BacnetPropertyIds, BACnetProperty>> RequiredProperties = new List<KeyValuePair<BacnetPropertyIds, BACnetProperty>>();
        [DataMember]
        public Dictionary<BacnetPropertyIds, BACnetProperty> RequiredProperties = new Dictionary<BacnetPropertyIds, BACnetProperty>();


        //order shouldn't matter for these, can control how they're displayed elsewhere.


        public static readonly BacnetPropertyIds[] RequiredPropertyIds = {
                                                                          BacnetPropertyIds.PROP_OBJECT_TYPE, 
                                                                          BacnetPropertyIds.PROP_OBJECT_IDENTIFIER,     //instance #
                                                                          BacnetPropertyIds.PROP_OBJECT_NAME };



        public BACnetDevice BacnetDevice;      //unless we have bacnetdevice inherit from this class...?


        [DataMember]
        public BacnetObjectId BacnetObjectId { get; set; }


        [DataMember]
        public String Name { get; set; }    //this is different than the property object name...keep it for now...


        [DataMember]
        public String DisplayName;



        public static List<BacnetObjectId> SortBacnetObjects(IList<BacnetValue> RawList)
        {

            List<BacnetObjectId> SortedList = new List<BacnetObjectId>();
            foreach (BacnetValue value in RawList)
                SortedList.Add((BacnetObjectId)value.Value);

            SortedList.Sort();

            return SortedList;
        }




        //TODO: instead, just get the list of properties...?

        //public void GetName()
        //{
        //    var comm = BacnetDevice.BacnetNetwork.BacnetClient;
        //    var adr = BacnetDevice.BacnetAddress;
        //    var bobj_id = BacnetObjectId;

        //    //  String name = "";       //not returning a value.  Just setting internal property...

        //    try
        //    {
        //        IList<BacnetValue> values;
        //        if (comm.ReadPropertyRequest(adr, bobj_id, BacnetPropertyIds.PROP_OBJECT_NAME, out values))
        //        {
        //            Name = values[0].ToString();
        //        }


        //        //TODO: is this always how to fetch a name?

        //    }
        //    catch { }



        //}


        public void FetchRequiredProperties()           //honesty could just fetch all properties and then add them in appropriately...
        {
            var object_id = this.BacnetObjectId;


            AddProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, new BacnetValue(object_id));

            

            // ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_TYPE, ref values);
            // No need to query it, known value
            //new_entry = new BacnetPropertyValue();
            //new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_TYPE, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
            //new_entry.value = new BacnetValue[] { new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type) };
            //values.Add(new_entry);


            AddProperty(BacnetPropertyIds.PROP_OBJECT_TYPE, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type));



            // We do not know the value here
            //ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, ref values);

            AddProperty(BacnetPropertyIds.PROP_OBJECT_NAME);    //not known, so it will get.




            Name = RequiredProperties[BacnetPropertyIds.PROP_OBJECT_NAME].BacnetValue.Value.ToString();


        }





        //private void GetObjectDetails()   //called by bacnetDevice, or parent object...
        //{
        //    var comm = BacnetDevice.BacnetNetwork.BacnetClient;
        //    var adr = BacnetDevice.BacnetAddress;
        //    var object_id = BacnetObjectId;

        //    //if (string.IsNullOrEmpty(name)) 
            

        //    //TODO: don't really need any of this stuff...

        //    string name = object_id.ToString();    //not sure if we still want this argument in here, but will see...

        //    //TreeNode node;

        //    String text;


        //    if (name.StartsWith("OBJECT_"))
        //        text = name.Substring(7);
        //    else
        //        text = "PROPRIETARY:" + object_id.Instance.ToString() + " (" + name + ")";  // Propertary Objects not in enum appears only with the number such as 584:0


        //    DisplayName = text;

        //    //TODO: does this become object name, or what?

        //    //Name = text;

        //    //no, get name same as for every object.

        //    ////in general, how do we get the name for an object?  is it 

        //    //// Get the property name if already known
        //    //String PropName;

        //    //lock (DevicesObjectsName)
        //    //    if (DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), object_id), out PropName) == true)
        //    //    {
        //    //        node.ToolTipText = node.Text;
        //    //        node.Text = PropName;
        //    //    }

        //    //fetch sub properties
        //    if (object_id.type == BacnetObjectTypes.OBJECT_GROUP)
        //        FetchGroupProperties();     //should obj_id be a param?  Isn't it just this?
        //    else if ((object_id.type == BacnetObjectTypes.OBJECT_STRUCTURED_VIEW)) // && Yabe.Properties.Settings.Default.DefaultPreferStructuredView)
        //        FetchViewObjects();
        //    else
        //        FetchProperties();

        //    //else
        //    //    FetchObjects();     //TODO: not sure about this...does Yabe do this?  Can there be sub-objects for other types?


        //    //FetchProperties();




        //    //TODO: get sub-objects, and properties...?

        //}




        private void FetchGroupProperties()     //for THIS object.  Then AddObjectEntry populates list of objects.
        {
            var comm = BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetDevice.BacnetAddress;
            var object_id = BacnetObjectId;

            try
            {
                IList<BacnetValue> values;
                if (comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_LIST_OF_GROUP_MEMBERS, out values))
                {
                    foreach (BacnetValue value in values)
                    {
                        if (value.Value is BacnetReadAccessSpecification)
                        {
                            BacnetReadAccessSpecification spec = (BacnetReadAccessSpecification)value.Value;
                            foreach (BacnetPropertyReference p in spec.propertyReferences)
                            {
                                var subObjId = spec.objectIdentifier;
                                AddObject(subObjId, new BACnetObject(BacnetDevice, subObjId));


                                //don't necessarily want to get properties then?  can't get property of property?

                                //AddObjectEntry(comm, adr, spec.objectIdentifier.ToString() + ":" + ((BacnetPropertyIds)p.propertyIdentifier).ToString(), spec.objectIdentifier);
                                //for now just treat as any other object, but maybe come back to...

                                //hmmm.  This code makes it seem like group members have to be properties...what is up w/ propertyIdentifier field?




                                //need to po
                            }
                        }
                    }
                }
                else
                {
                    Trace.TraceWarning("Couldn't fetch group members");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Couldn't fetch group members: " + ex.Message);
            }
        }


        private void FetchViewObjects()
        {
            var comm = BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetDevice.BacnetAddress;
            var object_id = BacnetObjectId;

            try
            {
                IList<BacnetValue> values;
                if (comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_SUBORDINATE_LIST, out values))
                {
                    List<BacnetObjectId> objectList = SortBacnetObjects(values);
                    foreach (BacnetObjectId objid in objectList)
                        AddObject(objid, new BACnetObject(BacnetDevice, objid));

                        //Objects.Add(objid, new BACnetObject(bacnetDevice, objid));  //then this object will self-populate.
                        //AddObjectEntry(comm, adr, null, objid, nodes);
                }
                else
                {
                    Trace.TraceWarning("Couldn't fetch view members");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Couldn't fetch view members: " + ex.Message);
            }
        }



        //private IList<BacnetValue> FetchObjects()
        //{
        //    var comm = BacnetDevice.BacnetNetwork.BacnetClient;
        //    var adr = BacnetDevice.BacnetAddress;
        //    var object_id = BacnetObjectId;

        //    IList<BacnetValue> value_list;

        //    try
        //    {

        //        //TOD: how does object ID and type get set properly for other cases?

        //        if (!comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_OBJECT_LIST, out value_list))
        //        {
        //            //Trace.TraceWarning("Didn't get response from 'Object List'");
        //            value_list = null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Trace.TraceWarning("Got exception from 'Object List'");
        //        value_list = null;
        //    }

        //    return value_list;

        //}



        protected void FetchProperties()      //if NOT structured view, etc...right?
        {


            //TODO: if this is a structured view or object group, handle differently
            //A structured view has no Present Value prooperty
            //An object group's present value property is just its list of members, so we really just want to make sub-objects, not properties.


            //Also, in Yabe the properties will actually show up in the address space...this is not what we want for plugin


            var comm = BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetDevice.BacnetAddress;
            var object_id = BacnetObjectId;
            BacnetPropertyReference[] properties = new BacnetPropertyReference[] { new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_ALL, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) };

            //do we want to skip the properties used above?  For structured view, group, etc.?

            IList<BacnetReadAccessResult> multi_value_list = new List<BacnetReadAccessResult>();
            try
            {
                //fetch properties. This might not be supported (ReadMultiple) or the response might be too long.
                if (comm.ReadPropertyMultipleRequest(adr, object_id, properties, out multi_value_list))
                {

                    //update grid
                    //Utilities.DynamicPropertyGridContainer bag = new Utilities.DynamicPropertyGridContainer();
                    foreach (BacnetPropertyValue p_value in multi_value_list[0].values)
                    {

                        var id = (BacnetPropertyIds)p_value.property.propertyIdentifier;
                        var val = p_value.value[0];


                        //var requiredProps = new List<BacnetPropertyIds>(){BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, BacnetPropertyIds.PROP_OBJECT_TYPE, BacnetPropertyIds.PROP_OBJECT_NAME};

                        //if (requiredProps.Contains(id))     //already fetched these before.
                        //    continue;


                        AddProperty(id, val);   //if required properties, will just overwrite
                    }




                }
                else{
                    Trace.TraceWarning("Couldn't perform ReadPropertyMultiple ... Trying ReadProperty instead");
                    if (!ReadAllPropertiesBySingle())   //shouldn't really have to return and process values...just add in place.
                    {
                        //MessageBox.Show(this, "Couldn't fetch properties", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            catch (Exception)
            {
                Trace.TraceWarning("Couldn't perform ReadPropertyMultiple ... Trying ReadProperty instead");
                return; //?
            }


            ////TODO: process value list...?




            ////update grid
            ////Utilities.DynamicPropertyGridContainer bag = new Utilities.DynamicPropertyGridContainer();
            //foreach (BacnetPropertyValue p_value in multi_value_list[0].values)
            //{

            //    var id = (BacnetPropertyIds)p_value.property.propertyIdentifier;
            //    var val = p_value.value[0];


            //    var requiredProps = new List<BacnetPropertyIds>(){BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, BacnetPropertyIds.PROP_OBJECT_TYPE, BacnetPropertyIds.PROP_OBJECT_NAME};

            //    if (requiredProps.Contains(id))     //already fetched these before.
            //        continue;


            //AddProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, new BacnetValue(object_id));

            //// ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_TYPE, ref values);
            //// No need to query it, known value
            ////new_entry = new BacnetPropertyValue();
            ////new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_TYPE, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
            ////new_entry.value = new BacnetValue[] { new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type) };
            ////values.Add(new_entry);


            //AddProperty(BacnetPropertyIds.PROP_OBJECT_TYPE, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type));



            //// We do not know the value here
            ////ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, ref values);

            //AddProperty(BacnetPropertyIds.PROP_OBJECT_NAME);

            //    AddProperty(id, p_value.value[0]

            //    //Properties.Add(new BACnetProperty(

            //    //create new BACnetProperty object.


            //    object value = null;
            //    BacnetValue[] b_values = null;
            //    if (p_value.value != null)
            //    {

            //        b_values = new BacnetValue[p_value.value.Count];

            //        p_value.value.CopyTo(b_values, 0);
            //        if (b_values.Length > 1)
            //        {
            //            object[] arr = new object[b_values.Length];
            //            for (int j = 0; j < arr.Length; j++)
            //                arr[j] = b_values[j].Value;
            //            value = arr;
            //        }
            //        else if (b_values.Length == 1)
            //            value = b_values[0].Value;
            //    }
            //    else
            //        b_values = new BacnetValue[0];


            //    //TODO: are these sub-properties or just lists of possible values?

            //     //Modif FC
            //    switch ((BacnetPropertyIds)p_value.property.propertyIdentifier)
            //    {
            //        // PROP_RELINQUISH_DEFAULT can be write to null value
            //        case BacnetPropertyIds.PROP_PRESENT_VALUE:
            //            // change to the related nullable type
            //            Type t = null;
            //            try
            //            {
            //                t = value.GetType();
            //                t = Type.GetType("System.Nullable`1[" + value.GetType().FullName + "]");
            //            }
            //            catch { }
            //            bag.Add(new Utilities.CustomProperty(GetNiceName((BacnetPropertyIds)p_value.property.propertyIdentifier), value, t != null ? t : typeof(string), false, "", b_values.Length > 0 ? b_values[0].Tag : (BacnetApplicationTags?)null, null, p_value.property));
            //            break;

            //        default:
            //            bag.Add(new Utilities.CustomProperty(GetNiceName((BacnetPropertyIds)p_value.property.propertyIdentifier), value, value != null ? value.GetType() : typeof(string), false, "", b_values.Length > 0 ? b_values[0].Tag : (BacnetApplicationTags?)null, null, p_value.property));
            //            break;
            //    }


            //    //TODO: implement display name stuff here too

            //    //// The Prop Name replace the PropId into the Treenode 
            //    //if (p_value.property.propertyIdentifier == (byte)BacnetPropertyIds.PROP_OBJECT_NAME)
            //    //{
            //    //    if (selected_node.ToolTipText == "")  // Tooltip not set is not null, strange !
            //    //        selected_node.ToolTipText = selected_node.Text;

            //    //    selected_node.Text = value.ToString(); // Update the object name if needed
            //    //    lock (DevicesObjectsName)
            //    //    {
            //    //        Tuple<String, BacnetObjectId> t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), object_id);
            //    //        DevicesObjectsName.Remove(t);
            //    //        DevicesObjectsName.Add(t, value.ToString());
            //    //    }
            //    //}
            //}














            //TODO: remove handlers for property reads from Bacnet Client?




        }




        private bool ReadAllPropertiesBySingle() //(out IList<BacnetReadAccessResult> value_list)
        {
            var comm = BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetDevice.BacnetAddress;
            var object_id = BacnetObjectId;

            //lock (BACnetGlobalNetwork.objectsDescriptionDefault)  //read-only, should be fine
            if (BACnetGlobalNetwork.objectsDescriptionDefault == null)  // first call, Read Objects description from internal & optional external xml file
            {
                StreamReader sr;
                XmlSerializer xs = new XmlSerializer(typeof(List<BacnetObjectDescription>));

                // embedded resource
                System.Reflection.Assembly _assembly;
                _assembly = System.Reflection.Assembly.GetExecutingAssembly();
                sr = new StreamReader(_assembly.GetManifestResourceStream("Yabe.ReadSinglePropDescrDefault.xml"));
                BACnetGlobalNetwork.objectsDescriptionDefault = (List<BacnetObjectDescription>)xs.Deserialize(sr);

                try  // External optional file
                {
                    sr = new StreamReader("ReadSinglePropDescr.xml");
                    BACnetGlobalNetwork.objectsDescriptionExternal = (List<BacnetObjectDescription>)xs.Deserialize(sr);
                }
                catch { }

            }

            //value_list = null;

            IList<BacnetPropertyValue> values = new List<BacnetPropertyValue>();

            int old_retries = comm.Retries;
            comm.Retries = 1;       //we don't want to spend too much time on non existing properties
            try
            {
                // Three mandatory common properties to all objects : PROP_OBJECT_IDENTIFIER,PROP_OBJECT_TYPE, PROP_OBJECT_NAME

                // ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, ref values)
                // No need to query it, known value
                //BacnetPropertyValue new_entry = new BacnetPropertyValue();
                //new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
                //new_entry.value = new BacnetValue[] { new BacnetValue(object_id) };
                //values.Add(new_entry);

                //AddProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, new BacnetValue(object_id));

                // ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_TYPE, ref values);
                // No need to query it, known value
                //new_entry = new BacnetPropertyValue();
                //new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_TYPE, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
                //new_entry.value = new BacnetValue[] { new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type) };
                //values.Add(new_entry);


                //AddProperty(BacnetPropertyIds.PROP_OBJECT_TYPE, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type));



                // We do not know the value here
                //ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, ref values);

                //AddProperty(BacnetPropertyIds.PROP_OBJECT_NAME);



                //FetchRequiredProperties();

                //this is done elsewhere (w/ device name) - put it in function...


                // for all other properties, the list is comming from the internal or external XML file

                BacnetObjectDescription objDescr = new BacnetObjectDescription(); ;

                int Idx = -1;
                // try to find the Object description from the optional external xml file
                if (BACnetGlobalNetwork.objectsDescriptionExternal != null)
                    Idx = BACnetGlobalNetwork.objectsDescriptionExternal.FindIndex(o => o.typeId == object_id.type);

                if (Idx != -1)
                    objDescr = BACnetGlobalNetwork.objectsDescriptionExternal[Idx];
                else
                {
                    // try to find from the embedded resoruce
                    Idx = BACnetGlobalNetwork.objectsDescriptionDefault.FindIndex(o => o.typeId == object_id.type);
                    if (Idx != -1)
                        objDescr = BACnetGlobalNetwork.objectsDescriptionDefault[Idx];
                }

                if (Idx != -1)
                    foreach (BacnetPropertyIds bpi in objDescr.propsId)
                        // read all specified properties given by the xml file
                        //ReadProperty(comm, adr, object_id, bpi, ref values);
                        AddProperty(bpi);
            }
            catch { }

            comm.Retries = old_retries;
            //value_list = new BacnetReadAccessResult[] { new BacnetReadAccessResult(object_id, values) };      //not using this structure anymore
            return true;
        }




        //private void AddProperty(BacnetPropertyIds id)
        //{
        //    //var id = BacnetPropertyIds.PROP_OBJECT_IDENTIFIER;
        //    //var val = new BacnetValue(object_id);
        //    var prop = new BACnetProperty(this, id);    //don't know the value, so query device after property instantiation.
        //    var kvp = new KeyValuePair<BacnetPropertyIds, BACnetProperty>(id, prop);


        //    if (BACnetObject.RequiredPropertyIds.Contains(id))
        //        this.RequiredProperties[id] = prop;
        //    else
        //        Properties.Add(kvp);


        //}






        private void AddProperty(BacnetPropertyIds id)
        {
            AddProperty(id, new BacnetValue(null));     //setting this to null means we still need to read the property.

        }


        private void AddProperty(BacnetPropertyIds id, BacnetValue val)
        {
            //var id = BacnetPropertyIds.PROP_OBJECT_IDENTIFIER;
            //var val = new BacnetValue(object_id);

            var prop = new BACnetProperty(this, id, val);


            //if id == BacnetPropertyIds.
            

            if (BACnetObject.RequiredPropertyIds.Contains(id))
                RequiredProperties[id] = prop;      //TODO: could put option to not overwrite, I guess...
            else
            {
                var kvp = new KeyValuePair<BacnetPropertyIds, BACnetProperty>(id, prop);
                Properties.Add(kvp);
            }


        }


        //private bool ReadProperty(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id, BacnetPropertyIds property_id, ref IList<BacnetPropertyValue> values, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL)
        //{
        //    BacnetPropertyValue new_entry = new BacnetPropertyValue();
        //    new_entry.property = new BacnetPropertyReference((uint)property_id, array_index);
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
