using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;

using HSPI_SAMPLE_CS.Modbus;

namespace HSPI_SAMPLE_CS
{
   public class SIID_Page : PageBuilderAndMenu.clsPageBuilder

    {
        public ModbusDevicePage ModPage { get; set; }
       
        public SIID_Page(string pagename) : base(pagename)
        {
          
        }

        public int selectedPlugin { get; set; }


        public void LoadINISettings()
        {

            selectedPlugin= Convert.ToInt32(Util.hs.GetINISetting("CONFIG", "Selected_Plugin", "1", "hspi_SIID.INI"));
            MosbusAjaxReceivers.loadModbusConfig();


            SaveAllINISettings();
        }

        public void SaveAllINISettings()
        {
            //general config
            Util.hs.SaveINISetting("CONFIG", "Selected_Plugin", selectedPlugin.ToString(), "hspi_SIID.INI");

            MosbusAjaxReceivers.saveModbusConfig();
            //modbus specific config



        }

        public void SaveSpecificINISetting(string section, string key, string value)
        {
            Util.hs.SaveINISetting(section, key , value , "hspi_SIID.INI");

        }

       

        public string postbackSSIDConfigPage(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            string ID = parts["id"].Split('_')[0];
            switch (ID)
            {
                case "SelectPlugin":
                    {
                        selectedPlugin = Convert.ToInt32(parts["pluginSelection"]);

                        break;
                    }
            }

            //So I guess AJAX calls come back here?
            //need to register

            SaveAllINISettings();
            return base.postBackProc(page, data, user, userRights);
        }

       

        public  string postBackProc(string page, string data, string user, int userRights)
        {
            //So I guess AJAX calls come back here?
            //need to register
            

            return base.postBackProc(page, data, user, userRights);
        }
        public string AllModbusDevices()
        {//gets list of all associated devices. 
         //Get the collection of these devices which are modbus gateways or devices
         //build Gateway / Devices table with the appropriate links and the appropriate Add Device buttons
         //returns the built html string
            StringBuilder sb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("AddModbusDevice");

          List<int>  ModbusGates=ModbusDevicePage.getAllGateways().ToList();
            List<Scheduler.Classes.DeviceClass> ModbusDevs = new List<Scheduler.Classes.DeviceClass>();

            foreach (int GID in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Dev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(GID));
                 var EDO = Dev.get_PlugExtraData_Get(Util.hs);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                StringBuilder updatedList = new StringBuilder();
                foreach (var subId in parts["LinkedDevices"].Split(','))
                {
                    try
                    {
                        Scheduler.Classes.DeviceClass MDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(subId));
                        if (MDevice != null)
                        {
                            ModbusDevs.Add(MDevice);
                            updatedList.Append(subId + ",");
                        }
                    }
                    catch
                    {

                    }


                }
                parts["LinkedDevices"] = updatedList.ToString();
                EDO.RemoveNamed("SSIDKey");
                EDO.AddNamed("SSIDKey", parts.ToString());
                Dev.set_PlugExtraData_Set(Util.hs, EDO);
            }

            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable(800);
            sb.Append("<br>");
            foreach (int GateRef in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GateRef);
                ModbusConfHtml.addDevHeader("Gateway");
                Gateway.get_Image(Util.hs);
                Gateway.get_Name(Util.hs);
                ModbusConfHtml.addDevMain(ModbusBuilder.MakeImage(16,16, Gateway.get_Image(Util.hs)).print()+
                    ModbusBuilder.MakeLink("/deviceutility?ref="+GateRef
                    +"&edit=1", Gateway.get_Name(Util.hs)).print(), ModbusBuilder.Qbutton("G_"+GateRef,"Add Device").print());
                sb.Append(ModbusConfHtml.print());
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
                ModbusConfHtml.addSubHeader("Enabled","Device Name","Address","Type","Format");
               
                
                foreach (Scheduler.Classes.DeviceClass MDevice in ModbusDevs)
                {
                 
                    if (MDevice != null)
                    {
                        var EDO = MDevice.get_PlugExtraData_Get(Util.hs);
                        var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                        if (Convert.ToInt32(parts["GateID"]) == GateRef)
                        {
                            ModbusConfHtml.addSubMain(ModbusBuilder.MakeImage(16, 16, MDevice.get_Image(Util.hs)).print(),
                               ModbusBuilder.MakeLink("/deviceutility?ref=" + MDevice.get_Ref(Util.hs) + "&edit=1", MDevice.get_Name(Util.hs)).print(),
                               parts["SlaveId"],
                               parts["RegisterType"],
                               parts["ReturnType"]);

                        }
                    }


                }
                sb.Append(ModbusConfHtml.print());
                sb.Append("<br>");
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
            }

            
            ModbusDevicePage.UpdateGateList(ModbusGates.ToArray());
            return sb.ToString();
        }


        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder stb = new StringBuilder();
            SIID_Page page = this;
            htmlBuilder ModbusBuilder = new htmlBuilder("ModBus");

            try
            {
                page.reset();

                page.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME + " main plugin page", "", "", false, true));

                //
                stb.Append("<hr>Select plugin API<br><br>");
               clsJQuery.jqRadioButton rb = new clsJQuery.jqRadioButton("pluginSelection", "SIIDConfPage", false);
               
                //rb.buttonset = False
                 rb.id = "SelectPlugin";
                rb.values.Add("Select an API", "1");
                rb.values.Add("ModBus", "2");
                rb.values.Add("BACnet", "3");
                rb.@checked = selectedPlugin.ToString();
                stb.Append(rb.Build());
                //

                //Modbus
                if (selectedPlugin == 2)
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style=''"));
                }
                else
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style='display:none;'"));
                }
               

                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("tab1id", this.PageName);
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab.tabTitle = "Devices";
                tab.tabDIVID = "modBusDevTab";

                //have the ModBus Add device button
                //Also list all associated modbus devices
                htmlBuilder AddModbusDevBuilder = new htmlBuilder("AddModbusGate");
                StringBuilder ModbusDevPage = new StringBuilder();
                ModbusDevPage.Append("<br><br><div style='display:block;'>");
                ModbusDevPage.Append(AddModbusDevBuilder.Gobutton("addModGateway", "Add Modbus IP Gateway").print()); 
                ModbusDevPage.Append("</div><br>");
                ModbusDevPage.Append(AllModbusDevices());
                tab.tabContent = ModbusDevPage.ToString();

                jqtabs.postOnTabClick = true;
                jqtabs.tabs.Add(tab);

                tab = new clsJQuery.Tab();
                tab.tabTitle = "Configuration";
                tab.tabDIVID = "modBusConfTab";


                new HSPI_SAMPLE_CS.Modbus.ModbusClasses.ModbusGeneralConfigSettings();


                    htmlTable ModbusConfHtml =  ModbusBuilder.htmlTable();
                ModbusConfHtml.add(" Configuration:");
                ModbusConfHtml.add(" Default Poll Interval in miliseconds<br>(can be overridden per gateway):", ModbusBuilder.numberInput("polltime", MosbusAjaxReceivers.modbusDefaultPoll).print());
                selectorInput loglevel = ModbusBuilder.selectorInput(new string[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" },"logL","Log Level", MosbusAjaxReceivers.modbusLogLevel);
                ModbusConfHtml.add(" Log Level:", loglevel.print());
                checkBoxInput logTF = ModbusBuilder.checkBoxInput("modlog", MosbusAjaxReceivers.modbusLogToFile);
                ModbusConfHtml.add(" Log To File:", logTF.print());

                 string ConfigTable = "<div id=confTab style='display:block;'>" + ModbusConfHtml.print() + "</div>";


               // string TestStuff = new numberInput().print() + loglevel.print() + logTF.print();
               // tab.tabContent = TestStuff;

                  tab.tabContent = ConfigTable;

                jqtabs.tabs.Add(tab);

                stb.Append(jqtabs.Build());


                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());

            }


            catch (Exception ex)
            {
                stb.Append("Test page error: " + ex.Message);
            }
            stb.Append("<br>");

            stb.Append("<script>");

            String Javascript = @"$('input:radio[name=""pluginSelection""]').change(
    function(){
                if (this.checked && this.value == 2) {
                        modbus.style.display = '';
                    }
                    else{
                        modbus.style.display = 'none';
                    }
                    });";


            stb.Append(Javascript);

            stb.Append("</script>");

            page.AddBody(stb.ToString());

            return page.BuildPage();

        }
    }
}
