using System;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.Generic
{
    [Serializable]  
    public class BalloonDataBag
    {
       //---------------------------------------------------------------------
        public BalloonDataBag()
		{}

        //---------------------------------------------------------------------
        public BalloonDataBag(bool block, MessageType type, string id)
		{
            Block = block;
            Type = type;
            Id = id;
		}

        //---------------------------------------------------------------------
        public BalloonDataBag(string id)
        {
            Id = id;
        }

        public bool Block { get; set; }
        public MessageType Type { get; set; }
        public string Id { get; set; }
       
    }
}
