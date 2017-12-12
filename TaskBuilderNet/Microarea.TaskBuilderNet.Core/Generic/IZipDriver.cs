using System;
using System.IO;

namespace Microarea.TaskBuilderNet.Core.Generic
{
    internal interface IZipDriver
    {
        string FileName { get; }
        bool IsOpened();

        bool AddFile(string file, Uri zippedUri, string fileTitle);
        bool AddFolder(string path, bool recursive, string relativePathFrom);
        bool AddStream(string name, Stream stream);
        void Close();
        bool ExistsEntry(Uri uri);
        bool ExtractAll(string outputPath);
        bool ExtractFile(string file, string outputPath);
        Stream ExtractFileAsStream(string file);
        bool ExtractFolder(string path, string outputPath, bool recursive);
        CompressedEntry[] GetAllEntries();
        CompressedEntry[] GetEntries(Uri relativeUri, CompressedEntry.EntryType type, bool recursive);
        CompressedEntry GetEntry(Uri uri);
        int GetNrOfEntries();
        Uri GetRelativeUri(string file, string relativePathFrom);
        bool Open(string fileName, CompressedFile.OpenMode openMode);
        bool Open(Stream stream, CompressedFile.OpenMode openMode);
    }
}