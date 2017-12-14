#include "stdafx.h"
#include "TBLoaderRestService.h"

#include <TbNameSolver\JsonSerializer.h>
#include <TbGenlib\CEFClasses.h>
#include <TbGeneric\CollateCultureFunctions.h>

using namespace System;
using namespace System::IO;
using namespace System::Text;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Specialized;
using namespace System::Collections::Generic;
using namespace Newtonsoft::Json::Linq;
using namespace Newtonsoft::Json;
using namespace Microarea::TaskBuilderNet::Core::Generic;
CTBLoaderRestService::CTBLoaderRestService()
{
}

//-----------------------------------------------------------------------------
bool CTBLoaderRestService::ProcessRequest(HttpListenerRequest^ request, HttpListenerResponse^ response)
{
	try
	{
		String^ subPath = request->Url->LocalPath;
		CTBRequestHandlerObj* pHandler = GetRequestHandlerByUrl(CString(subPath));
		if (!pHandler)
			return false;

		String^ menuString = gcnew System::String(pHandler->GetPath());
		String^ command = subPath->Substring(menuString->Length);//salto /tbloader/
		// converts parameters to json if needed
		CTBResponse tbResponse;

		CMap<CString, LPCTSTR, CString, LPCTSTR> requestHeaders;
		NameValueCollection^ headers = request->Headers;
		for each(String^ key in  headers)
		{
			requestHeaders[CString(key)] = headers[key];
		}

		if (pHandler->PreProcessRequest(CString(request->HttpMethod), requestHeaders, CString(command), tbResponse))
		{
			CNameValueCollection params;
			for each(String^ key in  request->QueryString)
			{
				params.Add(key, request->QueryString[key]);
			}
		
			String^ contentType = request->ContentType;
			NameValueCollection^ coll = nullptr;
			if (contentType == "application/json")
			{
				coll = JsonUtilities::InputStreamJsonToNameValueCollection(request->InputStream);
			}
			else
			{
				System::IO::Stream^ body = request->InputStream;
				System::IO::StreamReader^ reader = gcnew System::IO::StreamReader(body, request->ContentEncoding);
				String^ content = reader->ReadToEnd();
				coll = System::Web::HttpUtility::ParseQueryString(content);
				delete reader;
			}

			for each(String^ key in  coll)
			{
				params.Add(key, coll[key]);
			}

			for each(Cookie^  cookie in  request->Cookies)
			{
				params.Add(cookie->Name, cookie->Value);
			}
			pHandler->ProcessRequest(CString(command), params, tbResponse);
		}
		response->ContentEncoding = Encoding::UTF8;
		response->ContentType = gcnew String(tbResponse.GetMimeType());

		const CNameValueCollection& cookies = tbResponse.GetCookies();
		for (int i = 0; i < cookies.GetSize(); i++)
		{
			CNameValuePair* p = cookies.GetAt(i);
			CString sCookieType = p->GetName() + _T("=") + p->GetValue();
			CStringA sCookieTypeA = UnicodeToUTF8(sCookieType) + "; Path=/;";

			response->SetCookie(gcnew Cookie(gcnew String(p->GetName()), gcnew String(p->GetValue()), "/" ) ); //(HttpHeaderSetCookie, sCookieTypeA, sCookieTypeA.GetLength(), TRUE);

		}

		CString sRedirect = tbResponse.GetRedirectUrl();
		if (!sRedirect.IsEmpty())
		{
			response->Headers->Add("Location", gcnew String(sRedirect));
			response->StatusCode = 303;
			response->StatusDescription = "Login Redirect";
			return true;
		}
		response->StatusCode = tbResponse.GetStatus();

		const CNameValueCollection& oResponseHeaders = tbResponse.GetHeaders();
		for (int iHeader = 0; iHeader < oResponseHeaders.GetSize(); iHeader++)
		{
			CNameValuePair* p = oResponseHeaders.GetAt(iHeader);
			response->AddHeader(gcnew String(p->GetName()), gcnew String(p->GetValue()));
		}


		// Get a response stream and write the response to it.
		response->ContentLength64 = tbResponse.GetLength();
		if (response->ContentLength64 != 0)
		{
			array<Byte>^ buffer = gcnew array< Byte >(tbResponse.GetLength());

			Marshal::Copy((IntPtr)tbResponse.GetData(), buffer, 0, tbResponse.GetLength());

			Stream^ output = response->OutputStream;
			output->Write(buffer, 0, buffer->Length);
			// You must close the output stream.
			output->Close();
		}
		return true;
	}
	catch (Exception^ ex)
	{
		Stream^ output = response->OutputStream;
		String^ error = ex->Message;
		array<Byte>^ buffer = Encoding::UTF8->GetBytes(error);
		response->ContentLength64 = buffer->Length;
		output->Write(buffer, 0, buffer->Length);
		// You must close the output stream.
		output->Close();
		return true;
	}
}



