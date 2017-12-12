#include "stdafx.h"

#include "ZCompress.h"

using namespace System;
using namespace Microarea::TaskBuilderNet::Core::Generic;

//---------------------------------------------------------------------------
BOOL CompressFile(const CString& strFileIn, const CString& strFileOut, const CString& strTitle, CString* sError /*NULL*/)
{
	System::String^ fileOut = gcnew System::String(strFileOut);

	try
	{
		CompressedFile^ cf = gcnew CompressedFile ();
		if (cf->Open(fileOut, CompressedFile::OpenMode::CreateAlways))
		{
			cf->AddFileWithTitle(gcnew System::String(strFileIn), gcnew System::String(strTitle));
			cf->Close();
		}
	}
    catch (CompressionException^ e)
    {
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
    }

	return TRUE;
}

//-------------------------------------------------------------------------------------------------------------
BOOL UncompressFile(const CString& strFileIn, const CString& strPathOut, CString* sError /*NULL*/)
{
	try
	{
		CompressedFile^ cf = gcnew CompressedFile ();
		if (cf->Open(gcnew System::String(strFileIn), CompressedFile::OpenMode::Read))
		{
			cf->ExtractAll(gcnew System::String(strPathOut));
			cf->Close();
		}
	}
    catch (CompressionException^ e)
    {
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
    }

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CompressFolder(const CString& strPath, const CString& strFileOut, BOOL bRecursive /*= TRUE*/, const CString& strRelativePathFrom /*= _T("")*/, CString* sError /*NULL*/)
{
	System::String^ fileOut = gcnew System::String(strFileOut);

	try
	{
		CompressedFile^ cf = gcnew CompressedFile();
		if (cf->Open(fileOut, CompressedFile::OpenMode::CreateAlways))
		{
			cf->AddFolder(gcnew System::String(strPath), bRecursive == TRUE, gcnew System::String(strRelativePathFrom));
			cf->Close();
		}
	}
	catch (CompressionException^ e)
	{
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL UncompressFolder(const CString & strPath, const CString & strOutputPath, CString* sError /*= NULL*/)
{
	try
	{
		CompressedFile^ cf = gcnew CompressedFile();
		if (cf->Open(gcnew System::String(strPath), CompressedFile::OpenMode::Read))
		{
			cf->ExtractAll(gcnew System::String(strOutputPath));
			cf->Close();
		}
	}
	catch (CompressionException^ e)
	{
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CompressFileV2(const CString& strFileIn, const CString& strFileOut, const CString& strTitle, CString* sError /*NULL*/)
{
	System::String^ fileOut = gcnew System::String(strFileOut);

	try
	{
		CompressedFile^ cf = gcnew CompressedFile(CompressedFile::Version::V2);
		if (cf->Open(fileOut, CompressedFile::OpenMode::CreateAlways))
		{
			cf->AddFileWithTitle(gcnew System::String(strFileIn), gcnew System::String(strTitle));
			cf->Close();
		}
	}
	catch (CompressionException^ e)
	{
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
	}

	return TRUE;
}

//-------------------------------------------------------------------------------------------------------------
BOOL UncompressFileV2(const CString& strFileIn, const CString& strPathOut, CString* sError /*NULL*/)
{
	try
	{
		CompressedFile^ cf = gcnew CompressedFile(CompressedFile::Version::V2);
		if (cf->Open(gcnew System::String(strFileIn), CompressedFile::OpenMode::Read))
		{
			cf->ExtractAll(gcnew System::String(strPathOut));
			cf->Close();
		}
	}
	catch (CompressionException^ e)
	{
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CompressFolderV2(const CString& strPath, const CString& strFileOut, BOOL bRecursive /*= TRUE*/, const CString& strRelativePathFrom /*= _T("")*/, CString* sError /*NULL*/)
{
	System::String^ fileOut = gcnew System::String(strFileOut);

	try
	{
		CompressedFile^ cf = gcnew CompressedFile(CompressedFile::Version::V2);
		if (cf->Open(fileOut, CompressedFile::OpenMode::CreateAlways))
		{
			cf->AddFolder(gcnew System::String(strPath), bRecursive == TRUE, gcnew System::String(strRelativePathFrom));
			cf->Close();
		}
	}
	catch (CompressionException^ e)
	{
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL UncompressFolderV2(const CString & strPath, const CString & strOutputPath, CString* sError /*= NULL*/)
{
	try
	{
		CompressedFile^ cf = gcnew CompressedFile(CompressedFile::Version::V2);
		if (cf->Open(gcnew System::String(strPath), CompressedFile::OpenMode::Read))
		{
			cf->ExtractAll(gcnew System::String(strOutputPath));
			cf->Close();
		}
	}
	catch (CompressionException^ e)
	{
		if (sError != NULL)
			*sError = CString(e->Message);
		return false;
	}

	return TRUE;
}
