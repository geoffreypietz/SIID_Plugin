using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;

namespace HSPI_SAMPLE_CS.Modbus
{
    public class ModbusDevicePage : PageBuilderAndMenu.clsPageBuilder

    {
        public ModbusDevicePage(string pagename) : base(pagename)
        {
        }

        htmlBuilder ModbusBuilder = new htmlBuilder("ModBusGatewayPage");

        public string MakeAdvancedTab()
        {
            StringBuilder sb = new StringBuilder();

            htmlTable ModbusTable = ModbusBuilder.htmlTable();
            ModbusTable.addLong("Reference ID", "41");//Actually, get the device reference ID, put it here
            ModbusTable.addLong("Status", "2 = Dim");
            ModbusTable.addLong("Value", "2 = \"Disabled\"");
            ModbusTable.addLong("String", "");
            ModbusTable.addLong("Supports Status", "True");
            ModbusTable.addLong("Dimmable", "False");
            ModbusTable.addLong("Interface", "Modbus");
            ModbusTable.addLong("Extra Data Storage", "PLACEHOLDER");
            ModbusTable.addLong("Device Type Internal", "PLACEHOLDER");
            ModbusTable.addLong("Device Type (String)", ModbusBuilder.stringInput("devType", "Modbus").print());
            ModbusTable.addLong("Misc Settings", "PLACEHOLDER");
            ModbusTable.addLong("Device Image File", "PLACEHOLDER");
            ModbusTable.addLong("Thumbnail Image File", "PLACEHOLDER");
            ModbusTable.addLong("Relationship Status", "PLACEHOLDER");
            ModbusTable.addLong("Associated Devices", "PLACEHOLDER");
            sb.Append(ModbusTable.print());
            sb.Append(ModbusBuilder.button("selThumb", "Select Thumbnail Image").print()  + ModbusBuilder.button("doneA", "Done").print());
            return sb.ToString();



        }
        public string MakeConfigTab()
        {
            StringBuilder sb = new StringBuilder();

            htmlTable ModbusTable = ModbusBuilder.htmlTable();
            ModbusTable.addDev("Device Name:", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print(), true);
            // ModbusTable.addDev("Floor:", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print());//Must be doing it wrong, do placeholder until figure out real way
            //ModbusTable.addDev("Room:", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print());
            //ModbusTable.addDev("Code:", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print());
            //ModbusTable.addDev("Address:", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print());
            //ModbusTable.addDev("Status Only Device:", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print());
            //ModbusTable.addDev("Is Dimmable::", ModbusBuilder.stringInput("devName", "Modbus IP Gateway").print());
            ModbusTable.addDev("Hide Device from views:", ModbusBuilder.checkBoxInput("hideDev",false).print());
            ModbusTable.addDev("Do not log commands from this device:", ModbusBuilder.checkBoxInput("noLog", true).print());
            ModbusTable.addDev("Voice command::", ModbusBuilder.checkBoxInput("voice", true).print());
            ModbusTable.addDev("Confirm voice command:", ModbusBuilder.checkBoxInput("confVoice", false).print());
            ModbusTable.addDev("Include in power fail recovery:", ModbusBuilder.checkBoxInput("incldPwFail", false).print());
            ModbusTable.addDev("Use pop-up dialog for control:", ModbusBuilder.checkBoxInput("usePop", false).print());
            ModbusTable.addDev("Do not update device last change time if device value does not change", ModbusBuilder.checkBoxInput("doNotUpdate", false).print());
            ModbusTable.addDev("User Access:", "PLACEHOLDER TEXT");
            ModbusTable.addDev("Notes:", "PLACEHOLDER TEXT");
            sb.Append(ModbusTable.print());
            sb.Append(ModbusBuilder.button("del", "Delete").print() + ModbusBuilder.button("Img", "Select Device Image").print() + ModbusBuilder.button("done", "Done").print());
            return sb.ToString();

        }

        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            // htmlBuilder ModbusBuilder = new htmlBuilder("ModBus");

            try
            {
                page.reset();

                page.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME + " main plugin page", "", "", false, true));

                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("tab1id", this.PageName);

                

                //Reproduced configuration tab
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab.tabTitle = "Configuration";
                tab.tabDIVID = "conftab";
                tab.tabContent = MakeConfigTab();

                jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);

                //Reproduced Advanced tab
                 tab = new clsJQuery.Tab();
                tab.tabTitle = "Advanced";
                tab.tabDIVID = "advancedtab";
                tab.tabContent = MakeAdvancedTab() ;

                jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);

                //Reproduced status graphics tab tab
                tab = new clsJQuery.Tab();
                tab.tabTitle = "Status Graphics";
                tab.tabDIVID = "statusgraphicstab";
                tab.tabContent = "Status Graphics TAB CONTENT";

                jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);


                //Reproduced MODBUS  tab tab
                tab = new clsJQuery.Tab();
                tab.tabTitle = "Modbus";
                tab.tabDIVID = "modbustab";
                tab.tabContent = "Modbus TAB CONTENT";

                jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);


                stb.Append(jqtabs.Build());

            }

            catch (Exception ex)
            {
                stb.Append("Test page error: " + ex.Message);
            }
            stb.Append("<br>");

            

            page.AddBody(stb.ToString());

            return page.BuildPage();

        }
    }
}
