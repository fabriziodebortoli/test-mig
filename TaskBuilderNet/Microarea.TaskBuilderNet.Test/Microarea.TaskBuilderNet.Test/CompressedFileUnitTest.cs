using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;

namespace Microarea.TaskBuilderNet.Test
{
    [TestClass]
    public class CompressedFileUnitTest
    {
        [TestMethod, Ignore]
        public void AddFolder_should_zip_a_folder()
        {
            var fileOut = new FileInfo(Path.GetTempFileName());
            if (fileOut.Exists)
            {
                fileOut.Delete();
            }

            var cf = new CompressedFile(CompressedFile.Version.V2);
            if (cf.Open(fileOut.FullName, CompressedFile.OpenMode.CreateAlways))
            {
                cf.AddFolder("C:\\Scambi\\prova", true, "C:\\Scambi");
                cf.Close();
            }

            Assert.IsTrue(fileOut.Exists);
        }
        [TestMethod, Ignore]
        public void AddFolder_should_zip_a_folder_with_no_relative_path()
        {
            var fileOut = new FileInfo(Path.GetTempFileName());
            if (fileOut.Exists)
            {
                fileOut.Delete();
            }

            var cf = new CompressedFile(CompressedFile.Version.V2);
            if (cf.Open(fileOut.FullName, CompressedFile.OpenMode.CreateAlways))
            {
                cf.AddFolder("C:\\Scambi\\prova", true);
                cf.Close();
            }

            Assert.IsTrue(fileOut.Exists);
        }

        [TestMethod, Ignore]
        public void ExtractAll_should_extract_all_files()
        {
            var fileOut = new FileInfo(Path.GetTempFileName());
            if (fileOut.Exists)
            {
                fileOut.Delete();
            }

            var cf = new CompressedFile(CompressedFile.Version.V2);
            if (cf.Open(fileOut.FullName, CompressedFile.OpenMode.CreateAlways))
            {
                cf.AddFolder("C:\\Scambi\\prova", true, "C:\\Scambi");
                cf.Close();
            }

            cf = new CompressedFile(CompressedFile.Version.V2);
            var outputFolder = Path.Combine(Path.GetTempPath(), new Random().Next(1000, 9999).ToString());
            if (cf.Open(fileOut.FullName, CompressedFile.OpenMode.Read))
            {
                cf.ExtractAll(outputFolder);
            }
        }

        [TestMethod, Ignore]
        public void AddFolder_should_zip_a_folder_from_stream()
        {
            var fileOut = new FileInfo(Path.GetTempFileName());
            if (fileOut.Exists)
            {
                fileOut.Delete();
            }

            var cf = new CompressedFile(CompressedFile.Version.V2);
            if (cf.Open(File.OpenWrite(fileOut.FullName), CompressedFile.OpenMode.CreateAlways))
            {
                cf.AddFolder("C:\\Scambi\\prova", true, "C:\\Scambi");
                cf.Close();
            }

            Assert.IsTrue(fileOut.Exists);
        }

        [TestMethod, Ignore]
        public void ExtractAll_should_extract_all_files_from_stream()
        {
            var cf = new CompressedFile(CompressedFile.Version.V2);
            var outputStream = new MemoryStream();
            if (cf.Open(outputStream, CompressedFile.OpenMode.CreateAlways))
            {
                cf.AddFolder("C:\\Scambi\\prova", true, "C:\\Scambi");
                cf.Close();
            }
            var buffer = outputStream.ToArray();
            outputStream.Dispose();

            cf = new CompressedFile(CompressedFile.Version.V2);
            var inputStream = new MemoryStream(buffer);
            var outputFolder = Path.Combine(Path.GetTempPath(), new Random().Next(1000, 9999).ToString());
            if (cf.Open(inputStream, CompressedFile.OpenMode.Read))
            {
                cf.ExtractAll(outputFolder);
            }
            inputStream.Dispose();
        }

        [TestMethod, Ignore]
        public void ExtractAll_SampleData()
        {
            var cf = new CompressedFile("C:\\Users\\Canessa\\Desktop\\ultim_20170619.zip", CompressedFile.OpenMode.Read, CompressedFile.Version.V2);
            var outputFolder = Path.Combine(Path.GetTempPath(), new Random().Next(1000, 9999).ToString());
            cf.ExtractAll(outputFolder);
        }
    }
}
