using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// PathShrinker was designed for enabling displaying paths fitting
	/// the size of a System.Windows.Form.Control by replacing deeper
	/// directories names with a three dots (de facto standard behaviour).
	/// It is able to treat both file and directory names.
	/// </summary>
	/// <remarks>
	/// Federico:
	/// I've higly optimized performances of class methods for both speed
	/// and memory consumption. The final result is so quite satisfying as
	/// the perf impact is unnoticeble even with heavily repeated tests
	/// with large amounts of file names.
	/// Approximations always tend to approximate by defect in order not
	/// to exceed the control width.
	/// Tested controls are TextBox and Label, but should work with any.
	/// Implenting it I've used the disposable pattern in order to invite
	/// programmers to explicitely realese with a well known way unmanaged
	/// resources (which the class doesn't use directly, but uses
	/// System.Drawing.Graphics which uses Gdi+).
	/// </remarks>
	/// <example>
	///		public class PathWalker : System.Windows.Forms.Form
	///		{
	///			private PathShrinker pathShrinker = new PathShrinker();
	///			private Control outputControl;
	///			
	///			...
	///
	///			private void Display(string path)
	///			{
	///				string toDisplay = pathShrinker.Shrink(path, outputControl);
	///				outputControl.Text = toDisplay;
	///			}
	///			protected override void OnClosing(CancelEventArgs e)
	///			{
	///				if (pathShrinker != null)
	///					pathShrinker.Dispose();
	///				base.OnClosing(e);
	///			}
	///		}
	/// </example>
	public class PathShrinker : IDisposable
	{
		// NOTE - Unit tests added in tests solution

		private Graphics g;
		private readonly string pathSegment;
		private readonly string dscString;

		//---------------------------------------------------------------------
		public PathShrinker()
		{
			using (Control c = new Control())
			{
				g = c.CreateGraphics();
			}
			pathSegment = Path.DirectorySeparatorChar + "..." + Path.DirectorySeparatorChar;
			dscString = Path.DirectorySeparatorChar.ToString();
		}

		/// <summary>
		/// Returns a shrinked path string fitting the given control.
		/// </summary>
		/// <param name="path">the path to shrink</param>
		/// <param name="control">the control</param>
		/// <returns>the shrinked string (or the original if gives up)</returns>
		//---------------------------------------------------------------------
		public string Shrink(string path, Control control)
		{
			return Shrink(path, control.Font, control.Width);
		}

		//---------------------------------------------------------------------
		protected string Shrink(string path, Font font, int maxWidth)
		{
			int txtWidth = MeasureString(path, font);
			if (txtWidth > maxWidth)
			{
				int neededLength = CalculateNeededLength(path.Length, txtWidth, maxWidth);
				return PerformShrinking(path, neededLength);
			}
			return path;
		}

		//---------------------------------------------------------------------
		protected int MeasureString(string path, Font font)
		{
			// Convert.ToInt32() would be more precise, but
			// we can accept a 1pixel approximation.
			// This way is a tiny pweeny little bit quicker
			lock (g)
				return (int)(g.MeasureString(path, font).Width);
		}

		//---------------------------------------------------------------------
		protected int CalculateNeededLength(int pathLength, int txtWidth, int ctrlWidth)
		{
			if (txtWidth <= ctrlWidth)
				return 0;
			// Uses a multiplier to avoid using float.
			// Being the multiplier 2^x, multiplication
			// and division are very quick
			// The higher is x, the more precise the result is
			return (pathLength * ((4096 * ctrlWidth) / txtWidth)) / 4096;
		}

		//---------------------------------------------------------------------
		protected string PerformShrinking(string path, int neededLength)
		{
			if (neededLength < 0)
				throw new ArgumentException();
			if (neededLength == path.Length)
				return path;

			string[] tokens = path.Split(Path.DirectorySeparatorChar);
			if (tokens.Length <= 2)
				return path;

			bool isDir = tokens[tokens.Length - 1].Length == 0;	// there's a trailing DSP (if missing it's treated as a file)
			int reachedLength = path.Length;
			int fileTokenIndex = tokens.Length - 1;
			if (isDir)
				--fileTokenIndex;

			int startingToken = fileTokenIndex - 1;
			for (int i = startingToken; i >= 0; i--)
			{
				if (i == startingToken)
					reachedLength -= (tokens[i].Length - 3); // takes last dir off and adds "..."
				else
					reachedLength -= (tokens[i].Length + 1); // takes current dir and a DSC off
				if (reachedLength <= neededLength)
				{
					StringBuilder sb = new StringBuilder(reachedLength);
					sb.Append(string.Join(dscString, tokens, 0, i));
					sb.Append(pathSegment);
					sb.Append(tokens[fileTokenIndex]);	// adds file (o dir) name
					if (isDir)
						sb.Append(Path.DirectorySeparatorChar);
					return sb.ToString();
				}
			}

			return path; // if can't shrink enough, gives up, so at least it doesn't waste memory
		}

		#region IDisposable Members (and related cleanup and finalizer)

		//---------------------------------------------------------------------
		public void Dispose()
		{
			CleanUp();
			GC.SuppressFinalize(this);
		}
		
		//---------------------------------------------------------------------
		~PathShrinker()
		{
			CleanUp();
		}
		
		//---------------------------------------------------------------------
		private void CleanUp()
		{
			if (g != null)
				g.Dispose();
		}

		#endregion

		//---------------------------------------------------------------------
	}
}
