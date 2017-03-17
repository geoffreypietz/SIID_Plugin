﻿using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Scheduler;
using HomeSeerAPI;

using HSCF.Communication.Scs.Communication.EndPoints.Tcp;
using HSCF.Communication.ScsServices.Client;
using HSCF.Communication.ScsServices.Service;

namespace HSPI_SIID_ModBusDemo
{
    class Program
    {
        public static Dictionary<string,InstanceHolder> AllInstances = new Dictionary<string,InstanceHolder>();

        public class InstanceHolder
        {
            InstanceHolder(HSPI HSPI, HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> Client, HSCF.Communication.ScsServices.Client.IScsServiceClient<IAppCallbackAPI> ClientCallback, HomeSeerAPI.IHSApplication Host ) {
                hspi = HSPI;
                client = Client;
                callback = ClientCallback;
                host = Host;
            }

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
            public HSCF.Communication.ScsServices.Client.IScsServiceClient<IAppCallbackAPI> callback;
            public HomeSeerAPI.IHSApplication host;


        }




        public static void Main(string[] args) //To start a new instance, run command ...plugin.exe instance=InstanceName
        {
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

        public static void RemoveInstance(string InstanceName)
        {
            if (!AllInstances.ContainsKey(InstanceName))
            {
                return;
            }
            AllInstances[InstanceName].hspi.ShutdownIO();
            AllInstances[InstanceName].client.Disconnect();
            AllInstances[InstanceName].callback.Disconnect();
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
                PlugIn.Instance = InstanceName;
                HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> client = ScsServiceClientBuilder.CreateClient<IHSApplication>(new ScsTcpEndPoint(sIp, 10400), plugIn); //Maybe need to distinguish instance here for callbacks to work correctly
                HSCF.Communication.ScsServices.Client.IScsServiceClient<IAppCallbackAPI> clientCallback = ScsServiceClientBuilder.CreateClient<IAppCallbackAPI>(new ScsTcpEndPoint(sIp, 10400), plugIn);
    



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
                    host.Connect(Util.IFACE_NAME, InstanceName);
                    Console.WriteLine("Connected, waiting to be initialized...");
                    AllInstances[InstanceName] = new InstanceHolder(plugin, client, clientCallback, host);
                    plugin.OurInstanceFriendlyName = InstanceName;

                    // create the user object that is the real plugin, accessed from the pluginAPI wrapper
                    //  AllInstances[InstanceFriendlyName].callback = callback;
                    //  Util.hs = host;
                    //   plugIn.OurInstanceFriendlyName = Util.Instance;
                    // connect to HS so it can register a callback to us
                    //   host.Connect(Util.IFACE_NAME, Util.Instance);

                    do
                    {
                        System.Threading.Thread.Sleep(10);
                    } while (client.CommunicationState == HSCF.Communication.Scs.Communication.CommunicationStates.Connected & !HSPI.bShutDown);
                    Console.WriteLine("Connection lost, exiting");
                    // disconnect from server for good here
                    client.Disconnect();
                    clientCallback.Disconnect();
                    wait(2);
                    System.Environment.Exit(0);
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




        // private static HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> withEventsField_client;
        // private static IAppCallbackAPI callback;



        //  private static HomeSeerAPI.IHSApplication host;
        // real plugin functions, user supplied
        //  public static HSPI plugin = new HSPI();



        /*  public static void Main(string[] args) //To start a new instance, run command ...plugin.exe instance=InstanceName
        {
            string sIp = "127.0.0.1";
           
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
                            Util.Instance = parts[1];
                        }
                        catch (Exception)
                        {
                            Util.Instance = "";
                        }
                        break;
                }
            }

            Console.WriteLine("Plugin: " + Util.IFACE_NAME + " Instance: " + Util.Instance + " starting...");
            Console.WriteLine("Connecting to server at " + sIp + "...");

            AddInstance(Util.Instance);
          
            client = ScsServiceClientBuilder.CreateClient<IHSApplication>(new ScsTcpEndPoint(sIp, 10400), plugin); //Maybe need to distinguish instance here for callbacks to work correctly
            clientCallback = ScsServiceClientBuilder.CreateClient<IAppCallbackAPI>(new ScsTcpEndPoint(sIp, 10400), plugin);
  
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
                // create the user object that is the real plugin, accessed from the pluginAPI wrapper
                AllInstances[InstanceFriendlyName].callback = callback;
                Util.hs = host;
                plugin.OurInstanceFriendlyName = Util.Instance;
                // connect to HS so it can register a callback to us
                host.Connect(Util.IFACE_NAME, Util.Instance);
                Console.WriteLine("Connected, waiting to be initialized...");
                do
                {
                    System.Threading.Thread.Sleep(10);
                } while (client.CommunicationState == HSCF.Communication.Scs.Communication.CommunicationStates.Connected & !HSPI.bShutDown);
                Console.WriteLine("Connection lost, exiting");
                // disconnect from server for good here
                client.Disconnect();
                clientCallback.Disconnect();
                wait(2);
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect(2): " + ex.Message);
                wait(2);
                System.Environment.Exit(0);
                return;
            }
           

    } */

        private static void client_Disconnected(object sender, System.EventArgs e)
        {
            Console.WriteLine("Disconnected from server - client");
        }


        private static void wait(int secs)
        {
            System.Threading.Thread.Sleep(secs * 1000);
        }
    }


}

