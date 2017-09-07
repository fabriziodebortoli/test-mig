using System.Globalization;

using Microarea.Common.Generic;
using Microarea.Common.Lexan;
using System;

namespace Microarea.RSWeb.WoormViewer
{
	/// <summary>
	/// Summary description for Properties.
	/// </summary>
	/// ================================================================================
	public class Properties
	{
		private string	title;
		private string	subject;
		private string	author;
		private string	company;
		private string	comments;	
		private string	defaultSecurityRoles;	

		public string	Title		{ get { return title; }}
		public string	Subject		{ get { return subject; }}
		public string	Author		{ get { return author; }}
		public string	Company		{ get { return company; }}
		public string	Comments	{ get { return comments; }}	
		public string	DefaultSecurityRoles	{ get { return defaultSecurityRoles; }}	
	
		//------------------------------------------------------------------------------
		public bool Parse(WoormParser lex) 
		{	
			title	= "";
			subject	= "";
			author	= "";
			company	= "";
			comments= "";
			defaultSecurityRoles= "";

			// non � una sezione obbligatoria
			if (!lex.Matched(Token.PROPERTIES)) 
				return true;
			if (!lex.ParseBegin()) 
				return false;

			for(;;)
			{
				switch(lex.LookAhead())
				{
					case Token.TITLE:
					{
						if	(
							!(
							lex.Matched(Token.TITLE) && lex.ParseString(out title) 
							))
							return false;
						break;
					}
					case Token.SUBJECT:
					{
						if	(
							!(
							lex.Matched(Token.SUBJECT) && lex.ParseCEdit(out subject) 
							))
							return false;
						break;
					}
					case Token.AUTHOR:
					{
						if	(
							!(
							lex.Matched(Token.AUTHOR) && lex.ParseString(out author) 
							))
							return false;
						break;
					}
					case Token.REPORTPRODUCER:
					{
						if	(
							!(
							lex.Matched(Token.REPORTPRODUCER) && lex.ParseString(out company) 
							))
							return false;
						break;
					}
					case Token.COMMENTS:
					{
						if	(
							!(
							lex.Matched(Token.COMMENTS) && lex.ParseCEdit(out comments) 
							))
							return false;
						break;
					}
					case Token.DEFAULTSECURITYROLES:
					{
						if	(
							!(
							lex.Matched(Token.DEFAULTSECURITYROLES) && lex.ParseString(out defaultSecurityRoles) 
							))
							return false;
						break;
					}
					case Token.END:
					{
						if (!lex.ParseEnd())
						{
							return false;
						}	
						return true;
						//break;
					}
					default:
					{
						string sCompany;
						if (!lex.ParseID(out sCompany))
							return false;
						if (string.Compare(sCompany, "Company", StringComparison.OrdinalIgnoreCase) != 0)
							return false;
						if (!lex.ParseString(out company))
							return false;
						break;
					}
				}//switch
			}//for
		}

		//------------------------------------------------------------------------------
		internal bool IsEmpty
		{
			get
			{
				return title.IsNullOrEmpty() &&
					subject.IsNullOrEmpty() &&
					author.IsNullOrEmpty() &&
					company.IsNullOrEmpty() &&
					comments.IsNullOrEmpty() &&
					defaultSecurityRoles.IsNullOrEmpty();
			}
		}

		//------------------------------------------------------------------------------
		internal bool Unparse(Unparser unparser)
		{
			unparser.WriteLine();
			
			unparser.WriteTag(Token.PROPERTIES, true);
			unparser.IncTab();
			
			unparser.WriteBegin();
			unparser.IncTab();

			unparser.WriteTag(Token.TITLE, false);
			unparser.WriteString(unparser.LoadReportString(title), true); 

			unparser.WriteTag(Token.SUBJECT, false);
			unparser.WriteCEdit(unparser.LoadReportString(subject), true);

			unparser.WriteTag(Token.AUTHOR, false);
			unparser.WriteString(author, true);
			unparser.WriteTag(Token.REPORTPRODUCER, false);
			unparser.WriteString(company, true);
			unparser.WriteTag(Token.COMMENTS, false);
			unparser.WriteCEdit(comments, true);
			unparser.WriteTag(Token.DEFAULTSECURITYROLES, false);
			unparser.WriteString(defaultSecurityRoles, true);

			unparser.DecTab();
			unparser.WriteEnd();

			unparser.DecTab();

			return true;
		}
	}
}
