#include "StdAfx.h"

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LineFile.h>
#include <TbGeneric\TBStrings.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\LoginContext.h>

#include "PostaLiteNet.h"

CString CPostaLiteNet::GetPostaliteCountryFromIsoCode (const CString& isoCode)
{
	return Microarea::TaskBuilderNet::Core::Generic::Functions::GetPostaliteCountryFromIsoCode(gcnew System::String(isoCode));
}


BOOL CPostaLiteNet::SavePdfBlob 
	(
		CString sConnectionString,
		CString sFileName,
		int nMsgID,
		CDiagnostic* pDiagnostic/* = NULL*/
	)
{
	CString sQry;
	sQry.Format(_T("UPDATE TB_MsgQueue SET DocImage = @ImageFile WHERE MsgID = %d"), nMsgID);

	try
    {
		System::String^ sConn =  gcnew System::String(sConnectionString);
		System::Data::SqlClient::SqlConnection^ myConnection = gcnew System::Data::SqlClient::SqlConnection(sConn);
		myConnection->Open();
	
		System::Data::SqlClient::SqlCommand^ myCommand = myConnection->CreateCommand();
    
		myCommand->CommandText = gcnew System::String(sQry);

		myCommand->Parameters->Add
			(
				gcnew System::Data::SqlClient::SqlParameter
					("@ImageFile", System::IO::File::ReadAllBytes(gcnew System::String(sFileName)))
			);

		myCommand->ExecuteNonQuery();
		delete myCommand;
    
		myConnection->Close();
		delete myConnection;
	}
	catch (System::Exception^ ex)
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(ex->Message));
		delete ex;
	}
	catch (CException* pE)
	{
		if (pDiagnostic)
			pDiagnostic->Add(pE);
		pE->Delete();
	}
	catch (...)
	{
		if (pDiagnostic)
			pDiagnostic->Add(_TB("Internal error on read PostaLite msg's document"));
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CPostaLiteNet::ReadPdfBlob 
	(
		CString sConnectionString,
		CString sFileName,
		int nMsgID,
		CDiagnostic* pDiagnostic/* = NULL*/
	)
{
	CString sQry;
	sQry.Format(_T("SELECT DocImage FROM TB_MsgQueue WHERE MsgID = %d"), nMsgID);

	try
    {
		System::String^ sConn =  gcnew System::String(sConnectionString);
		System::Data::SqlClient::SqlConnection^ myConnection = gcnew System::Data::SqlClient::SqlConnection(sConn);
		myConnection->Open();
	
		System::Data::SqlClient::SqlCommand^ myCommand = myConnection->CreateCommand();
    
		myCommand->CommandText = gcnew System::String(sQry);

		cli::array<System::Byte, 1>^ contentArray = (cli::array<System::Byte,1>^) myCommand->ExecuteScalar();
		delete myCommand;
    
		myConnection->Close();
		delete myConnection;
		
		System::IO::FileStream^ s = gcnew System::IO::FileStream(gcnew System::String(sFileName), System::IO::FileMode::OpenOrCreate);
		s->Write(contentArray, 0, contentArray->Length);
		s->Close();
		delete s;
		delete contentArray;

		//System::Data::SqlClient::SqlDataReader^ reader = myCommand->ExecuteReader();
		//if (reader->Read())
		//{
		//}
		//reader->Close();
		//delete reader;
	}
	catch (System::Exception^ ex)
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(ex->Message));
		delete ex;
		return FALSE;
	}
	catch (CException* pE)
	{
		if (pDiagnostic)
			pDiagnostic->Add(pE);
		pE->Delete();
		return FALSE;
	}
	catch (...)
	{
		if (pDiagnostic)
			pDiagnostic->Add(_TB("Internal error on read PostaLite msg's document"));
		return FALSE;
	}

	return TRUE;
}