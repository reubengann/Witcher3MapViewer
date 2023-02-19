using Moq;
using Witcher3MapViewer.Core;

namespace Witcher3MapViewer.Test
{


    public class MainWindowTests
    {
        MarkerSpec RoadSign = new MarkerSpec(
            @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MarkerImages\RoadSign.png",
            new List<Point> { new Point(0, 0) },
            "RoadSign",
            "Road sign"
            );
        MarkerSpec PlaceOfPower = new MarkerSpec(
            @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MarkerImages\PlaceOfPower.png",
            new List<Point> { new Point(0, 0) },
            "PlaceOfPower",
            "Place of Power"
            );
        MarkerSpec Harbor = new MarkerSpec(
            @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MarkerImages\Harbor.png",
            new List<Point> { new Point(0, 0) },
            "Harbor",
            "Harbor"
            );
        MarkerSpec Herbalist = new MarkerSpec(
            @"C:\repos\Witcher3MapViewer\Witcher3MapViewer\MarkerImages\Herbalist.png",
            new List<Point> { new Point(0, 0) },
            "Herbalist",
            "Herbalist"
            );

        Mock<IMap> mockMap;
        Mock<IMapSettingsProvider> mockSettingsProvider;
        Mock<IMarkerProvider> mockMarkerProvider;
        Mock<IQuestAvailabilityProvider> mockQuestAvailabilityProvider;
        Mock<IQuestListProvider> mockQuestListProvider;
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
                new List<MarkerSpec>() { RoadSign, PlaceOfPower });
            mockMarkerProvider.Setup(x => x.GetMarkerSpecs("loc2")).Returns(() =>
                new List<MarkerSpec>() { RoadSign });
            mockQuestAvailabilityProvider = new Mock<IQuestAvailabilityProvider>();
            mockQuestListProvider = new Mock<IQuestListProvider>();
            mockQuestListProvider.Setup(x => x.GetAllQuests()).Returns(() => new List<Quest>());
            vm = new MainWindowViewModel(mockMap.Object, mockMarkerProvider.Object,
                mockSettingsProvider.Object, mockQuestListProvider.Object, mockQuestAvailabilityProvider.Object, new Mock<IGwentCardProvider>().Object, new Mock<ILevelProvider>().Object, new Mock<IGwentStatusProvider>().Object);
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

        [Test]
        public void LoadsRoadSignLastIfPresent()
        {


            int callOrder = 0;
            mockMap.Setup(x => x.LoadMarkers(PlaceOfPower)).Callback(() => Assert.That(callOrder++, Is.EqualTo(0)));
            mockMap.Setup(x => x.LoadMarkers(RoadSign)).Callback(() => Assert.That(callOrder++, Is.EqualTo(1)));
            vm.LoadInitialMapCommand.Execute(null);
        }

        [Test]
        public void AddsMarkerLayersToToggleMenu()
        {
            mockMarkerProvider.Setup(x => x.GetMarkerSpecs("loc1")).Returns(() =>
                new List<MarkerSpec>() { RoadSign, PlaceOfPower, Harbor, Herbalist });
            vm.LoadInitialMapCommand.Execute(null);
            Assert.That(vm.MarkerToggleViewModel.Markers[0].Children.Count, Is.EqualTo(3));
        }

        [Test]
        public void ClearsToggleMenuOnNewLoad()
        {
            vm.LoadInitialMapCommand.Execute(null);
            vm.SelectedMap = vm.ListOfMaps[1];
            Assert.That(vm.MarkerToggleViewModel.Markers[0].Children.Count, Is.EqualTo(0));
        }

        [Test]
        public void ToggleMenuShowsFullName()
        {
            vm.LoadInitialMapCommand.Execute(null);
            Assert.That(vm.MarkerToggleViewModel.Markers[0].Children[0].Text, Is.EqualTo(PlaceOfPower.FullName));
        }
    }
}