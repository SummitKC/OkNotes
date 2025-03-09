using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Ink;
using TwoOkNotes.Model;
using TwoOkNotes.Services;

namespace TwoOkNotes.Tests
{
    [TestClass]
    public class FileSavingServicesTests
    {
        private MockFileSystem _fileSystem;
        private string _appDataFolder;
        private string _appFolder;
        private string _defaultStorageFile;
        private string _notesDirectory;
        private string _globalMetadataFile;

        // Helper class for testing private methods through subclassing
        private class TestableFileSavingServices : FileSavingServices
        {
            public TestableFileSavingServices() : base() 
            {
            
            }
        }
    }
}
