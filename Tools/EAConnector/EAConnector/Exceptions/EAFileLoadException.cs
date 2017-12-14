using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EAConnector.Exceptions {
	class EAFileLoadException : Exception {
		public EAFileLoadException() { } 
		public EAFileLoadException(string message) : base(message) { }
		public EAFileLoadException(string message, Exception inner) : base(message, inner) { }
	}
}
