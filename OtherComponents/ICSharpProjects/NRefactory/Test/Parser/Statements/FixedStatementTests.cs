﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.IO;
using NUnit.Framework;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Ast;

namespace ICSharpCode.NRefactory.Tests.Ast
{
	[TestFixture]
	public class FixedStatementTests
	{
		#region C#
		[Test]
		public void CSharpFixedStatementTest()
		{
			FixedStatement fixedStmt = ParseUtilCSharp.ParseStatement<FixedStatement>("fixed (int* ptr = &myIntArr) { }");
			// TODO : Extend test.
		}
		#endregion
		
		#region VB.NET
			// No VB.NET representation
		#endregion	
	}
}
