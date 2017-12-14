using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    public class TransactionalFolder : Folder
    {
        public TransactionalFolder()
            : this(string.Empty)
        {
            //needed for serialization
        }

        public override IFolder ParentFolder
        {
            get
            {
                return new TransactionalFolder(Path.GetDirectoryName(this.FullName));
            }
        }

        public TransactionalFolder(string fullName)
            : base (fullName)
        {
            if (fullName != null && !string.IsNullOrWhiteSpace(fullName))
            {
                this.SetName(Path.GetFileName(fullName));
            }
        }

        public override IFolder[] GetFolders(IFolder rootFolder, string searchPattern)
        {
            if (rootFolder != null)
            {
                var dir = new DirectoryInfo(Path.Combine(rootFolder.FullName, this.FullName));
                if (!dir.Exists)
                {
                    return new TransactionalFolder[] { };
                }
                var subDirs = dir.GetDirectories(searchPattern);
                if (subDirs == null || subDirs.Count() == 0)
                {
                    return new TransactionalFolder[] { };
                }
                return
                    subDirs
                    .Select(f => new TransactionalFolder(f.FullName))
                    .ToArray();
            }
            else
            {
                var dir = new DirectoryInfo(this.FullName);
                if (!dir.Exists)
                {
                    return new TransactionalFolder[] { };
                }
                var subDirs = dir.GetDirectories(searchPattern);
                if (subDirs == null || subDirs.Count() == 0)
                {
                    return new TransactionalFolder[] { };
                }
                return
                    subDirs
                    .Select(f => new TransactionalFolder(f.FullName))
                    .ToArray();
            }
        }

        public override IFile[] GetFiles(IFolder rootFolder, string searchPattern)
        {
            if (rootFolder != null)
            {
                var dir = new DirectoryInfo(Path.Combine(rootFolder.FullName, this.FullName));
                if (!dir.Exists)
                {
                    return new TransactionalFile[] { };
                }
                var subFiles = dir.GetFiles(searchPattern);
                if (subFiles == null || subFiles.Count() == 0)
                {
                    return new TransactionalFile[] { };
                }
                return
                    subFiles
                    .Select(f => new TransactionalFile(f.FullName))
                    .ToArray();
            }
            else
            {
                var dir = new DirectoryInfo(this.FullName);
                if (!dir.Exists)
                {
                    return new TransactionalFile[] { };
                }
                var subFiles = dir.GetFiles(searchPattern);
                if (subFiles == null || subFiles.Count() == 0)
                {
                    return new TransactionalFile[] { };
                }
                return
                    subFiles
                    .Select(f => new TransactionalFile(f.FullName))
                    .ToArray();
            }
        }

        public override void Delete(IFolder rootFolder)
        {
            this.WrapWithTransaction(
                (KernelTransaction transaction)
                =>
                {
                    if (!IsPathRooted && rootFolder != null)
                    {
                        var workingPath = Path.Combine(rootFolder.FullName, this.FullName);
                        if (Directory.Exists(workingPath))
                        {
                            DeleteRecursive(transaction, workingPath);
                        }
                    }
                    if (Exists)
                    {
                        DeleteRecursive(transaction, this.FullName);
                    }
                }
                );
        }

        private void DeleteRecursive(KernelTransaction transaction, string workingPath)
        {
            var subDirs = Directory.GetDirectoriesTransacted(transaction, workingPath);
            foreach (var subDir in subDirs)
            {
                DeleteRecursive(transaction, subDir);
            }
            var subFiles = Directory.GetFilesTransacted(transaction, workingPath, "*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var subFile in subFiles)
            {
                Alphaleonis.Win32.Filesystem.File.DeleteTransacted(transaction, subFile);
            }
            Directory.DeleteTransacted(transaction, workingPath);
        }

        public override void Create()
        {
            this.WrapWithTransaction(
                (KernelTransaction transaction)
                =>
                {
                    Directory.CreateDirectoryTransacted(transaction, this.FullName);
                }
                );
        }

        public override void CopyTo(IFolder destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (string.IsNullOrWhiteSpace(destination.FullName))
            {
                throw new ArgumentException("Invalid destination: '" + destination.FullName + "'");
            }

            if (string.CompareOrdinal(this.FullName, destination.FullName) == 0)
            {
                return;
            }

            this.WrapWithTransaction(
                (KernelTransaction transaction)
                =>
                {
                    Directory.CopyTransacted(transaction, this.FullName, destination.FullName, true);
                }
                );
        }
    }
}
