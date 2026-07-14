using System.IO;
using System.Text.Json;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

namespace MapDrawer
{
	public static class IOHelper
	{
		public class SimpleVec2 // Unity's Vec2s do not serialize cleanly, so we have to have a simple version to convert back later.
		{
			public float x {get; set;}
			public float y {get; set;}
		}
		public class SimpleLine
		{
			public string color {get; set;}
			public SimpleVec2 origin {get; set;}
			public SimpleVec2 target {get; set;}
		}
		public class JsonSave
		{
			public string header {get; set;}
			public UInt16[] version {get; set;}
			public List<SimpleLine> lines {get; set;}
		}

		public static string dir = Path.Combine(MelonEnvironment.UserDataDirectory, "MapDrawer");
		public static string output = Path.Combine(dir, "output");
		public static string gallery = Path.Combine(dir, "gallery");
		public static List<String> galleryFiles = new List<String>();
		private static int saveIndex = 0; // Next index to write to for saving drawings
		private static UInt16[] saveVersion = {1, 0, 0};

		public static void Init()
		{
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			if (!Directory.Exists(output))
			{
				Directory.CreateDirectory(output);
				MelonLogger.Msg($"Created ouput folder, saved draw files can be found here: {output}");
			} else {
				// Find the next open save slot
				saveIndex = -1;
				string testPath = Path.Combine(output, $"{saveIndex}.draw");
				do {
					saveIndex++;
					testPath = Path.Combine(output, $"{saveIndex}.draw");
				} while (File.Exists(testPath));
			}
			if (!Directory.Exists(gallery))
			{
				Directory.CreateDirectory(gallery);
				MelonLogger.Msg($"Created gallery folder, put draw files for viewing here: {gallery}");
			} else {
				foreach (var f in Directory.GetFiles(gallery))
				{
					if (".draw" == Path.GetExtension(f).ToLowerInvariant())
						galleryFiles.Add(f);
				}
				galleryFiles.Sort(StringComparer.OrdinalIgnoreCase);
				MelonLogger.Msg($"[IOHelper] Found {galleryFiles.Count} gallery drawings");
			}

			MelonLogger.Msg("[IOHelper] Initialized");
		}

		public static void SaveDrawing(List<LineData> saveData, string savePath = null)
		{
			if (saveData.Count <= 0) {return;} // There aren't any lines to save

			if (savePath == null)
			{
				savePath = Path.Combine(output, $"{saveIndex}.draw");
				while (File.Exists(savePath))
				{
					saveIndex++;
					savePath = Path.Combine(output, $"{saveIndex}.draw");
				}
			}

			try
			{
				using (FileStream stream = File.Open(savePath, FileMode.Create))
				{
					using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, false))
					{
						writer.Write("MapDrawer"); // String
						writer.Write(saveVersion[0]); //UInt16
						writer.Write(saveVersion[1]);
						writer.Write(saveVersion[2]);
						foreach (LineData line in saveData)
						{
							//MelonLogger.Msg($"[IOHelper] saving {line}");
							line.WriteTo(writer); // See DrawHelper.CS
						}
					}
				}
				MelonLogger.Msg($"[IOHelper] Saved drawing to: {savePath}");
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"[IOHelper] Failed to save drawing: {ex}");
			}
		}

		public static void LoadDrawing(string filePath)
		{
			List<LineData> lines = new List<LineData>();
			try
			{
				using (FileStream stream = File.Open(filePath, FileMode.Open))
				{
					using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
					{
						// Load in header information
						string header = reader.ReadString();
						UInt16[] version = new UInt16[3];
						version[0] = reader.ReadUInt16();
						version[1] = reader.ReadUInt16();
						version[2] = reader.ReadUInt16();
						if (header != "MapDrawer") {
							MelonLogger.Error($"[IOHelper] Failed to load drawing, invalid header: {header}");
							return;
						}
						if (version[0] != 1) {
							MelonLogger.Error($"[IOHelper] Failed to load drawing, invalid version: {version[0]}.{version[1]}.{version[2]}");
							return;
						}

						// Load in lines to draw
						while (stream.Position < stream.Length)
						{
							LineData newLine = new LineData{
								color = reader.ReadString(),
								origin = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
								target = new Vector2(reader.ReadSingle(), reader.ReadSingle())
							};
							lines.Add(newLine);
							//MelonLogger.Msg($"[IOHelper] Loaded {newLine}");
						}
						MelonLogger.Msg($"[IOHelper] Loaded {lines.Count} lines into memory, beginning drawing");

					}
				}
			}
			catch (Exception ex)
			{
				MelonLogger.Error($"[IOHelper] Failed to load drawing file: {ex}");
				return;
			}

			DrawHelper.DrawLines(lines);
		}


		// Converts json based files into draw files
		/* Example JSON:
		{
			"header": "MapDrawer",
			"version": [1, 0, 0],
			"lines": [
				{
					"color":"white",
					"origin": {"x":0, "y":0},
					"target": {"x":20, "y":10}
				},
				{
					"color":"red",
					"origin": {"x":0, "y":10},
					"target": {"x":20, "y":0}
				}
			]
		} */
		public static void ConvertJson()
		{
			List<String> jsonFiles = new List<String>();
			foreach (var f in Directory.GetFiles(gallery))
			{
				if (".json" == Path.GetExtension(f).ToLowerInvariant())
					jsonFiles.Add(f);
			}
			jsonFiles.Sort(StringComparer.OrdinalIgnoreCase);
			MelonLogger.Msg($"[IOHelper] Found {jsonFiles.Count} json drawings");

			foreach (string filePath in jsonFiles)
			{
				JsonSave saveData;
				try
				{
					string jsonString = File.ReadAllText(filePath);
					saveData = JsonSerializer.Deserialize<JsonSave>(jsonString);
				}
				catch (Exception ex)
				{
					MelonLogger.Error($"[IOHelper] Failed to load json file: {ex}");
					return;
				}

				List<LineData> lineData = new List<LineData>();
				foreach (SimpleLine line in saveData.lines)
				{
					lineData.Add(new LineData{
						color = line.color, 
						origin = new Vector2(line.origin.x, line.origin.y), 
						target = new Vector2(line.target.x, line.target.y)});
				}

				SaveDrawing(lineData, Path.ChangeExtension(filePath, ".draw"));
			}
			
		}
	}
}