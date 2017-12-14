using System;

namespace Microarea.Library.Internet.BitsWrap
{

    public class JobCollection : System.Collections.CollectionBase
	{
        //Typed Collection for Jobs

        public Job this[int index]
		{
            get
			{
                //return CType(this.List[index], Job);
				return (Job)this.List[index];
			}
            set
			{
                this.List[index] = value;
            }
        }

        protected internal int Add(Job job)
		{
            return this.List.Add(job);
        }

        protected internal void Insert(int index, Job job)
		{
            this.List.Insert(index, job);
        }

        private int IndexOf(Job job)
		{
            return this.List.IndexOf(job);
        }

        private bool Contains(Job job)
		{
            return this.List.Contains(job);
        }

        private void Remove(Job job)
		{
            this.List.Remove(job);
        }

        public void CopyTo(Job[] array, int index)
		{
            this.List.CopyTo(array, index);
        }

    }


    public class FileCollection : System.Collections.CollectionBase
	{
        //Typed Collection for File objects

        internal FileCollection(){}

        public BITSFile this[int index]
		{
            get
			{
                //return CType(this.List[index], BITSFile);
				return (BITSFile)this.List[index];
			}
            set
			{
                this.List[index] = value;
            }
        }

        protected internal int Add(BITSFile v)
		{
            return this.List.Add(v);
        }

        protected internal void Insert(int index, BITSFile v)
		{
            this.List.Insert(index, v);
        }

        private int IndexOf(BITSFile v)
		{
            return this.List.IndexOf(v);
        }

        private bool Contains(BITSFile v)
		{
            return this.List.Contains(v);
        }

        private void Remove(BITSFile v)
		{
            this.List.Remove(v);
        }

        public void CopyTo(BITSFile[] array, int index)
		{
            this.List.CopyTo(array, index);
        }
    }

}