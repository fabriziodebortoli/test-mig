using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	# region Classe WeightItem
	/// <summary>
	/// definizione della classe per oggetti di tipo WeightItem
	/// da utilizzare x tenere traccia, nel grafo pesato, dei pesi e relativi indici delle colonne
	/// </summary>
	//============================================================================
	public class WeightItem
	{
		public int Weight		= 0;	// peso dell'arco
		public int Index		= 0;	// indice nella vertex list
		private string label = string.Empty;	// indice nel sorted array

		public string Label { get { return label; } }

		//---------------------------------------------------------------------------
		public WeightItem(int w, int idx, string label)
		{
			Weight	= w;
			Index	= idx;
			this.label = label;
		}
	}
	# endregion

	# region Classe Vertex
	/// <summary>
	/// Classe di definizione di un vertice
	/// </summary>
	//============================================================================
	public class Vertex
	{
		public string			Label;
		public bool				Visited;
		public List<WeightItem> WeightedList;	// array di oggetti di tipo WeightItem per gestire i pesi degli archi

		/// <summary>
		/// costruttore classe Vertex
		/// </summary>
		//---------------------------------------------------------------------------
		public Vertex(string lab)
		{
			Label			= lab;
			Visited			= false;
			WeightedList	= new List<WeightItem>();
		}	

		/// <summary>
		/// metodo per aggiungere un oggetto di tipo WeightItem alla lista dei pesi del vertice
		/// (senza duplicati)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddWeightItem(WeightItem x)
		{
			WeightedList.Add(x);

			// MICHI: ho tolto lo skip dei valori duplicati nell'array dei pesi perchè non si riesce a gestire la
			// doppia dipendenza dallo stesso modulo (vedi anomalia nr. 13582)
/*			if (WeightedList.Count == 0)
			{
				WeightedList.Add(x);
				return;
			}

			foreach (WeightItem item in WeightedList)
				if (item.Index != x.Index && item.Weight != x.Weight)
				{
					WeightedList.Add(x);
					break;
				}
*/		
		}
	}
	# endregion

	# region Classe DirectGraph
	/// <summary>
	/// Classe per costruire il grafo
	/// </summary>
	//============================================================================
	public class DirectGraph
	{
		# region Variabili
		private int					maxVerts = 100;	// n° max di vertici consentiti 
													// eventualmente viene ridimensionata la matrice
		private int					vertsCount;		// n° effettivo di vertici presenti nel grafo
		private int[,]				adjMatrix;		// matrice di interi bidimensionale (o di adiacenza)

		private List<Vertex>		vertexList;		// array di oggetti di tipo Vertex
		public	List<string>		SortedArray;	// array di label
		public	List<Vertex>		visitingNodes	= new List<Vertex>();
		public  DependencyInfoList	DependencyList	= null; // per tenere traccia delle dipendenze

		// mi serve per inserire nell'output la struttura delle dipendenze (a solo scopo debug)
		//----------------------------------------------------
		public bool WriteLogInfo = false; // di default e' a false
		//----------------------------------------------------
		# endregion

		# region Costruttori
		/// <summary>
		/// costruttore classe DirectGraph
		/// </summary>
		//---------------------------------------------------------------------------
		public DirectGraph()
		{
			vertexList	= new List<Vertex>();
			vertsCount	= 0;

			// creo ed inizializzo la matrice di adiacenza
			CreateMatrix(maxVerts);
		}

		/// <summary>
		/// overload costruttore DirectGraph, con il parametro relativo all'effettivo numero
		/// di vertici per dimensionare la matrice di adiacenza
		/// </summary>
		//---------------------------------------------------------------------------
		public DirectGraph(int numVerts)
		{
			vertexList	= new List<Vertex>();
			vertsCount	= 0;
			maxVerts	= numVerts;

			// creo ed inizializzo la matrice di adiacenza
			CreateMatrix(numVerts);
		}
		# endregion

		# region Creazione e ridimensionamento matrice di adiacenza
		/// <summary>
		/// creazione ed inizializzazione della matrice di adiacenza.
		/// </summary>
		/// <param name="num">dimensione della matrice</param>
		//---------------------------------------------------------------------------
		private void CreateMatrix(int num)
		{
			adjMatrix = new int [num, num];

			// inizializzo la matrice di adiacenza con uno zero in ogni elemento
			for (int j = 0; j < num; j++)
				for (int k = 0; k < num; k++)
					adjMatrix[j,k] = 0;
		}

		/// <summary>
		/// funzione per ridimensionare la matrice di adiacenza qualora questa non sia abbastanza
		/// grande per ospitare tutti gli archi necessari per la costruzione del grafo
		/// </summary>
		//---------------------------------------------------------------------------
		private void CreateNewMatrix()
		{
			int[,] myMatrix = (int[,])adjMatrix.Clone();

			int num = maxVerts * 2;
			CreateMatrix(num);

			// copio il contenuto della matrice clonata in quella nuova ridimensionata
			for (int j = 0; j < maxVerts; j++)
				for (int k = 0; k < maxVerts; k++)
					adjMatrix[j,k] = myMatrix[j,k];

			maxVerts = num;
		}
		# endregion

		# region AddVertex, AddEdge
		/// <summary>
		/// se non esiste già, aggiunge un nuovo vertice nel grafo e nel relativo array 
		/// di vertici e ritorna l'indice assegnato all'elemento dell'array
		/// </summary>
		/// <param name="label">label da assegnare del vertice</param>
		/// <returns>indice del vertice</returns>
		//---------------------------------------------------------------------------
		public int AddVertex(string label)
		{
			int idx = GetIdxVertex(label);

			if (idx > -1)
				return idx;
			
			vertsCount++;

			// se il vertice che si vuole inserire eccede il numero max previsto, 
			// devo ampliare la matrice e poi aggiungere cmq il vertice
			if (maxVerts < vertsCount)
				CreateNewMatrix();

			Vertex v = new Vertex(label);
			vertexList.Add(v);
			return vertexList.IndexOf(v);
		}

		/// <summary>
		/// aggiunge un arco tra due vertici (unidirezionale)
		/// </summary>
		/// <param name="start">indice vertice di partenza</param>
		/// <param name="end">indice vertice di arrivo</param>
		//---------------------------------------------------------------------------
		public void AddEdge(int start, int end) 
		{
			AddEdge(start, end, 1);
		}

		/// <summary>
		/// aggiunge un arco tra due vertici (unidirezionale) attribuendo allo stesso un "peso"
		/// per default weight = 1
		/// </summary>
		/// <param name="start">indice vertice di partenza</param>
		/// <param name="end">indice vertice di arrivo</param>
		/// <param name="weight">peso attribuito all'arco</param>
		//---------------------------------------------------------------------------
		public void AddEdge(int start, int end, int weight) 
		{
			// se gli indici dei vertici rispettano i limiti max e min
			// inserisco l'arco tra i vertici ed imposto 1 nella matrice
			if (
				(start > -1 && start < maxVerts) && 
				(end > -1 && end < maxVerts)
				)
				adjMatrix[start,end] = weight;
			else
			{
				// se sto inserendo un numero di vertici eccedenti quelli stabiliti dalla matrice
				// devo ampliare la stessa e poi aggiungere cmq l'arco esaminato
				CreateNewMatrix();
				adjMatrix[start,end] = weight;
			}
		}
		# endregion

		# region Varie funzioni di Get
		/// <summary>
		/// attraverso la label controllo se il nome del vertice è già presente nell'array
		/// il controllo viene fatto CASE-INSENSITIVE
		/// </summary>
		/// <param name="label">label vertice</param>
		/// <returns>se esiste già il corrispondente numero associato a quel vertice nell'array,
		/// se non esiste ritorna -1</returns>
		//---------------------------------------------------------------------------
		public int GetIdxVertex(string label)
		{	
			for (int i = 0; i < vertexList.Count; i++)
			{
				if (string.Compare(vertexList[i].Label, label, true) == 0)
					return i; // il vertice ESISTE nell'array
			}
			return -1; // il vertice non è stato ancora inserito nell'array
		}
	
		/// <summary>
		/// funzione che restituisce la sola label del vertice corrispondente all'indice specificato
		/// </summary>
		/// <param name="selIdx">indice del vertice</param>
		/// <returns>label del vertice</returns>
		//---------------------------------------------------------------------------
		private string GetVertexLabel(int selIdx)
		{
			return vertexList[selIdx].Label;
		}

		/// <summary>
		/// funzione per identificare un oggetto di tipo Vertex corrispondente 
		/// all'indice specificato
		/// </summary>
		/// <param name="selIdx">indice del vertice</param>
		/// <returns>oggetto di tipo Vertex</returns>
		//---------------------------------------------------------------------------
		public Vertex GetVertex(int selIdx)
		{
			return vertexList[selIdx];
		}

		/// <summary>
		/// funzione per identificare un oggetto di tipo Vertex corrispondente 
		/// alla label specificata
		/// </summary>
		/// <param name="label">label del vertice</param>
		/// <returns>oggetto di tipo Vertex</returns>
		//---------------------------------------------------------------------------
		private Vertex GetVertexData(string label)
		{
			foreach (Vertex v in vertexList)
			{
				if (string.Compare(v.Label, label, StringComparison.CurrentCultureIgnoreCase) == 0)
					return v; // il vertice ESISTE nell'array
			}

			return null; // il vertice non è stato ancora inserito nell'array
		}

		/// <summary>
		/// attraverso la label controllo se il nome del vertice è già presente nel sorted array
		/// il controllo viene fatto CASE-INSENSITIVE
		/// </summary>
		/// <param name="l1">label vertice</param>
		/// <returns>se esiste già ritorna il corrispondente numero associato a quel vertice nell'array,
		/// se non esiste ritorna -1</returns>
		//---------------------------------------------------------------------------
		public int GetIdxSortedArray(string label)
		{	
			for (int i = 0; i < SortedArray.Count; i++)
			{
				if (string.Compare(SortedArray[i], label, StringComparison.CurrentCultureIgnoreCase) == 0)
					return i; // il vertice ESISTE nell'array
			}

			return -1; // il vertice non è stato ancora inserito nell'array
		}

		# endregion

		# region Algoritmo gestione del grafo direzionato e non pesato
		/// <summary>
		/// funzione che determina il Topological sorting tra i vertici e gli archi
		/// inseriti nella matrice di adiacenza
		/// </summary>
		//---------------------------------------------------------------------------
		public void Topo()
		{
			// costruisco in memoria la struttura delle dipendenze sulla base del grafo costruito.
			AnalyzeDependencies();

			int orig_Verts = vertsCount;
			SortedArray = new List<string>(vertsCount);

			while (vertsCount > 0)
			{
				int currVertex = NoSuccessors();
				
				// se entro in questo if vuol dire che ci sono dei cicli 
				// all'interno del grafo -> ERRORE!
				// @@TODOMICHI: aggiungere diagnostica!!!
				if (currVertex == -1)
				{
					Debug.WriteLine("------------------------------------------------------------------");
					Debug.WriteLine("ATTENTION: Cycles detected in the Topological sort of DirectGraph!");
					Debug.WriteLine("------------------------------------------------------------------");
					return;
				}

				string s = GetVertexLabel(currVertex);
				SortedArray.Add(s);
				DeleteVertex(currVertex);
			}

			SortedArray.Reverse();
		}

		// per un numero di volte pari al n° effettivo dei vertici presenti nel grafo
		// accede alla matrice e scorre tutte le colonne
		// si cerca di individuare se quel particolare vertice preso in esame ha dei
		// vertici figli (presenza di almeno un 1).
		// se la riga presenta tutti zeri vuol dire che non ha "figli"
		// in quest'ultimo caso elimino il nome del vertice dall'array di vertici e
		// inserisco la sua label nell'array di stringhe che verrà letto dalla funzione
		// Topo() - Topological Sorting
		/// <summary>
		/// individua se un vertice ha dei vertici figli
		/// </summary>
		/// <returns>indice del vertice</returns>
		//---------------------------------------------------------------------------
		private int NoSuccessors()
		{
			bool isEdge;

			for (int row = 0; row < vertsCount; row++)
			{
				isEdge = false;

				for (int col = 0; col < vertsCount; col++)
				{
					if (adjMatrix[row, col] > 0)
					{
						isEdge = true;
						break;
					}
				}

				if (!isEdge)
					return row;
			}

			return -1;
		}

		/// <summary>
		/// elimina un vertice dalla matrice di adiacenza e sposta le relative righe e colonne
		/// </summary>
		/// <param name="delVert">indice del vertice da eliminare</param>
		//---------------------------------------------------------------------------
		private void DeleteVertex(int delVert)
		{
			if (delVert != (vertsCount - 1))
			{
				// se non è l'ultimo vertice, lo elimino dalla vertex list
				for (int j = delVert; j < vertsCount - 1; j++)
					vertexList[j] = vertexList[j + 1];

				// elimino la corrispondente riga dalla matrice
				for (int row = delVert; row < vertsCount - 1; row++)
					MoveRowUp(row, vertsCount);

				// elimino la corrispondente colonna dalla matrice
				for (int col = delVert; col < vertsCount - 1; col++)
					MoveColLeft(col, vertsCount - 1);
			}
			vertsCount--;
		}

		/// <summary>
		/// elimina una riga e sposta le altre in su di un posizione
		/// </summary>
		/// <param name="row">numero riga</param>
		/// <param name="len">numero elementi</param>
		//---------------------------------------------------------------------------
		private void MoveRowUp(int row, int len)
		{
			for (int col = 0; col < len; col++)
				adjMatrix[row, col] = adjMatrix[row + 1, col];
		}

		/// <summary>
		/// elimina una colonna e sposta le altre a sinistra
		/// </summary>
		/// <param name="col">numero colonna</param>
		/// <param name="len">numero elementi</param>
		//---------------------------------------------------------------------------
		private void MoveColLeft(int col, int len)
		{
			for (int row = 0; row < len; row++)
				adjMatrix[row, col] = adjMatrix[row, col + 1];
		}
		# endregion

		# region Algoritmo per la gestione del grafo direzionato e pesato
		/// <summary>
		/// entry-point per ottenere l'array di moduli ordinato per peso di cui poi 
		/// rintracciare i vari script fare l'upgrade.
		/// per ogni riga si scorrono le colonne e dove viene trovato un arco (> 1) si 
		/// va in ricorsione sulla relativa riga
		/// </summary>
		//---------------------------------------------------------------------------
		public void WeightedTopo()
		{
			try
			{
				// costruisco in memoria la struttura delle dipendenze sulla base del grafo costruito.
				AnalyzeDependencies();

				SortedArray = new List<string>(vertsCount);
				Vertex myVertex = null;
				for (int row = 0; row < vertsCount; row++)
				{
					myVertex = GetVertex(row);
					if (myVertex.Visited)
						continue;

					LookOnVertex(row, myVertex);
				}

				// dopo aver analizzato tutti i vertici dipendenti in cascata, devo inserire
				// nell'array il vertice relativo alla riga di partenza ed impostarlo come visitato.
				if (myVertex != null && !myVertex.Visited)
				{
					myVertex.Visited = true;
					SortedArray.Add(myVertex.Label);
				}

				if (WriteLogInfo)
				{
					AppendTextToOutput("//==========================");
					AppendTextToOutput("SortedArray items:");
					foreach (string item in SortedArray)
						AppendTextToOutput(item);
					AppendTextToOutput("//==========================");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error in WeightedTopo method!!!");
				Debug.WriteLine(ex.Message);
				throw;
			}
		}

		//---------------------------------------------------------------------------
		public int GetVertexSortedIdx(int idx)
		{
			Vertex v = (Vertex)vertexList[idx];
			for (int i = 0; i < SortedArray.Count; i++)
			{
				if (SortedArray[i].Equals(v.Label))
					return i;
			}
			
			return -1;
		}

		/// <summary>
		/// funzione che viene richiamata in modo ricorsivo per determinare l'ordine dei moduli
		/// di cui eseguire i relativi script di upgrade. 
		/// </summary>
		//---------------------------------------------------------------------------
		private int LookOnVertex(int row, Vertex vx)
		{
			for (int col = 0; col < vertsCount; col++)
			{
				// ora il controllo e' solo maggiore di 1 (e non piu' maggiore stretto)
				// xchè si che si tratta di valori "pesati" (relativi a scatti di release di upgrade)
				// ma in questi valori ci sono anche i moduli nuovi creati durante un aggiornamento e
				// che quindi possono avere delle dipendenze tra loro)
				// per ogni cella con valore >= 1 inserisco il peso e l'indice della colonna
				// in un array di appoggio (contenente oggetti di tipo WeightItem) 
				if (adjMatrix[row, col] >= 1)
					vx.AddWeightItem(new WeightItem(adjMatrix[row, col], col, GetVertexLabel(col)));
			}

			// check di ricorsivita' in caso di cicli di dipendenze
			if (visitingNodes.Contains(vx))
			{
				StringBuilder sb = new StringBuilder();
				for (int i = visitingNodes.Count - 1; i >= 0; i--)
				{
					Vertex v = visitingNodes[i];
					if (sb.Length > 0)
						sb.Append(" --> ");
					sb.Append(v.Label);
				}

				sb.Append(" --> ");
				sb.Append(visitingNodes[visitingNodes.Count - 1].Label);

				string message = string.Concat("Recursion detected! Node sequence: \r\n", sb);
				Debug.Fail(message);
				throw new Exception(sb.ToString());
			}

			try
			{
				visitingNodes.Add(vx);
				int precIdx = int.MaxValue;

				// solo se è presente almeno un elemento nell'array vado ad analizzarlo
				if (vx.WeightedList.Count > 0)
				{
					// se gli elementi sono due o piu' eseguo un sort per numero di peso
					if (vx.WeightedList.Count > 1)
						vx.WeightedList.Sort(new CustomSortWeightList());

					// x ogni elemento presente nella lista vado ad verificare:
					// 1. se è già presente nel SortedArray calcolo dinamicamente quale indice gli è stato assegnato nel SortedArray
					// 2. se non è già stato inserito nel SortedArray passo ad analizzare la sua riga e 
					//    le sue dirette dipendenze in ricorsione
					for (int i = 0; i < vx.WeightedList.Count; i++)
					{
						WeightItem wItem = (WeightItem)vx.WeightedList[i];
						Vertex myV = GetVertex(wItem.Index);

						if (myV.Visited)
							precIdx = Math.Min(precIdx, GetVertexSortedIdx(wItem.Index));
						else
							precIdx = Math.Min(precIdx, LookOnVertex(wItem.Index, myV));
					}
				}

				// se non ho trovato in tutta la riga una cella valorizzata con un peso 
				// inserisco il vertice nel sorted array
				if (!vx.Visited)
				{
					vx.Visited = true;
					if (precIdx >= 0 && precIdx != int.MaxValue)
					{
						SortedArray.Insert(precIdx, vx.Label);
						AppendTextToOutput(string.Format("SortedArray.Insert label {0} at idx {1}", vx.Label, precIdx.ToString()));
					}
					else
					{
						SortedArray.Add(vx.Label);
						precIdx = SortedArray.IndexOf(vx.Label);
						AppendTextToOutput(string.Format("SortedArray.Add label {0}", vx.Label));
					}
				}
				return precIdx;
			}
			finally
			{
				visitingNodes.Remove(vx);			
			}
		}
		# endregion

		# region Sorting WeightList
		/// <summary>
		/// Questa classe si occupa di re-sortare un array list.
		/// Nello specifico caso mi serve per fare il sort dell'array degli archi "pesati"
		/// (weightList), ordinandolo per numero di peso crescente.
		/// </summary>
		//============================================================================
		public class CustomSortWeightList : IComparer<WeightItem>
		{
			//---------------------------------------------------------------------------
			public int Compare(WeightItem x, WeightItem y)
			{
				return Compare(x.Weight, y.Weight);
			}

			//---------------------------------------------------------------------------
			private int Compare(int weight1, int weight2)
			{
				// CompareTo results
				// -1     First int is smaller.
				// 1      First int is larger.
				// 0      Ints are equal.
				return weight1.CompareTo(weight2);
			}
		}
		# endregion

		# region AnalyzeDependencies
		//****************************************************************************
		//	ANALISI DIPENDENZE (OGNI MODULO HA AGGANCIATO UN ARRAY DI CHILD)
		//****************************************************************************
		/// <summary>
		/// per ogni modulo viene costruito un array con l'elenco dei moduli individuati
		/// come child, ossia che dipendono da lui.
		/// </summary>
		//---------------------------------------------------------------------------
		public void AnalyzeDependencies()
		{
			AppendTextToOutput("//==========================");
			AppendTextToOutput("Start AnalyzeDependencies");
			AppendTextToOutput("//==========================");

			DependencyInfo depInfo = null;
			DependencyList = new DependencyInfoList();

			for (int col = 0; col < vertsCount; col++)
			{
				depInfo = new DependencyInfo();

				// inserisco subito il nome dell'ancestor
				depInfo.AncestorName = GetVertexLabel(col);
				AppendTextToOutput("Ancestor name: " + depInfo.AncestorName);
				
				for (int row = 0; row < vertsCount; row++)				
				{
					if (adjMatrix[row, col] > 0)
					{
						string childName = GetVertexLabel(row);
						depInfo.ChildList.Add(childName);
						AppendTextToOutput("--- Child: " + childName);
					}
				}

				DependencyList.Add(depInfo);
			}

			AppendTextToOutput("//==========================");
			AppendTextToOutput("End AnalyzeDependencies");
			AppendTextToOutput("//==========================");
		}
		# endregion

		//---------------------------------------------------------------------------
		private void AppendTextToOutput(string text)
		{
			if (WriteLogInfo)
				Debug.WriteLine(text);
		}
	}
	# endregion

	# region Gestione classi DependencyInfo
	/// <summary>
	/// classe per memorizzare per ogni AddOnApp+Module tutti i suoi AddOnApp+Module da cui dipende
	/// </summary>
	//============================================================================
	public class DependencyInfo
	{
		public string		AncestorName	= string.Empty; 
		public List<string> ChildList		= null;

		/// <summary>
		/// costruttore classe DependencyInfo
		/// </summary>
		//---------------------------------------------------------------------------
		public DependencyInfo()
		{
			ChildList = new List<string>();
		}
	}

	//============================================================================
	public class DependencyInfoList : List<DependencyInfo>
	{
		//---------------------------------------------------------------------------
		public DependencyInfoList()
		{
		}

		/// <summary>
		/// funzione che ritorna un oggetto di tipo DependencyInfo, passando come parametro il nome del module
		/// </summary>
		/// <param name="moduleName">nome module</param>
		/// <returns>oggetto di tipo DependencyInfo</returns>
		//---------------------------------------------------------------------------
		public DependencyInfo GetItem(string moduleName)
		{
			foreach (DependencyInfo info in this)
				if (string.Compare(info.AncestorName, moduleName, StringComparison.CurrentCultureIgnoreCase) == 0)
					return info;

			return null;
		}
	}
	# endregion
}