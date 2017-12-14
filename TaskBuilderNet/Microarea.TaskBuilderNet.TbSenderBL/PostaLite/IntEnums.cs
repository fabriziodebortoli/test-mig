using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microarea.TaskBuilderNet.TbSenderBL.PostaLite
{
	// Enumerativi che mappano gli interi usati dal servizio PostaLite
	// Alcuni non sono definiti ancora, soggetti a modifica nei valori.

	internal enum EnumLotStatus // valori interi specificati via e-mail da P. Ravizza
	{
		// valori Postalite/Postel
		PresoInCarico = 1,
		Annullato = 2,
		InElaborazione = 3, // Postel
		Errato = 4, // Postel
		Spedito = 5, // Postel // Il lotto passa nello stato spedito quando Postel consegna il lotto alle Poste per il recapito. Da qui in poi non abbiamo più informazioni. [cit. Paolo. R.]
		SpeditoConInesitati = 6,
		InStampa = 9, // Postel
		Sospeso = 11,
		AnnullatoParzialmente = 12
	}

	internal enum EnumCreditStatus
	{
		//La proprietà CodeState conterrà un valore numerico positivo che identifica lo stato dell'utente
		//(abilitato, non abilitato, sospeso) e Credit il credito a disposizione dell'utente. In caso di errore
		//CodeState conterrà un valore numerico negativo e Credit valorizzato a zero.

		// valori confermati da Benelli il 9/7/2012 via e-mail
		Abilitato = 1,
		Sospeso = 2,
		NonAttivo = -21
	}
}