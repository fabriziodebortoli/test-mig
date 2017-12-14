using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    public class File : FileSystemElement, IFile
    {
        public override bool Exists
        {
            get
            {
                return System.IO.File.Exists(this.FullName);
            }
        }

        public File(string fullName)
           : base(fullName)
        {
            if (fullName != null && !string.IsNullOrWhiteSpace(fullName))
            {
                this.SetName(Path.GetFileName(fullName));
            }
        }

        public virtual IFolder ParentFolder
        {
            get
            {
                return new Folder(Path.GetDirectoryName(this.FullName));
            }
        }

        public virtual Stream OpenRead()
        {
            return OpenRead(null);
        }

        public virtual Stream OpenWrite()
        {
            return OpenWrite(null);
        }

        public virtual Stream OpenRead(IFolder rootFolder)
        {
            var path = this.FullName;
            if (rootFolder != null)
            {
                path = Path.Combine(rootFolder.FullName, path);
            }
            return System.IO.File.OpenRead(path);
        }

        public virtual Stream OpenWrite(IFolder rootFolder)
        {
            var path = this.FullName;
            if (rootFolder != null)
            {
                path = Path.Combine(rootFolder.FullName, path);
                var fileInfo = new FileInfo(path);
                DirectoryInfo parentDirInfo = null;
                if (!(parentDirInfo = fileInfo.Directory).Exists)
                {
                    parentDirInfo.Create();
                }
            }
            else
            {
                var parentFolder = this.ParentFolder;
                if (!parentFolder.Exists)
                {
                    parentFolder.Create();
                }
            }

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            return System.IO.File.OpenWrite(path);
        }

        public virtual void Delete()
        {
            Delete(null);
        }

        public virtual void Delete(IFolder rootFolder)
        {
            if (!IsPathRooted && rootFolder != null)
            {
                var workingPath = Path.Combine(rootFolder.FullName, this.FullName);
                if (System.IO.File.Exists(workingPath))
                {
                    System.IO.File.Delete(workingPath);
                }
            }
            if (Exists)
            {
                System.IO.File.Delete(this.FullName);
            }
        }

        public virtual void CopyTo(IFile destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (string.IsNullOrWhiteSpace(destination.FullName))
            {
                throw new ArgumentException("destination is empty");
            }
            if (string.CompareOrdinal(this.FullName, destination.FullName) == 0)
            {
                return;
            }
            
            System.IO.File.Copy(this.FullName, destination.FullName);
        }
    }
}
