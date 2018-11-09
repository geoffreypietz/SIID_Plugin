using System;
using System.Collections.Generic;
using HomeSeerAPI;

using HSCF.Communication.Scs.Communication.EndPoints.Tcp;
using HSCF.Communication.ScsServices.Client;
using HSPI_Utilities_Plugin.Modbus;
using HSPI_Utilities_Plugin.BACnet;
using HSPI_Utilities_Plugin.ScratchPad;
using HSPI_Utilities_Plugin.General;
using System.IO;

namespace HSPI_Utilities_Plugin
{


    public class InstanceHolder
    {

        private List<SiidDevice> GetDevices(InstanceHolder Instance) {
            List<SiidDevice> AssociatedDevices = new List<SiidDevice>();

            Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Instance.host.GetDeviceEnumerator();
            var Dev = DevNum.GetNext();
            while (Dev != null)
            {
                if ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name))
                {
                    AssociatedDevices.Add(new SiidDevice(Instance,Dev));
                
                }

                Dev = DevNum.GetNext();

            }

                return AssociatedDevices;

        }

        private static void client_Disconnected(object sender, System.EventArgs e)
        {
            Console.WriteLine("Disconnected from server - client");
        }

        public InstanceHolder(HSPI HSPI, HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> Client, HomeSeerAPI.IAppCallbackAPI ClientCallback, HomeSeerAPI.IHSApplication Host, string Name)
        {
            name = Name;
            hspi = HSPI;
            hspi.OurInstanceFriendlyName = name;
            ajaxName = name;
            if (name != "")
            {
                ajaxName = ":" + name;
            }
            client = Client;
            callback = ClientCallback;
            host = Host;
            Devices = GetDevices(this);


            modPage = new ModbusDevicePage("ModbusDevicePage", this);
            scrPage = new ScratchpadDevicePage("ScratchpadPage", this);
            modAjax = new MosbusAjaxReceivers(this);


            bacnetDevices = new BACnetDevices(BACnetDevices.BaseUrl, this);
            bacnetDataService = new BACnetDataService(BACnetDataService.BaseUrl, this);
            bacnetHomeSeerDevices = new BACnetHomeSeerDevices(BACnetHomeSeerDevices.BaseUrl, this);


            siidPage = new SIID_Page(Util.IFACE_NAME, this);

            //bacnetDevices = new BACnetDevices(BACnetDataService.BaseUrl + this.ajaxName.Replace(":", "_"), this);
            //bacnetDataService = new BACnetDataService(BACnetDataService.BaseUrl + this.ajaxName.Replace(":", "_"), this);
            //bacnetHomeSeerDevices = new BACnetHomeSeerDevices(BACnetHomeSeerDevices.BaseUrl + this.ajaxName.Replace(":", "_"), this);




            //CPU use is high, so try to minimize iterating through devices
            //Also minimize calls to and from the device's plugin extra data store. Keep parallel copy, maybe only update when change


        }
        public List<SiidDevice> Devices;

        public string name;
        public string ajaxName;
        public HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> withEventsField_client;
        public HSPI hspi;
        public HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> client
        {
            get { return withEventsField_client; }
            set
            {
                if (withEventsField_client != null)
                {
                    withEventsField_client.Disconnected -= client_Disconnected;
                }
                withEventsField_client = value;
                if (withEventsField_client != null)
                {
                    withEventsField_client.Disconnected += client_Disconnected;
                }
            }
        }
        public HomeSeerAPI.IAppCallbackAPI callback;
        public HomeSeerAPI.IHSApplication host;
        public Int32 modbusDefaultPoll { get; set; }
        public int modbusLogLevel { get; set; }
        public bool modbusLogToFile { get; set; }
        public ModbusDevicePage modPage;
        public SIID_Page siidPage;
        public MosbusAjaxReceivers modAjax;

        public ScratchpadDevicePage scrPage;

        public BACnetDevices bacnetDevices;
        public BACnetDataService bacnetDataService;
        public BACnetHomeSeerDevices bacnetHomeSeerDevices;


    }
 

    class Program
    {
        public static Dictionary<string, InstanceHolder> AllInstances = new Dictionary<string, InstanceHolder>();

        public static void RemoveInstance(string InstanceName)
        {
            if (!AllInstances.ContainsKey(InstanceName))
            {
                return;
            }
            AllInstances[InstanceName].hspi.ShutdownIO();
            AllInstances[InstanceName].client.Disconnect();
            //AllInstances[InstanceName].callback.Disconnect();
            AllInstances.Remove(InstanceName);



        }

        public static void AddInstance(string InstanceName)
        {
            if (AllInstances.ContainsKey(InstanceName))
            {
                return;
            }
            else
            {


                HSPI plugIn = new HSPI();

                string sIp = "127.0.0.1";
                // plugIn.OurInstanceFriendlyName = InstanceName;
                HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> client = ScsServiceClientBuilder.CreateClient<IHSApplication>(new ScsTcpEndPoint(sIp, 10400), plugIn); 
                HSCF.Communication.ScsServices.Client.IScsServiceClient<IAppCallbackAPI> clientCallback = ScsServiceClientBuilder.CreateClient<IAppCallbackAPI>(new ScsTcpEndPoint(sIp, 10400), plugIn);
                HomeSeerAPI.IHSApplication host = null;
                IAppCallbackAPI callback;


                int Attempts = 1;
                TryAgain:

                try
                {
                    client.Connect();
                    clientCallback.Connect();

                    double APIVersion = 0;

                    try
                    {
                        host = client.ServiceProxy;
                        APIVersion = host.APIVersion;
                        // will cause an error if not really connected
                        Console.WriteLine("Host API Version: " + APIVersion.ToString());
                    }
                    catch (Exception ex)
                    {
                 
                        Console.WriteLine("Error getting API version from host object: " + ex.Message + "->" + ex.StackTrace);
                        //Return
                    }

                    try
                    {
                        callback = clientCallback.ServiceProxy;
                        APIVersion = callback.APIVersion;
                        // will cause an error if not really connected
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error getting API version from callback object: " + ex.Message + "->" + ex.StackTrace);
                        return;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot connect attempt " + Attempts.ToString() + ": " + ex.Message);
                    if (ex.Message.ToLower().Contains("timeout occurred."))
                    {
                        Attempts += 1;
                        if (Attempts < 6)
                            goto TryAgain;
                    }

                    if (client != null)
                    {
                        client.Dispose();
                        client = null;
                    }
                    if (clientCallback != null)
                    {
                        clientCallback.Dispose();
                        clientCallback = null;
                    }
                    wait(4);
                    return;
                }

                try
                {
                    AllInstances[InstanceName] = new InstanceHolder(plugIn, client, callback, host, InstanceName);
                    host.Connect(Util.IFACE_NAME, InstanceName);
                    Console.WriteLine("Connected, waiting to be initialized...");

                    //  plugIn.OurInstanceFriendlyName = InstanceName;

                    // create the user object that is the real plugIn, accessed from the plugInAPI wrapper
                    //  AllInstances[InstanceFriendlyName].callback = callback;
                    //  Util.hs = host;
                    //   plugIn.OurInstanceFriendlyName = Util.Instance;
                    // connect to HS so it can register a callback to us
                    //   host.Connect(Util.IFACE_NAME, Util.Instance);
                    int Count = 0;
                    do
                    {
                     System.Threading.Thread.Sleep(1000);
                        /*     Count = (Count + 1) % (60*5);//save every 5 minutes, not every minute   
                          if (Count == 0)
                          {
                              foreach (var Instance in AllInstances)
                              {
                                  Instance.Value.hspi.Log("Saving devices for Instance "+Instance.Key, 0);
                                  Instance.Value.host.SaveEventsDevices();  //THis function doesn't save plugin created devices, so not necessary to call
                              }
                          }*/

                      } while (client.CommunicationState == HSCF.Communication.Scs.Communication.CommunicationStates.Connected & !HSPI.bShutDown);
                      Console.WriteLine("Connection lost, exiting");

                      // disconnect from server for good here
                      // 
                      // clientCallback.Disconnect();
                      //wait(2);
                      if (AllInstances.Count == 0)
                      {
                          client.Disconnect();
                          wait(2);
                          System.Environment.Exit(0);
                      }
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine("Cannot connect(2): " + ex.Message);
                      wait(2);
                      System.Environment.Exit(0);
                      return;
                  }
              }

          }






          public static void Main(string[] args) //To start a new instance, run command ...plugIn.exe instance=InstanceName
          {
              //BACnetGlobalNetwork.Discover();

              //return;
  /*
              try
              {
                  //check the /bin/HSPI_SIID directory for a file called flag.flag
                  //if that file does not exist then create it, and check to see if the SIID, plugins is installed correctly
                  //check to make sure the .config file is correct, fix it if not
                  string basePath = Path.Combine("bin", "HSPI_SIID"); 
               if(!File.Exists(Path.Combine(basePath,"flag.flag"))){

                      File.Create((Path.Combine(basePath, "flag.flag")));
                      Directory.Delete("js");
                      String[] FileList = new String[] { "ADODB.dll", "CalendarView.dll", "FireBase.dll", "HSPI_SIID.application", "HSPI_SIID.exe.manifest", "HSPI_SIID.pdb", "Newtonsoft.Json.dll", "NModbus4.dll", "NModbus4.xml", "PacketDotNet.dll", "ReadSinglePropDescr.xml", "ReadSinglePropDescrDefault.xml", "RestSharp.dll", "SharpPcap.dll", "System.Reactive.Core.dll", "System.Reactive.Interfaces.dll", "System.Reactive.Linq.dll", "System.Reactive.PlatformServices.dll", "System.Reactive.Windows.Threading.dll", "Yabe.exe", "Yabe.pdb" };
                      foreach (var file in FileList)
                      {
                          File.Delete(file);
                      }

                      //Need to modify HSPI_SIID.exe.conf and other configs to include the new paths. THis is bad design btw
                  }


              }
              catch
              {
              }*/

                        string sIp = "127.0.0.1";
            string Instance = "";
            string sCmd = null;
            foreach (string sCmd_loopVariable in args)
            {
                sCmd = sCmd_loopVariable;
                string[] ch = new string[1];
                ch[0] = "=";
                string[] parts = sCmd.Split(ch, StringSplitOptions.None);
                switch (parts[0].ToLower())
                {
                    case "server":
                        sIp = parts[1];
                        break;
                    case "instance":
                        try
                        {
                            Instance = parts[1];
                        }
                        catch (Exception)
                        {
                           Instance = "";
                        }
                        break;
                }
            }

            Console.WriteLine("Plugin: " + Util.IFACE_NAME + " Instance: " + Instance + " starting...");
            Console.WriteLine("Connecting to server at " + sIp + "...");

            AddInstance(Instance);
        }

      





        private static void wait(int secs)
        {
            System.Threading.Thread.Sleep(secs * 1000);
        }
    }


}

