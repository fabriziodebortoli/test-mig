using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
    //=========================================================================
    public interface IRetryableObject
    {
        //---------------------------------------------------------------------
        object CallerObject { get; set; }
        
        //---------------------------------------------------------------------
        string MethodName { get; set; }
       
        //---------------------------------------------------------------------
        object[] MethodActualParameters { get; set; }

        //---------------------------------------------------------------------
        Type[] GetMethodActualParametersType { get; }
    }

    //=========================================================================
    public class SimpleRetryableObject : IRetryableObject
    {
        //---------------------------------------------------------------------
        public object CallerObject { get; set; }
        
        //---------------------------------------------------------------------
        public string MethodName { get; set; }
        
        //---------------------------------------------------------------------
        public object[] MethodActualParameters { get; set; }
        
        //---------------------------------------------------------------------
        public SimpleRetryableObject(object callerObj, string name, object[] parameters)
        { 
            CallerObject = callerObj;
            MethodName = name;
            MethodActualParameters = parameters;
        }

        //---------------------------------------------------------------------
        public Type[] GetMethodActualParametersType 
        { 
            get
            {
                if (MethodActualParameters == null)
                    return null;

                int size = MethodActualParameters.Length;
                Type[] Array = new Type[size];

                for (int i = 0; i < size; i++)
                    Array[i] = MethodActualParameters[i].GetType();

                return Array;
             }
        }
    }

    // Viene sollevata quando falliscono tutti i ritentavi di IRetryable
    //=========================================================================
    public class RetryException : ApplicationException
    {
        //---------------------------------------------------------------------
        public RetryException()
            :
            base()
        { }

        //---------------------------------------------------------------------
        public RetryException(string message)
            :
            base(message)
        { }

        //---------------------------------------------------------------------
        public RetryException(string message, Exception innerException)
            :
            base(message, innerException)
        { }
    }

    // Viene sollevata quando si vuole attivare la riesecuzione del metodo dopo tot. millisecondi
    //=========================================================================
    public class RetryAfterMsException : InvalidOperationException
    {
        public int NoMsSleep { get; private set; }

        //---------------------------------------------------------------------
        public RetryAfterMsException(int noMsSleep = 0)
            :
            base()
        {
            NoMsSleep = noMsSleep;
        }

        //---------------------------------------------------------------------
        public RetryAfterMsException(string message, int noMsSleep = 0)
            :
            base(message)
        {
            NoMsSleep = noMsSleep;
        }

        //---------------------------------------------------------------------
        public RetryAfterMsException(string message, Exception innerException, int noMsSleep = 0)
            :
            base(message, innerException)
        {
            NoMsSleep = noMsSleep;
        }
    }

    //=========================================================================
    public interface IRetryable
    {
        //---------------------------------------------------------------------
        T RetryGenerics<T>(IRetryableObject toRetry, Exception e) where T : struct;

        //---------------------------------------------------------------------
        string RetryMethodReturnString(IRetryableObject toRetry, Exception e);

        //---------------------------------------------------------------------
        void RetryMethodReturnVoid(IRetryableObject toRetry, Exception e);

        //---------------------------------------------------------------------
        int NoAttempt { get; set; }

        //---------------------------------------------------------------------
        int NoMsSleep { get; set; }

        //---------------------------------------------------------------------
        void Setup(Exception e);
    }

    //=========================================================================
    public class FakeRetry : IRetryable
    {
        //---------------------------------------------------------------------
        public T RetryGenerics<T>(IRetryableObject toRetry, Exception e) where T : struct
        {
            return default(T);
        }

        //---------------------------------------------------------------------
        public string RetryMethodReturnString(IRetryableObject toRetry, Exception e) 
        {
            return string.Empty;
        }

        //---------------------------------------------------------------------
        public void RetryMethodReturnVoid(IRetryableObject toRetry, Exception e)
        {
        }

        //---------------------------------------------------------------------
        public int NoAttempt { get; set; }

        //---------------------------------------------------------------------
        public int NoMsSleep { get; set; }

        //---------------------------------------------------------------------
        public void Setup(Exception e) { }
    }

    //=========================================================================
    public class AttemptRetry : IRetryable
    {
        /// <exception cref="System.NotSupportedException"/>
        /// <exception cref="Microarea.TaskBuilderNet.DataSynchroProviders.RetryException"/>
        //---------------------------------------------------------------------
        public T RetryGenerics<T>(IRetryableObject toRetry, Exception exc) where T : struct
        {
            MethodInfo mi = toRetry.CallerObject.GetType().GetMethod(toRetry.MethodName, toRetry.GetMethodActualParametersType);

            if (mi == null)
                throw new NotSupportedException(string.Format("The method {0} does not exist.", toRetry.MethodName));

            T result = default(T);

            try
            {
                result = (T)mi.Invoke(toRetry.CallerObject, toRetry.MethodActualParameters);
                return result;
            }
            catch (Exception e)
            {
                if (!e.GetType().Equals(exc.GetType()) ||
                    !e.Message.Equals(exc.Message))
                {
                    throw e;
                }
            }

            throw new RetryException("Attempt retry failed");
        }

        //---------------------------------------------------------------------
        public string RetryMethodReturnString(IRetryableObject toRetry, Exception exc)
        {
            MethodInfo mi = toRetry.CallerObject.GetType().GetMethod(toRetry.MethodName, toRetry.GetMethodActualParametersType);

            if (mi == null)
                throw new NotSupportedException(string.Format("The method {0} does not exist.", toRetry.MethodName));

            string result = string.Empty;

            try
            {
                result = (string)mi.Invoke(toRetry.CallerObject, toRetry.MethodActualParameters);
                return result;
            }
            catch (Exception e)
            {
                if (!e.GetType().Equals(exc.GetType()) ||
                    !e.Message.Equals(exc.Message))
                {
                    throw e;
                }
            }

            throw new RetryException("Attempt retry failed");
        }

        //---------------------------------------------------------------------
        public void RetryMethodReturnVoid(IRetryableObject toRetry, Exception exc)
        {
            MethodInfo mi = toRetry.CallerObject.GetType().GetMethod(toRetry.MethodName, toRetry.GetMethodActualParametersType);

            if (mi == null)
                throw new NotSupportedException(string.Format("The method {0} does not exist.", toRetry.MethodName));

            try
            {
                mi.Invoke(toRetry.CallerObject, toRetry.MethodActualParameters);
                return;
            }
            catch (Exception e)
            {
                if (!e.GetType().Equals(exc.GetType()) ||
                    !e.Message.Equals(exc.Message))
                {
                    throw e;
                }
            }

            throw new RetryException("Attempt retry failed");
        }

        //---------------------------------------------------------------------
        public int NoAttempt { get; set; }

        //---------------------------------------------------------------------
        public int NoMsSleep { get; set; }

        //---------------------------------------------------------------------
        public void Setup(Exception e) { }
    }

    // Questa classe si occupa di rieseguire un metodo chiamando prima di tutto la Connect di Infinity. 
    // Viene definito un numero di tentavi in modo da rieseguire il metodo un tot di volte prima di propagare l'eccezione.
    //=========================================================================
    public class ConnectAndAttemptRetry : IRetryable
    {
        public const int msSleep = 500;

        private AttemptRetry ForMethod = new AttemptRetry();
        private AttemptRetry ForConnect = new AttemptRetry();

        //---------------------------------------------------------------------
        public ConnectAndAttemptRetry()
        {
            NoAttempt = 3;
            NoMsSleep = msSleep;
        }

        //---------------------------------------------------------------------
        public T RetryGenerics<T>(IRetryableObject toRetry, Exception e) where T : struct
        {
            Debug.WriteLine("--------------------------------");
            Debug.WriteLine(string.Format("Use Retrier for {0} attemps\r\n", NoAttempt));

            for (int i = 0; i < NoAttempt; i++)
            {
                try
                {
                    if (NoMsSleep > 0)
                        Thread.Sleep(NoMsSleep);

                    Debug.WriteLine("--------------------------------");
                    Debug.WriteLine(string.Format("Attempt Retry InfinityConnect ({0} attemp)\r\n", i+1));

                    bool bConnect = ForConnect.RetryGenerics<bool>(new SimpleRetryableObject(toRetry.CallerObject, "Connect", new object[] { }), e);

                    if (bConnect)
                    {
                        Debug.WriteLine("--------------------------------");
                        Debug.WriteLine("InfinityConnect retry succeded.\r\n");
                        Debug.WriteLine(string.Format("Attempt Retry {0} ({1} attemp)\r\n", toRetry.MethodName, i + 1));

                        T result = ForMethod.RetryGenerics<T>(toRetry, e);
                        Debug.WriteLine(string.Format("Attempt Retry {0} succeeded\r\n", toRetry.MethodName));

                        return result;
                    }
                }
                catch (RetryException)
                {
                    continue;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            throw e;
        }

        //---------------------------------------------------------------------
        public string RetryMethodReturnString(IRetryableObject toRetry, Exception exc)
        {
            Debug.WriteLine("--------------------------------");
            Debug.WriteLine(string.Format("Use Retrier for {0} attemps\r\n", NoAttempt));

            for (int i = 0; i < NoAttempt; i++)
            {
                try
                {
                    if (NoMsSleep > 0)
                        Thread.Sleep(NoMsSleep);

                    Debug.WriteLine("--------------------------------");
                    Debug.WriteLine(string.Format("Attempt Retry InfinityConnect ({0} attemp)\r\n", i + 1));

                    bool bConnect = ForConnect.RetryGenerics<bool>(new SimpleRetryableObject(toRetry.CallerObject, "Connect", new object[] { }), exc);

                    if (bConnect)
                    {
                        Debug.WriteLine("--------------------------------");
                        Debug.WriteLine("InfinityConnect retry succeded.\r\n");
                        Debug.WriteLine(string.Format("Attempt Retry {0} ({1} attemp)\r\n", toRetry.MethodName, i + 1));

                        string result = ForMethod.RetryMethodReturnString(toRetry, exc);
                        Debug.WriteLine(string.Format("Attempt Retry {0} succeeded\r\n", toRetry.MethodName));

                        return result;
                    }
                }
                catch (RetryException)
                {
                    continue;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            throw exc;
        }

        //---------------------------------------------------------------------
        public void RetryMethodReturnVoid(IRetryableObject toRetry, Exception exc)
        {
            for (int i = 0; i < NoAttempt; i++)
            {
                try
                {
                    if (NoMsSleep > 0)
                        Thread.Sleep(NoMsSleep);

                   bool bConnect = ForConnect.RetryGenerics<bool>(new SimpleRetryableObject(toRetry.CallerObject, "Connect", new object[] { }), exc);

                    if (bConnect)
                    {
                        ForMethod.RetryMethodReturnString(toRetry, exc);
                        return;
                    }
                }
                catch (RetryException)
                {
                    continue;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            throw exc;
        }

        //---------------------------------------------------------------------
        public int NoAttempt { get; set; }

        //---------------------------------------------------------------------
        public int NoMsSleep { get; set; }

        /// <summary>
        /// Imposta il numero di millisecondi dopo il quale si vuole eseguire il tentativo.
        /// </summary>
        public void Setup(Exception e)
        {
            if (e.GetType().Equals(typeof(RetryAfterMsException)) && ((RetryAfterMsException)e).NoMsSleep > 0)
                NoMsSleep = ((RetryAfterMsException)e).NoMsSleep;
            else
                NoMsSleep = msSleep;
        }
    }

    //=========================================================================
    public static class RetryableFactory
    {
        //---------------------------------------------------------------------
        public static IRetryable GetRetryer(bool useRetry)
        {
            if (useRetry)
                return new ConnectAndAttemptRetry();

            return new FakeRetry();
        }
    }
}
