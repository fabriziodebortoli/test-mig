using Microarea.Common.NameSolver;
using System;
using System.Linq;
using System.Reflection;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model;
namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class Controller : IController
    {
        public event EventHandler PathFinderChanged;
        PathFinder pathFinder;

        //---------------------------------------------------------------
        virtual public string Name
        {
            get
            {
                var nameAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() as NameAttribute;
                return nameAttribute == null ? null : nameAttribute.Name;
            }
        }

        //---------------------------------------------------------------
        virtual public string Description
        {
            get
            {
                var desAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute;
                return desAttribute == null ? string.Empty : desAttribute.Description;
            }
        }

        //---------------------------------------------------------------
        public PathFinder PathFinder { get => pathFinder; set { pathFinder = value; PathFinderChanged?.Invoke(this, EventArgs.Empty); } }
    }
}
