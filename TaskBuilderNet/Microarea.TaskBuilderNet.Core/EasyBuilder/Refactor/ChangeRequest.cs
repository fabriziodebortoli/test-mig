using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder.Refactor
{
    //=========================================================================
    /// <summary>
    /// 
    /// </summary>
    public class ChangeRequest
    {
        INameSpace componentNamespace;
        string serializedType;
        INameSpace documentNamespace;
        ChangeSubject subject;
        Action action;
        Version oldVersion;
        string oldToken;

        public ChangeSubject Subject
        {
            get
            {
                return subject;
            }

            set
            {
                subject = value;
            }
        }

        public Action Action
        {
            get
            {
                return action;
            }

            set
            {
                action = value;
            }
        }

        public string OldToken
        {
            get
            {
                return oldToken;
            }

            set
            {
                oldToken = value;
            }
        }

        public Version OldVersion
        {
            get
            {
                return oldVersion;
            }

            set
            {
                oldVersion = value;
            }
        }

        public INameSpace ComponentNamespace
        {
            get
            {
                return componentNamespace;
            }

            set
            {
                componentNamespace = value;
            }
        }

        public string SerializedType
        {
            get
            {
                return serializedType;
            }

            set
            {
                serializedType = value;
            }
        }

        public INameSpace DocumentNamespace
        {
            get
            {
                return documentNamespace;
            }

            set
            {
                documentNamespace = value;
            }
        }

        //-----------------------------------------------------------------
        public ChangeRequest(ChangeSubject subject, INameSpace componentNameSpace, INameSpace documentNameSpace, string serializedType = null, string oldToken = null, Version oldVersion = null)
        {
            Init(subject, componentNameSpace, documentNameSpace, serializedType, oldToken, oldVersion);
        }

        //-----------------------------------------------------------------
        public ChangeRequest(ChangeSubject subject, INameSpace componentNameSpace, EasyBuilderComponent component, string oldToken = null)
        {
            Init(subject, componentNameSpace, component.Document.Namespace, component.SerializedType, oldToken, component.GetType().Assembly.GetName().Version);
        }

        //-----------------------------------------------------------------
        private void Init (ChangeSubject subject, INameSpace componentNs, INameSpace docNameSpace, string serializedType, string oldToken, Version oldVersion)
        {
            this.ComponentNamespace = componentNs;
            this.DocumentNamespace = docNameSpace;
            this.SerializedType = serializedType;
            this.Subject = subject;
            this.OldToken = oldToken;
            if (oldVersion != null)
                this.OldVersion = new Version(oldVersion.Major, oldVersion.Minor,  oldVersion.Build);
        }
    }

    //=========================================================================
    /// <summary>
    /// Rename request to inspect refactor changes
    /// </summary>
    public class RenameChangeRequest : ChangeRequest
    {
        public RenameChangeRequest(ChangeSubject subject, INameSpace componentNameSpace, INameSpace documentNamespace, string serializedType, string oldToken, Version oldVersion = null)
            :
            base(subject, componentNameSpace, documentNamespace, serializedType,  oldToken, oldVersion)
        {
            this.Action = Action.Rename;
        }

        //-----------------------------------------------------------------
        public RenameChangeRequest(ChangeSubject subject, INameSpace componentNameSpace, EasyBuilderComponent component, string oldToken)
         :
         base(subject, componentNameSpace, component, oldToken)
        {
            this.Action = Action.Rename;
        }
    }

    //=========================================================================
    /// <summary>
    /// Delete request to inspect refactor changes
    /// </summary>
    public class DeleteChangeRequest : ChangeRequest
    {
        public DeleteChangeRequest(ChangeSubject subject, INameSpace componentNameSpace, INameSpace documentNamespace, Version oldVersion)
            :
            base(subject, componentNameSpace, documentNamespace,null, null, oldVersion)
        {
            this.Action = Action.Delete;
        }
    }
}