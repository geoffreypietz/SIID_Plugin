using System;
using System.Collections.Generic;
using System.Linq;
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

            this.Name = "";

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

        }




        public static string[] Attributes = new string[]{
       //        "Type",
       //"BACnetString",    //what are these?


       "Type",

       "NetworkIPAddress",


       "DeviceIPAddress",

       "DeviceUDPPort",


       "DeviceInstance",

       "ObjectType",

       "ObjectInstance",

       "ObjectName",

            "PollInterval",

            "RawValue",

            "ProcessedValue"
        
        };





        public BACnetTreeNode GetTreeNode()
        {
            var tn = new BACnetTreeNode();
            //tn.children = null;
            tn.title = this.Name;
            tn.CopyNodeData(this.BacnetDevice.GetTreeNode());
            tn.data["object_type"] = (Int32)this.BacnetObjectId.Type;      //could have done string, probably...but Int is more consistent (it's the underlying type)
            tn.data["object_instance"] = this.BacnetObjectId.Instance;
            tn.data["node_type"] = "object";
            return tn;
        }



        public List<BACnetTreeNode> GetChildNodes()     //this one only works from within the application...or on discover/refresh button?
        {
            var childNodes = new List<BACnetTreeNode>();

            //TODO: for now, objects don't get child nodes

            return childNodes;
        }




        public List<Object> GetProperties()
        {
            //if (!AllPropertiesFetched)
                FetchProperties();              //still want to refresh, in case properties are changing

            var propertiesData = new List<Object>();

            //foreach (var bacnetProperty in RequiredProperties)
            //{
            //    propertiesData.Add(new
            //    {
            //        ID = bacnetProperty.Value.Id,
            //        Name = bacnetProperty.Value.Name,
            //        Value = bacnetProperty.Value.BacnetValue.ToString()
            //    });
            //}


            foreach (var item in BacnetProperties)
            {
                var bacnetProperty =item.Value;

                propertiesData.Add(new {
                    ID = bacnetProperty.Id,
                    Name = bacnetProperty.Name,
                    Value = bacnetProperty.ValueString()
                });
            }

            return propertiesData;
        }



        //public Boolean TryGetProperty(BacnetObjectId boi, out BACnetObject bo)
        //{
        //    bo = null;
        //    foreach (var kvp in BacnetObjects)
        //    {
        //        if (kvp.Key.Equals(boi))
        //        {
        //            bo = kvp.Value;
        //            return true;
        //        }
        //    }
        //    return false;
        //}




        //TODO: shouldn't be adding objects to objects anymore, with current structure.

        public void AddObject(BacnetObjectId boi, BACnetObject bo)
        {
            var kvp = new KeyValuePair<BacnetObjectId, BACnetObject>(boi, bo);
            Objects.Add(kvp);
        }


        [DataMember]
        public List<KeyValuePair<BacnetObjectId, BACnetObject>> Objects = new List<KeyValuePair<BacnetObjectId, BACnetObject>>();


        [DataMember]
        public List<KeyValuePair<BacnetPropertyIds, BACnetProperty>> BacnetProperties = new List<KeyValuePair<BacnetPropertyIds, BACnetProperty>>();



        [DataMember]
        public Dictionary<BacnetPropertyIds, BACnetProperty> RequiredProperties = new Dictionary<BacnetPropertyIds, BACnetProperty>();



        public static readonly BacnetPropertyIds[] RequiredPropertyIds = {
                                                                          BacnetPropertyIds.PROP_OBJECT_TYPE, 
                                                                          BacnetPropertyIds.PROP_OBJECT_IDENTIFIER,     //instance #
                                                                          BacnetPropertyIds.PROP_OBJECT_NAME };


        public static readonly BacnetPropertyIds[] SupportedPropertyIds = {
                                                                          BacnetPropertyIds.PROP_OBJECT_IDENTIFIER,    
                                                                          BacnetPropertyIds.PROP_OBJECT_NAME,
                                                                          BacnetPropertyIds.PROP_OBJECT_TYPE, 
                                                                          BacnetPropertyIds.PROP_PRESENT_VALUE, 
                                                                          BacnetPropertyIds.PROP_PRIORITY_ARRAY,
                                                                          BacnetPropertyIds.PROP_DESCRIPTION, 
                                                                          BacnetPropertyIds.PROP_DEVICE_TYPE, 
                                                                          BacnetPropertyIds.PROP_STATUS_FLAGS, 
                                                                          BacnetPropertyIds.PROP_EVENT_STATE, 
                                                                          BacnetPropertyIds.PROP_OUT_OF_SERVICE, 
                                                                          BacnetPropertyIds.PROP_UPDATE_INTERVAL, 
                                                                          BacnetPropertyIds.PROP_UNITS, 
                                                                          BacnetPropertyIds.PROP_MIN_PRES_VALUE, 
                                                                          BacnetPropertyIds.PROP_MAX_PRES_VALUE, 
                                                                          BacnetPropertyIds.PROP_RESOLUTION, 
                                                                          BacnetPropertyIds.PROP_COV_INCREMENT, 
                                                                          BacnetPropertyIds.PROP_NOTIFICATION_CLASS,
                                                                          BacnetPropertyIds.PROP_HIGH_LIMIT,
                                                                          BacnetPropertyIds.PROP_LOW_LIMIT,
                                                                          BacnetPropertyIds.PROP_DEADBAND,
                                                                          BacnetPropertyIds.PROP_LIMIT_ENABLE,
                                                                          BacnetPropertyIds.PROP_EVENT_ENABLE,
                                                                          BacnetPropertyIds.PROP_NOTIFY_TYPE,
                                                                          BacnetPropertyIds.PROP_STATE_TEXT
                                                                          };


        public int WritePriority = 0;


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




        public void FetchRequiredProperties()           //honesty could just fetch all properties and then add them in appropriately...
        {
            var object_id = this.BacnetObjectId;



            BacnetPropertyValue new_entry = new BacnetPropertyValue();
            new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_IDENTIFIER); //, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
            new_entry.value = new BacnetValue[] { new BacnetValue(object_id) };
            AddProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, new_entry);


            //AddProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, new BacnetValue(object_id));

            

            // ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_TYPE, ref values);
            // No need to query it, known value
            new_entry = new BacnetPropertyValue();
            new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_TYPE, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
            new_entry.value = new BacnetValue[] { new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type) };
            //values.Add(new_entry);
            AddProperty(BacnetPropertyIds.PROP_OBJECT_TYPE, new_entry);

            //AddProperty(BacnetPropertyIds.PROP_OBJECT_TYPE, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type));



            // We do not know the value here
            //ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, ref values);

            AddProperty(BacnetPropertyIds.PROP_OBJECT_NAME);    //not known, so it will get.


            SetName();

            //Name = RequiredProperties[BacnetPropertyIds.PROP_OBJECT_NAME].BacnetValue.Value.ToString();


        }



        private void SetName()      //don't call in the middle of FetchProperties...        actually, now it's fine because of bypass refresh
        {
            try
            {
                BacnetPropertyValue namePropVal = (BacnetPropertyValue)GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_NAME, true).BacnetPropertyValue;
                //careful - don't call GetBacnetProperty internally, or else it will keep trying to refresh properties since AllPropertiesFetched is still false

                Name = namePropVal.value[0].ToString();   //BacnetValue.Value.ToString();

            }
            catch (Exception ex)
            {
                //leave it alone?
            }

        }


        public Boolean AllPropertiesFetched = false;



        //public Boolean TryGetBacnetProperty(BacnetObjectId boi, out BACnetProperty bnp)
        //{
        //    bnp = null;
        //    foreach (var kvp in BacnetProperties)
        //    {
        //        if (kvp.Key.Equals(boi))
        //        {
        //            bo = kvp.Value;
        //            return true;
        //        }
        //    }
        //    return false;
        //}



        public BACnetProperty GetBacnetProperty(BacnetPropertyIds bpi, Boolean bypassRefresh = false)
        {

            if (!AllPropertiesFetched && !bypassRefresh)
                FetchProperties();

            foreach (var kvp in BacnetProperties)
            {
                BacnetPropertyIds thisPropId = kvp.Key;
                if (thisPropId == bpi)
                    return kvp.Value;
                
            }
            return null;
        }





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





        public void FetchProperties()      //if NOT structured view, etc...right?
        {

            lock (this.BacnetProperties)
            {

                //BacnetProperties.Clear();     //yes, just re-fetch required properties.       //don't want to wipe this out if an error occurs though


                var oldProperties = new List<KeyValuePair<BacnetPropertyIds, BACnetProperty>>();
                oldProperties.AddRange(BacnetProperties);




                //TODO: put a lock on this in case another is trying at the same time...
                //but wait, these are kept per-instance, right?

                BacnetProperties.Clear();


                AllPropertiesFetched = false;


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
                            AddProperty(p_value);



                            //var id = (BacnetPropertyIds)p_value.property.propertyIdentifier;



                            //var val = p_value.value[0];


                            ////var requiredProps = new List<BacnetPropertyIds>(){BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, BacnetPropertyIds.PROP_OBJECT_TYPE, BacnetPropertyIds.PROP_OBJECT_NAME};

                            ////if (requiredProps.Contains(id))     //already fetched these before.
                            ////    continue;


                            //AddProperty(id, val);   //if required properties, will just overwrite
                        }




                    }
                    else
                    {
                        Trace.TraceWarning("Couldn't perform ReadPropertyMultiple ... Trying ReadProperty instead");

                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Couldn't perform ReadPropertyMultiple ... Trying ReadProperty instead");

                    if (!ReadAllPropertiesBySingle())   //shouldn't really have to return and process values...just add in place.
                    {
                        Trace.TraceWarning("Couldn't perform ReadProperty ... Trying ReadProperty instead");
                        //MessageBox.Show(this, "Couldn't fetch properties", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        BacnetProperties = oldProperties;
                        return;
                    }

                    //return; //?
                }







                AllPropertiesFetched = true;

                SetName();

            }

        }








        private bool ReadAllPropertiesBySingle() //(out IList<BacnetReadAccessResult> value_list)
        {

            FetchRequiredProperties();

            var comm = BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetDevice.BacnetAddress;
            var object_id = BacnetObjectId;

            //lock (BACnetGlobalNetwork.objectsDescriptionDefault)  //read-only, should be fine
            //lock (BACnetGlobalNetwork.objectsDescriptionExternal)
            if (BACnetGlobalNetwork.objectsDescriptionDefault == null)  // first call, Read Objects description from internal & optional external xml file
            {
                StreamReader sr;
                XmlSerializer xs = new XmlSerializer(typeof(List<BacnetObjectDescription>));

                // embedded resource
                //System.Reflection.Assembly _assembly;
                //_assembly = System.Reflection.Assembly.GetExecutingAssembly();
                //sr = new StreamReader(_assembly.GetManifestResourceStream("ReadSinglePropDescrDefault.xml"));
                sr = new StreamReader("ReadSinglePropDescrDefault.xml");
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

            if (!BACnetObject.SupportedPropertyIds.Contains(id))        //make this better later
                return;

            //var bpv = new BacnetPropertyValue();


            var prop = new BACnetProperty(this, id);

            addProp(id, prop);

            //AddProperty(id, new BacnetValue(null));     //setting this to null means we still need to read the property.

        }



        private void AddProperty(BacnetPropertyValue val)
        {
            var id = (BacnetPropertyIds)val.property.propertyIdentifier;
            AddProperty(id, val);
        }



        private void AddProperty(BacnetPropertyIds id, BacnetPropertyValue val)
        {
            //var id = BacnetPropertyIds.PROP_OBJECT_IDENTIFIER;
            //var val = new BacnetValue(object_id);



            if (!BACnetObject.SupportedPropertyIds.Contains(id))
                return;


            var prop = new BACnetProperty(this, id, val);

            addProp(id, prop);
            //if id == BacnetPropertyIds.





            //if (BACnetObject.RequiredPropertyIds.Contains(id))        //don't keep required properties separate anymore.....
            //    RequiredProperties[id] = prop;      //TODO: could put option to not overwrite, I guess...
            //else
            //{
            //var kvp = new KeyValuePair<BacnetPropertyIds, BACnetProperty>(id, prop);
            //BacnetProperties.Add(kvp);
        }


        private void addProp(BacnetPropertyIds id, BACnetProperty prop)
        {
            if (!BACnetObject.SupportedPropertyIds.Contains(id))
                return;


            //Console.WriteLine("Read property: " + id.ToString() + " (value: " + prop.ValueString() + ")");

            if (id == BacnetPropertyIds.PROP_PRESENT_VALUE)
                Console.WriteLine(this.BacnetObjectId +    ": present value = " + prop.ValueString());


            var kvp = new KeyValuePair<BacnetPropertyIds, BACnetProperty>(id, prop);
            BacnetProperties.Add(kvp);
        }


    }











        //private void AddProperty(BacnetPropertyIds id)
        //{
        //    AddProperty(id, new BacnetValue(null));     //setting this to null means we still need to read the property.

        //}


        //private void AddProperty(BacnetPropertyIds id, BacnetValue val)
        //{
        //    //var id = BacnetPropertyIds.PROP_OBJECT_IDENTIFIER;
        //    //var val = new BacnetValue(object_id);






        //    if (!BACnetObject.SupportedPropertyIds.Contains(id))
        //        return;

        //    var prop = new BACnetProperty(this, id, val);


        //    //if id == BacnetPropertyIds.



            

        //    //if (BACnetObject.RequiredPropertyIds.Contains(id))        //don't keep required properties separate anymore.....
        //    //    RequiredProperties[id] = prop;      //TODO: could put option to not overwrite, I guess...
        //    //else
        //    //{
        //        var kvp = new KeyValuePair<BacnetPropertyIds, BACnetProperty>(id, prop);
        //        BacnetProperties.Add(kvp);
        //    }


        //}


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
