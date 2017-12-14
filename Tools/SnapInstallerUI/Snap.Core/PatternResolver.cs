using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    //================================================================================
    /// <summary>
    /// Classe che risolve i pattern per le inclusioni/esclusioni di file e cartelle
    /// </summary>
    /// <remarks>
    /// Data una cartella che contiene file o folder, detta working set, e un insieme di 
    /// regole (inclusione o esclusione di file o folder con nomi specificati da pattern 
    /// tipo *.txt, pto*.dll ecc), il funzionamento è il seguente:
    /// Vengono applicate sull'insieme originale tutte le regole e vengono salvati,
    /// per ogni regola, il numero di file/ folder selezionati (detto cardinalità della regola).
    /// Le regole vengono poi ordinate in maniera decrescente a partire da quella che
    /// ha selezionato più file/folder fino a quella che ne ha selezionati di meno.
    /// Vengono dunque applciate nell'ordina calcolato scremando il working set fino ad
    /// avere l'insieme finale.
    /// In caso di stessa cardinalità abbiamo deciso che vince una regola di inclusione
    /// su una di esclusione.
    /// </remarks>
    public static class PatternResolver
    {
        private const string dot = ".";
        private const string pipe = "|";
        private const string star = "*";//l'asterisco di file system si traduce in regex con .*(con . che significa un qualunque carattere e * che significa: zero o più)
        private const string regExStar = ".*";
        private const string regExDot = "\\.";

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Ritorna un insieme di cartelle selezionate dal <code>workingSet</code>
        /// in base alle regole <code>wIncludeFolders</code> e <code>wExcludeFolders</code>
        /// </summary>
        public static IDictionary<string, IncludeFolder> ResolveFolders(
            IList<string> workingSet,
            IncludeFolder[] includeFolders,
            ExcludeFolder[] excludeFolders
            )
        {
            if (workingSet == null || workingSet.Count == 0)
                return new Dictionary<string, IncludeFolder>();

            List<RuleInfo> ruleInfos = new List<RuleInfo>();

            //Calcola la cardinalità per le regole IncludeFolder e salva l'array di cardinalità in ruleInfos
            if (includeFolders != null && includeFolders.Length > 0)
            {
                foreach (var includeFolder in includeFolders)
                {
                    CalculateRuleInfoForIncludeFolder(
                        workingSet,
                        ruleInfos,
                        includeFolder
                        );
                }
            }
            //Calcola la cardinalità per le regole ExcludeFolder e salva l'array di cardinalità in ruleInfos
            if (excludeFolders != null && excludeFolders.Length > 0)
            {
                foreach (var excludeFolder in excludeFolders)
                {
                    CalculateRuleInfoForExcludeFolder(
                        workingSet,
                        ruleInfos,
                        excludeFolder
                        );
                }
            }

            if (ruleInfos.Count == 0)
            {
                workingSet.Clear();
                return new Dictionary<string, IncludeFolder>();
            }

            //Ordinamento decrescente in base alla cardinalità.
            ruleInfos.Sort(new PatternComparer());//TODO ottimizzare ordinamento.

            //Effettiva applciazione delle regole ordinate al working set per l'ottenimento dell'insieme risultante.
            Dictionary<string, IncludeFolder> results = new Dictionary<string, IncludeFolder>();
            int indexOfItem = -1;
            foreach (RuleInfo pc in ruleInfos)
            {
                foreach (string item in pc.PatternMatches)
                {
                    if ((indexOfItem = workingSet.IndexOf(item)) != -1)
                    {
                        if (pc.PatternType == PatternType.Include)
                            results.Add(item, pc.IncludeFolder);

                        workingSet.RemoveAt(indexOfItem);
                    }
                }
            }

            workingSet.Clear();

            foreach (var folder in results.Keys)
            {
                workingSet.Add(folder);
            }

            return results;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Ritorna un insieme di file selezionati dal <code>workingSet</code>
        /// in base alle regole <code>wIncludeFiles</code> e <code>wExcludeFiles</code>
        /// </summary>
        public static void ResolveFiles(
            IList<string> workingSet,
            IncludeFile[] includeFiles,
            ExcludeFile[] excludeFiles
            )
        {
            if (workingSet == null || workingSet.Count == 0)
                return;

            List<RuleInfo> ruleInfos = new List<RuleInfo>();

            //Calcola la cardinalità per le regole IncludeFile e salva l'array di cardinalità in ruleInfos
            if (includeFiles != null && includeFiles.Length > 0)
            {
                foreach (var includeFile in includeFiles)
                {
                    CalculateRuleInfoForIncludeFile(
                        workingSet,
                        ruleInfos,
                        includeFile
                        );
                }
            }
            //Calcola la cardinalità per le regole ExcludeFile e salva l'array di cardinalità in ruleInfos
            if (excludeFiles != null && excludeFiles.Length > 0)
            {
                foreach (var excludeFile in excludeFiles)
                {
                    CalculateRuleInfoForExcludeFile(
                        workingSet,
                        ruleInfos,
                        excludeFile
                        );
                }
            }

            if (ruleInfos.Count == 0)
            {
                workingSet.Clear();
                return;
            }

            //Ordinamento decrescente in base alla cardinalità.
            ruleInfos.Sort(new PatternComparer());//TODO ottimizzare ordinamento.

            //Effettiva applciazione delle regole ordinate al working set per l'ottenimento dell'insieme risultante.
            List<string> results = new List<string>();
            int indexOfItem = -1;
            foreach (var pc in ruleInfos)
            {
                foreach (string item in pc.PatternMatches)
                {
                    if ((indexOfItem = workingSet.IndexOf(item)) != -1)
                    {
                        if (pc.PatternType == PatternType.Include)
                            results.Add(item);

                        workingSet.RemoveAt(indexOfItem);
                    }
                }
            }
            workingSet.Clear();

            foreach (var file in results)
            {
                workingSet.Add(file);
            }
        }

        //--------------------------------------------------------------------------------
        private static void CalculateRuleInfoForExcludeFile(
            IList<string> workingSet,
            IList<RuleInfo> ruleInfos,
            ExcludeFile excludeFile
            )
        {
            string[] patterns = excludeFile.Name.Split(pipe.ToCharArray());
            IList<string> patternMatches;
            RuleInfo ruleInfo = null;
            foreach (string pattern in patterns)
            {
                patternMatches = GetPatternMatches(workingSet, pattern);

                if (patternMatches != null && patternMatches.Count > 0)
                {
                    ruleInfo = new RuleInfo(PatternType.Exclude, patternMatches.Count, patternMatches);
                    ruleInfos.Add(ruleInfo);
                }
            }
        }

        //--------------------------------------------------------------------------------
        private static void CalculateRuleInfoForIncludeFile(
            IList<string> workingSet,
            IList<RuleInfo> ruleInfos,
            IncludeFile includeFile
            )
        {
            string[] patterns = includeFile.Name.Split(pipe.ToCharArray());
            IList<string> patternMatches;
            RuleInfo patternCardinality = null;
            foreach (string pattern in patterns)
            {
                patternMatches = GetPatternMatches(workingSet, pattern);

                if (patternMatches != null && patternMatches.Count > 0)
                {
                    patternCardinality = new RuleInfo(PatternType.Include, patternMatches.Count, patternMatches);
                    patternCardinality.IncludeFile = includeFile;
                    ruleInfos.Add(patternCardinality);
                }
            }
        }

        //--------------------------------------------------------------------------------
        private static void CalculateRuleInfoForIncludeFolder(
            IList<string> workingSet,
            List<RuleInfo> ruleInfos,
            IncludeFolder includeFolder
            )
        {
            string[] patterns = includeFolder.Name.Split(pipe.ToCharArray());
            IList<string> patternMatches;
            RuleInfo patternCardinality = null;
            foreach (string pattern in patterns)
            {
                patternMatches = GetPatternMatches(workingSet, pattern);

                if (patternMatches != null && patternMatches.Count > 0)
                {
                    patternCardinality = new RuleInfo(PatternType.Include, patternMatches.Count, patternMatches);
                    patternCardinality.IncludeFolder = includeFolder;
                    ruleInfos.Add(patternCardinality);
                }
            }
        }

        //--------------------------------------------------------------------------------
        private static void CalculateRuleInfoForExcludeFolder(
            IList<string> workingSet,
            List<RuleInfo> ruleInfos,
            ExcludeFolder excludeFolder
            )
        {
            string[] patterns = excludeFolder.Name.Split(pipe.ToCharArray());
            IList<string> patternMatches;
            RuleInfo ruleInfo = null;
            foreach (string pattern in patterns)
            {
                patternMatches = GetPatternMatches(workingSet, pattern);

                if (patternMatches != null && patternMatches.Count > 0)
                {
                    ruleInfo = new RuleInfo(PatternType.Exclude, patternMatches.Count, patternMatches);
                    ruleInfos.Add(ruleInfo);
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Elabora i pattern per il match di file e cartelle traducendoli in regular expression.
        /// </summary>
        /// <param name="workingSet">Insieme da valutare</param>
        /// <param name="pattern">pattern da applicare</param>
        /// <returns>Risultato ottenuto filtrando l'insieme 
        /// in ingresso con il pattern trasformato in regular expression</returns>
        private static IList<string> GetPatternMatches(IList<string> workingSet, string pattern)
        {
            if (!pattern.StartsWith(star, StringComparison.OrdinalIgnoreCase))
                pattern = pattern.Insert(0, "^");

            if (!pattern.EndsWith(star, StringComparison.OrdinalIgnoreCase))
                pattern = String.Concat(pattern, "$");

            if (pattern.IndexOf(dot, StringComparison.OrdinalIgnoreCase) != -1)
                pattern = pattern.Replace(dot, regExDot);

            if (pattern.IndexOf(star, StringComparison.OrdinalIgnoreCase) != -1)
                pattern = pattern.Replace(star, regExStar);

            var aRegEx = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            IList<string> patternMatches = new List<string>();
            foreach (string item in workingSet)
            {
                if (aRegEx.Match(item).Success)
                    patternMatches.Add(item);
            }

            return patternMatches;
        }
    }

    //================================================================================
    /// <summary>
    /// Classe che mantiene le informazioni circa l'applicazione di una regola
    /// (includeFolder, exlcudeFolder ecc) in modo da permettere l'ordinamento e la scelta
    /// del risultato
    /// </summary>
    public class RuleInfo
    {
        private int cardinality;
        private IList<string> patternMatches;
        private IncludeFolder includeFolder;
        private IncludeFile includeFile;
        private PatternType patternType;

        //--------------------------------------------------------------------------------
        public PatternType PatternType
        {
            get { return patternType; }
            set { patternType = value; }
        }

        //--------------------------------------------------------------------------------
        public IncludeFolder IncludeFolder
        {
            get { return includeFolder; }
            set { includeFolder = value; }
        }

        //--------------------------------------------------------------------------------
        public IncludeFile IncludeFile
        {
            get { return includeFile; }
            set { includeFile = value; }
        }

        //--------------------------------------------------------------------------------
        public IList<string> PatternMatches
        {
            get { return patternMatches; }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Quantità di file/folder selezionati da pattern
        /// </summary>
        public int Cardinality
        {
            get { return cardinality; }
            set { cardinality = value; }
        }

        //--------------------------------------------------------------------------------
        public RuleInfo(PatternType patternType, int cardinality, IList<string> patternMatches)
        {
            this.patternType = patternType;
            this.cardinality = cardinality;
            this.patternMatches = patternMatches;
        }
    }

    //================================================================================
    public enum PatternType
    {
        Include,
        Exclude
    }

    //================================================================================
    public class PatternComparer : IComparer<RuleInfo>
    {
        #region IComparer<PatternCardinality> Members

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Compara due pattern in base alla cardinalità e al tipo.
        /// La cardinalità di un pattern è la quantità di elementi che soddisfano il pattern.
        /// Il tipo è definito dall'enumerativo <code>PatternType</code>.
        /// Se due pattern hanno la medesima cardinalità allora il tipo <code>PatternType.Include</code>
        /// prevale sul tipo <code>PatternType.Exclude</code>.
        /// Se due pattern hanno anche il medesimo tipo allora l'ordine di ritorno è indifferente.
        /// </summary>
        public int Compare(RuleInfo x, RuleInfo y)
        {
            if (Object.ReferenceEquals(x, y))
                return 0;

            if (Object.ReferenceEquals(x, null))
            {
                return 1;
            }
            if (Object.ReferenceEquals(y, null))
            {
                return -1;
            }

            int result = x.Cardinality - y.Cardinality;

            if (result == 0)
                return (x.PatternType == PatternType.Include) ? -1 : 1;

            return result;
        }

        #endregion
    }
}
