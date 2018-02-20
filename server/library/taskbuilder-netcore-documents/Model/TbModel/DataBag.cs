using Microarea.Common.CoreTypes;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces.Model;

namespace TaskBuilderNetCore.Documents.Model.TbModel
{
    //=========================================================================
    public class DataObjBag : IDataBag
    {
        object owner;
        DataObj dataObj;

        //---------------------------------------------------------------------
        public object Owner { get => owner; set => owner = value; }
        public DataObj Data { get => dataObj; set => dataObj = value; }

        //---------------------------------------------------------------------
        public DataObjBag(object owner)
        {
            this.owner = owner;
            string dataType = ObjectHelper.DataType(owner);
            this.dataObj = ObjectHelper.CreateObject(dataType) as DataObj;
        }


        //---------------------------------------------------------------------
        public IDataType DataType { get => dataObj.DataType; }
        //---------------------------------------------------------------------
        public bool IsReadOnly { get => dataObj.IsReadOnly; set => dataObj.IsReadOnly = value; }
        //---------------------------------------------------------------------
        public bool IsModified { get => dataObj.IsModified; set => dataObj.IsModified = value; }
        //---------------------------------------------------------------------
        public bool IsHidden { get => dataObj.IsHide; set => dataObj.IsHide = value; }
    }

    //====================================================================================    
    public static class DataBagExtensions
    {
        //-----------------------------------------------------------------------------------------------------
        private static IDataBag GetDataBag(this object owner, Interfaces.IComponent component)
        {
            Component theComponent = component as Component;
            IDataBag dataBag = theComponent.DataBags[owner];
            if (dataBag == null)
            {
                dataBag = CreateDataBagFor(owner);
                theComponent.DataBags[owner] = dataBag;
            }
            return dataBag;
        }

        //-----------------------------------------------------------------------------------------------------
        public static IDataBag CreateDataBagFor(object owner)
        {
            return new DataObjBag(owner);
        }

        //-----------------------------------------------------------------------------------------------------
        public static void SetReadOnly(this object owner, Interfaces.IComponent component, bool value)
        {
            IDataBag dataBag = GetDataBag(owner, component);
            if (dataBag != null)
                dataBag.IsReadOnly = value;
        }

        //-----------------------------------------------------------------------------------------------------
        public static void SetHidden(this object owner, Interfaces.IComponent component, bool value)
        {
            IDataBag dataBag = GetDataBag(owner, component);
            if (dataBag != null)
                dataBag.IsHidden = value;
        }

        //-----------------------------------------------------------------------------------------------------
        public static IDataType GetDataType(this object owner, Interfaces.IComponent component)
        {
            DataObjBag dataBag = GetDataBag(owner, component) as DataObjBag;
            return (dataBag == null ? null : dataBag.DataType);
        }
    }

}
