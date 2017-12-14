using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microarea.EasyBuilder
{
    //================================================================================
    /// <summary>
    /// Compilation results for EasyStudio
    /// </summary>
    public class EBCompilerResults
    {
        CompilerResults compilerResults;

        //-----------------------------------------------------------------------------
        /// <remarks />
        internal EBCompilerResults()
        {}


        //-----------------------------------------------------------------------------
        /// <remarks />
        internal EBCompilerResults(CompilerResults compilerResults)
        {
            this.compilerResults = compilerResults;
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Get the compiled assembly
        /// </summary>
        public virtual Assembly CompiledAssembly { get { return this.compilerResults.CompiledAssembly; } }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Get the errors collection for the compilation
        /// </summary>
        public virtual EBCompilerErrors Errors
        {
            get
            {
                var errors = new EBCompilerErrors();
                foreach (CompilerError error in this.compilerResults.Errors)
                {
                    errors.Add(new EBCompilerError(error));
                }

                return errors;
            }
        }
    }

    //================================================================================
    /// <summary>
    /// Compilation results from msbuild
    /// </summary>
    public class MsBuildCompilerResults : EBCompilerResults
    {
        const string compilationErrorPattern = "(?<errorRow>[\\\\\\.A-Za-z0-9]+\\([0-9]+,[0-9]+\\): error [A-Za-z0-9]+: .+)";
        readonly Regex compilationErrorRegex = new Regex(compilationErrorPattern);

        Assembly compiledAssembly;
        EBCompilerErrors errors = new EBCompilerErrors();

        //-----------------------------------------------------------------------------
        /// <remarks />
        internal MsBuildCompilerResults(string msBuildOutput, string outputPath)
        {
            var match = compilationErrorRegex.Match(msBuildOutput);

            while(match.Success)
            {
                Errors.Add(new EBCompilerError(match.Groups["errorRow"].Value));
                match = match.NextMatch();
            }

            if (!Errors.HasErrors && File.Exists(outputPath))
            {
                this.compiledAssembly = Assembly.LoadFile(outputPath);
            }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Get the compiled assembly
        /// </summary>
        public override Assembly CompiledAssembly
        {
            get
            {
                return this.compiledAssembly;
            }
        }

        //-----------------------------------------------------------------------------
        /// <summary>
        /// Get the errors collection for the compilation
        /// </summary>
        public override EBCompilerErrors Errors
        {
            get
            {
                return this.errors;
            }
        }
    }

    //================================================================================
    /// <summary>
    /// Extensions for EBCompilerResults
    /// </summary>
    public static class EBCompilerResultsFactory
    {
        //-----------------------------------------------------------------------------
        /// <remarks />
        public static EBCompilerResults FromCompilerResults(
            CompilerResults compilerResults
            )
        {
            return new EBCompilerResults(compilerResults);
        }
        //-----------------------------------------------------------------------------
        /// <remarks />
        public static EBCompilerResults FromMsBuildOutput(
            string msBuildOutput,
            string outputPath
            )
        {
            return new MsBuildCompilerResults(msBuildOutput, outputPath);
        }
    }

    //================================================================================
    /// <summary>
    /// Compilation error for EasyStudio
    /// </summary>
    public class EBCompilerError
    {
        int column;
        string errorNumber;
        string errorText;
        string fileName;
        bool isWarning;
        int line;

        //designer\ProspectiveSuppliers.DocumentView.cs(26,11): error CS1520: Method must have a return type [C:\Development\Custom\Companies\AllCompanies\Applications\ERP\Contacts\ModuleObjects\ProspectiveSuppliers\sa\src\ProspectiveSuppliers.csproj]
        const string pattern = "(?<fileName>.+)\\((?<line>[0-9]+),(?<column>[0-9]+)\\): error (?<errorNumber>[A-Za-z0-9]+):(?<errorText>.+)";
        readonly Regex regex = new Regex(pattern);


        //-----------------------------------------------------------------------------
        /// <remarks />
        public EBCompilerError(CompilerError compilerError)
        {
            this.column = compilerError.Column;
            this.errorNumber = compilerError.ErrorNumber;
            this.errorText = compilerError.ErrorText;
            this.fileName = compilerError.FileName;
            this.isWarning = compilerError.IsWarning;
            this.line = compilerError.Line;
        }

        //-----------------------------------------------------------------------------
        /// <remarks />
        public EBCompilerError(string error)
        {
            var match = regex.Match(error);

            if (match.Success)
            {
                this.fileName = match.Groups["fileName"].Value;
                this.errorNumber = match.Groups["errorNumber"].Value;
                this.errorText = match.Groups["errorText"].Value;
                Int32.TryParse(match.Groups["line"].Value, out this.line);
                Int32.TryParse(match.Groups["column"].Value, out this.column);
            }
        }

        //-----------------------------------------------------------------------------
        /// <remarks />
        public int Column { get { return this.column; } }
        //-----------------------------------------------------------------------------
        /// <remarks />
        public string ErrorNumber { get { return this.errorNumber; } }
        //-----------------------------------------------------------------------------
        /// <remarks />
        public string ErrorText { get { return this.errorText; } }
        //-----------------------------------------------------------------------------
        /// <remarks />
        public string FileName { get { return this.fileName; } }
        //-----------------------------------------------------------------------------
        /// <remarks />
        public bool IsWarning { get { return this.isWarning; } }
        //-----------------------------------------------------------------------------
        /// <remarks />
        public int Line { get { return this.line; } }

        //-----------------------------------------------------------------------------
        /// <remarks />
        public override string ToString()
        {
            return String.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}({1},{2}): error {3}: {4}",
                this.fileName,
                this.line,
                this.column,
                this.errorNumber,
                this.errorText
                );
        }
    }

    //================================================================================
    /// <summary>
    /// Compilation error collection for EasyStudio
    /// </summary>
    public class EBCompilerErrors : List<EBCompilerError>
    {
        //-----------------------------------------------------------------------------
        /// <summary>
        /// True if there are errors, otherwise false.
        /// </summary>
        public bool HasErrors { get { return this.Count > 0; } }
    }
}
