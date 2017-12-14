//using System;
//using System.Configuration;
//using System.Globalization;
//using System.Web.Services.Protocols;
////
//using Microsoft.Web.Services;
//using Microsoft.Web.Services.Timestamp;

////	L'utilizzo di questi filtri presuppone l'esistenza di un file dell'applicazione
////  (di solito del tipo *.config) dove sono specificati i parametri.
////  I parametri che devono essere specificati sono:
////	-	maxDeltaTime	:	intervallo di tempo massimo ammissibile tra client e server.
////							Il valore di default e' 1500000 (= 25 muniti);
////	-	SyncServerUrl	:	url del web service per la sincronizzazione dell'orario per la comunicazione;
////	-	TTL				:	TTL del pacchetto per la comunicazione.
////							Il valore di default e' 300000 (= 5 minuti);
////  N.B.: l'unico parametro la cui mancanza causa il malfunzionamento del meccanismo e' l'url del WS
////        poiche' per tutti gli altri sono previsti valori di default.

//namespace Microarea.TaskBuilderNet.Licence.Activation
//{
//    /// <summary>
//    /// Filtro di input per la valutazione della differenza di tempo tra client e server.
//    /// </summary>
//    //=======================================================================================================
//    public abstract class DTCSInputFilter : SoapInputFilter
//    {
//        /// <summary>
//        /// Valuta la differenza di ora tra gli attori della comunicazione sicura.
//        /// </summary>
//        // --------------------------------------------------------------------------------------------------
//        public override void ProcessMessage(SoapEnvelope envelope)
//        {
//            if (envelope != null && envelope.Context != null && envelope.Context.Timestamp != null)
//                EvaluateDeltaTime(
//                                    GetClientTime(envelope.Context.Timestamp),
//                                    GetServerTime(envelope.Context.Timestamp) 
//                                  );
//        }

//        // --------------------------------------------------------------------------------------------------
//        protected virtual long GetClientTime(Timestamp timeStamp)
//        {
//            return Int64.MinValue;
//        }

//        // --------------------------------------------------------------------------------------------------
//        protected virtual long GetServerTime(Timestamp timeStamp)
//        {
//            return Int64.MaxValue;
//        }

//        /// <summary>
//        /// Valuta la differenza di ora tra client e server: se e' maggiore di un valore impostato lancia
//        /// un'eccezione e blocca l'esecuzione.
//        /// </summary>
//        // --------------------------------------------------------------------------------------------------
//        protected virtual void EvaluateDeltaTime(long clientTime, long serverTime)
//        {
//            string	maxDeltaTimeString	=	ConfigurationManager.AppSettings["maxDeltaTime"];
//            long	maxDeltaTime		=	0;

//            if (maxDeltaTimeString == null)
//                maxDeltaTimeString = "1500000";		//	valore di default in millisecondi.

//            try
//            {
//                maxDeltaTime = Int64.Parse(maxDeltaTimeString, CultureInfo.InvariantCulture);
//            }
//            catch
//            {
//                maxDeltaTime = 1500000;		//	valore di default in millisecondi.
//            }

//            if (Math.Abs(serverTime - clientTime) > maxDeltaTime)
//                throw new DeltaTimeException("Too much difference between client time and server time",
//                                                new System.Xml.XmlQualifiedName("Time", "soap"));
//        }
//    }

//    /// <summary>
//    /// DTCSServerInputFilter: filtro che interviene lato server per valutare la differenza di orario
//    /// tra client e server coinvolti nella comunicazione sul pacchetto in ingresso al server proveniente
//    /// dal client.
//    /// </summary>
//    //=======================================================================================================
//    public class DTCSServerInputFilter : DTCSInputFilter
//    {
//        // --------------------------------------------------------------------------------------------------
//        protected override long GetClientTime(Timestamp timeStamp)
//        {
//            if (timeStamp != null)
//                return (timeStamp.Created.Ticks / 10000);
//            return base.GetClientTime(timeStamp);

//        }

//        // --------------------------------------------------------------------------------------------------
//        protected override long GetServerTime(Timestamp timeStamp)
//        {
//            return (DateTime.UtcNow.Ticks / 10000);
//        }
//    }

//    /// <summary>
//    /// Filtro di output per impostare il TTL corretto per la comunicazione sicura tra client e server.
//    /// </summary>
//    //=======================================================================================================
//    public abstract class DTCSOutputFilter : SoapOutputFilter
//    {
//        /// <summary>
//        /// Imposta il TTL del pacchetto basandosi sulla differenza di ora tra gli attori coinvolti
//        /// nella comunicazione.
//        /// </summary>
//        // --------------------------------------------------------------------------------------------------
//        public override void ProcessMessage(SoapEnvelope envelope)
//        {
//            if (envelope != null && envelope.Context != null && envelope.Context.Timestamp != null && HttpSoapContext.RequestContext.Timestamp != null)
//                SetTTL(envelope, GetClientTime(envelope.Context.Timestamp), GetServerTime());
//        }

//        // --------------------------------------------------------------------------------------------------
//        protected virtual long GetClientTime(Timestamp timeStamp)
//        {
//            return Int64.MinValue;
//        }

//        // --------------------------------------------------------------------------------------------------
//        protected virtual long GetServerTime()
//        {
//            return Int64.MaxValue;
//        }

//        /// <summary>
//        /// Valuta la differenza di ora tra client e server: se e' maggiore di un valore impostato lancia
//        /// un'eccezione e blocca l'esecuzione.
//        /// </summary>
//        // --------------------------------------------------------------------------------------------------
//        protected virtual void EvaluateDeltaTime(long clientTime, long serverTime)
//        {
//            string maxDeltaTimeString   = ConfigurationManager.AppSettings["maxDeltaTime"];
//            long	maxDeltaTime		=	0;

//            if ( maxDeltaTimeString == null )
//                maxDeltaTimeString = "1500000";		//	valore di default in millisecondi.

//            try
//            {
//                maxDeltaTime = Int64.Parse(maxDeltaTimeString, CultureInfo.InvariantCulture);
//            }
//            catch
//            {
//                maxDeltaTime = 1500000;		//	valore di default in millisecondi.
//            }

//            if (Math.Abs( serverTime - clientTime ) > maxDeltaTime)
//                throw new DeltaTimeException("Too much difference between client time and server time",
//                                                new System.Xml.XmlQualifiedName("Time", "soap"));
//        }

//        /// <summary>
//        /// Imposta il TTL per la comunicazione.
//        /// </summary>
//        // --------------------------------------------------------------------------------------------------
//        protected virtual void SetTTL(SoapEnvelope envelope, long clientTime, long serverTime)
//        {}
//    }

//    /// <summary>
//    /// DTCSServerOutputFilter: filtro che interviene lato server per valutare la differenza di orario
//    /// tra client e server coinvolti nella comunicazione sul pacchetto in uscita dal server verso il client.
//    /// </summary>
//    //=======================================================================================================
//    public class DTCSServerOutputFilter : DTCSOutputFilter
//    {
//        // Il server stima l'ora del client in base all'ora in cui e' stato creato il pacchetto di RICHIESTA.
//        // --------------------------------------------------------------------------------------------------
//        protected override long GetClientTime(Timestamp timeStamp)
//        {
//            if(HttpSoapContext.RequestContext != null && HttpSoapContext.RequestContext.Timestamp != null)
//                return (HttpSoapContext.RequestContext.Timestamp.Created.Ticks / 10000);
//            return base.GetClientTime(timeStamp);
//        }

//        // --------------------------------------------------------------------------------------------------
//        protected override long GetServerTime()
//        {
//            return (DateTime.UtcNow.Ticks / 10000);
//        }

//        /// <summary>
//        /// Imposta il TTL per la comunicazione. Se la differenza di ora tra gli attori e' maggiore di un
//        /// parametro impostabile lancia un'eccezione e blocca l'esecuzione.
//        /// </summary>
//        // --------------------------------------------------------------------------------------------------
//        protected override void SetTTL(SoapEnvelope envelope, long clientTime, long serverTime)
//        {
//            EvaluateDeltaTime(clientTime, serverTime);

//            string ttlString = ConfigurationManager.AppSettings["TTL"];
//            long	ttl		 =	0;

//            if (ttlString == null)
//                ttlString = "300000";

//            try
//            {
//                ttl = Int64.Parse(ttlString, CultureInfo.InvariantCulture);
//            }
//            catch
//            {
//                ttl = 300000;
//            }

//            if (serverTime > clientTime)
//                envelope.Context.Timestamp.Ttl = ttl;
//            else
//                envelope.Context.Timestamp.Ttl = clientTime - serverTime + ttl;
//        }
//    }

//    /// <summary>
//    /// DeltaTimeException: eccezione lanciata nel caso la differenza tra l'ora del client e quella del
//    /// server coinvolti nella comunicazione sia maggiore di un valore fissato.
//    /// </summary>
//    //=======================================================================================================
//    public class DeltaTimeException : SoapException
//    {
//        //---------------------------------------------------------------------------------------------------
//        public DeltaTimeException(string errorMessage, System.Xml.XmlQualifiedName qName)
//                                : base(errorMessage, qName)
//        {}
//    }
//}