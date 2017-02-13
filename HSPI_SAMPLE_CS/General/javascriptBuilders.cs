﻿using System;
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
        public htmlTable htmlTable()
        {
            return new htmlTable(AjaxPostDestination);
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
<button type = 'submit' id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' role='button' aria-disabled='false'>
<a class='ui-button-text' href=/" + AjaxPostDestination + @">" + label + @"</a></button>";

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

            html = prescript + @"<input id=" + id + " ><script>" + id + ".value=" + def + "</script>";

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

            html = prescript+@"<input id=" +id+" type='number'><script>"+id+".value="+def+"</script>";

        }
      


    }



  public  class htmlTable:htmlObject
    {
        public static string footer = "</tbody></table>";
        public static string header = "<table border='0' cellpadding='0' cellspacing='0' width='1000'><tbody>";
        public StringBuilder body = new StringBuilder();
        public  htmlTable(string aj)
        {
            AjaxPostDestination = aj;

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
