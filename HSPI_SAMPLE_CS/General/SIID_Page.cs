﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;

namespace HSPI_SAMPLE_CS
{
   public class SIID_Page : PageBuilderAndMenu.clsPageBuilder

    {

        public SIID_Page(string pagename) : base(pagename)
        {
        }

        public int selectedPlugin { get; set; }
        public Int32 modbusDefaultPoll { get; set; }
        public int modbusLogLevel { get; set; }
        public bool modbusLogToFile { get; set; }

        public void LoadINISettings()
        {

            selectedPlugin= Convert.ToInt32(Util.hs.GetINISetting("CONFIG", "Selected_Plugin", "1", "hspi_SIID.INI"));
            modbusDefaultPoll = Convert.ToInt32(Util.hs.GetINISetting("MODBUS_CONFIG", "DefaultPoll", "300000", "hspi_SIID.INI"));
            modbusLogLevel = Convert.ToInt32(Util.hs.GetINISetting("MODBUS_CONFIG", "LogLevel", "2", "hspi_SIID.INI"));
            modbusLogToFile = bool.Parse(Util.hs.GetINISetting("MODBUS_CONFIG", "LogToFile", "false", "hspi_SIID.INI"));

            SaveAllINISettings();
        }

        public void SaveAllINISettings()
        {
            //general config
            Util.hs.SaveINISetting("CONFIG", "Selected_Plugin", selectedPlugin.ToString(), "hspi_SIID.INI");

            //modbus specific config
            Util.hs.SaveINISetting("MODBUS_CONFIG", "DefaultPoll", modbusDefaultPoll.ToString(), "hspi_SIID.INI");
            Util.hs.SaveINISetting("MODBUS_CONFIG", "LogLevel", modbusLogLevel.ToString(), "hspi_SIID.INI");
            Util.hs.SaveINISetting("MODBUS_CONFIG", "LogToFile", modbusLogToFile.ToString(), "hspi_SIID.INI");


        }

        public void SaveSpecificINISetting(string section, string key, string value)
        {
            Util.hs.SaveINISetting(section, key , value , "hspi_SIID.INI");

        }

        public string postBackProcModBus(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
               string ID = parts["id"];
            string value = parts["value"];

            switch (ID)
            {
                case "polltime":
                    {
                        modbusDefaultPoll = Convert.ToInt32(value);
                    
                        break;
                    }
                case "logL":
                    {
                        modbusLogLevel = Convert.ToInt32(value);
                        
                        break;
                    }
                case "modlog":
                    {
                        modbusLogToFile = bool.Parse(value);
                       
                        break;
                    }


            }
            //So in the main plugin stuff
            //need to add
            //Util.hs.RegisterPage("ModBus", Util.IFACE_NAME, Util.Instance);
            //where ModBus is the name of our ajax callback 

            //then in PostBackProc in the main plugin stuff, the pagename that comes back will be our ajax call
            //n


            SaveAllINISettings();
            return base.postBackProc(page, data, user, userRights);
        }

        public string psotBackSIIDConfPage(string page, string data, string user, int userRights)
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
                tab.tabContent = "SOME STUFF OUTSIDE OF A DIV<br><div id='test'>SOME STUFF INSIDE OF A DIV</div><br>";
                jqtabs.postOnTabClick = true;
                jqtabs.tabs.Add(tab);

                tab = new clsJQuery.Tab();
                tab.tabTitle = "Configuration";
                tab.tabDIVID = "modBusConfTab";


                new HSPI_SAMPLE_CS.Modbus.ModbusClasses.ModbusGeneralConfigSettings();


                    htmlTable ModbusConfHtml =  ModbusBuilder.htmlTable();
                ModbusConfHtml.add(" Configuration:");
                ModbusConfHtml.add(" Default Poll Interval in miliseconds<br>(can be overridden per gateway):", ModbusBuilder.numberInput("polltime", modbusDefaultPoll).print());
                selectorInput loglevel = ModbusBuilder.selectorInput(new string[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" },"logL","Log Level", modbusLogLevel);
                ModbusConfHtml.add(" Log Level:", loglevel.print());
                checkBoxInput logTF = ModbusBuilder.checkBoxInput("modlog", modbusLogToFile);
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
