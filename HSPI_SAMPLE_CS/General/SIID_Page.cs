using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;


namespace HSPI_SAMPLE_CS
{
   public class SIID_Page : PageBuilderAndMenu.clsPageBuilder

    {

        public SIID_Page(string pagename) : base(pagename)
        {
        }




        public string postBackProcModBus(string page, string data, string user, int userRights)
        {
            //So in the main plugin stuff
            //need to add
            //Util.hs.RegisterPage("ModBus", Util.IFACE_NAME, Util.Instance);
            //where ModBus is the name of our ajax callback 

            //then in PostBackProc in the main plugin stuff, the pagename that comes back will be our ajax call
            //n



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
                clsJQuery.jqRadioButton rb = new clsJQuery.jqRadioButton("pluginSelection", this.PageName, false);
                //rb.buttonset = False
                rb.values.Add("Select an API", "1");
                rb.values.Add("ModBus", "2");
                rb.values.Add("BACnet", "3");
                rb.@checked = "Item 1";
                stb.Append(rb.Build());
                //

                //Modbus
                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style='display:none;'"));

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


             
           

                 htmlTable ModbusConfHtml =  ModbusBuilder.htmlTable();
                ModbusConfHtml.add(" Configuration:");
                ModbusConfHtml.add(" Default Poll Interval in miliseconds<br>(can be overridden per gateway):", ModbusBuilder.numberInput("polltime",300000).print());
                selectorInput loglevel = ModbusBuilder.selectorInput(new string[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" },"logL","Log Level",2);
                ModbusConfHtml.add(" Log Level:", loglevel.print());
                checkBoxInput logTF = ModbusBuilder.checkBoxInput("modlog", false);
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
