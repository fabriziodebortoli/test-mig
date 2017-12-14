using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EAConnector.Exceptions {
	public class EAConnectionException:Exception {
		public EAConnectionException() { }
		public EAConnectionException(string message) : base(message) { }
		public EAConnectionException(string message, Exception inner) : base(message, inner) { }
	}
}
