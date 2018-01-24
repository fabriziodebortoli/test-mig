using Microarea.Common.NameSolver;
using System;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Engine.Serializers
{
    //====================================================================
    public class SerializerEventArgs : EventArgs
    {
        INameSpace nameSpace;
        string fileName;
        string code;

        //---------------------------------------------------------------
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
            }
        }

        //---------------------------------------------------------------
        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        //---------------------------------------------------------------
        public SerializerEventArgs(INameSpace nameSpace)
        {
            this.nameSpace = nameSpace;
        }
    }

    //====================================================================
    public class Serializer : Component, ISerializer
    {
        PathFinder pathFinder;

        //---------------------------------------------------------------
        public PathFinder PathFinder
        {
            get
            {
                return pathFinder;
            }

            set
            {
                pathFinder = value;
            }
        }

        //---------------------------------------------------------------
        protected void EnsurePathFinder()
        {
            if (pathFinder == null)
                throw (new SerializerException(this, Strings.PathFinderUninitializated));
        }
    }
}
