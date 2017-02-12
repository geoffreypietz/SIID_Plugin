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
            return new selectorInput(selectorOptions,id,name,def,AjaxPostDestination);
                }
        public checkBoxInput checkBoxInput(string id, bool Checked = false)
        {
            return new checkBoxInput( id,  Checked, AjaxPostDestination);
        }
        public numberInput numberInput(string id, int def = 0)
        {
            return new numberInput( id,  def, AjaxPostDestination);
        }
        public htmlTable htmlTable()
        {
            return new htmlTable(AjaxPostDestination);
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
                var theData = value+ '&id=' + '" + id + @"';
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
                var theData = value+ '&id=' + '" + id + @"';
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
                var theData = value+ '&id=' + '" + id + @"';
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
