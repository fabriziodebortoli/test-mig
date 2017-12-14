using System.Collections.Generic;
using System;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder.Refactor
{
    //=========================================================================
    public class Changes : List<VersionChanges>
    {
        string targetVersion;

        const string typeDbt = "DBT";
        const string typeGrid = "GRID";
        const string typeForm = "FORM";
        const string typeTabDlg = "TABDLG";
        const string typeTabber = "TABBER";
        const string typeTilePanelTab = "TILEPANELTAB";
        const string pfxGrid = "BE";
        const string pfxTile = "Tile";
        const string pfxTileGroup = "TileGroup";

        //-----------------------------------------------------------------
        public string TargetVersion
        {
            get
            {
                return targetVersion;
            }

            set
            {
                targetVersion = value;
            }
        }

        //-----------------------------------------------------------------
        public string GetNewNameOf(ChangeRequest request)
        {
            VersionChangesOwnerChange change = GetChange(request);
            return change == null ? request.OldToken : change.NewToken;
        }

        //-----------------------------------------------------------------
        public bool IsDeletedObject(ChangeRequest request)
        {
            VersionChangesOwnerChange change = GetChange(request);
            return change != null;
        }

        // il namespace c# non e' l'equivalente del c++ nei tipi!!!!
        //-----------------------------------------------------------------
        public string GetType(string nameSpace)
        {
            // il namespace c# non e' l'equivalente del c++ nei tipi!!!!
            int pos = nameSpace.IndexOf(".");
            return (pos >= 0 ? nameSpace.Left(pos).ToUpper() : string.Empty);
        }

        //-----------------------------------------------------------------
        public string GetLeaf(string nameSpace)
        {
            int pos = nameSpace.LastIndexOf(".");
            return (pos >= 0 ? nameSpace.Mid(pos+1) : string.Empty);
        }

        //-----------------------------------------------------------------
        public List<string> GetOldClassesNamesFor(INameSpace nameSpace, Version version)
        {
            List<string> classNames = new List<string>();

            List<VersionChangesOwnerChange> changes = GetChangesFor(nameSpace, version);
            if (changes == null)
                return classNames;

            foreach (VersionChangesOwnerChange change in changes)
            {
                string oldObjectName = change.action == Action.Delete ? change.objectName : change.OldToken;
                string type = GetType(oldObjectName);
                string prefix = null;

                switch (type)
                {
                    case typeDbt: prefix = NameSpaceObjectType.Dbt.ToString().ToUpper(); break;
                    case typeGrid: prefix = pfxGrid; break;
                    case typeTabDlg: prefix = pfxTileGroup; break;
                    case typeForm:
                        {
                            string parentType = GetType(change.objectName);
                            switch (parentType)
                            {
                                case typeTabDlg:
                                case typeTilePanelTab:
                                    prefix = pfxTile;
                                    break;
                                default: break;
                            }
                            break;
                        }
                    default: break;
                }

                if (!prefix.IsNullOrEmpty())
                    classNames.Add(prefix + GetLeaf(oldObjectName));

            }

            return classNames;
        }

        //-----------------------------------------------------------------
        private bool IsToApplyToVersion(Version oldVersion, string currentVersion)
        {
            Version vXML = new Version(currentVersion);

            //return String.IsNullOrEmpty(oldVersion)/*oldVersion == string.Empty*/ || String.Compare(oldVersion, currentVersion, true) <= 0; /*Convert.ToDouble(oldVersion) <= Convert.ToDouble(currentVersion)*/
            return vXML.CompareTo(oldVersion) > 0;
        }

        //-----------------------------------------------------------------
        //controllo se il cambiamento va applicato al documento
        private bool IsDocumentToProcess(VersionChangesOwnerChange change, INameSpace documentNamespace, bool exact = false)
        {
            if (!exact && string.Compare(change.Document, "*", true) == 0)
                return true; 
            return string.Compare(change.Document, documentNamespace.GetNameSpaceWithoutType(), true) == 0;
        }

        //-----------------------------------------------------------------
        private bool IsChangeRequested(VersionChangesOwnerChange change, ChangeRequest request)
        {
            NameSpace changeNameSpace = new NameSpace(change.objectName);

            //controllo se l'oggetto che sto esaminando è l'oggetto definito del cambiamento sul file ObjectName + ChangedObject
            if (string.Compare(changeNameSpace.FullNameSpace, request.ComponentNamespace.FullNameSpace, true) != 0)
                return false;

            if (request.Action == Action.Delete)
                return true;
            
            // non appartiene all'oggetto modificato
            if (
                !string.IsNullOrEmpty(change.ChangedObject) &&
                !string.IsNullOrEmpty(request.SerializedType) && 
                string.Compare(change.ChangedObject, request.SerializedType, true) != 0)
                return false;

            // corrisponde l'oldToken
            return string.Compare(change.OldToken, request.OldToken, true) == 0;
        }

        //-----------------------------------------------------------------
        private VersionChangesOwnerChange GetChange(ChangeRequest request)
        {
            foreach (VersionChanges vc in this)
            {
                foreach (VersionChangesOwner vco in vc.Items)
                {
                    // prendo in considerazione solo i cambiamenti che sul file hanno versione minore o uguale a quella dell'oggetto modificato
                    if (!IsToApplyToVersion(request.OldVersion, vco.version))
                        continue;

                    // scorro l'elenco dei cambiamenti che ho nell'array e li valuto
                    foreach (VersionChangesOwnerChange change in vco.Changes)
                    {
                        // azione e tipo di cambiamento sono i primi che controllo così non vado
                        // nemmeno a fare altre considerazioni
                        if (change.action != request.Action || change.changeOn != request.Subject)
                            continue;

                        // controllo se la modifica va apportata al documento che sto analizzando, oppure se va apportata a tutti i documenti
                        if (IsDocumentToProcess(change, request.DocumentNamespace) && IsChangeRequested(change, request))
                            return change;
                    }
                }

            }
            return null;
        }

        //-----------------------------------------------------------------
        private List<VersionChangesOwnerChange> GetChangesFor(INameSpace documentNameSpace, Version version)
        {
            List<VersionChangesOwnerChange> changes = new List<VersionChangesOwnerChange>();
            foreach (VersionChanges vc in this)
            {
                foreach (VersionChangesOwner vco in vc.Items)
                {
                    // prendo in considerazione solo i cambiamenti che sul file hanno versione minore o uguale a quella dell'oggetto modificato
                    if (!IsToApplyToVersion(version, vco.version))
                        continue;

                    // scorro l'elenco dei cambiamenti che ho nell'array e li valuto
                    foreach (VersionChangesOwnerChange change in vco.Changes)
                    {
                        // controllo se la modifica va apportata al documento che sto analizzando, oppure se va apportata a tutti i documenti
                        if (IsDocumentToProcess(change, documentNameSpace, true))
                            changes.Add(change);
                    }
                }
            }
            return changes;
        }
    }
}