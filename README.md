# Witcher3MapViewer
A companion app for The Witcher 3: Wild Hunt (and DLC)

![Screenshot](/W3MVScreenshot2.PNG?raw=true)

The program is designed to help players do (nearly) all side quests during a playthrough of Witcher 3: Wild Hunt. The idea was inspired by the [JustCause2MapViewer](https://github.com/KarboniteKream/JC2MapViewer), which helped people get 100% by showing locations of settlements on a map, and used the savegame file to show only those elements that were undiscovered. However, this app does more.

The left panel shows all available quests. Highlighting an item will show on the map where to go to discover the quest, sometimes with a small bit of info to help you. Now, in **manual mode**, you tell the app which missions are done and your current level, and this will work if you're playing on a console.

But the app was really designed to work in **automatic mode**. In automatic mode, the app will read the most recent save file from the PC version's savegame folder, determine which quests are done, and lead you to side quests at or below your level. The current level is auto-detected. When the game is saved, the app automatically updates without user intervention. If you park it on a secondary monitor, you can have it lead you through the entire game without ever switching out of the game.

The app also tracks how many Gwent cards are obtained toward the "Collect 'em all" quest, and clicking the status will pop up a menu showing you exactly which cards are missing and where to get them. The window will also highlight random players to obtain cards from.

The coordinates shown in the lower right as you move your mouse over a location are X-Y coordinates in game space, so you can teleport Geralt to a location you find on the map by using the console command `XY()` (for instance: `XY(-210, 1125)`) You'll need god-mode on to use this, though.

The program can list and prompt miniquest events that aren't in the Witcher journal (such as when bandits try to extort you while crossing a bridge in Velen). However, these quests cannot be tracked by the app, and will have to be checked manually by the user. I recommend leaving these turned off.

Note that getting 100% in Witcher 3 is impossible, since certain quests will be unavailable depending on the choices made. The app does try to maximize the number of side quests that are completed.

I have tested the code with version 1.31A of the game, Hearts of Stone and Blood and Wine included. Please update to this version before using. The app does not modify the save game folder in any way. If you don't have the DLC, you can simply defer the quests "Evil's Soft First Touches" and "Envoys, Wineboys" and never be bothered again.

## Usage Instructions

Extract zip to anywhere you want (except the savegame folder itself) and run Witcher3MapViewer.exe. To use automatic mode, click the settings icon in the upper-right and toggle to automatic mode. The program will try to autodetect your save folder, but if that fails click on "choose" and navigate to the folder. If you want the program to stop suggesting a certain quest, right click it and choose "defer". You can turn off certain marker types in the upper left-hand dropdown menu.

## Issues

The code to read save game files is still likely in an experimental state, though I have never had it crash or fail to read a file. However, I had to write this code nearly from scratch, so if you have a problem, please report it. If the data in the app is faulty, such as if you find a given quest cannot be found under a set of conditions when the app says it should be, then the fault is likely in the `Quests.xml` file. You can report it to me or try to fix it yourself (the quest logic is in xml and fairly human-readable, but sort of complicated). In both cases, uploading the `.sav` file with your issue will be very helpful.

### Known Issues:     
* Gwent window showing random players does not update the marker treeview
* Info window visibility should hide when there is no info, but it does not
* I have no earthly idea how to trigger "A Feast for Crows"
* Level suggestion will be inaccurate for NewGame+

### To do:     
*  Gwent window highlight should show on map (this is kind of a lot of work)
*  Gwent players should be circles around regular map pins?
*  Is there a way to tell whether a random gwent player has been played? Probably not easy
*  DLC gwent tracking isn't really necessary, since the quest "Never Fear Skellige's Here" shows you the location of all the cards, but it might be nice to have.
*  Save gwent info in manual mode

## Building
Depends on Mapsui and LZ4PCL, both obtainable through NuGet. Solution is made in VS 2015 with updates.

## Details
The Witcher 3 save game file is a beast. [Atvaark's code](https://github.com/Atvaark/W3SavegameEditor) was highly informative on the compression format and header, but didn't read the parts of the file I was interested in. In particular, I had to read quite far down in the `universe` variable just to find the player's current level. A full read of the save file takes several seconds of CPU time on my core 2 quad, so the app uses an abbreviated form that only reads the fields I use. That being said, if someone wants to make another app, the code is a solid starting place. The code still skips over one type of variable whose format I wasn't able to reverse engineer.

The other challenge was figuring out what the GUIDs meant for all the quests in the journal. To find out that, say, `9EFDC32A-48F7E75E-421189B7-97877E0E` corresponds to the quest "Bloody Baron" meant having to read and parse the CR2W files in the uncooked game files. [Dmitry Zaitsev's LUA code](https://github.com/hhrhhr/Lua-utils-for-Witcher-3) was highly useful, but had to be adapted to read the `.journal` files. They also needed to be localized in English from the w3strings files, which was not straightforward.

Map Pin data was read directly from the uncooked files. These are a slightly simpler form of CR2W files that only have one data chunk.

The map image was pulled from screenshots. I suspect there's a way to get the map from the uncooked files (as sites like witcher3map.com look highly detailed even zoomed in) but I wasn't able to figure that out. The tiles are stored in `.mbtile` format generated by TileMill.

As for Gwent cards, the game stores them as numbers (1 through 10000), not by name. I had to run through the game getting all of them, and recording which card went to which number. (Maybe there's a smarter way! But I did not find it.)
