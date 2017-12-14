using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PAT.Workflow.Runtime;
using System.ComponentModel;
using EAConnector.Activities.ActivitiesById;
using EAConnector.Activities.EATestActivity;

namespace EAConnector {
	[Category("EasyAttachments")]
	[Connection(typeof(EAConnection))]
	class ActivityProvider : IActivityProvider {
		public List<AdminActivity> RegisterActivities() {
			var list = new List<AdminActivity>();

			//Activities by Id
			list.Add(new AdminActivity(typeof(ApproveAttachmentById)));
			list.Add(new AdminActivity(typeof(RejectAttachmentById)));
			list.Add(new AdminActivity(typeof(GetDescriptionById)));
			list.Add(new AdminActivity(typeof(EasyAttachmentIsAlive)));

			return ListHelpers.GetOrderedObjects(list);
		}
	}
}
