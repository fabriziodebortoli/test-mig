using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Classe utilizzata per cambiare il valore del booleano Control.CheckForIllegalCrossThreadCalls
	/// il quale lancia una eccezione quando si accedono le proprietà di un controllo da un thread a ll'altro
	/// Purtroppo esiste (secondo noi: Marco, Matteo) un buco del framework
	/// che lancia questa eccezione quando si effettua una ShowDialog di una form
	/// che prende come parenbt una finestra che vive in un altr thread.
	/// Secondo noi è un buco perché nel codice disassemblato si tiene conto della possibilità che le due finestre stiano in thread separati,
	/// ma poi in un punto non viene fatta la Invoke per ottenere la proprietà Handle e si verifica l'eccezione
	/// </summary>
	public class SafeThreadCallContext : IDisposable
	{
		bool original = false;
		bool disposed = false;

		public SafeThreadCallContext()
		{
			//metto da parte il valore originario
			original = Control.CheckForIllegalCrossThreadCalls;
			//disabilito il controllo
			Control.CheckForIllegalCrossThreadCalls = false;
		}
		
		public void Dispose()
		{
			if (disposed)
				return;
			//ripristino il valore originario
			Control.CheckForIllegalCrossThreadCalls = original;
			disposed = true;
		}

	}
}
