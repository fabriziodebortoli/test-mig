

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 6.00.0361 */
/* at Fri May 23 11:15:32 2003
 */
/* Compiler settings for .\XTender.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __XTender_h__
#define __XTender_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IXTender_FWD_DEFINED__
#define __IXTender_FWD_DEFINED__
typedef interface IXTender IXTender;
#endif 	/* __IXTender_FWD_DEFINED__ */


#ifndef ___ITenderEvents_FWD_DEFINED__
#define ___ITenderEvents_FWD_DEFINED__
typedef interface _ITenderEvents _ITenderEvents;
#endif 	/* ___ITenderEvents_FWD_DEFINED__ */


#ifndef __XMLTender_FWD_DEFINED__
#define __XMLTender_FWD_DEFINED__

#ifdef __cplusplus
typedef class XMLTender XMLTender;
#else
typedef struct XMLTender XMLTender;
#endif /* __cplusplus */

#endif 	/* __XMLTender_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 

void * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void * ); 

/* interface __MIDL_itf_XTender_0000 */
/* [local] */ 

typedef /* [public][public][public][helpstring][uuid] */  DECLSPEC_UUID("ED26B456-B5BA-41cc-867C-9CE4609A8B36") 
enum __MIDL___MIDL_itf_XTender_0000_0001
    {	E_UTF8	= 1,
	E_UTF16	= 2
    } 	EncodingType;

typedef /* [public][public][helpstring][uuid] */  DECLSPEC_UUID("AAFB8FC2-8248-4c9f-9632-0D59C07ACC3B") 
enum __MIDL___MIDL_itf_XTender_0000_0002
    {	PROG_ENV_CHANGED	= 1,
	PROG_CLASS_CHANGED	= 2,
	THREAD_TERMINATED	= 3,
	START_ENVELOPE	= 4,
	START_FILE	= 5
    } 	EventType;



extern RPC_IF_HANDLE __MIDL_itf_XTender_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_XTender_0000_v0_0_s_ifspec;

#ifndef __IXTender_INTERFACE_DEFINED__
#define __IXTender_INTERFACE_DEFINED__

/* interface IXTender */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IXTender;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("44C8E640-532D-461B-84ED-C5627E6F4739")
    IXTender : public IDispatch
    {
    public:
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_Site( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_Site( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_RepositoryServerURL( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_RepositoryServerURL( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_AppRoot( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_AppRoot( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE SendEnvelope( 
            /* [in] */ BSTR pEnvPath,
            /* [in] */ BSTR pEnvClass,
            /* [in] */ BSTR pEnvName,
            /* [out] */ long *ThreadID) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_TxSubPath( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_TxSubPath( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_RxSubPath( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_RxSubPath( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_Encoding( 
            /* [retval][out] */ EncodingType *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_Encoding( 
            /* [in] */ EncodingType newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_MaxAttempts( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_MaxAttempts( 
            /* [in] */ long newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_MaxTime( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_MaxTime( 
            /* [in] */ long newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_PendingSubPath( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_PendingSubPath( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_SuccessSubPath( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_SuccessSubPath( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_FailureSubPath( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_FailureSubPath( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_SupMaxAttempts( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_InfMaxAttempts( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_InfMaxTime( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_SupMaxTime( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_LastError( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetAvailableEnvelope( 
            /* [in] */ BSTR bstrEnvClass,
            /* [in] */ BSTR bstrSenderSite,
            /* [out] */ long *ThreadID,
            /* [out] */ BOOL *bThreadStarted) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE CanDestroyTender( 
            /* [out] */ BOOL *IsDestroyable) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetClassProgress( 
            /* [in] */ long ThreadID,
            /* [out] */ float *Percent) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetEnvelopeProgress( 
            /* [in] */ long ThreadID,
            /* [out] */ float *Percent) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetDescription( 
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetCurrentFile( 
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetCurrentEnvelope( 
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetAvailableSites( 
            /* [in] */ BSTR bstrEnvClass,
            /* [out] */ VARIANT *bstrSiteList) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Connect( 
            /* [in] */ IUnknown *pUnkSink,
            /* [in] */ long ThreadID,
            /* [out] */ long *pCookie) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Disconnect( 
            /* [in] */ long cookie,
            /* [in] */ long ThreadID) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE FireEvent( 
            /* [in] */ EventType e,
            /* [in] */ long ThreadID,
            /* [in] */ BOOL bSuccess) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE StopThread( 
            /* [in] */ long ThreadID) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE SendPendingData( 
            /* [in] */ BOOL bSerialized) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_Password( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_Password( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_UserID( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_UserID( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IsThreadAlive( 
            /* [in] */ long ThreadID,
            /* [out] */ BOOL *bAlive) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_Domain( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_Domain( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_MaxThreads( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_MaxThreads( 
            /* [in] */ long newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetThreadHandle( 
            /* [in] */ LONG ThreadID,
            /* [out] */ LONGLONG *ThreadHandle) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_AttemptsBeforeFailure( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_AttemptsBeforeFailure( 
            /* [in] */ long newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetCurrentClass( 
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE GetCurrentSenderSite( 
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE SendPendingEnvelope( 
            /* [in] */ BSTR EnvFolder,
            /* [in] */ BSTR EnvClass,
            /* [in] */ BSTR EnvName,
            /* [out] */ long *ThreadID) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_XSLFile( 
            /* [retval][out] */ BSTR *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_XSLFile( 
            /* [in] */ BSTR newVal) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE TestURL( void) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IsActivationValid( 
            /* [out] */ BOOL *bIsValid) = 0;
        
        virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_CompressSize( 
            /* [retval][out] */ long *pVal) = 0;
        
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_CompressSize( 
            /* [in] */ long newVal) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IXTenderVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IXTender * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IXTender * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IXTender * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IXTender * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IXTender * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IXTender * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IXTender * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_Site )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_Site )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_RepositoryServerURL )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_RepositoryServerURL )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_AppRoot )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_AppRoot )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *SendEnvelope )( 
            IXTender * This,
            /* [in] */ BSTR pEnvPath,
            /* [in] */ BSTR pEnvClass,
            /* [in] */ BSTR pEnvName,
            /* [out] */ long *ThreadID);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_TxSubPath )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_TxSubPath )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_RxSubPath )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_RxSubPath )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_Encoding )( 
            IXTender * This,
            /* [retval][out] */ EncodingType *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_Encoding )( 
            IXTender * This,
            /* [in] */ EncodingType newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_MaxAttempts )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_MaxAttempts )( 
            IXTender * This,
            /* [in] */ long newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_MaxTime )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_MaxTime )( 
            IXTender * This,
            /* [in] */ long newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_PendingSubPath )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_PendingSubPath )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_SuccessSubPath )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_SuccessSubPath )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_FailureSubPath )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_FailureSubPath )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_SupMaxAttempts )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_InfMaxAttempts )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_InfMaxTime )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_SupMaxTime )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_LastError )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetAvailableEnvelope )( 
            IXTender * This,
            /* [in] */ BSTR bstrEnvClass,
            /* [in] */ BSTR bstrSenderSite,
            /* [out] */ long *ThreadID,
            /* [out] */ BOOL *bThreadStarted);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *CanDestroyTender )( 
            IXTender * This,
            /* [out] */ BOOL *IsDestroyable);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetClassProgress )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ float *Percent);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetEnvelopeProgress )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ float *Percent);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetDescription )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetCurrentFile )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetCurrentEnvelope )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetAvailableSites )( 
            IXTender * This,
            /* [in] */ BSTR bstrEnvClass,
            /* [out] */ VARIANT *bstrSiteList);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Connect )( 
            IXTender * This,
            /* [in] */ IUnknown *pUnkSink,
            /* [in] */ long ThreadID,
            /* [out] */ long *pCookie);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Disconnect )( 
            IXTender * This,
            /* [in] */ long cookie,
            /* [in] */ long ThreadID);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *FireEvent )( 
            IXTender * This,
            /* [in] */ EventType e,
            /* [in] */ long ThreadID,
            /* [in] */ BOOL bSuccess);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *StopThread )( 
            IXTender * This,
            /* [in] */ long ThreadID);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *SendPendingData )( 
            IXTender * This,
            /* [in] */ BOOL bSerialized);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_Password )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_Password )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_UserID )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_UserID )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *IsThreadAlive )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ BOOL *bAlive);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_Domain )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_Domain )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_MaxThreads )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_MaxThreads )( 
            IXTender * This,
            /* [in] */ long newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetThreadHandle )( 
            IXTender * This,
            /* [in] */ LONG ThreadID,
            /* [out] */ LONGLONG *ThreadHandle);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_AttemptsBeforeFailure )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_AttemptsBeforeFailure )( 
            IXTender * This,
            /* [in] */ long newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetCurrentClass )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *GetCurrentSenderSite )( 
            IXTender * This,
            /* [in] */ long ThreadID,
            /* [out] */ BSTR *Description);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *SendPendingEnvelope )( 
            IXTender * This,
            /* [in] */ BSTR EnvFolder,
            /* [in] */ BSTR EnvClass,
            /* [in] */ BSTR EnvName,
            /* [out] */ long *ThreadID);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_XSLFile )( 
            IXTender * This,
            /* [retval][out] */ BSTR *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_XSLFile )( 
            IXTender * This,
            /* [in] */ BSTR newVal);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *TestURL )( 
            IXTender * This);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *IsActivationValid )( 
            IXTender * This,
            /* [out] */ BOOL *bIsValid);
        
        /* [helpstring][id][propget] */ HRESULT ( STDMETHODCALLTYPE *get_CompressSize )( 
            IXTender * This,
            /* [retval][out] */ long *pVal);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_CompressSize )( 
            IXTender * This,
            /* [in] */ long newVal);
        
        END_INTERFACE
    } IXTenderVtbl;

    interface IXTender
    {
        CONST_VTBL struct IXTenderVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IXTender_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IXTender_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IXTender_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IXTender_GetTypeInfoCount(This,pctinfo)	\
    (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo)

#define IXTender_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo)

#define IXTender_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)

#define IXTender_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)


#define IXTender_get_Site(This,pVal)	\
    (This)->lpVtbl -> get_Site(This,pVal)

#define IXTender_put_Site(This,newVal)	\
    (This)->lpVtbl -> put_Site(This,newVal)

#define IXTender_get_RepositoryServerURL(This,pVal)	\
    (This)->lpVtbl -> get_RepositoryServerURL(This,pVal)

#define IXTender_put_RepositoryServerURL(This,newVal)	\
    (This)->lpVtbl -> put_RepositoryServerURL(This,newVal)

#define IXTender_get_AppRoot(This,pVal)	\
    (This)->lpVtbl -> get_AppRoot(This,pVal)

#define IXTender_put_AppRoot(This,newVal)	\
    (This)->lpVtbl -> put_AppRoot(This,newVal)

#define IXTender_SendEnvelope(This,pEnvPath,pEnvClass,pEnvName,ThreadID)	\
    (This)->lpVtbl -> SendEnvelope(This,pEnvPath,pEnvClass,pEnvName,ThreadID)

#define IXTender_get_TxSubPath(This,pVal)	\
    (This)->lpVtbl -> get_TxSubPath(This,pVal)

#define IXTender_put_TxSubPath(This,newVal)	\
    (This)->lpVtbl -> put_TxSubPath(This,newVal)

#define IXTender_get_RxSubPath(This,pVal)	\
    (This)->lpVtbl -> get_RxSubPath(This,pVal)

#define IXTender_put_RxSubPath(This,newVal)	\
    (This)->lpVtbl -> put_RxSubPath(This,newVal)

#define IXTender_get_Encoding(This,pVal)	\
    (This)->lpVtbl -> get_Encoding(This,pVal)

#define IXTender_put_Encoding(This,newVal)	\
    (This)->lpVtbl -> put_Encoding(This,newVal)

#define IXTender_get_MaxAttempts(This,pVal)	\
    (This)->lpVtbl -> get_MaxAttempts(This,pVal)

#define IXTender_put_MaxAttempts(This,newVal)	\
    (This)->lpVtbl -> put_MaxAttempts(This,newVal)

#define IXTender_get_MaxTime(This,pVal)	\
    (This)->lpVtbl -> get_MaxTime(This,pVal)

#define IXTender_put_MaxTime(This,newVal)	\
    (This)->lpVtbl -> put_MaxTime(This,newVal)

#define IXTender_get_PendingSubPath(This,pVal)	\
    (This)->lpVtbl -> get_PendingSubPath(This,pVal)

#define IXTender_put_PendingSubPath(This,newVal)	\
    (This)->lpVtbl -> put_PendingSubPath(This,newVal)

#define IXTender_get_SuccessSubPath(This,pVal)	\
    (This)->lpVtbl -> get_SuccessSubPath(This,pVal)

#define IXTender_put_SuccessSubPath(This,newVal)	\
    (This)->lpVtbl -> put_SuccessSubPath(This,newVal)

#define IXTender_get_FailureSubPath(This,pVal)	\
    (This)->lpVtbl -> get_FailureSubPath(This,pVal)

#define IXTender_put_FailureSubPath(This,newVal)	\
    (This)->lpVtbl -> put_FailureSubPath(This,newVal)

#define IXTender_get_SupMaxAttempts(This,pVal)	\
    (This)->lpVtbl -> get_SupMaxAttempts(This,pVal)

#define IXTender_get_InfMaxAttempts(This,pVal)	\
    (This)->lpVtbl -> get_InfMaxAttempts(This,pVal)

#define IXTender_get_InfMaxTime(This,pVal)	\
    (This)->lpVtbl -> get_InfMaxTime(This,pVal)

#define IXTender_get_SupMaxTime(This,pVal)	\
    (This)->lpVtbl -> get_SupMaxTime(This,pVal)

#define IXTender_get_LastError(This,pVal)	\
    (This)->lpVtbl -> get_LastError(This,pVal)

#define IXTender_GetAvailableEnvelope(This,bstrEnvClass,bstrSenderSite,ThreadID,bThreadStarted)	\
    (This)->lpVtbl -> GetAvailableEnvelope(This,bstrEnvClass,bstrSenderSite,ThreadID,bThreadStarted)

#define IXTender_CanDestroyTender(This,IsDestroyable)	\
    (This)->lpVtbl -> CanDestroyTender(This,IsDestroyable)

#define IXTender_GetClassProgress(This,ThreadID,Percent)	\
    (This)->lpVtbl -> GetClassProgress(This,ThreadID,Percent)

#define IXTender_GetEnvelopeProgress(This,ThreadID,Percent)	\
    (This)->lpVtbl -> GetEnvelopeProgress(This,ThreadID,Percent)

#define IXTender_GetDescription(This,ThreadID,Description)	\
    (This)->lpVtbl -> GetDescription(This,ThreadID,Description)

#define IXTender_GetCurrentFile(This,ThreadID,Description)	\
    (This)->lpVtbl -> GetCurrentFile(This,ThreadID,Description)

#define IXTender_GetCurrentEnvelope(This,ThreadID,Description)	\
    (This)->lpVtbl -> GetCurrentEnvelope(This,ThreadID,Description)

#define IXTender_GetAvailableSites(This,bstrEnvClass,bstrSiteList)	\
    (This)->lpVtbl -> GetAvailableSites(This,bstrEnvClass,bstrSiteList)

#define IXTender_Connect(This,pUnkSink,ThreadID,pCookie)	\
    (This)->lpVtbl -> Connect(This,pUnkSink,ThreadID,pCookie)

#define IXTender_Disconnect(This,cookie,ThreadID)	\
    (This)->lpVtbl -> Disconnect(This,cookie,ThreadID)

#define IXTender_FireEvent(This,e,ThreadID,bSuccess)	\
    (This)->lpVtbl -> FireEvent(This,e,ThreadID,bSuccess)

#define IXTender_StopThread(This,ThreadID)	\
    (This)->lpVtbl -> StopThread(This,ThreadID)

#define IXTender_SendPendingData(This,bSerialized)	\
    (This)->lpVtbl -> SendPendingData(This,bSerialized)

#define IXTender_get_Password(This,pVal)	\
    (This)->lpVtbl -> get_Password(This,pVal)

#define IXTender_put_Password(This,newVal)	\
    (This)->lpVtbl -> put_Password(This,newVal)

#define IXTender_get_UserID(This,pVal)	\
    (This)->lpVtbl -> get_UserID(This,pVal)

#define IXTender_put_UserID(This,newVal)	\
    (This)->lpVtbl -> put_UserID(This,newVal)

#define IXTender_IsThreadAlive(This,ThreadID,bAlive)	\
    (This)->lpVtbl -> IsThreadAlive(This,ThreadID,bAlive)

#define IXTender_get_Domain(This,pVal)	\
    (This)->lpVtbl -> get_Domain(This,pVal)

#define IXTender_put_Domain(This,newVal)	\
    (This)->lpVtbl -> put_Domain(This,newVal)

#define IXTender_get_MaxThreads(This,pVal)	\
    (This)->lpVtbl -> get_MaxThreads(This,pVal)

#define IXTender_put_MaxThreads(This,newVal)	\
    (This)->lpVtbl -> put_MaxThreads(This,newVal)

#define IXTender_GetThreadHandle(This,ThreadID,ThreadHandle)	\
    (This)->lpVtbl -> GetThreadHandle(This,ThreadID,ThreadHandle)

#define IXTender_get_AttemptsBeforeFailure(This,pVal)	\
    (This)->lpVtbl -> get_AttemptsBeforeFailure(This,pVal)

#define IXTender_put_AttemptsBeforeFailure(This,newVal)	\
    (This)->lpVtbl -> put_AttemptsBeforeFailure(This,newVal)

#define IXTender_GetCurrentClass(This,ThreadID,Description)	\
    (This)->lpVtbl -> GetCurrentClass(This,ThreadID,Description)

#define IXTender_GetCurrentSenderSite(This,ThreadID,Description)	\
    (This)->lpVtbl -> GetCurrentSenderSite(This,ThreadID,Description)

#define IXTender_SendPendingEnvelope(This,EnvFolder,EnvClass,EnvName,ThreadID)	\
    (This)->lpVtbl -> SendPendingEnvelope(This,EnvFolder,EnvClass,EnvName,ThreadID)

#define IXTender_get_XSLFile(This,pVal)	\
    (This)->lpVtbl -> get_XSLFile(This,pVal)

#define IXTender_put_XSLFile(This,newVal)	\
    (This)->lpVtbl -> put_XSLFile(This,newVal)

#define IXTender_TestURL(This)	\
    (This)->lpVtbl -> TestURL(This)

#define IXTender_IsActivationValid(This,bIsValid)	\
    (This)->lpVtbl -> IsActivationValid(This,bIsValid)

#define IXTender_get_CompressSize(This,pVal)	\
    (This)->lpVtbl -> get_CompressSize(This,pVal)

#define IXTender_put_CompressSize(This,newVal)	\
    (This)->lpVtbl -> put_CompressSize(This,newVal)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_Site_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_Site_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_Site_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_Site_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_RepositoryServerURL_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_RepositoryServerURL_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_RepositoryServerURL_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_RepositoryServerURL_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_AppRoot_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_AppRoot_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_AppRoot_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_AppRoot_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_SendEnvelope_Proxy( 
    IXTender * This,
    /* [in] */ BSTR pEnvPath,
    /* [in] */ BSTR pEnvClass,
    /* [in] */ BSTR pEnvName,
    /* [out] */ long *ThreadID);


void __RPC_STUB IXTender_SendEnvelope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_TxSubPath_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_TxSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_TxSubPath_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_TxSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_RxSubPath_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_RxSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_RxSubPath_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_RxSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_Encoding_Proxy( 
    IXTender * This,
    /* [retval][out] */ EncodingType *pVal);


void __RPC_STUB IXTender_get_Encoding_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_Encoding_Proxy( 
    IXTender * This,
    /* [in] */ EncodingType newVal);


void __RPC_STUB IXTender_put_Encoding_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_MaxAttempts_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_MaxAttempts_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_MaxAttempts_Proxy( 
    IXTender * This,
    /* [in] */ long newVal);


void __RPC_STUB IXTender_put_MaxAttempts_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_MaxTime_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_MaxTime_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_MaxTime_Proxy( 
    IXTender * This,
    /* [in] */ long newVal);


void __RPC_STUB IXTender_put_MaxTime_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_PendingSubPath_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_PendingSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_PendingSubPath_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_PendingSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_SuccessSubPath_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_SuccessSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_SuccessSubPath_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_SuccessSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_FailureSubPath_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_FailureSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_FailureSubPath_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_FailureSubPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_SupMaxAttempts_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_SupMaxAttempts_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_InfMaxAttempts_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_InfMaxAttempts_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_InfMaxTime_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_InfMaxTime_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_SupMaxTime_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_SupMaxTime_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_LastError_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_LastError_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetAvailableEnvelope_Proxy( 
    IXTender * This,
    /* [in] */ BSTR bstrEnvClass,
    /* [in] */ BSTR bstrSenderSite,
    /* [out] */ long *ThreadID,
    /* [out] */ BOOL *bThreadStarted);


void __RPC_STUB IXTender_GetAvailableEnvelope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_CanDestroyTender_Proxy( 
    IXTender * This,
    /* [out] */ BOOL *IsDestroyable);


void __RPC_STUB IXTender_CanDestroyTender_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetClassProgress_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ float *Percent);


void __RPC_STUB IXTender_GetClassProgress_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetEnvelopeProgress_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ float *Percent);


void __RPC_STUB IXTender_GetEnvelopeProgress_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetDescription_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ BSTR *Description);


void __RPC_STUB IXTender_GetDescription_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetCurrentFile_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ BSTR *Description);


void __RPC_STUB IXTender_GetCurrentFile_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetCurrentEnvelope_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ BSTR *Description);


void __RPC_STUB IXTender_GetCurrentEnvelope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetAvailableSites_Proxy( 
    IXTender * This,
    /* [in] */ BSTR bstrEnvClass,
    /* [out] */ VARIANT *bstrSiteList);


void __RPC_STUB IXTender_GetAvailableSites_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_Connect_Proxy( 
    IXTender * This,
    /* [in] */ IUnknown *pUnkSink,
    /* [in] */ long ThreadID,
    /* [out] */ long *pCookie);


void __RPC_STUB IXTender_Connect_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_Disconnect_Proxy( 
    IXTender * This,
    /* [in] */ long cookie,
    /* [in] */ long ThreadID);


void __RPC_STUB IXTender_Disconnect_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_FireEvent_Proxy( 
    IXTender * This,
    /* [in] */ EventType e,
    /* [in] */ long ThreadID,
    /* [in] */ BOOL bSuccess);


void __RPC_STUB IXTender_FireEvent_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_StopThread_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID);


void __RPC_STUB IXTender_StopThread_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_SendPendingData_Proxy( 
    IXTender * This,
    /* [in] */ BOOL bSerialized);


void __RPC_STUB IXTender_SendPendingData_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_Password_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_Password_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_Password_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_Password_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_UserID_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_UserID_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_UserID_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_UserID_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_IsThreadAlive_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ BOOL *bAlive);


void __RPC_STUB IXTender_IsThreadAlive_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_Domain_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_Domain_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_Domain_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_Domain_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_MaxThreads_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_MaxThreads_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_MaxThreads_Proxy( 
    IXTender * This,
    /* [in] */ long newVal);


void __RPC_STUB IXTender_put_MaxThreads_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetThreadHandle_Proxy( 
    IXTender * This,
    /* [in] */ LONG ThreadID,
    /* [out] */ LONGLONG *ThreadHandle);


void __RPC_STUB IXTender_GetThreadHandle_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_AttemptsBeforeFailure_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_AttemptsBeforeFailure_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_AttemptsBeforeFailure_Proxy( 
    IXTender * This,
    /* [in] */ long newVal);


void __RPC_STUB IXTender_put_AttemptsBeforeFailure_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetCurrentClass_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ BSTR *Description);


void __RPC_STUB IXTender_GetCurrentClass_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_GetCurrentSenderSite_Proxy( 
    IXTender * This,
    /* [in] */ long ThreadID,
    /* [out] */ BSTR *Description);


void __RPC_STUB IXTender_GetCurrentSenderSite_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_SendPendingEnvelope_Proxy( 
    IXTender * This,
    /* [in] */ BSTR EnvFolder,
    /* [in] */ BSTR EnvClass,
    /* [in] */ BSTR EnvName,
    /* [out] */ long *ThreadID);


void __RPC_STUB IXTender_SendPendingEnvelope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_XSLFile_Proxy( 
    IXTender * This,
    /* [retval][out] */ BSTR *pVal);


void __RPC_STUB IXTender_get_XSLFile_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_XSLFile_Proxy( 
    IXTender * This,
    /* [in] */ BSTR newVal);


void __RPC_STUB IXTender_put_XSLFile_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_TestURL_Proxy( 
    IXTender * This);


void __RPC_STUB IXTender_TestURL_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IXTender_IsActivationValid_Proxy( 
    IXTender * This,
    /* [out] */ BOOL *bIsValid);


void __RPC_STUB IXTender_IsActivationValid_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE IXTender_get_CompressSize_Proxy( 
    IXTender * This,
    /* [retval][out] */ long *pVal);


void __RPC_STUB IXTender_get_CompressSize_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE IXTender_put_CompressSize_Proxy( 
    IXTender * This,
    /* [in] */ long newVal);


void __RPC_STUB IXTender_put_CompressSize_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IXTender_INTERFACE_DEFINED__ */



#ifndef __XTenderLib_LIBRARY_DEFINED__
#define __XTenderLib_LIBRARY_DEFINED__

/* library XTenderLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_XTenderLib;

#ifndef ___ITenderEvents_DISPINTERFACE_DEFINED__
#define ___ITenderEvents_DISPINTERFACE_DEFINED__

/* dispinterface _ITenderEvents */
/* [helpstring][uuid] */ 


EXTERN_C const IID DIID__ITenderEvents;

#if defined(__cplusplus) && !defined(CINTERFACE)

    MIDL_INTERFACE("568467E8-3EE2-4655-B847-64A89847E6E9")
    _ITenderEvents : public IDispatch
    {
    };
    
#else 	/* C style interface */

    typedef struct _ITenderEventsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            _ITenderEvents * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            _ITenderEvents * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            _ITenderEvents * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            _ITenderEvents * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            _ITenderEvents * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            _ITenderEvents * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            _ITenderEvents * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        END_INTERFACE
    } _ITenderEventsVtbl;

    interface _ITenderEvents
    {
        CONST_VTBL struct _ITenderEventsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define _ITenderEvents_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define _ITenderEvents_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define _ITenderEvents_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define _ITenderEvents_GetTypeInfoCount(This,pctinfo)	\
    (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo)

#define _ITenderEvents_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo)

#define _ITenderEvents_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)

#define _ITenderEvents_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)

#endif /* COBJMACROS */


#endif 	/* C style interface */


#endif 	/* ___ITenderEvents_DISPINTERFACE_DEFINED__ */


EXTERN_C const CLSID CLSID_XMLTender;

#ifdef __cplusplus

class DECLSPEC_UUID("854367EA-9FF0-48D4-BD1D-5F4916A0CC64")
XMLTender;
#endif
#endif /* __XTenderLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

unsigned long             __RPC_USER  VARIANT_UserSize(     unsigned long *, unsigned long            , VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserMarshal(  unsigned long *, unsigned char *, VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserUnmarshal(unsigned long *, unsigned char *, VARIANT * ); 
void                      __RPC_USER  VARIANT_UserFree(     unsigned long *, VARIANT * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


