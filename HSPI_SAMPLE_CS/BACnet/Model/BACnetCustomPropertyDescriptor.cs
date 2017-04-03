using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.BACnet;
using System.Globalization;

using System.Drawing.Design;


namespace HSPI_SIID.BACnet.Model
{
    /// <summary>
    /// Custom PropertyDescriptor
    /// </summary>
    /// 
    public class BACnetCustomPropertyDescriptor : PropertyDescriptor
    {
        CustomProperty m_Property;

        static BACnetCustomPropertyDescriptor()
        {
            TypeDescriptor.AddAttributes(typeof(BacnetDeviceObjectPropertyReference), new TypeConverterAttribute(typeof(BacnetDeviceObjectPropertyReferenceConverter)));
            TypeDescriptor.AddAttributes(typeof(BacnetObjectId), new TypeConverterAttribute(typeof(BacnetObjectIdentifierConverter)));
            TypeDescriptor.AddAttributes(typeof(BacnetBitString), new TypeConverterAttribute(typeof(BacnetBitStringConverter)));
        }

        public BACnetCustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs)
            : base(myProperty.Name, attrs)
        {
            m_Property = myProperty;
        }

        public CustomProperty CustomProperty
        {
            get { return m_Property; }
        }

        #region PropertyDescriptor specific

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get
            {
                return null;
            }
        }

        public override object GetValue(object component)
        {
            return m_Property.Value;
        }

        public override string Description
        {
            get
            {
                return m_Property.Description;
            }
        }

        public override string Category
        {
            get
            {
                return m_Property.Category;
            }
        }

        public override string DisplayName
        {
            get
            {
                return m_Property.Name;
            }

        }

        public override bool IsReadOnly
        {
            get
            {
                return m_Property.ReadOnly;
            }
        }

        public override void ResetValue(object component)
        {
            m_Property.Reset();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            m_Property.Value = value;
        }

        public override Type PropertyType
        {
            get { return m_Property.Type; }
        }

        public override TypeConverter Converter
        {
            get
            {
                if (m_Property.Options != null) return new DynamicEnumConverter(m_Property.Options);
                else if (m_Property.Type == typeof(float)) return new CustomSingleConverter();
                else if (m_Property.bacnetApplicationTags == BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME) return new BacnetTimeConverter();

                // A lot of classic Bacnet Enum
                BacnetPropertyReference bpr = (BacnetPropertyReference)m_Property.Tag;
                switch ((BacnetPropertyIds)bpr.propertyIdentifier)
                {
                    case BacnetPropertyIds.PROP_OBJECT_TYPE:
                        return new BacnetEnumValueConverter(new BacnetObjectTypes());
                    case BacnetPropertyIds.PROP_NOTIFY_TYPE:
                        return new BacnetEnumValueConverter(new BacnetEventNotificationData.BacnetNotifyTypes());
                    case BacnetPropertyIds.PROP_EVENT_TYPE:
                        return new BacnetEnumValueConverter(new BacnetEventNotificationData.BacnetEventTypes());
                    case BacnetPropertyIds.PROP_EVENT_STATE:
                        return new BacnetEnumValueConverter(new BacnetEventNotificationData.BacnetEventStates());
                    case BacnetPropertyIds.PROP_POLARITY:
                        return new BacnetEnumValueConverter(new BacnetPolarity());
                    case BacnetPropertyIds.PROP_UNITS:
                        return new BacnetEnumValueConverter(new BacnetUnitsId());
                    case BacnetPropertyIds.PROP_RELIABILITY:
                        return new BacnetEnumValueConverter(new BacnetReliability());
                    case BacnetPropertyIds.PROP_SEGMENTATION_SUPPORTED:
                        return new BacnetEnumValueConverter(new BacnetSegmentations());
                    case BacnetPropertyIds.PROP_SYSTEM_STATUS:
                        return new BacnetEnumValueConverter(new BacnetDeviceStatus());
                    case BacnetPropertyIds.PROP_LAST_RESTART_REASON:
                        return new BacnetEnumValueConverter(new BacnetRestartReason());
                    case BacnetPropertyIds.PROP_PRIORITY_FOR_WRITING:
                        return new BacnetEnumValueConverter(new BacnetWritePriority());
                    case BacnetPropertyIds.PROP_PRIORITY_ARRAY:
                        return new BacnetArrayIndexConverter(new BacnetWritePriority());
                    case BacnetPropertyIds.PROP_PRIORITY:
                        return new BacnetArrayIndexConverter(new BacnetArrayIndexConverter.BacnetEventStates());
                    case BacnetPropertyIds.PROP_EVENT_TIME_STAMPS:
                        return new BacnetArrayIndexConverter(new BacnetArrayIndexConverter.BacnetEventStates());
                    case BacnetPropertyIds.PROP_STATE_TEXT:
                        return new BacnetArrayIndexConverter(null);
                    case BacnetPropertyIds.PROP_PROGRAM_CHANGE:
                        return new BacnetEnumValueConverter(new BacnetProgramChange());
                    case BacnetPropertyIds.PROP_PROGRAM_STATE:
                        return new BacnetEnumValueConverter(new BacnetProgramState());
                    case BacnetPropertyIds.PROP_REASON_FOR_HALT:
                        return new BacnetEnumValueConverter(new BacnetReasonForHalt());
                    case BacnetPropertyIds.PROP_BACKUP_AND_RESTORE_STATE:
                        return new BacnetEnumValueConverter(new BACnetBackupState());
                    default:
                        return base.Converter;
                }
            }
        }

        // Give a way to display/modify some specifics values in a ListBox
        public override object GetEditor(Type editorBaseType = null)            //shouldn't need to specify the type...      
        {
            if (editorBaseType == null)
                editorBaseType = typeof(System.String);

            // All Bacnet Time as this
            if (m_Property.bacnetApplicationTags == BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME) return new BacnetTimePickerEditor();

            BacnetPropertyReference bpr = (BacnetPropertyReference)m_Property.Tag;

            // A lot of classic Bacnet Enum & BitString
            switch ((BacnetPropertyIds)bpr.propertyIdentifier)
            {
                case BacnetPropertyIds.PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED:
                    return new BacnetBitStringToEnumListDisplay(new BacnetObjectTypes(), true);
                case BacnetPropertyIds.PROP_PROTOCOL_SERVICES_SUPPORTED:
                    return new BacnetBitStringToEnumListDisplay(new BacnetServicesSupported(), true);
                case BacnetPropertyIds.PROP_STATUS_FLAGS:
                    return new BacnetBitStringToEnumListDisplay(new BacnetStatusFlags(), false, true);
                case BacnetPropertyIds.PROP_LIMIT_ENABLE:
                    return new BacnetBitStringToEnumListDisplay(new BacnetEventNotificationData.BacnetLimitEnable(), false, true);

                case BacnetPropertyIds.PROP_EVENT_ENABLE:
                case BacnetPropertyIds.PROP_ACK_REQUIRED:
                case BacnetPropertyIds.PROP_ACKED_TRANSITIONS:
                    return new BacnetBitStringToEnumListDisplay(new BacnetEventNotificationData.BacnetEventEnable(), false, true);

                case BacnetPropertyIds.PROP_OBJECT_TYPE:
                    return new BacnetEnumValueDisplay(new BacnetObjectTypes());
                case BacnetPropertyIds.PROP_NOTIFY_TYPE:
                    return new BacnetEnumValueDisplay(new BacnetEventNotificationData.BacnetNotifyTypes());
                case BacnetPropertyIds.PROP_EVENT_TYPE:
                    return new BacnetEnumValueDisplay(new BacnetEventNotificationData.BacnetEventTypes());
                case BacnetPropertyIds.PROP_EVENT_STATE:
                    return new BacnetEnumValueDisplay(new BacnetEventNotificationData.BacnetEventStates());
                case BacnetPropertyIds.PROP_POLARITY:
                    return new BacnetEnumValueDisplay(new BacnetPolarity());
                case BacnetPropertyIds.PROP_UNITS:
                    return new BacnetEnumValueDisplay(new BacnetUnitsId());
                case BacnetPropertyIds.PROP_RELIABILITY:
                    return new BacnetEnumValueDisplay(new BacnetReliability());
                case BacnetPropertyIds.PROP_SEGMENTATION_SUPPORTED:
                    return new BacnetEnumValueDisplay(new BacnetSegmentations());
                case BacnetPropertyIds.PROP_SYSTEM_STATUS:
                    return new BacnetEnumValueDisplay(new BacnetDeviceStatus());
                case BacnetPropertyIds.PROP_LAST_RESTART_REASON:
                    return new BacnetEnumValueDisplay(new BacnetRestartReason());
                case BacnetPropertyIds.PROP_PRIORITY_FOR_WRITING:
                    return new BacnetEnumValueDisplay(new BacnetWritePriority());
                case BacnetPropertyIds.PROP_PROGRAM_CHANGE:
                    return new BacnetEnumValueDisplay(new BacnetProgramChange());
                case BacnetPropertyIds.PROP_PRIORITY_ARRAY:
                    return new BacnetEditPriorityArray();
                case BacnetPropertyIds.PROP_BACKUP_AND_RESTORE_STATE:
                    return new BacnetEnumValueDisplay(new BACnetBackupState());

                case BacnetPropertyIds.PROP_PRESENT_VALUE:      //will only get here for enumerated binary value pres. value property
                    return new BacnetEnumValueDisplay(new BacnetBinaryValues());


                default:
                    return base.GetEditor(editorBaseType);
            }
        }

        public DynamicEnum Options
        {
            get
            {
                return m_Property.Options;
            }
        }

        #endregion

    }


    public enum BacnetBinaryValues
    {
        INACTIVE = 0,
        ACTIVE = 1
    };

}
