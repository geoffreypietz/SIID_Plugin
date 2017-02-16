using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SAMPLE_CS
{
    public class htmlBuilder //interface for the html buidling functions, to enable one to set the AJax destination
    {
        string AjaxPostDestination = "";
        public htmlBuilder(string Ajax)
        {
            AjaxPostDestination = Ajax;

        }


        public selectorInput selectorInput(string[] selectorOptions, string id = "", string name = "", int def = 0)
        {
            return new selectorInput(selectorOptions, id, name, def, AjaxPostDestination);
        }
        public checkBoxInput checkBoxInput(string id, bool Checked = false)
        {
            return new checkBoxInput(id, Checked, AjaxPostDestination);
        }
        public numberInput numberInput(string id, int def = 0)
        {
            return new numberInput(id, def, AjaxPostDestination);
        }
        public htmlTable htmlTable(int width=1000)
        {
            return new htmlTable(AjaxPostDestination,width);
        }
        public Gobutton Gobutton(string id, string label)
        {
            return new Gobutton(id, label, AjaxPostDestination);
        }
        public stringInput stringInput(string id, string def) {
            return new stringInput( id,  def, AjaxPostDestination);
            }
        public button button(string id, string label)
        {

            return new button(id, label, AjaxPostDestination);
        }
        public radioButton radioButton(string id, string[] choices, int selected)
        {
            return new radioButton(id, choices, selected, AjaxPostDestination);
        }
        public MakeImage MakeImage(int w, int  h ,string link)
        {
            return new MakeImage(w, h, link);
        }
        public MakeLink MakeLink(string l, string n)
        {
            return new MakeLink(l, n);
        }
    }
    public class htmlObject
    {
        public string html = "";

        public string AjaxPostDestination = "";



        public string print()
        {
           
            return html;
        }


    }

    /*    $(function()
        {
     $('#oLogLevel').change(function() {
                var value = $(this).val();
                value = encodeURIComponent(value);
                var theID;
                var theform =$('#' +$(this)[0].form.id);
                var theData = theform.serialize() + '&id=' + 'oLogLevel';
                commonAjaxPost(theData, 'Modbus_Config');
            });
        });*/
        public class radioButton : htmlObject
    {
        public radioButton(string id, string[] choices, int selected, string AJX)
        {
        
                AjaxPostDestination = AJX;

                string prescript = @"<script>  $(function()
              {
           $('#" + id + "_" + @"').click(function() {

                      var value =true;
                      value = encodeURIComponent(value);
                    var theData ='&value='+ value+ '&id=' + '" + id + @"';
      console.log(theData);
                      commonAjaxPost(theData, '" + AjaxPostDestination + @"');
                  });
              })</script>";
        

            StringBuilder body = new StringBuilder();
            body.Append("<div id = " + id + "_Holder>");
            int count = 0;
            foreach(string choice in choices)
            {
                body.Append(@"<script>  $(function()
              {
           $('#" + id + "_" +count+@"').click(function() {

                      var value =true;
                      value = encodeURIComponent(value);
                    var theData ='&value='+ value+ '&id=' + '" + id + @"';
      console.log(theData);
                      commonAjaxPost(theData, '" + AjaxPostDestination + @"');
                  });
              })</script>");
                if (count == selected)
                {
                    body.Append("<input type=\"radio\" name=\""+id+"\" id=\""+id+"_"+count+"\" checked>");

                }
                else
                {
                    body.Append("<input type=\"radio\" name=\"" + id + "\" id=\"" + id + "_" + count + "\" >");

                }
                body.Append("<label for  \"" + id + "_" + count +"\">"+choice+"</label>");

            }
            body.Append("</div>");

            html = prescript + body.ToString();

        }

    }
        
       
    public class button : htmlObject
    {
        public button(string id, string label, string AJX)
        {
            AjaxPostDestination = AJX;
                 string prescript = @"<script>  $(function()
              {
           $('#" + id + @"').click(function() {

                      var value =true;
                      value = encodeURIComponent(value);
                    var theData ='&value='+ value+ '&id=' + '" + id + @"';
      console.log(theData);
                      commonAjaxPost(theData, '" + AjaxPostDestination + @"');
                  });
              })</script>";

              html = prescript+@"
<button type = 'submit' id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' role='button' aria-disabled='false'>" + label + @"</button>";

        }

    }

    public class Gobutton:htmlObject
    {
        public Gobutton(string id, string label, string AJX)
        {
            AjaxPostDestination = AJX;
      /*      string prescript = @"<script>  $(function()
        {
     $('#" + id + @"').click(function() {

                var value =true;
                value = encodeURIComponent(value);
              var theData ='&value='+ value+ '&id=' + '" + id + @"';
console.log(theData);
                commonAjaxPost(theData, '" + AjaxPostDestination + @"');
            });
        })</script>";*/

           //  html = prescript+@"
           html=@"
<button type = 'submit' id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' role='button' aria-disabled='false'>
<a class='ui-button-text' href=/"+AjaxPostDestination+@">" + label + @"</a></button>";

        }

    }


    public class selectorInput:htmlObject
    {
     
        public selectorInput(string[] selectorOptions,string id,string name, int def,string AJX)
        {
            AjaxPostDestination = AJX;
            string prescript = @"<script>  $(function()
        {
     $('#"+id+ @"').change(function() {

                var value = $(this).val();
                value = encodeURIComponent(value);
              var theData ='&value='+ value+ '&id=' + '" + id + @"';
console.log(theData);
                commonAjaxPost(theData, '" + AjaxPostDestination+@"');
            });
        })</script>";
           string header = "<select id='" + id + "' name = '" + name + "' class = '.jqDropList'>";
            StringBuilder body = new StringBuilder();
            int count = 0;
            foreach (string ops in selectorOptions)
            {
                if(count == def)
                {
                    body.Append("<option selected='selected' value='" + count + "'>" + ops + "</option>");
                }
                else
                {
                    body.Append("<option value='" + count + "'>" + ops + "</option>");
                }
               

                count++;
            }
            html = prescript+header + body.ToString() + "</select>";


            }

        }

    public class checkBoxInput:htmlObject
    {
        public checkBoxInput( string id, bool Checked,string AJ)
        {
            AjaxPostDestination = AJ;

            string prescript = @"<script>  $(function()
        {
     $('#" + id + @"').change(function() {

                var value = $(this)[0].checked;
                value = encodeURIComponent(value);
                var theData ='&value='+ value+ '&id=' + '" + id + @"';
console.log(theData);
                commonAjaxPost(theData, '" + AjaxPostDestination + @"');
            });
        })</script>";

            if (Checked)
            {
                html = prescript+@"<input id = " + id + " checked  type='checkbox'>";

            }
            else
            {
                html = prescript+@"<input id = " + id + " type='checkbox'>";
            }
        }
    }

    

    public class stringInput:htmlObject
    {
        public stringInput(string id, string def, string aj)
        {
            AjaxPostDestination = aj;

            string prescript = @"<script>  $(function()
        {
     $('#" + id + @"').bind('input', function() {

                var value = $(this).val();
                value = encodeURIComponent(value);
              var theData ='&value='+ value+ '&id=' + '" + id + @"';
console.log(theData);
                commonAjaxPost(theData, '" + AjaxPostDestination + @"');
            });
        })</script>";

            html = prescript + @"<input id=" + id + " ><script> $('#"+id+"')[0].value = '" + def + "';</script>";

        }



    }

    public class numberInput:htmlObject
    {

        
        public numberInput(string id, int def, string aj)
        {
            AjaxPostDestination = aj;

            string prescript = @"<script>  $(function()
        {
     $('#" + id + @"').bind('input', function() {

                var value = $(this).val();
                value = encodeURIComponent(value);
              var theData ='&value='+ value+ '&id=' + '" + id + @"';
console.log(theData);
                commonAjaxPost(theData, '" + AjaxPostDestination + @"');
            });
        })</script>";

            html = prescript+@"<input id=" +id+" type='number'><script>$('#"+id+"')[0].value="+def+";</script>";

        }
      


    }

    public class MakeImage:htmlObject
    {
        public  MakeImage(int width, int height, string source)
        {
            html = "<img src=\"" + source + "\" height=\"" + height + "\" width=\"" + width + "\">";

        }

    }

   public class MakeLink : htmlObject
    {
        public MakeLink(string link, string name )
        {
            html = "<a href=\"" + link + "\">" + name + "</a>";
        }
    }

  public  class htmlTable:htmlObject
    {
        public static string footer = "</tbody></table>";
        public  string header = "<table border='0' cellpadding='0' cellspacing='0' width='1000'><tbody>";
        public StringBuilder body = new StringBuilder();

        public  htmlTable(string aj, int width=1000)
        {
            AjaxPostDestination = aj;
            header = "<table border='0' cellpadding='0' cellspacing='0' width='"+width+"'><tbody>";

        }
        public void addT(string title)
        {
            addRow("<td class=\"tableheader\" width=\"100%\" colspan=\"2\">" + title + "</td>");

        }
        public void addDevHeader(string title)
        {
            addRow("<td class=\"columnheader\" colspan=\"4\">"+title+"</td>");

    }


        public void addSubHeader(string I1,string I2, string I3, string I4, string I5)
        {
            StringBuilder row = new StringBuilder();
            row.Append("<td width = '2%' ></td>");
            row.Append("<td class ='columnheader' width='20px'>"+I1+"</td>");
            row.Append("<td class ='columnheader' >" + I2 + "</td>");
            row.Append("<td class ='columnheader' >" + I3 + "</td>");
            row.Append("<td class ='columnheader' >" + I4 + "</td>");
            row.Append("<td class ='columnheader' >" + I5 + "</td>");
            addRow(row.ToString());
        }
        public void addSubMain(string I1, string I2, string I3, string I4, string I5)
        {
            StringBuilder row = new StringBuilder();
            row.Append("<td width = '2%' ></td>");
            row.Append("<td class ='tableroweven' width='5%'>" + I1 + "</td>");
            row.Append("<td class ='tableroweven' >" + I2 + "</td>");
            row.Append("<td class ='tablecell' width='100px' >" + I3 + "</td>");
            row.Append("<td class ='tablecell' width='15%'>" + I4 + "</td>");
            row.Append("<td class ='tablecell'>" + I5 + "</td>");
            addRow(row.ToString());

        }

        public void addDevMain(string Item1, string Item2)
        {
            addRow("<td class=\"tableroweven\" width='80%'>" + Item1 + "</td> <td class=\"tableroweven\" >" + Item2 + "</td>");
        }
        public void add(string title, string value ="")
        {
            StringBuilder row = new StringBuilder();

         
            if(value != "")
            {
                row.Append("<td class='tablecell' width='30 % '>" + title + "</ td >");
                row.Append("<td class='tableroweven' width='70 % '>" + value + "</ td >");

            }
            else
            {
                row.Append("<td class='columnheader' width='30 % '>" + title + "</ td >");

            }
            addRow(row.ToString());




        }
        public void addDev(string title, string value = "", bool isPicture = false)
        {
        StringBuilder row = new StringBuilder();
        row.Append("<td class='tablecell' colspan='1' >" + title + "</ td >");
        row.Append("<td class='tablecell' colspan='1'>" + value + "</ td >");

            if (isPicture)
            {
                row.Append("<td class='tablecelldevice' colspan='1' rowspan='16' style='width:600px; text-align:center;'></td>");
      

            }
           
            addRow(row.ToString());




        }
        public void addLong(string title, string value = "")
        {
            StringBuilder row = new StringBuilder();
            row.Append("<td class='tablecell_label' colspan='1' style = 'width:150px;' align='left'>" + title + "</ td >");
            row.Append("<td class='tablecell' align='left' colspan='9'>" + value + "</ td >");

          
            addRow(row.ToString());




        }

        private void addRow(string rowstring)
        {

            body.Append("<tr>"+rowstring+"</tr>");
         
        }

        
        public  new  string print()
        {
            html = header + body.ToString() + footer;
            return html;

        }
    




    }
}
