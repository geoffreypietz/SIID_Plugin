using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO.BACnet;
using Utilities;
using System.ComponentModel;

using HSPI_SIID.BACnet.Model;   //namespaces got screwed up by arranging things into folders...


namespace HSPI_SIID.BACnet
{
    [DataContract]
    public class BACnetProperty
    {



        private void Initialize(BACnetObject bno, BacnetPropertyIds property_id )
        {
            this.BacnetObject = bno;
            this.BacnetPropertyId = property_id;        //this is how these are indexed within the object
            
            this.Id = (Int32)property_id;
            SetName();

            

        }


        public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id)
        {
            Initialize(bno, property_id);
            ReadValue();
            ParseValue();

            //if (property_value = null)
            //    ReadProperty();
        }



        public BACnetProperty(BACnetObject bno, BacnetPropertyIds property_id, BacnetPropertyValue property_value) //, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) //this(bno, property_id, array_index)
        {
            Initialize(bno, property_id);
            this.BacnetPropertyValue = property_value;
            ParseValue();

        }





        private void ParseValue()
        {
            if (this.BacnetPropertyValue == null)
                return;

            BacnetPropertyValue p_value = (BacnetPropertyValue)this.BacnetPropertyValue;

            object value = null;
            BacnetValue[] b_values = null;
            if (p_value.value != null)
            {

                b_values = new BacnetValue[p_value.value.Count];

                p_value.value.CopyTo(b_values, 0);
                if (b_values.Length > 1)
                {
                    object[] arr = new object[b_values.Length];
                    for (int j = 0; j < arr.Length; j++)
                        arr[j] = b_values[j].Value;
                    value = arr;
                }
                else if (b_values.Length == 1)
                    value = b_values[0].Value;
            }
            else
                b_values = new BacnetValue[0];


            var propName = GetNiceName((BacnetPropertyIds)p_value.property.propertyIdentifier);
            var propRef = p_value.property;
            //var readOnly = false;   //may change...
            var appTag = b_values.Length > 0 ? b_values[0].Tag : (BacnetApplicationTags?)null;


            // Modif FC
            CustomProperty cp;

            Type propType = null;
            Boolean readOnly;

            switch ((BacnetPropertyIds)p_value.property.propertyIdentifier)
            {
                // PROP_RELINQUISH_DEFAULT can be write to null value
                case BacnetPropertyIds.PROP_PRESENT_VALUE:
                    // change to the related nullable type
                    Type t = null;
                    try
                    {
                        t = value.GetType();
                        t = Type.GetType("System.Nullable`1[" + value.GetType().FullName + "]");
                    }
                    catch { }

                    propType = t != null ? t : typeof(string);
                    readOnly = false;   //for now - in future, some properties may be
                    break;

                default:

                    propType = value != null ? value.GetType() : typeof(string);
                    readOnly = false;
                    break;
            }

            this.CustomProperty = new Utilities.CustomProperty(propName, value, propType, readOnly, "", appTag, null, propRef);

            this.PropertyDescriptor = new BACnetCustomPropertyDescriptor(ref this.CustomProperty, new Attribute[] { });
            //this.


            //want to put in logic here so that priority array gets read as a string.  Look at how property html is generated



        }





        private void SetName()          //todo: this function exists in Yabe already as GetNiceName....it's what we display as opposed to the enum value
        {

            String ts = BacnetPropertyId.ToString();    //should just get numeric value if proprietary and outside of predefined alues.

            if (ts.StartsWith("PROP_"))
                ts = ts.Substring(5);   //else - proprietary...?  No, since it was already cast into BacnetObjectTypes enum.
            //else
            //    ts = "PROPRIETARY_(" + ts + ")";   //not sure if this handles it adequately, but will see...

            String[] tw = ts.Split("_".ToCharArray());
            String tsFinal = "";
            foreach (String word in tw)
                tsFinal += word[0].ToString().ToUpper() + word.Substring(1).ToLower() + " ";

            tsFinal = tsFinal.TrimEnd();
            this.Name = tsFinal;   

        }



        private static string GetNiceName(BacnetPropertyIds property)
        {
            string name = property.ToString();
            if (name.StartsWith("PROP_"))
            {
                name = name.Substring(5);
                name = name.Replace('_', ' ');
                name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            }
            else
                //name = "Proprietary (" + property.ToString() + ")";
                name = property.ToString() + " - Proprietary";
            return name;
        }




        private uint arrayIndex;


        public String Name;

        public Int32 Id;



        public BACnetObject BacnetObject;


        [DataMember]
        public BacnetValue BacnetValue;



        public BacnetPropertyValue? BacnetPropertyValue;



        public Utilities.CustomProperty CustomProperty = null;


        public BACnetCustomPropertyDescriptor PropertyDescriptor = null;


        public IList<BacnetValue> BacnetValues;  //not sure how this works yet...



        //TODO: have all objects hold three required properties separately?

        public BacnetPropertyIds BacnetPropertyId;







        public bool WriteValue(object new_value, int writePriority = 0)
        {
            Console.WriteLine(this.BacnetPropertyId + " - Writing value: " + new_value.ToString() + " with write priority: " + writePriority);


            var comm = BacnetObject.BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetObject.BacnetDevice.BacnetAddress;
            var object_id = BacnetObject.BacnetObjectId;
            var property_id = BacnetPropertyId;

            var success = true;     //unless we hit one of the failure cases below


            var customProperty = this.CustomProperty;  //customProperty

            //fetch property

            //BacnetPropertyReference property = (BacnetPropertyReference)customProperty.Tag;

            //new value
            //object new_value = gridItem.Value;
            //var ty = new_value.GetType();

            //convert to bacnet
            BacnetValue[] b_value = null;




            if (new_value == null)
            {

                b_value = new BacnetValue[] { new BacnetValue(null) };

            }
            else
            {


                try
                {
                    if (new_value != null && new_value.GetType().IsArray)
                    {
                        Array arr = (Array)new_value;
                        b_value = new BacnetValue[arr.Length];
                        for (int i = 0; i < arr.Length; i++)
                            b_value[i] = new BacnetValue(arr.GetValue(i));
                    }
                    else
                    {
                        {
                            // Modif FC
                            b_value = new BacnetValue[1];
                            if ((BacnetApplicationTags)customProperty.bacnetApplicationTags != BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL)
                            {
                                b_value[0] = new BacnetValue((BacnetApplicationTags)customProperty.bacnetApplicationTags, new_value);
                            }
                            else
                            {
                                object o = null;
                                TypeConverter t = new TypeConverter();
                                // try to convert to the simplest type
                                String[] typelist = { "Boolean", "UInt32", "Int32", "Single", "Double" };

                                foreach (String typename in typelist)
                                {
                                    try
                                    {
                                        o = Convert.ChangeType(new_value, Type.GetType("System." + typename));
                                        break;
                                    }
                                    catch { }
                                }

                                if (o == null)
                                    b_value[0] = new BacnetValue(new_value);
                                else
                                    b_value[0] = new BacnetValue(o);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                   // Instance.hspi.Log("BACnetDevice Exception " + ex.Message, 2);
                    //MessageBox.Show(this, "Couldn't convert property: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }


            }

            //write
            try
            {
                //comm.WritePriority = (uint)Yabe.Properties.Settings.Default.DefaultWritePriority;   //TODO: let this be changeable
                //comm.WritePriority = (uint)BacnetObject.WritePriority;
                comm.WritePriority = (uint)writePriority;

                //b_value[0] = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL, b_value[0].Value);

                //b_value[0].Value = null;      //This is how to relinquish a value at a certain priority

                if (!comm.WritePropertyRequest(adr, object_id, BacnetPropertyId, b_value))
                {
                    Console.WriteLine(this.BacnetPropertyId + " - error writing property.  WritePropertyRequest returned false");
                    success = false;
                    //MessageBox.Show(this, "Couldn't write property", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                //Instance.hspi.Log("BACnetDevice Exception " + ex.Message, 2);
                Console.WriteLine(this.BacnetPropertyId + " - error writing property: " + ex.StackTrace + " " + ex.Message);
                success = false;
                //MessageBox.Show(this, "Error during write: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //success = true;
            if (success)
                Console.WriteLine(this.BacnetPropertyId + " - Successful writing property");

            //ReadValue();            //or do we let the webpage do this?  Once write callback has returned, read the value again?

            return success;

        }

        //instead of doing fancy HTML stuff, what about using AJAX call for each read and each write?  Generate the table row with the ability to get its initial value as well as write...
        //Yes...essentially the JS will include all of this.  The generated HTML will know to, on click, open up a dialog that, on close, will read




        public string priorityArrayHtml(IList<BacnetValue> vals, int startIndex, int endIndex)
        {

            var tblString = "<table class='priorityArray'><thead><tr>"; //<tr></tr>
            for (int i = startIndex; i <= endIndex; i++)
            {
                tblString += "<th>" + i + "</th>";
            }
            tblString += "</tr></thead>";


            tblString += "<tbody><tr>";



            for (int i = startIndex; i <= endIndex; i++)
            {
                var val = vals[i - 1];

                String valString;
                if (val.Value == null)
                    valString = "null";
                else
                    valString = val.ToString();

                tblString += "<td>" + valString + "</td>";

            }

            tblString += "</tr></tbody>";


            tblString += "</table>";

            return tblString;

        }







        public string ValueString()
        {

            try
            {

                var bacnetValue = this.BacnetPropertyValue.Value;

                IList<BacnetValue> vals = bacnetValue.value;



                if (vals.Count == 1)
                    return vals[0].ToString();      //this gets more tricky if enum, etc....gets underlying Value (of type object) and calls ToString...



                var valsString = "{";

                foreach (var val in vals)
                {
                    String valString;
                    if (val.Value == null)
                        valString = "null";
                    else
                        valString = val.ToString();

                    //valsString += "<td>" + valString + "</td>";

                    valsString += valString + ",";
                }
                valsString = valsString.TrimEnd(",".ToCharArray());

                // valsString += "</tr></tbody>";


                //valsString += "</table>";

                valsString += "}";

                return valsString;

            }
            catch (Exception ex)
            {
                //Instance.hspi.Log("BACnetDevice Exception " + ex.Message, 2);
                return "";
            }



            



        }


        public string PriorityArrayTable()
        {

            var bacnetValue = this.BacnetPropertyValue.Value;

            IList<BacnetValue> vals = bacnetValue.value;


            //if (vals.Count == 1)
            //     return vals[0].ToString();      //this gets more tricky if enum, etc....gets underlying Value (of type object) and calls ToString...


            //var valsString = "{";



            //valsString = "";

            var valsString = @"<style>table.priorityArray {
    border-collapse: collapse;
    table-layout: fixed;
    width: 290px;
    margin: 5px;
}

table.priorityArray, .priorityArray th, .priorityArray td {
    border: 1px solid black;
}</style>";


            //TODO: find a different place to inject this?  But only really needed if there is a priority array present....
//            valsString += @"
//
//<script>
//$(function() {
//
//        $('span.ui-button-text').filter(function(){
//      return ($(this).text() == 'Submit');
//    }).text('Command').closest('form').css('display','');
//
//
//});
////replace 'Submit' button text with 'Command' (in context of priority array)
////and change styling of parent form so that the buttons stack
//
//</script>
//
//
//
//";





            valsString += priorityArrayHtml(vals, 1, 8);


            valsString += priorityArrayHtml(vals, 9, 16);


            //valsString += "<table class='priorityArray'><thead><tr>"; //<tr></tr>
            //for (int i = 1; i <= 8; i++)
            //{
            //    valsString += "<td>" + i + "</td>";
            //}
            //valsString += "</tr></thead>";


            //valsString += "<tbody><tr>";



            //for (int i = 1; i <= 8; i++)
            //{
            //    var val = vals[i - 1];

            //    String valString;
            //    if (val.Value == null)
            //        valString = "null";
            //    else
            //        valString = val.ToString();

            //    valsString += "<td>" + valString + "</td>";

            //}

            //foreach (var val in vals)
            //{
            //    String valString;
            //    if (val.Value == null)
            //        valString = "null";
            //    else
            //        valString = val.ToString();

            //    valsString += "<td>" + valString + "</td>";

            //    //valsString += valString + ",";
            //}
            ////valsString = valsString.TrimEnd(",".ToCharArray());

            //valsString += "</tr></tbody>";


            //valsString += "</table>";

            //valsString += "}";

            return valsString;


        }



        public bool ReadValue()
        {


            var comm = BacnetObject.BacnetDevice.BacnetNetwork.BacnetClient;
            var adr = BacnetObject.BacnetDevice.BacnetAddress;
            var object_id = BacnetObject.BacnetObjectId;
            var property_id = BacnetPropertyId;
            var array_index = arrayIndex;


            BacnetPropertyValue new_entry = new BacnetPropertyValue();
            new_entry.property = new BacnetPropertyReference((uint)property_id); //, array_index);   //don't need this.  Already have.
            IList<BacnetValue> value;
            try
            {
                if (!comm.ReadPropertyRequest(adr, object_id, property_id, out value)) //, 0, array_index))      //TODO: do we ever need array index?
                    return false;     //ignore
            }
            catch
            {
                return false;         //ignore
            }
            new_entry.value = value;


            this.BacnetPropertyValue = new_entry;


            //if (value.Count == 1)
            //    this.BacnetValue = value[0];    //this should never be called on object groups or structured views, so will only contain one value....
            //else
            //    this.BacnetValues = value;  //otherwise leave value as Null, since we can't handle multiples here...

            //this.BacnetValues = value;      //will this list always only contain one?  What about possible properties?

            //new_entry.value = value;
            //values.Add(new_entry);
            return true;
        }





    }
}
