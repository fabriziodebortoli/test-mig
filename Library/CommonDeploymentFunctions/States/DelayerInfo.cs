using System;
//
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Classe che permette di configurare le impostazioni del Delayer
	/// usato da WebUpdater
	/// </summary>
	/// <remarks>
	/// Tutti i suoi membri sono public r/w, perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer
	/// </remarks>
	[Serializable]
	public class DelayerInfo : State
	{
		public uint		MaxAttempts	= 5;		// # massimo di tentativi prima di dare errore
		public double	DeltaZero	= 60000;	// ms del primo step

		/// <summary>
		/// ritorna Di(i) come
		/// 	Di = D0			per	i = 0
		/// 	Di = D0 x 2		per	i > 0	i <= Nmax
		/// </summary>
		/// <param name="i"></param>
		/// <returns>numero di millisecondi da attendere</returns>
		//---------------------------------------------------------------------
		public double GetDelayTime(int i)
		{
			if (i == 0)
				return DeltaZero;
			double delay = DeltaZero * 2;
			return delay;
		}
	}

	/*
	Alcuni step delle macchine a stati, per esempio la conferma di completamento
	degli aggiornamenti da parte del server remoto ISU, o la gestione di errori 
	ripetuti in fasi ripetibili (es. fallimento chiamate SOAP), necessitano che la macchina
	a stati attendo un tempo non determinabile a priori, per cui vanno richieste
	periodicamente fino all’esaudirsi della richiesta o fino al raggiungimento di
	un numero massimo di Nmax tentativi.
	Se la conferma di esistenza di aggiornamenti viene ricevuta nel tempo t0, 
	le richieste delle credenziali potranno essere effettuate nei tempi

		t1, ... , tNmax
	con
		t i =  t i-1 + Di

	Le richieste delle credenziali potrebbe essere effettuata ad intervalli 
	temporali non equamente spaziati, in modo da dare più tempo a processi 
	che sembrano necessitarlo adottando una politica opportuna, per esempio (lineare):

		Di = D0 i K	i > 0	i <= Nmax

	dove K è una costante opportunamente scelta.
	Possibili valori proposti sono:

		D0		= 30 s
		K		= 2
		Nmax	= 4

	L’ordine di grandezza degli intervalli temporali deve essere di molto inferiore 
	a quello delle periodiche richieste di esistenza di aggiornamenti, che comunque 
	non potranno essere effettuati quando il processo di aggiornamento è in corso, 
	quindi, possibilmente:

		tNmax < Tscheduled
	*/
}