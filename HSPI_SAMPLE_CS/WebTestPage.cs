using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using Scheduler;

namespace HSPI_SIID_ModBusDemo
{
    public class WebTestPage : PageBuilderAndMenu.clsPageBuilder
    {

	public WebTestPage(string pagename) : base(pagename)
	{
	}

	public override string postBackProc(string page, string data, string user, int userRights)
	{
		Console.WriteLine(Util.IFACE_NAME + " test page post: " + data);
		try {
			System.Collections.Specialized.NameValueCollection parts = null;
			parts = HttpUtility.ParseQueryString(data);
			if (parts["uploadfile"] == "true") {
				// handle uploading of file
				// the orig file is ID_OriginalFile
				// the temp file is ID_TempFile
				// status is ID_Status
				// return the proper JSON so the uploader completes
				// success return=Return "{""success"":""true""}"
				// if error return=Return "{""error"":""No directory specified""}"
				return "{\"success\":\"true\"}";
			}

			string id = parts["id"];
			if (id == "btimerstop") {
				this.pageCommands.Add("stoptimer", "");
			}
			if (id == "btimerstart") {
				this.pageCommands.Add("starttimer", "");
			}
			if (id == "sradd") {
				this.propertySet.Add("my_region", "appenddiv=<br>" + System.DateTime.Now.ToString() + ":added this to the scrolling region value=0 added this to the scrolling region added this to the scrolling region added this to the scrolling region added this to the scrolling region added this to the scrolling region added this to the scrolling region");
			}
			if (id == "spbut") {
				this.pageCommands.Add("stopspinner", "spin");
			}
			if (id == "butOpenSlide") {
				this.pageCommands.Add("slidingtabopen", "myslide1");
			}
			if (id == "butCloseSlide") {
				this.pageCommands.Add("slidingtabclose", "myslide1");
			}
			if (id == "myslide1_ID") {
				//Me.pageCommands.Add("refresh", True)
			}

            

			if (id == "b111") {
				this.divToUpdate.Add("myslide1_ID_content", "new stuff for the tab");
			}

			return base.postBackProc(page, data, user, userRights);
		} catch (Exception ex) {
			Console.WriteLine("Error Test page postback: " + ex.Message);
		}
		return "";
	}

	public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
	{
		StringBuilder stb = new StringBuilder();
		WebTestPage page = this;

		try {
			page.reset();

			// handle queries with special data
			System.Collections.Specialized.NameValueCollection parts = null;
			if ((!string.IsNullOrEmpty(queryString))) {
				parts = HttpUtility.ParseQueryString(queryString);
			}
			if (parts != null) {
				if (parts["myslide1"] == "myslide_name_open") {
					// handle a get for tab content
					string name = parts["name"];
					return ("<table><tr><td>cell1</td><td>cell2</td></tr><tr><td>cell row 2</td><td>cell 2 row 2</td></tr></table>");
					//Return ("<div><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br><b>content data for tab</b><br></div>")
				}
				if (parts["myslide1"] == "myslide_name_close") {
					return "";
				}
			}


			page.AddHeader(stb.ToString());
			//page.RefreshIntervalMilliSeconds = 5000
			// handler for our status div
			//stb.Append(page.AddAjaxHandler("/devicestatus?ref=3576", "stat"))
			stb.Append(this.AddAjaxHandlerPost("action=updatetime", this.PageName));



			// page body starts here

            this.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME + " Controls Test", "", "", false, true));

			//Dim dv As DeviceClass = GetDeviceByRef(3576)
			//Dim CS As CAPIStatus
			//CS = dv.GetStatus

			stb.Append("This page displays all of the jquery controls availible<br><br>");

			stb.Append("<hr>Timer Test<br>");
			clsJQuery.jqButton btimerstop = new clsJQuery.jqButton("btimerstop", "Stop Timer", this.PageName, false);
			stb.Append(btimerstop.Build());
			clsJQuery.jqButton btimerstart = new clsJQuery.jqButton("btimerstart", "Start Timer", this.PageName, false);
            stb.Append(btimerstart.Build());

			stb.Append("<hr>Listbox Test<br><br>");
			clsJQuery.jqListBox lb = new clsJQuery.jqListBox("list1", this.PageName);
			lb.style = "height:100px;width:300px;";
			lb.items.Add("item 1");
			lb.items.Add("item 2");
            stb.Append(lb.Build());

			stb.Append("<hr>Listbox Test<br><br>");
			clsJQuery.jqListBox lb1 = new clsJQuery.jqListBox("list2", this.PageName);
			lb1.style = "height:100px;width:300px;";
			lb1.items.Add("item 3");
			lb1.items.Add("item 4");
            stb.Append(lb1.Build());


			stb.Append("<hr>Scrolling Region Test<br>");
			clsJQuery.jqButton srb = new clsJQuery.jqButton("sradd", "Add To Region", this.PageName, false);

            stb.Append(srb.Build() + "<br><br>");
			clsJQuery.jqScrollingRegion sr = new clsJQuery.jqScrollingRegion("my_region");
			sr.className = "";
			sr.AddStyle("height:100px;overflow:auto;width: 500px;background: #BCE5FC;");
			sr.content = "Here is the content to scroll=0<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>Here is the content to scroll<br>";
            stb.Append(sr.Build());
			stb.Append("<br>");

			stb.Append("<hr>Spinner Test<br><br><br>");
			clsJQuery.jqDynSpinner sp = new clsJQuery.jqDynSpinner("spin");
			sp.left = 0;
            stb.Append(sp.Build());
			clsJQuery.jqButton spbut = new clsJQuery.jqButton("spbut", "Stop Spinner", this.PageName, false);
            stb.Append("<br><br><br>" + spbut.Build());

			stb.Append("<hr>Tabs Test<br><br>");
            clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("tab1id", this.PageName);
			clsJQuery.Tab tab = new clsJQuery.Tab();
			tab.tabTitle = "Tab 1";
			tab.tabDIVID = "1234";
			tab.tabContent = "The content of tab 1<br><div id='test'>The content of tab 1</div><br>";
			jqtabs.postOnTabClick = true;
			jqtabs.tabs.Add(tab);

			tab = new clsJQuery.Tab();
			tab.tabTitle = "Tab 2";
			tab.tabContent = "The content of tab 2";
			jqtabs.tabs.Add(tab);

            stb.Append(jqtabs.Build());

			stb.Append("<br><br>");


			stb.Append("<span title='the tip'><input ></input></span>\r\n");

			stb.Append("<hr>Progress Bar Test<br><br>");
			clsJQuery.jqProgressBar pb = new clsJQuery.jqProgressBar("pb1", 50);
			pb.className = "update_progress";
            stb.Append(pb.Build());
			stb.Append("<br>");


			stb.Append("<hr>ToolTip Test<br>");
			stb.Append("There should be a help icon after this text");
            stb.Append(new clsJQuery.jqToolTip("This is the tool tip!").build());

			stb.Append("<hr>Select File Test<br>");
			clsJQuery.jqLocalFileSelector fs = new clsJQuery.jqLocalFileSelector("fs1", this.PageName, false);
			fs.label = "HS Folder";
            fs.path = Util.gEXEPath;
			fs.AddExtension("*.*");
            stb.Append(fs.Build());

			clsJQuery.jqLocalFileSelector fs2 = new clsJQuery.jqLocalFileSelector("fs2", this.PageName, false);
			fs2.toolTip = "This will select a local file<br>This is on the next line.<a href=\"/elog\">Log</a>";
			fs2.label = "Root of Drive";
			fs2.path = "C:\\";
			fs2.AddExtension(".vb");
            stb.Append(fs2.Build());

			stb.Append("<hr>Overlay Test<br>");

			clsJQuery.jqOverlay ol = new clsJQuery.jqOverlay("ov1", this.PageName, false, "events_overlay");
			ol.toolTip = "This will display the overlay";
			ol.label = "Display Overlay";
			//Dim bov1 As New clsJQuery.jqButton("ov1button", "Edit", Me.PageName, False)
			//bov1.includeClickFunction = False


			clsJQuery.jqTextBox tbov1 = new clsJQuery.jqTextBox("tbov1", "text", "hello", this.PageName, 20, false);
			tbov1.toolTip = "This should be the tooltip for this textbox";
			clsJQuery.jqButton tbut1 = new clsJQuery.jqButton("tbut1", "submit", this.PageName, true);
			ol.overlayHTML = PageBuilderAndMenu.clsPageBuilder.FormStart("overlayformm", "testpage", "post");
            ol.overlayHTML += "<div>This is the overlay text<br><br>" + tbov1.Build() + tbut1.Build() + "</div>";
			ol.overlayHTML += PageBuilderAndMenu.clsPageBuilder.FormEnd();
            stb.Append(ol.Build());

			stb.Append("<hr>Upload File Test<br><br>");
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("uploadform", "testpage", "post"));
			stb.Append("<input name='uploadaction' value='uploadactionvalue' type='hidden'>");


			clsJQuery.jqFileUploader ul = new clsJQuery.jqFileUploader("uploader", this.PageName);
			ul.toolTip = "This will upload a file";
			ul.values.Add("uploadfile", "true");
			ul.AddExtension("*.jpg");
			ul.AddExtension("*.wav");
			ul.AddExtension("*.png");
			ul.acceptFiles = "audio/*";
			ul.label = "My Label for Uploading";
            stb.Append(ul.Build());
			stb.Append("Stuff after the uploader");

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

            stb.Append(Web_UI_Util.HTML_StartTable(1));
            stb.Append(Web_UI_Util.HTML_StartRow());
			stb.Append(Web_UI_Util.HTML_StartCell("", 1));

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("sliding_tab_form", "testpage", "post"));

			stb.Append("<hr>Button Textbox Test");
			clsJQuery.jqButton b1 = new clsJQuery.jqButton("b1", "Hide", this.PageName, false);
			b1.toolTip = "This button will hide the slider";
			clsJQuery.jqButton b11 = new clsJQuery.jqButton("b11", "Show", this.PageName, false);
			clsJQuery.jqButton b111 = new clsJQuery.jqButton("b111", "New Content", this.PageName, false);
			clsJQuery.jqTextBox t1 = new clsJQuery.jqTextBox("t1", "text", "hello", this.PageName, 20, false);




			stb.Append("<hr>Sliding tab Test<br><br>");
			clsJQuery.jqButton butOpenSlide = new clsJQuery.jqButton("butOpenSlide", "Open Slider", this.PageName, false);
            stb.Append(butOpenSlide.Build());
			clsJQuery.jqButton butCloseSlide = new clsJQuery.jqButton("butCloseSlide", "Close Slider", this.PageName, false);
            stb.Append(butCloseSlide.Build());
			stb.Append("<br><br>");
			clsJQuery.jqSlidingTab st = new clsJQuery.jqSlidingTab("myslide1", this.PageName, false);
			st.initiallyOpen = true;
			st.toolTip = "This is the tooltip for the sliding tab";
			st.tab.AddContent("the content");
			st.tab.name = "myslide_name";
			st.tab.tabName.Unselected = "Unselected Tab Title";
            st.tab.tabName.Selected = "Selected Tab Title" + t1.Build() + b1.Build() + b11.Build() + b111.Build();
            stb.Append(st.Build());

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

            stb.Append(Web_UI_Util.HTML_EndCell);
            stb.Append(Web_UI_Util.HTML_EndRow);
			stb.Append(Web_UI_Util.HTML_EndTable);

			stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("demo", ""));
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform1", "testpage", "post"));


			clsJQuery.jqSlider slid = new clsJQuery.jqSlider("sliderid", 0, 100, 25, clsJQuery.jqSlider.jqSliderOrientation.horizontal, 90, this.PageName, false);
			slid.toolTip = "This is the tooltip for the slider";
            stb.Append("<br><br>" + slid.build());

			stb.Append("<hr>jqTextBox Test<br><br>");
			clsJQuery.jqTextBox tb = new clsJQuery.jqTextBox("tb1", "text", "default text", this.PageName, 10, true);
			tb.promptText = "This is the prompt text for this textbox";
			tb.toolTip = "This is the tooltip for this text box";
			//tb.dialogWidth = 200
            stb.Append(tb.Build());
			stb.Append("<br><br>");
			tb.name = "tb2";
			tb.promptText = "This is the prompt text for this textbox";
			tb.toolTip = "This is the tooltip for this text box";
			//tb.dialogWidth = 200
            stb.Append(tb.Build());
			tb.name = "tb3";
			tb.promptText = "This is the prompt text for this textbox";
			tb.toolTip = "This is the tooltip for this text box";
			//tb.dialogWidth = 200
            stb.Append(tb.Build());

			stb.Append("<hr>jqCheckBox Test");
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("FormTestForCheckbox", "Something", "post"));
			clsJQuery.jqCheckBox c = new clsJQuery.jqCheckBox("check1", "CheckLabel", this.PageName, true, false);
			c.toolTip = "This is the tooltip for the checkbox";
			c.@checked = true;
			stb.Append("<br>");
            stb.Append(c.Build());
			stb.Append("<br>");

			clsJQuery.jqCheckBox c2 = new clsJQuery.jqCheckBox("check2", "CheckLabel", this.PageName, true, true);
			stb.Append("<br>");
            stb.Append(c2.Build());
			stb.Append("<br>");
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

            stb.Append(Web_UI_Util.HTML_StartTable(0));
            stb.Append(Web_UI_Util.HTML_StartRow());
            stb.Append(Web_UI_Util.HTML_StartCell("", 1, Web_UI_Util.HTML_Align.LEFT, true));
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("FormTestForDrag", "Something", "post"));
			stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("SomesortOfDIV", ""));

			stb.Append(clsJQuery.DivStart("d1", "", true, false, "", "", this.PageName));



			stb.Append("<hr>jqTimeSpanPicker Test<br><br>");

			clsJQuery.jqTimeSpanPicker ts = new clsJQuery.jqTimeSpanPicker("tm1", "Time:", this.PageName, true);
			ts.toolTip = "This is the tooltip for the timespan picker";
			ts.showSeconds = true;
			//ts.formToPostID = "myform1"
            stb.Append(ts.Build());
			stb.Append("<br><br>");

			clsJQuery.jqButton tsbutt = new clsJQuery.jqButton("tsbutt", "submit timespan picker", this.PageName, true);
            stb.Append(tsbutt.Build());
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

            stb.Append(Web_UI_Util.HTML_EndCell);
            stb.Append(Web_UI_Util.HTML_EndRow);
            stb.Append(Web_UI_Util.HTML_EndTable);

			stb.Append(clsJQuery.DivEnd());

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("FormTestForDrag", "Something", "post"));

			stb.Append(clsJQuery.DivStart("d2", "tmdroppable", true, true, ".tmdroppable", "", this.PageName));
			clsJQuery.jqTimeSpanPicker ts2 = new clsJQuery.jqTimeSpanPicker("tm2", "Time:", this.PageName, false);
			ts2.toolTip = "Time in line tooltip";
			ts2.showSeconds = false;
            stb.Append("Time in line: " + ts2.Build());
			stb.Append("<br><br>");
            stb.Append(clsJQuery.DivEnd());

			stb.Append(clsJQuery.DivStart("d3", "tmdroppable", true, true, ".tmdroppable", "", this.PageName));



			clsJQuery.jqTimeSpanPicker ts3 = new clsJQuery.jqTimeSpanPicker("tm3", "Time:", this.PageName, true);
			ts3.showSeconds = false;
			ts3.showDays = false;
			//ts3.formToPostID = "myform1"
            stb.Append(ts3.Build());
			stb.Append("<br><br>");
            stb.Append(clsJQuery.DivEnd());

            stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

            stb.Append(Web_UI_Util.HTML_EndCell);
            stb.Append(Web_UI_Util.HTML_EndRow);
            stb.Append(Web_UI_Util.HTML_EndTable);





			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform3", "testpage", "post"));

			stb.Append("<hr>jqTimePicker Test<br><br>");
			clsJQuery.jqTimePicker tp = new clsJQuery.jqTimePicker("mytm", "Time:", this.PageName, false);
			tp.toolTip = "This is the tooltip for the Time Picker";
			tp.ampm = true;
			tp.showSeconds = true;
			tp.minutesSeconds = false;
			tp.defaultValue = "1:30:45";
            stb.Append(tp.Build());
			stb.Append("<br>");
			clsJQuery.jqButton btimebut = new clsJQuery.jqButton("button", "Button for timepickerpost", this.PageName, true);
            stb.Append(btimebut.Build());

			stb.Append("<hr>jqDatePicker Test");

			clsJQuery.jqDatePicker dp = new clsJQuery.jqDatePicker("mydp", "Date:", this.PageName, false, ",");
			//dp.toolTip = "This is the tooltip for the Date Picker"
			dp.multipleDates = true;
			dp.allowWildcards = true;
			dp.defaultDate = "1/1/2011,2/1/2012";
			dp.size = 40;
            stb.Append(dp.Build());

			stb.Append(PageBuilderAndMenu.clsPageBuilder.TextBox("name", "name", "text", "", 8));
			stb.Append(PageBuilderAndMenu.clsPageBuilder.TextBox("address", "address", "text", "", 8));
			clsJQuery.jqButton b = new clsJQuery.jqButton("button1", "Button for form1", this.PageName, true);
			b.primaryIcon = "ui-icon-locked";
			b.toolTip = "This is the tooltip";
			b.imagePathNormal = "images/HomeSeer/ui/Expand.png";

			stb.Append("<DIV id='u1'>DIV U1</DIV>");
			stb.Append("<DIV id='u2'>DIV U2</DIV>");

            stb.Append(b.Build());
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

			stb.Append("<hr>jqTextBox/jqButton Test form post<br>");
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform2", "testpage", "post"));
			clsJQuery.jqTextBox tb2 = new clsJQuery.jqTextBox("textbox2", "text", "", this.PageName, 40, false);
			tb2.editable = true;

            stb.Append(tb2.Build());
			//stb.Append(PageBuilderAndMenu.clsPageBuilder.TextBox("textbox2", "textbox2", "text", "", 8))
			clsJQuery.jqButton b2 = new clsJQuery.jqButton("buttonform2", "Button for form2", this.PageName, true);
            stb.Append(b2.Build());
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());


			stb.Append("<hr>jqDropList Test<br><br>");
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("mydroplistform", "mydroplistform_name", "post"));
			// droplist

			clsJQuery.jqDropList dl = new clsJQuery.jqDropList("droplistname", this.PageName, false);
			dl.toolTip = "This is the tooltip for the Droplist";
			dl.AddItem("first one", "1", false);
			dl.AddItem("second one", "2", true);
			dl.autoPostBack = false;
            stb.Append(dl.Build());

			dl.id = "droplistname2";
			dl.name = "droplistname3";
			dl.toolTip = "This is the tooltip for the second Droplist";
			dl.items.Clear();
			dl.AddItem("first one again", "1", false);
			dl.AddItem("second one again", "2", true);
            stb.Append(dl.Build());

			clsJQuery.jqButton dlbut = new clsJQuery.jqButton("dlbutt", "Submit Droplist Form", this.PageName, true);
            stb.Append("<br>" + dlbut.Build());

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());


			// selector
			stb.Append("<hr>jqSelector Test");
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("myform4", "testpage", "post"));

			clsJQuery.jqSelector s = new clsJQuery.jqSelector("sel1", this.PageName, false);
			s.toolTip = "This is the tooltip for the selector";
			s.AddItem("Red", "1", false);
			s.AddItem("Blue", "2", true);
			s.AddItem("Green", "3", false);
			s.AddItem("Any Color", "4", true);
			s.label = "Edit Colors";
			s.dialogCaption = "Edit Colors 123";
			stb.Append(s.Build());

			clsJQuery.jqSelector s2 = new clsJQuery.jqSelector("sel2", this.PageName, false);
			s2.AddItem("value1", "1", false);
			s2.AddItem("value2", "2", true);
			s2.AddItem("value3", "3", false);
			s2.AddItem("my big long value here", "3", true);
			s2.label = "Edit Items";
			stb.Append(s2.Build());
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

			stb.Append("<hr>jqRadioButton Test<br><br>");
			clsJQuery.jqRadioButton rb = new clsJQuery.jqRadioButton("rb1", this.PageName, false);
			//rb.buttonset = False
			rb.values.Add("Item 1", "1");
			rb.values.Add("Item 2", "2");
			rb.@checked = "Item 2";
			stb.Append(rb.Build());

			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("colorpickerpost", "testpage", "post"));
			stb.Append("<hr>jqColorPicker Test<br>");
			clsJQuery.jqColorPicker cp = new clsJQuery.jqColorPicker("color", "status", 40, "0000");
			stb.Append(cp.Build());
			clsJQuery.jqButton bcolor = new clsJQuery.jqButton("colorbutton", "Button for color picker submit", this.PageName, true);
			stb.Append(bcolor.Build());
			stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

			stb.Append("<hr>jqContainer Test<br><br>");
			stb.Append(clsJQuery.DivStart("container_div", "", false, false, "", "", this.PageName));
			clsJQuery.jqContainer room = new clsJQuery.jqContainer("parent1", "", "Container Title", "", 0, 0, 200, 100, "This is the container content on the first line<br><b>This is the second line in bold</b>", false,
			this.PageName, false);
			room.backgroundColor = "e5e5e5";
			room.positionAbsolute = false;
			stb.Append(room.build());
			stb.Append(clsJQuery.DivEnd());

			stb.Append("<hr>jqButton Test<br><br>");
			clsJQuery.jqButton jqbut = new clsJQuery.jqButton("jqb1", "Normal Button", this.PageName, false);
			jqbut.toolTip = "Tooltip for the button";
            stb.Append(jqbut.Build());

			clsJQuery.jqButton jqbut2 = new clsJQuery.jqButton("jqb2", "", this.PageName, false);
			jqbut2.imagePathNormal = "\\images\\HomeSeer\\ui\\Delete.png";
			jqbut2.imagePathPressed = "\\images\\HomeSeer\\ui\\Delete.png";
			jqbut2.toolTip = "Tooltip for the image button";
			stb.Append(jqbut2.Build());

			stb.Append("<hr>jqButton URL Test<br><br>");
			clsJQuery.jqButton urlbut = new clsJQuery.jqButton("urlbut", "Button", this.PageName, false);
			urlbut.url = "\\deviceutility";
			stb.Append(urlbut.Build());

			stb.Append("<hr>jqMultiselect Test<br><br>");
			clsJQuery.jqMultiSelect msel = new clsJQuery.jqMultiSelect("msel1", this.PageName, false);
			msel.label = "Select test value";
			msel.AddItem("Option Name 1", "val1", false);
			msel.AddItem("Option Name 2", "val2", true);
			msel.AddItem("Option Name 3", "val3", true);
			stb.Append(msel.Build());




			stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("", "id='result'"));
			stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());


			// container test
			//Dim statCont As New clsJQuery.jqContainer("contid", "Office Lamp", "/homeseer/on.gif", 100, 100, "this is the content")
			//stb.Append(statCont.build)


			stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());



		} catch (Exception ex) {
			stb.Append("Test page error: " + ex.Message);
		}
		stb.Append("<br>");


		page.AddBody(stb.ToString());

        return page.BuildPage();
	}
}

}
