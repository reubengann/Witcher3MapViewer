using Moq;
using Witcher3MapViewer;
using Witcher3MapViewer.Core;

namespace WitcherMapViewerMark2.Test
{


    public class MainWindowTests
    {
        MarkerSpec MapMarkerSpec = new MarkerSpec(
            @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MarkerImages\RoadSign.png",
            new List<Point> { new Point(0, 0) }
            );

        Mock<IMap> mockMap;
        Mock<IMapSettingsProvider> mockSettingsProvider;
        Mock<IMarkerProvider> mockMarkerProvider;
        MainWindowViewModel vm;

        [SetUp]
        public void SetUp()
        {
            mockMap = new Mock<IMap>();
            mockMarkerProvider = new Mock<IMarkerProvider>();
            mockSettingsProvider = new Mock<IMapSettingsProvider>();
            mockSettingsProvider.Setup(x => x.GetAll()).Returns(() =>
                new List<WorldSetting>()
                {
                    new WorldSetting {Name = "Location 1",  ShortName = "loc1", TileSource = "l1.mbtiles"},
                    new WorldSetting {Name = "Location 2", ShortName = "loc2", TileSource = "l2.mbtiles"},
                });
            mockMarkerProvider.Setup(x => x.GetMarkerSpecs("loc1")).Returns(() =>
                new List<MarkerSpec>() { MapMarkerSpec });
            mockMarkerProvider.Setup(x => x.GetMarkerSpecs("loc2")).Returns(() =>
                new List<MarkerSpec>() { MapMarkerSpec });
            vm = new MainWindowViewModel(mockMap.Object, mockMarkerProvider.Object, mockSettingsProvider.Object);
        }

        [Test]
        public void WhenMainWindowLoads_LoadsAMap()
        {
            vm.LoadInitialMapCommand.Execute(null);
            mockMap.Verify(m => m.LoadMap(It.IsAny<string>()));
        }

        [Test]
        public void UsesSettingsToPopulateListOfMaps()
        {
            mockSettingsProvider.Verify(x => x.GetAll());
        }

        [Test]
        public void MainWindow_UsesMapSettingsToPopulateListOfMaps()
        {

            Assert.That(vm.ListOfMaps.Count, Is.EqualTo(2));
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
            vm.SelectedMap = vm.ListOfMaps[1];
            mockMap.Verify(m => m.LoadMap("l2.mbtiles"));
        }

        [Test]
        public void WhenMapIsLoaded_LoadsMarkers()
        {
            vm.LoadInitialMapCommand.Execute(null);
            mockMap.Verify(m => m.LoadMarkers(It.IsAny<MarkerSpec>()));
        }

        [Test]
        public void LoadsMapsFromWorldSettings()
        {
            vm.LoadInitialMapCommand.Execute(null);
            mockMap.Verify(m => m.LoadMap("l1.mbtiles"));
        }

        [Test]
        public void GetsMarkersFromSettings()
        {
            vm.LoadInitialMapCommand.Execute(null);
            mockMarkerProvider.Verify(x => x.GetMarkerSpecs("loc1"));
        }
    }
}