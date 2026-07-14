# Overview
This is the source code for the Iron Nest mod MapDrawer. This mod allows you to save and load whatever you've drawn on the in-game tactical map. There is currently no GUI, nor do I currently have any plans to implement one, the mod is controlled solely through some hardbound keybinds.

## Known Issues
Currently lines drawn by the mod cannot be manually deleted. The only way to remove them is all at once either via the 'clear map drawings' button (which clears everything) or via pressing Numpad 9 to clear only the lines drawn by the mod. I am completely in the dark as to what could be causing this beyond I'm probably missing some initialization call or something when spawning the line prefabs.

## Keybinds
**Numpad 9**: Clear lines drawn by the mod. This does not include manually drawn lines.

**Numpad 8**: Save all currently drawn lines (by the mod or manually) to a .draw file located at `IRON NEST Heavy Turret Simulator Demo/UserData/MapDrawer/output`

**Numpad 7**: Refreshes the list of .draw files in the gallery folder (`IRON NEST Heavy Turret Simulator Demo/UserData/MapDrawer/gallery`)

**Numpad 6**: Cycles to the next .draw file in the gallery folder and draws it on the map

**Numpad 5**: Draws (or re-draws) the currently selected .draw file

**Numpad 4**: Cycles to the previous .draw file in the gallery folder and draws it on the map

**Numpad 1**: Iterates through all .json files in the gallery folder and attempts to convert them to .draw files

## Advanced Usage
Currently the mod only loads .draw files that have been created by the mod itself, though it is possible to convert specifically formatted JSON into a .draw file. Below is an example of how the JSON file needs to be formatted:
```
{
	"header":"MapDrawer",
	"version":[1, 0, 0],
	"lines":[
		{
			"color":"white",
			"origin":{"x":0, "y":0},
			"target":{"x":20, "y":10}
		},
		{
			"color":"yellow",
			"origin":{"x":0, "y":10},
			"target":{"x":20, "y":0}
		}
	]
}
```
`"header"` and `"version"` fields aren't currently checked, but are read from. If the format significantly changes in the future then the version numbers will be checked for converting older formats.

`"lines"` is a list of simplified LineData objects that contain three fields:
  
  `"color"` should be one of `"white"`, `"yellow"`, or `"red"`. Unrecognized color strings will default to `"white"`.
  
  `"origin"` is the x and y coordinates of the start of the line.
  
  `"target"` is the x and y coordinates of where the line ends.

Coordinates on the tactical map go from (0, 0) at the bottom left to (20, 10) at the top right. The bottom edge is towards the firing calculator, and the left edge towards the front of the nest. Integer values correspond to the edges of each large grid square, with floating point values falling inbetween.
