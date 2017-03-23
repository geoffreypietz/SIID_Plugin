using Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID_ModBusDemo
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
        public Qbutton Qbutton(string l, string n)
        {
            return new Qbutton(l, n, AjaxPostDestination);

        }
        public timeInput timeInput(string l, string n)
        {
            return new timeInput(l, n, AjaxPostDestination);
            
        }
        public Downloadbutton Downloadbutton(string l, string n)
        {
            return new Downloadbutton(l, n, AjaxPostDestination);

        }

        public Uploadbutton Uploadbutton(string l, string n)
        {
            return new Uploadbutton(l, n, AjaxPostDestination);

        }
        

        public ShowMesbutton ShowMesbutton(string l, string n)
        {
            return new ShowMesbutton(l, n, AjaxPostDestination);

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

                      var value ="+count+@";
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
                count++;

            }
            body.Append("</div>");

            html = prescript + body.ToString();

        }

    }

    public class ShowMesbutton : htmlObject
    {//actually this is just for gateway connection test button
        public ShowMesbutton(string id, string label, string AJX)
        {
            AjaxPostDestination = AJX;
            string prescript = @"<script>  $(function()
              {
           $('#" + id + @"').click(function() {

                      var value =true;
                      value = encodeURIComponent(value);
                    var theData ='&value='+ value+ '&id=' + '" + id + @"';
      console.log(theData);

conMes.style.display='';
conMes.textContent='Testing connection...';
conMes.style.color='blue';
 $.ajax({
  type: 'POST',
  async: true,
  url: '/' + '" + AjaxPostDestination + @"',
  data: theData,
  error: function() {
      $('div#errormessage').html('Page is refreshing...');
      $('div#contentdiv').html('');
            },
  beforeSend: function() {
            },
  success: function(response) { 
if(response.length>0){
conMes.style.display='';
conMes.textContent=response;
if(response.indexOf('Connection')==0){
conMes.style.color='#00cc00';
}
else if(response.indexOf('ailed')>-1){
conMes.style.color='red';
}
else{
conMes.style.color='#b38600';
}
}
}
        });
});
});
              </script>";

            html = prescript + @"
<button  id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' type='button' role='button' aria-disabled='false'><span  class='ui-button-text'>" + label + @"</span></button>";

        }

    }


    public class Uploadbutton : htmlObject
    {
        public Uploadbutton(string id, string label, string AJX)
        {
            AjaxPostDestination = AJX;
            //OK on click open file selector
            //On select, send the file to backend via ajax
            string prescript = @"<script>  $(function()
              {


function handleFileSelect(e){
console.log('ARRIVED IN THERE');
 f = e.target.files[0];
 fr = new FileReader();
fr.onload = function(e){
console.log( e.target.result);
V= e.target.result;
 var theData ='&value='+  e.target.result+ '&id=' + '" + id + @"';

$.ajax({
  type: 'POST',
  async: true,
  url: '/' + '" + AjaxPostDestination + @"',
  data: theData,
  error: function() {
      $('div#errormessage').html('Page is refreshing...');
      $('div#contentdiv').html('');
            },
  beforeSend: function() {
            },
  success: function(response) { console.log(response);
location.reload();
        }
        });



};
fr.readAsText(f);

     }

link=document.createElement('input');
link.type='file';
link.id=this.id+'Upload';
link.style.display='none';
document.body.appendChild(link);
link.addEventListener('change', handleFileSelect, false);

           $('#" + id + @"').click(function() {

            link.click();
console.log(link.id);
});





});
              </script>";

            html = prescript + @"
<button  id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' type='button' role='button' aria-disabled='false'><span  class='ui-button-text'>" + label + @"</span></button>";

        }

    }

    public class Downloadbutton : htmlObject
    {
        public Downloadbutton(string id, string label, string AJX)
        {
            AjaxPostDestination = AJX;
            string prescript = @"<script>  $(function()
              {
           $('#" + id + @"').click(function() {

                      var value =true;
                      value = encodeURIComponent(value);
                    var theData ='&value='+ value+ '&id=' + '" + id + @"';
      console.log(theData);

 $.ajax({
  type: 'POST',
  async: true,
  url: '/' + '" + AjaxPostDestination + @"',
  data: theData,
  error: function() {
      $('div#errormessage').html('Page is refreshing...');
      $('div#contentdiv').html('');
            },
  beforeSend: function() {
            },
  success: function(response) { console.log(response);
G=response.split('_)(*&^%$#@!');
FileName=G[0];
FileContent=G[1];

var link=document.createElement('a');
 link.download = FileName;
            link.href = 'data:text/UTF8,' + escape(FileContent);
            link.click()


//Take string response, parse it into FileName, FileContent
//Make the file of that name and content, and download it

        }
        });
});
});
              </script>";

            html = prescript + @"
<button  id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' type='button' role='button' aria-disabled='false'><span  class='ui-button-text'>" + label + @"</span></button>";

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

 $.ajax({
  type: 'POST',
  async: true,
  url: '/' + '"+AjaxPostDestination+ @"',
  data: theData,
  error: function() {
      $('div#errormessage').html('Page is refreshing...');
      $('div#contentdiv').html('');
            },
  beforeSend: function() {
            },
  success: function(response) { console.log(response);
if(response=='refresh'){
location.reload();
}}
        });
});
});
              </script>";

              html = prescript+@"
<button  id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' type='button' role='button' aria-disabled='false'><span  class='ui-button-text'>" + label + @"</span></button>";

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
<a class='ui-button-text' href=/"+AjaxPostDestination+ @"><span  class='ui-button-tex'>" + label + @"</span></a></button>";

        }

    }
    public class Qbutton : htmlObject
    {
        public Qbutton(string id, string label, string AJX)
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
            html = @"
<button type = 'submit' id = '" + id + @"'  class='ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only' role='button' aria-disabled='false'>
<a class='ui-button-text' href=/" + AjaxPostDestination + @"?"+id+ @"><span  class='ui-button-tex'>" + label + @"</span></a></button>";


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

    //Numbers are displayed as hh:mm:ss so given a number 
    public class timeInput : htmlObject
    {

        public timeInput(string id, string def, string aj)
        {
            AjaxPostDestination = aj;

            string prescript = @"<script>  $(function()
        {
     $('#" + id + @"').bind('change', function() {

                var value = $(this).val();
                value = encodeURIComponent(value);
              var theData ='&value='+ value+ '&id=' + '" + id + @"';
console.log(theData);
                commonAjaxPost(theData, '" + AjaxPostDestination + @"');
            });
        })

</script>";
            var tp = new clsJQuery.jqTimePicker("mytm", "Time:", "test", false);
            tp.toolTip = "Pick a time of day";
            tp.ampm = true;
            tp.showSeconds = true;
            tp.minutesSeconds = false;
            tp.editable = false ;
            /* while (def.Count() < 6)
             {
                 def = def + "0";
             }
             tp.defaultValue = new string(new char[] { def[0], def[1], ':', def[2], def[3], ':', def[4], def[5] });*/
            tp.defaultValue = def;
            tp.id = id;



            html = prescript + tp.Build();

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
        public void addHead(string[] HeadArray)
        {
            StringBuilder row = new StringBuilder();
            foreach (string head in HeadArray)
            {
                row.Append("<td class=\"columnheader\"> "+ head + "</td>");
            }
            addRow(row.ToString());

        }
        public void addArrayRow(string[] HeadArray)
        {
            StringBuilder row = new StringBuilder();
            foreach (string head in HeadArray)
            {
                row.Append("<td  class ='tablecell'> " + head + "</td>");
            }
            addRow(row.ToString());

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
