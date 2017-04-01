using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.BACnet;
using Yabe;


namespace HSPI_SIID.BACnet
{
    //Mostly for use with import and export function, not yet used for any more
    public class BACnetDevice : IBACnetTreeDataObject
    {

        public BACnetDevice(BACnetNetwork bnn, BacnetAddress bna, uint deviceId ) //  : base()  //: base(BACnetDevice bnd, BacnetObjectId boi)
        {

            this.BacnetNetwork = bnn;   //may be multiple devices on one network
            this.BacnetAddress = bna;   //each device has single bacnet network
            this.InstanceNumber = deviceId; //pretty much just for internal use

        }





        public static string[] Attributes = new string[]{
               "Type",
       "BACnetString",

       //From design doc
       "InstanceNumber",
       "NetworkNumber",
       "UDPPort",
            "IPAddress",
            "PollInterval",
            "BACnetName",

            "RawValue",
            "ProcessedValue"};



        public BACnetNetwork BacnetNetwork { get; set; }


        //not sure if need to store bacnet address.  This is external to device...


        //could also store this in deviceobject.
        //public uint DeviceId { get; set; }



        public uint InstanceNumber { get; set; }


        public BacnetAddress BacnetAddress { get; set; }



        //public Dictionary<BacnetObjectId, BACnetObject> Objects = new Dictionary<BacnetObjectId, BACnetObject>();


        public List<KeyValuePair<BacnetObjectId, BACnetObject>> BacnetObjects = new List<KeyValuePair<BacnetObjectId, BACnetObject>>();



        //public KeyValuePair<BacnetObjectId, BACnetObject> DeviceObject { get; set; }



        private BACnetObject _mDeviceObject = null;

        public BACnetObject DeviceObject { 
            
            get {

                if (_mDeviceObject == null)     //in case user tries to add device node before its children have been populated...
                    GetObjects();
                return _mDeviceObject;
            }
            
             set {

                 _mDeviceObject = value;
             }
        
        
        
        }      //TODO: don't really need to nest these.



        //private int AsynchRequestId = 0;





        public BACnetTreeNode GetTreeNode()
        {
            var tn = new BACnetTreeNode();
            tn.title = "Device " + this.InstanceNumber + " - " + this.BacnetAddress.ToString();    //TODO: use device name instead?
            tn.lazy = true;
            tn.CopyNodeData(this.BacnetNetwork.GetTreeNode());
            tn.data["device_instance"] = this.InstanceNumber;
            tn.data["node_type"] = "device";
            return tn;
        }



        //only need to define this function for lazy loaded nodes.
        public List<BACnetTreeNode> GetChildNodes()
        {
            GetObjects();   //when this node is expanded, need to know all of its objects

            var bacnetObjectTypeGroups = BACnetObjectTypeGroup.OrganizeBacnetObjects(BacnetObjects);


            var childNodes = new List<BACnetTreeNode>();

            foreach (var bacnetObjectTypeGroup in bacnetObjectTypeGroups)
                childNodes.Add(bacnetObjectTypeGroup.GetTreeNode());

            return childNodes;
        }




        public static readonly BacnetObjectTypes[] SupportedObjectTypes = { 
                                                                          BacnetObjectTypes.OBJECT_ANALOG_INPUT,
                                                                          BacnetObjectTypes.OBJECT_ANALOG_OUTPUT,
                                                                          BacnetObjectTypes.OBJECT_ANALOG_VALUE,
                                                                          BacnetObjectTypes.OBJECT_BINARY_INPUT,
                                                                          BacnetObjectTypes.OBJECT_BINARY_OUTPUT,
                                                                          BacnetObjectTypes.OBJECT_BINARY_VALUE,
                                                                          BacnetObjectTypes.OBJECT_DEVICE,
                                                                          BacnetObjectTypes.OBJECT_EVENT_ENROLLMENT,
                                                                          BacnetObjectTypes.OBJECT_MULTI_STATE_INPUT,
                                                                          BacnetObjectTypes.OBJECT_MULTI_STATE_OUTPUT,
                                                                          BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS,
                                                                          };






        //for this device, get its properties, and all object names

        public void GetObjects()
        {
            DeviceObject = null;
            BacnetObjects.Clear();


            //AsynchRequestId++; // disabled a possible thread pool work (update) on the AddressSpaceTree     

            //TODO: not sure if this should be kept at a device level or network level?

            var comm = BacnetNetwork.BacnetClient;
            var adr = BacnetAddress;

                //BacnetAddress adr = entry.Value.Key;
                //uint device_id = entry.Value.Value;

                //unconfigured MSTP?
                if (comm.Transport is BacnetMstpProtocolTransport && ((BacnetMstpProtocolTransport)comm.Transport).SourceAddress == -1)
                {
                    //if (MessageBox.Show("The MSTP transport is not yet configured. Would you like to set source_address now?", "Set Source Address", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;

                    ////find suggested address
                    //byte address = 0xFF;
                    //BacnetDeviceLine line = m_devices[comm];
                    //lock (line.mstp_sources_seen)
                    //{
                    //    foreach (byte s in line.mstp_pfm_destinations_seen)
                    //    {
                    //        if (s < address && !line.mstp_sources_seen.Contains(s))
                    //            address = s;
                    //    }
                    //}

                    ////display choice
                    //SourceAddressDialog dlg = new SourceAddressDialog();
                    //dlg.SourceAddress = address;
                    //if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel) return;
                    //((BacnetMstpProtocolTransport)comm.Transport).SourceAddress = dlg.SourceAddress;
                    //Application.DoEvents();     //let the interface relax
                }

                //update "address space"?
                //this.Cursor = Cursors.WaitCursor;
                //Application.DoEvents();
                //int old_timeout = comm.Timeout;


                IList<BacnetValue> value_list = null;
                try
                {
                    //fetch structured view if possible
                    //if (Yabe.Properties.Settings.Default.DefaultPreferStructuredView) 
                        //value_list = FetchStructuredObjects();

                    //fetch normal list
                    if (value_list == null)
                        value_list = FetchObjects() ?? FetchObjectsOneByOne();      //TODO: if this is the case, then we need to handle it once the value list returns populated...

                    if (value_list == null)
                        return;

                    List<BacnetObjectId> objectList = BACnetObject.SortBacnetObjects(value_list);
                    //add to tree
                    foreach (BacnetObjectId bobj_id in objectList)  //this will include sub-objects (analog_value, etc.) as well as object representing device itself.
                    {

                        var bacnetObject = new BACnetObject(this, bobj_id);





                        AddObject(bobj_id, bacnetObject);


                        //AddObjectEntry(comm, adr, null, bobj_id, m_AddressSpaceTree.Nodes);//AddObjectEntry(comm, adr, null, bobj_id, e.Node.Nodes); 
                    }
                }
                finally
                {
                    //this.Cursor = Cursors.Default;
                    //m_DataGrid.SelectedObject = null;
                }
            }




        public void AddObject(BacnetObjectId boi, BACnetObject bo)
        {
            if (!BACnetDevice.SupportedObjectTypes.Contains(boi.Type))
                return;

            if (boi.Type == BacnetObjectTypes.OBJECT_DEVICE)
                this.DeviceObject = bo;

            var kvp = new KeyValuePair<BacnetObjectId, BACnetObject>(boi, bo);
            BacnetObjects.Add(kvp);
        }




        //public Boolean TryGetBacnetObject(BacnetObjectId boi, out BACnetObject bo)
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



        public BACnetObject GetBacnetObject(BacnetObjectId boi)
        {
            foreach (var kvp in BacnetObjects)
            {
                if (kvp.Key.Equals(boi))
                    return kvp.Value;
                
            }
            return null;
        }





        //not used anymore...
        private IList<BacnetValue> FetchStructuredObjects()
        {
            IList<BacnetValue> ret;
            var comm = BacnetNetwork.BacnetClient;
            int old_reties = comm.Retries;


            //TODO: replace InstanceNumber with thing from required properties...

            try
            {
                comm.Retries = 1;       //only do 1 retry
                if (!comm.ReadPropertyRequest(BacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, InstanceNumber), BacnetPropertyIds.PROP_STRUCTURED_OBJECT_LIST, out ret))
                {
                    //Trace.TraceInformation("Didn't get response from 'Structured Object List'");
                    return null;
                }
                return ret == null || ret.Count == 0 ? null : ret;
            }
            catch (Exception)
            {
                //Trace.TraceInformation("Got exception from 'Structured Object List'");
                return null;
            }
            finally
            {
                comm.Retries = old_reties;
            }
        }


        private IList<BacnetValue> FetchObjects()
        {
            IList<BacnetValue> value_list;
            var comm = BacnetNetwork.BacnetClient;
            try
            {
                if (!comm.ReadPropertyRequest(BacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, InstanceNumber), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list))
                {
                    //Trace.TraceWarning("Didn't get response from 'Object List'");
                    value_list = null;
                }
            }
            catch (Exception ex)
            {
                //Trace.TraceWarning("Got exception from 'Object List'");
                value_list = null;
            }

            return value_list;

        }


        private IList<BacnetValue> FetchObjectsOneByOne()   //kind of a last-ditch effort.  Async - will need to hope objects come back later, and then process them.
        {
            IList<BacnetValue> value_list;
            var comm = BacnetNetwork.BacnetClient;

            try
            {
                //fetch object list count
                if (!comm.ReadPropertyRequest(BacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, InstanceNumber), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list, 0, 0))
                {
                    //MessageBox.Show(this, "Couldn't fetch objects", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                    //value_list = null;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(this, "Error during read: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
                //value_list = null;
            }

            if (value_list != null && value_list.Count == 1 && value_list[0].Value is uint)
            {
                uint list_count = (uint)value_list[0].Value;
                return AddObjectListOneByOne(list_count);     //still not sure where AsynchRequestId should be kept....
                //return; 
            }
            else
            {
                //MessageBox.Show(this, "Couldn't read 'Object List' count", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }

            return null;    //either way if we got in 
        }





        //This seems to be the one that's getting called...is it because there's only one device object and all the rest are values?  Is there a way to do this synchronously?
        private IList<BacnetValue> AddObjectListOneByOne(uint count) //, int AsynchRequestId)
        {
            var comm = BacnetNetwork.BacnetClient;


            IList<BacnetValue> value_list = new List<BacnetValue>();
                try
                {
                    for (int i = 1; i <= count; i++)
                    {
                        value_list = null;
                        if (!comm.ReadPropertyRequest(BacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, InstanceNumber), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list, 0, (uint)i))
                        {
                            //MessageBox.Show("Couldn't fetch object list index", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            //return null;
                        }

                        //if (AsynchRequestId != this.AsynchRequestId) return; // Selected device is no more the good one

                        //add to tree
                        //foreach (BacnetValue value in value_list)
                        //{
                        //    var boi = (BacnetObjectId)value.Value;
                        //    AddObject(boi, new BACnetObject(this, boi));


                        //    //this.Invoke((MethodInvoker)delegate
                        //    //{
                        //    //    if (AsynchRequestId != this.AsynchRequestId) return;  // another test in the GUI thread
                        //    //    AddObjectEntry(comm, adr, null, (BacnetObjectId)value.Value, m_AddressSpaceTree.Nodes);
                        //    //});
                        //}
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Error during read: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }

                return value_list;

        }







        //TODO: make this synchronous

        //This seems to be the one that's getting called...is it because there's only one device object and all the rest are values?  Is there a way to do this synchronously?
        private void AddObjectListOneByOneAsync(uint count, int AsynchRequestId)
        {
            var comm = BacnetNetwork.BacnetClient;


            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            {
                IList<BacnetValue> value_list;
                try
                {
                    for (int i = 1; i <= count; i++)
                    {
                        value_list = null;
                        if (!comm.ReadPropertyRequest(BacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, InstanceNumber), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list, 0, (uint)i))
                        {
                            //MessageBox.Show("Couldn't fetch object list index", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        //if (AsynchRequestId != this.AsynchRequestId) return; // Selected device is no more the good one

                        //add to tree
                        foreach (BacnetValue value in value_list)
                        {
                            //this.Invoke((MethodInvoker)delegate
                            //{
                            //    if (AsynchRequestId != this.AsynchRequestId) return;  // another test in the GUI thread
                            //    AddObjectEntry(comm, adr, null, (BacnetObjectId)value.Value, m_AddressSpaceTree.Nodes);
                            //});
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Error during read: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            });
        }



    }
}
