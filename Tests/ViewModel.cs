using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.ObjectModel;
using System.Windows.Ink;
using TwoOkNotes.Model;
using TwoOkNotes.Services;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes.Tests
{
    [TestClass]
    public class ViewModelTests
    {
        private Mock<FileSavingServices> _mockFileSavingServices;
        private Mock<SettingsServices> _mockSettingsServices;
        private Mock<CanvasModel> _mockCanvasModel;
        private EditingWIndowViewModel _viewModel;
        private StrokeCollection _testStrokes;

        [TestInitialize]
        public void Setup()
        {
            // Create mocks
            _mockFileSavingServices = new Mock<FileSavingServices>();
            _mockSettingsServices = new Mock<SettingsServices>();
            _mockCanvasModel = new Mock<CanvasModel>();
            _testStrokes = new StrokeCollection();
            
            // Setup mock canvas model
            _mockCanvasModel.SetupGet(m => m.Strokes).Returns(_testStrokes);
            
            // Create sections and pages for testing
            var testSections = new ObservableCollection<NoteBookSection> 
            { 
                new NoteBookSection { Name = "Section 1" } 
            };
            
            var testPages = new ObservableCollection<NoteBookPage> 
            { 
                new NoteBookPage { Name = "Page 1.isf" } 
            };
            
            // Setup mock file saving service
            _mockFileSavingServices
                .Setup(m => m.GetNotebookMetadata(It.IsAny<string>()))
                .ReturnsAsync((testSections, 0));
                
            _mockFileSavingServices
                .Setup(m => m.GetSectionMetadata(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((testPages, 0));
                
            _mockFileSavingServices
                .Setup(m => m.GetCurrFilePath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test.isf");
                
            // Initialize view model with mocks
            _viewModel = new EditingWIndowViewModel(_mockCanvasModel.Object, "testPath", "TestNotebook");
        }

        [TestMethod]
        public void CreateNewPage_AddsPageToCollection()
        {
            // Arrange
            var initialPageCount = _viewModel.Pages.Count;
            _mockFileSavingServices
                .Setup(m => m.CreatePage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
                
            // Act
            _viewModel.NewPageCommand.Execute(null);
            
            // Assert
            Assert.IsTrue(_viewModel.Pages.Count > initialPageCount);
        }
        
        [TestMethod]
        public void SwitchSection_UpdatesActiveSection()
        {
            // Arrange
            var newSection = new NoteBookSection { Name = "Section 2" };
            _viewModel.Sections.Add(newSection);
            
            // Act
            _viewModel.SwitchSectionCommand.Execute(newSection);
            
            // Assert
            Assert.IsTrue(newSection.IsActive);
            Assert.IsFalse(_viewModel.Sections[0].IsActive);
        }
        
        [TestMethod]
        public void ToggleVisibility_CyclesThroughVisibilityStates()
        {
            // Arrange
            bool initialSectionVisibility = _viewModel.IsSectionsGridVisible;
            bool initialPageVisibility = _viewModel.IsPagesGridVisible;
            
            // Act
            _viewModel.ToggleVisibilityCommand.Execute(null);
            
            // Assert
            Assert.AreNotEqual(initialSectionVisibility, _viewModel.IsSectionsGridVisible);
            // Note: Page visibility follows section visibility in first toggle state
        }
        
        [TestMethod]
        public void UndoCommand_RemovesLastStroke()
        {
            // Arrange
            var mockStroke = new Mock<Stroke>();
            var action = new StrokeTypeAction(mockStroke.Object, true);
            var undoStack = new Stack<StrokeTypeAction>();
            undoStack.Push(action);
            
            _mockCanvasModel.SetupGet(m => m.UndoStack).Returns(undoStack);
            
            // Act
            _viewModel.UndoCommand.Execute(null);
            
            // Assert
            _mockCanvasModel.Verify(m => m.Strokes.Remove(It.IsAny<Stroke>()), Times.Once);
        }
        
        [TestMethod]
        public void RedoCommand_RestoresRemovedStroke()
        {
            // Arrange
            var mockStroke = new Mock<Stroke>();
            var action = new StrokeTypeAction(mockStroke.Object, true);
            var redoStack = new Stack<StrokeTypeAction>();
            redoStack.Push(action);
            
            _mockCanvasModel.SetupGet(m => m.RedoStack).Returns(redoStack);
            
            // Act
            _viewModel.RedoCommand.Execute(null);
            
            // Assert
            _mockCanvasModel.Verify(m => m.Strokes.Add(It.IsAny<Stroke>()), Times.Once);
        }
    }
}
