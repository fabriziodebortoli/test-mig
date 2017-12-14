using System;

namespace Microarea.Library.TranslationManager
{
	//-------------------------------------------------------------------------
	public abstract class BaseTranslator
	{
		//---------------------------------------------------------------------------
		protected TranslationManager	transManager = null;
		protected LookUpFileType		defaultLookUpType;
		
		//---------------------------------------------------------------------------
		public abstract	void	Run		(TranslationManager tManager);
		public delegate	void	Finish();
		public virtual	event	Finish	OnFinish;
		public delegate	void	ShowProgressMessage(string msg);
		public virtual	event	ShowProgressMessage OnShowProgressMessage;
		public delegate	void	SetError(string message, string owner);
		public virtual	event	SetError OnSetError;

		//---------------------------------------------------------------------------
		protected void EndRun(bool save)
		{
			if (save)
			{
				transManager.SaveLookUpFile(defaultLookUpType);
				transManager.SaveLookUpFile(LookUpFileType.Glossary);
			}

			if (OnFinish != null)
			{
				OnFinish();
			}
		}

		//---------------------------------------------------------------------------
		protected void SetProgressMessage(string msg, params object[] args)
		{
			SetProgressMessage(string.Format(msg, args));
		}

		//---------------------------------------------------------------------------
		protected void SetProgressMessage(string msg)
		{
			if (OnShowProgressMessage != null)
			{
				OnShowProgressMessage(msg);
			}
		}

		//---------------------------------------------------------------------------
		protected void SetLogError(string message, string owner)
		{
			if (OnSetError != null)
			{
				OnSetError(message, owner);
			}
		}
	}
}
