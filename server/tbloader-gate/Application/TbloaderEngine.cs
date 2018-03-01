using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading;

namespace Microarea.TbLoaderGate
{
    public class TBLoaderEngine
    {
        static List<TBLoaderInstance> tbloaders = new List<TBLoaderInstance>();
        static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        //-----------------------------------------------------------------------------------------
        private static TBLoaderInstance GetTbLoader(string name)
        {
           using (Locker l = new Locker(rwLock, false))

            for (int i = tbloaders.Count - 1; i >= 0; i--)
            {
                TBLoaderInstance item = tbloaders[i];
                if (item == null)
                    continue;

        

                if (item.Name == name)
                    return item;
            }
            return null;
        }

        //-----------------------------------------------------------------------------------------
        internal static TBLoaderInstance GetTbLoader(string server, int port, string name, out bool newInstance)
        {
            newInstance = false;
            var tbLoader = GetTbLoader(name);
            if (tbLoader != null)
            {
                if (!tbLoader.IsProcessRunning())
                {
                    RemoveTbLoader(tbLoader);
                    tbLoader = null;
                }
            }
            if (tbLoader == null)
            {
                using (Locker l = new Locker(rwLock, true))
                {
                    tbLoader = GetTbLoader(name);//ci riprovo, quqlche altro thread potrebbe averlo creato nel frattempo
                    if (tbLoader == null)
                    {
                        tbLoader = new TBLoaderInstance(server, port, name);
                        tbLoader.ExecuteAsync().Wait();
                        tbloaders.Add(tbLoader);
                    }
                }
                newInstance = true;
            }
            return tbLoader;
        }

        //-----------------------------------------------------------------------------------------
        internal static void RemoveTbLoader(string name)
        {
            var tbLoader = GetTbLoader(name);
            if (tbLoader == null)
                return;
            RemoveTbLoader(tbLoader);
        }
        //-----------------------------------------------------------------------------------------
        internal static void RemoveTbLoader(TBLoaderInstance tbLoader)
        {
            using (Locker l = new Locker(rwLock, true))
            {
                tbloaders.Remove(tbLoader);
            }
        }
    }

    class Locker : IDisposable
    {
        ReaderWriterLockSlim rwLock;
        bool forWrite;
        bool locked = false;
        public Locker(ReaderWriterLockSlim rwLock, bool forWrite)
        {
            this.rwLock = rwLock;
            this.forWrite = forWrite;
            if (forWrite)
            {
                rwLock.EnterWriteLock();
                locked = true;
            }
            else
            {
                if (!rwLock.IsWriteLockHeld)
                {
                    rwLock.EnterReadLock();
                    locked = true;
                }
            }
        }
        public void Dispose()
        {
            if (!locked)
                return;

            if (forWrite)
                rwLock.ExitWriteLock();
            else
                rwLock.ExitReadLock();
        }
    }
}