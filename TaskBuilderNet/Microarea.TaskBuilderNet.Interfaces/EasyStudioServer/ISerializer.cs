using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Interfaces.EasyStudioServer
{
	//====================================================================
	public interface ISerializer
    {
		IBasePathFinder PathFinder { get; set; }
	}
}
