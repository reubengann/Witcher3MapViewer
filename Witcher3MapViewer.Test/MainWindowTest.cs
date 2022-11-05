using Moq;
using Witcher3MapViewer.Core;

namespace WitcherMapViewerMark2.Test
{
    public class Tests
    {
        Mock<IMap> mockMap;
        MainWindowViewModel vm;

        [SetUp]
        public void SetUp()
        {
            mockMap = new Mock<IMap>();
            vm = new MainWindowViewModel(mockMap.Object);
        }

        [Test]
        public void WhenMainWindowLoads_LoadsAMap()
        {
            vm.LoadInitialMapCommand.Execute(null);
            mockMap.Verify(m => m.LoadMap(It.IsAny<string>()));
        }

        [Test]
        public void MainWindow_HasAllFiveMapsAsOptions()
        {
            Assert.That(vm.ListOfMaps.Count, Is.EqualTo(5));
        }

        [Test]
        public void WhenMainWindowLoads_SelectsAListItem()
        {
            vm.LoadInitialMapCommand.Execute(null);
            Assert.That(vm.SelectedMap, Is.Not.Null);
        }

        [Test]
        public void WhenMapSelectionChanges_LoadsCorrespondingMap()
        {
            vm.LoadInitialMapCommand.Execute(null);
            vm.SelectedMap = vm.ListOfMaps[2];
            mockMap.Verify(m => m.LoadMap(MapInfo.TileMapPathMap[vm.ListOfMaps[2]]));
        }

        [Test]
        public void WhenMapIsLoaded_LoadsMarkers()
        {
            vm.LoadInitialMapCommand.Execute(null);
            mockMap.Verify(m => m.LoadMarkers(MapMarkers.MapMarkerSpec));
        }
    }
}