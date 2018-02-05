using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.EasyBuilder.Properties;

namespace Microarea.EasyBuilder.ComponentModel
{
	//================================================================================
	internal class TemplatesServiceEventArgs : EventArgs
    {
        EasyStudioTemplate template;
        //--------------------------------------------------------------------------------
        internal TemplatesServiceEventArgs(EasyStudioTemplate template)
        {
            this.template = template;
        }
    }


    //================================================================================
    internal class TemplatesService
    {
        List<EasyStudioTemplate> templates;
        static string defaultName = "Template";
        public event EventHandler<TemplatesServiceEventArgs> TemplateAdded;
        public event EventHandler<TemplatesServiceEventArgs> RemovingTemplate;
 
        //--------------------------------------------------------------------------------
        public EasyStudioTemplate this[string name]
        {
            get
            {
                foreach (EasyStudioTemplate template in templates)
                {
                    if (template.Name == name)
                        return template;
                }
                return null;
            }
        }

        //--------------------------------------------------------------------------------
        internal TemplatesService()
        {
            templates = new List<EasyStudioTemplate>();
            LoadTemplates();
        }

        //--------------------------------------------------------------------------------
        internal List<EasyStudioTemplate> Templates
        {
            get { return templates; }
        }

        //--------------------------------------------------------------------------------
        internal bool GenerateTemplate(WindowWrapperContainer container, bool inCustom)
        {
            string name = string.Concat(defaultName, (templates.Count + 1).ToString());
            EasyStudioTemplate template = new EasyStudioTemplate(container, name);
            bool bSaved = SaveTemplate(template, inCustom);
            if (bSaved)
            {
                AddTemplate(template);
				MessageBox.Show(string.Format(Resources.SaveTemplateMessage, template.Name), Resources.SaveTemplateCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
			else
				MessageBox.Show(Resources.SaveTemplateError, Resources.SaveTemplateCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

			return bSaved;
        }

        //--------------------------------------------------------------------------------
        internal bool DeleteTemplate(string name)
        {
            EasyStudioTemplate template = this[name];
            if (template != null  && template.Delete())
            {
                RemoveTemplate(template);
                return true;
            }
            return false;
        }

        //--------------------------------------------------------------------------------
        internal void LoadTemplates()
        {
            templates.Clear();
            // standard
            string path = PathFinderWrapper.GetTemplatesPath(false, false);
            LoadTemplates(path);
            // custom
            path = PathFinderWrapper.GetTemplatesPath(true, false);
            LoadTemplates(path);
        }

        //--------------------------------------------------------------------------------
        private void LoadTemplates(string path)
        {
            string searchKey = "*" + NameSolverStrings.JsonExtension;
            try
            {
                if (PathFinderWrapper.ExistFolder(path))
                    foreach (string file in PathFinderWrapper.GetFiles(path, searchKey))
                    {
                        EasyStudioTemplate template = new EasyStudioTemplate();
                        if (template.LoadFrom(file))
                            templates.Add(template);
                    }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return;
            }
        }

        //--------------------------------------------------------------------------------
        internal bool SaveTemplate(EasyStudioTemplate template, bool inCustom)
        {
            return template.Save(inCustom);
        }

        //--------------------------------------------------------------------------------
        internal void AddTemplate(EasyStudioTemplate template)
        {
            templates.Add(template);
            OnTemplateAdded(template);
        }

        //--------------------------------------------------------------------------------
        internal void OnTemplateAdded(EasyStudioTemplate template)
        {
            if (TemplateAdded != null)
                TemplateAdded(this, new TemplatesServiceEventArgs(template));
        }

        //--------------------------------------------------------------------------------
        internal void RemoveTemplate(EasyStudioTemplate template)
        {
            OnRemovingTemplate(template);
            templates.Remove(template);
        }

        //--------------------------------------------------------------------------------
        internal void OnRemovingTemplate(EasyStudioTemplate template)
        {
            if (RemovingTemplate != null)
                RemovingTemplate(this, new TemplatesServiceEventArgs(template));
        }

        //-----------------------------------------------------------------------------
        internal void RenameTemplate(string oldName, string newName)
        {
            EasyStudioTemplate template = this[oldName];
            if (template != null)
                template.Rename(oldName, newName);
        } 
    }
}