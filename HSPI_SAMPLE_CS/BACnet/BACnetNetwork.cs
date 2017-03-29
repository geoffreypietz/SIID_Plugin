using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.IO.BACnet;
using System.IO.BACnet.Storage;
using System.IO;
using Yabe;
using Yabe.Properties;


namespace HSPI_SIID.BACnet
{
    [DataContract]
    public class BACnetNetwork : IBACnetTreeDataObject
    {

        public BACnetNetwork(BACnetGlobalNetwork bgn, String ipAddr) //, BACnetGlobalNetwork bgn)
        {
            this.IpAddress = ipAddr;
            this.BacnetGlobalNetwork = bgn;
            //this.BacnetTreeNode = new TreeNode(this);

            //Discover();   //no, not until user clicks on node to get child devices.
        }

        [DataMember]
        public string IpAddress;   //use this for display in treeView, I guess...

        [DataMember]
        //public Dictionary<BacnetAddress, BACnetDevice> Devices = new Dictionary<BacnetAddress, BACnetDevice>();
        public Dictionary<uint, BACnetDevice> BacnetDevices = new Dictionary<uint, BACnetDevice>();




        //can be multiple devices...just need one client to communicate on one IP endpoint
        public BacnetClient BacnetClient;


        public BacnetDeviceLine BacnetDeviceLine;


        public BACnetGlobalNetwork BacnetGlobalNetwork;







        public BACnetTreeNode GetTreeNode()
        {
            var tn = new BACnetTreeNode();
            tn.title = this.IpAddress;
            tn.lazy = true;
            tn.data["ip_address"] = this.IpAddress;
            return tn;
        }





        public List<BACnetTreeNode> GetChildNodes()     //this one only works from within the application...or on discover/refresh button?
        {
            Discover();     //don't need to get child networks until data requested?  This is a general pattern for these objects.



            var childNodes = new List<BACnetTreeNode>();


            foreach (BACnetDevice bnd in BacnetDevices.Values)
                childNodes.Add(bnd.GetTreeNode());

                //children.Add(bn.GetTreeNodeData());

            return childNodes;
        }









        public void Discover()
        {
            int udpPort = int.Parse(BacnetGlobalNetwork.UdpPort, System.Globalization.NumberStyles.HexNumber);
            //String adr = Properties.Settings.Default.DefaultUdpIp;
            if (IpAddress.Contains(':'))
                BacnetClient = new BacnetClient(new BacnetIpV6UdpProtocolTransport(udpPort, Yabe.Properties.Settings.Default.YabeDeviceId, Yabe.Properties.Settings.Default.Udp_ExclusiveUseOfSocket, Yabe.Properties.Settings.Default.Udp_DontFragment, Yabe.Properties.Settings.Default.Udp_MaxPayload, IpAddress), BacnetGlobalNetwork.TimeoutValue, BacnetGlobalNetwork.RetriesValue);
            else
                BacnetClient = new BacnetClient(new BacnetIpUdpProtocolTransport(udpPort, Yabe.Properties.Settings.Default.Udp_ExclusiveUseOfSocket, Yabe.Properties.Settings.Default.Udp_DontFragment, Yabe.Properties.Settings.Default.Udp_MaxPayload, IpAddress), BacnetGlobalNetwork.TimeoutValue, BacnetGlobalNetwork.RetriesValue);

            BacnetDeviceLine = new BacnetDeviceLine(BacnetClient);

            GetDevices();

            System.Threading.Thread.Sleep(5000);    //wait for OnIam's...
        }





        public void GetDevices()
        {

            try
            {
                //var bne = new BACnetNetworkEvents(this);


                //start BACnet
                BacnetClient.ProposedWindowSize = Yabe.Properties.Settings.Default.Segments_ProposedWindowSize;
                BacnetClient.Retries = (int)Yabe.Properties.Settings.Default.DefaultRetries;
                BacnetClient.Timeout = (int)Yabe.Properties.Settings.Default.DefaultTimeout;
                BacnetClient.MaxSegments = BacnetClient.GetSegmentsCount(Yabe.Properties.Settings.Default.Segments_Max);
                if (Yabe.Properties.Settings.Default.YabeDeviceId >= 0) // If Yabe get a Device id
                {
                    if (BACnetGlobalNetwork.m_storage == null)
                        lock (BACnetGlobalNetwork.m_storage)
                        {
                            // Load descriptor from the embedded xml resource
                            BACnetGlobalNetwork.m_storage = DeviceStorage.Load("Yabe.YabeDeviceDescriptor.xml", (uint)Yabe.Properties.Settings.Default.YabeDeviceId);
                            // A fast way to change the PROP_OBJECT_LIST
                            Property Prop = Array.Find<Property>(BACnetGlobalNetwork.m_storage.Objects[0].Properties, p => p.Id == BacnetPropertyIds.PROP_OBJECT_LIST);
                            Prop.Value[0] = "OBJECT_DEVICE:" + Yabe.Properties.Settings.Default.YabeDeviceId.ToString();
                            // change PROP_FIRMWARE_REVISION
                            Prop = Array.Find<Property>(BACnetGlobalNetwork.m_storage.Objects[0].Properties, p => p.Id == BacnetPropertyIds.PROP_FIRMWARE_REVISION);
                            Prop.Value[0] = this.GetType().Assembly.GetName().Version.ToString();
                            // change PROP_APPLICATION_SOFTWARE_VERSION
                            Prop = Array.Find<Property>(BACnetGlobalNetwork.m_storage.Objects[0].Properties, p => p.Id == BacnetPropertyIds.PROP_APPLICATION_SOFTWARE_VERSION);
                            Prop.Value[0] = this.GetType().Assembly.GetName().Version.ToString();
                        }
                    BacnetClient.OnWhoIs += new BacnetClient.WhoIsHandler(OnWhoIs);    //maybe this can't be static...does anything need to be stored in it?
                    BacnetClient.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(OnReadPropertyRequest);
                    BacnetClient.OnReadPropertyMultipleRequest += new BacnetClient.ReadPropertyMultipleRequestHandler(OnReadPropertyMultipleRequest);
                }
                else
                {
                    BacnetClient.OnWhoIs += new BacnetClient.WhoIsHandler(OnWhoIsIgnore);
                }
                BacnetClient.OnIam += new BacnetClient.IamHandler(OnIam);

                // Not sure if we need these; implement later
                //BacnetClient.OnCOVNotification += new BacnetClient.COVNotificationHandler(bne.OnCOVNotification);
                //BacnetClient.OnEventNotify += new BacnetClient.EventNotificationCallbackHandler(bne.OnEventNotify);

                BacnetClient.Start();

                //start search
                if (BacnetClient.Transport.Type == BacnetAddressTypes.IP || BacnetClient.Transport.Type == BacnetAddressTypes.Ethernet
                    || BacnetClient.Transport.Type == BacnetAddressTypes.IPV6
                    || (BacnetClient.Transport is BacnetMstpProtocolTransport && ((BacnetMstpProtocolTransport)BacnetClient.Transport).SourceAddress != -1)
                    || BacnetClient.Transport.Type == BacnetAddressTypes.PTP)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                    {
                        for (int i = 0; i < BacnetClient.Retries; i++)
                        {
                            BacnetClient.WhoIs();
                            System.Threading.Thread.Sleep(BacnetClient.Timeout);
                        }
                    }, null);
                }

                //special MSTP auto discovery
                if (BacnetClient.Transport is BacnetMstpProtocolTransport)
                {
                    //not sure this is needed...
                    //((BacnetMstpProtocolTransport)BacnetClient.Transport).FrameRecieved += new BacnetMstpProtocolTransport.FrameRecievedHandler(bne.MSTP_FrameRecieved);
                }
            }
            catch (Exception ex)
            {
                //m_devices.Remove(bacnetClient);
                //node.Remove();
                //MessageBox.Show(this, "Couldn't start Bacnet bacnetClientunication: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        public void OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            //Don't really get what this is doing...not sure this is device instance like we need to filter on
            uint myId = (uint)Yabe.Properties.Settings.Default.YabeDeviceId;    //is this reliable?  Can this be changed from multiple places?

            if (low_limit != -1 && myId < low_limit) return;
            else if (high_limit != -1 && myId > high_limit) return;
            sender.Iam(myId, BacnetSegmentations.SEGMENTATION_BOTH, 61440);
        }


        public void OnWhoIsIgnore(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            //ignore whois responses from other devices (or loopbacks)
        }





        //TODO: just put in network.
        public void OnIam(BacnetClient sender, BacnetAddress adr, uint device_id, uint max_apdu, BacnetSegmentations segmentation, ushort vendor_id)
        {

            if (BacnetDevices.ContainsKey(device_id))       //may happen since multiple WhoIs's are sent out...
                return;

            if (BacnetGlobalNetwork.FilterDeviceInstance && (device_id < BacnetGlobalNetwork.DeviceInstanceMin || device_id > BacnetGlobalNetwork.DeviceInstanceMax))
                return;

            var device = new BACnetDevice(this, adr, device_id);
            BacnetDevices.Add(device_id, device);

        }



        public void OnReadPropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyReference property, BacnetMaxSegments max_segments)
        {
            lock (BACnetGlobalNetwork.m_storage)
            {
                try
                {
                    IList<BacnetValue> value;
                    DeviceStorage.ErrorCodes code = BACnetGlobalNetwork.m_storage.ReadProperty(object_id, (BacnetPropertyIds)property.propertyIdentifier, property.propertyArrayIndex, out value);
                    if (code == DeviceStorage.ErrorCodes.Good)
                        sender.ReadPropertyResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), object_id, property, value);
                    else
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }

        public void OnReadPropertyMultipleRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, IList<BacnetReadAccessSpecification> properties, BacnetMaxSegments max_segments)
        {
            lock (BACnetGlobalNetwork.m_storage)
            {
                try
                {
                    IList<BacnetPropertyValue> value;
                    List<BacnetReadAccessResult> values = new List<BacnetReadAccessResult>();
                    foreach (BacnetReadAccessSpecification p in properties)
                    {
                        if (p.propertyReferences.Count == 1 && p.propertyReferences[0].propertyIdentifier == (uint)BacnetPropertyIds.PROP_ALL)
                        {
                            if (!BACnetGlobalNetwork.m_storage.ReadPropertyAll(p.objectIdentifier, out value))
                            {
                                sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_OBJECT, BacnetErrorCodes.ERROR_CODE_UNKNOWN_OBJECT);
                                return;
                            }
                        }
                        else
                            BACnetGlobalNetwork.m_storage.ReadPropertyMultiple(p.objectIdentifier, p.propertyReferences, out value);
                        values.Add(new BacnetReadAccessResult(p.objectIdentifier, value));
                    }

                    sender.ReadPropertyMultipleResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), values);

                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }

    }


}






