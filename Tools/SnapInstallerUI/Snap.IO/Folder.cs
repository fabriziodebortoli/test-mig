using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    public class Folder : FileSystemElement, IFolder
    {
        public override bool Exists
        {
            get
            {
                return System.IO.Directory.Exists(this.FullName);
            }
        }

        public virtual IFolder ParentFolder
        {
            get
            {
                return new Folder(Path.GetDirectoryName(this.FullName));
            }
        }

        public virtual IFolder[] GetFolders(IFolder rootFolder, string searchPattern)
        {
            if (rootFolder != null)
            {
                return
                    new DirectoryInfo(Path.Combine(rootFolder.FullName, this.FullName))
                    .GetDirectories(searchPattern)
                    .Select(f => new Folder(f.FullName))
                    .ToArray();
            }
            else
            {
                return
                    new DirectoryInfo(this.FullName)
                    .GetDirectories(searchPattern, SearchOption.TopDirectoryOnly)
                    .Select(f => new Folder(f.FullName))
                    .ToArray();
            }
        }

        public virtual IFolder[] GetFolders(string searchPattern)
        {
            return GetFolders(null, searchPattern);
        }

        public virtual IFile[] GetFiles(IFolder rootFolder, string searchPattern)
        {
            if (rootFolder != null)
            {
                return
                    new DirectoryInfo(Path.Combine(rootFolder.FullName, this.FullName))
                    .GetFiles(searchPattern)
                    .Select(f => new File(f.FullName))
                    .ToArray();
            }
            else
            {
                return
                    new DirectoryInfo(this.FullName)
                    .GetFiles(searchPattern)
                    .Select(f => new File(f.FullName))
                    .ToArray();
            }
        }

        public virtual IFile[] GetFiles(string searchPattern)
        {
            return GetFiles(null, searchPattern);
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
                if (System.IO.Directory.Exists(workingPath))
                {
                    System.IO.Directory.Delete(workingPath, true);
                }
            }
            if (Exists)
            {
                System.IO.Directory.Delete(this.FullName, true);
            }
        }

        public virtual void Create()
        {
            Directory.CreateDirectory(this.FullName);
        }

        public virtual bool IsEmpty(IFolder rootFolder)
        {
            return GetFiles(rootFolder, "*").Count() == 0 && GetFolders(rootFolder, "*").Count() == 0;
        }
        public virtual bool IsEmpty()
        {
            return IsEmpty(null);
        }

        public Folder()
            : base(string.Empty)
        {
            //needed for serialization
        }

        public Folder(string fullName)
            : base (fullName)
        {
            if (fullName != null && !string.IsNullOrWhiteSpace(fullName))
            {
                this.SetName(Path.GetFileName(fullName));
            }
        }

        public virtual void CopyTo(IFolder destination)
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

            Alphaleonis.Win32.Filesystem.Directory.Copy(this.FullName, destination.FullName, true);
        }
    }
}
