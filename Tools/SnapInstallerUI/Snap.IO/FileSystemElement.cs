using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    public abstract class FileSystemElement : IFileSystemElement
    {
        string fullName;
        string name;
        bool isPathRooted;

        public abstract bool Exists
        {
            get;
        }

        public virtual bool IsPathRooted
        {
            get
            {
                return isPathRooted;
            }
        }

        public virtual string FullName
        {
            get
            {
                return fullName;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }
        protected void SetName(string value)
        {
            this.name = value;
        }

        protected FileSystemElement(string fullName)
        {
            this.fullName = fullName;
            this.isPathRooted = Path.IsPathRooted(this.fullName);
        }
    }
}
