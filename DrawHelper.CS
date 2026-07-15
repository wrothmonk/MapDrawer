using Il2Cpp;
using MelonLoader;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MapDrawer
{
	public struct LineData {
		public string color;
		public Vector2 origin;
		public Vector2 target;

		public override string ToString() => $"({color}, {origin}, {target})";
		public void WriteTo(BinaryWriter writer)
		{
			writer.Write(color); // String
			writer.Write(origin.x); // Single
			writer.Write(origin.y);
			writer.Write(target.x);
			writer.Write(target.y);
		}
	}

	public static class DrawHelper
	{
		private static GameObject _redPrefab;
		private static GameObject _yellowPrefab;
		private static GameObject _whitePrefab;
		private static GameObject _discPrefab;
		private static RectTransform _mapRect;
		private static List<GameObject> _drawnLines = new List<GameObject>();

		public static uint drawPerFrameLimit = 50;

		// Gets references to base line prefabs and map origin
		public static void FetchPrefabs()
		{
			if (_mapRect != null && _whitePrefab != null && _yellowPrefab != null && _redPrefab != null) { 
				return;
			}

			MelonLogger.Msg($"Fetching Prefabs");

			var prefabs = Resources.FindObjectsOfTypeAll<GameObject>()
				.Where(o => o.name.Contains("MapMarker") && !o.name.Contains("Clone"))
				.Distinct()
				.ToList();	

			foreach (var p in prefabs)
			{
				if (p.name.Contains("RED"))
				{
					_redPrefab = p;
					MelonLogger.Msg($"RED found? {p.name}");
				} else if (p.name.Contains("Yellow"))
				{
					_yellowPrefab = p;
					MelonLogger.Msg($"Yellow found? {p.name}");
				} else if (p.name.Contains("White"))
				{
					_whitePrefab = p;
					MelonLogger.Msg($"White found? {p.name}");
				} else if (p.name.Contains("DiscCompass"))
				{
					_discPrefab = p;
					MelonLogger.Msg($"Disc found? {p.name}");
				} else if (p.name.Contains("MapMarkers"))
				{
					_mapRect = p.GetComponent<RectTransform>();
					MelonLogger.Msg($"MapRect found? {p.name} | {_mapRect != null}");
				}
			}
		}

		// Draws a line on the map from origin to target
		// (0, 0) is bottom left, (20, 10) is top right
		public static void DrawLine(string color, Vector2 origin, Vector2 target)
		{
			FetchPrefabs(); // Ensure line prefabs and _mapRect are initialized

			var prefab = _whitePrefab; // Default to white line
			switch(color)
			{
				case "red":
					prefab = _redPrefab;
					break;
				case "yellow":
					prefab = _yellowPrefab;
					break;
				case "disc":
					prefab = _discPrefab;
					break;
				default:
					break;
			}

			var newLineObj = UnityEngine.Object.Instantiate(prefab, _mapRect);
			_drawnLines.Add(newLineObj);
			newLineObj.SetActive(true);

			var newLine = newLineObj.GetComponent<MapMarkerLineUI>();
			var rect = newLine.GetComponent<RectTransform>();

			rect.anchoredPosition = origin;
			rect.localRotation = Quaternion.identity;

			newLine.Initialize(origin, rect);
			newLine.UpdateLine(origin, target, rect);
			newLine.FinalizePlacement();
		}

		public static void DrawLineFromData(LineData line)
		{
			DrawLine(line.color, line.origin, line.target);
		}

		public static void DrawLines(List<LineData> lines)
		{
			MelonCoroutines.Start(DrawLinesRoutine(lines));
		}

		// Draws lines at a fixed number per frame to avoid stuttering
		private static System.Collections.IEnumerator DrawLinesRoutine(List<LineData> lines)
		{
			int linesRemaining = lines.Count;
			var lineIndex = 0;
			while(linesRemaining > 0)
			{
				for (int x = 0; x < Math.Min(linesRemaining, drawPerFrameLimit+1); x++)
				{
					DrawLineFromData(lines[lineIndex]);
					lineIndex++;
					linesRemaining--;
				}
				yield return null; // Resume next frame
			}
			
		}

		public static void ClearLines()
		{
			foreach(GameObject line in _drawnLines)
			{
				UnityEngine.Object.Destroy(line);
			}
			_drawnLines.Clear();
		}

		// Creates a list of LineData representing all currently drawn lines on the map for saving to a file
		public static List<LineData> GetLines()
		{
			List<LineData> results = new List<LineData>();

			var prefabs = Resources.FindObjectsOfTypeAll<GameObject>()
				.Where(o => o.name.Contains("MapMarker") && o.name.Contains("Clone"))
				.ToList();
			
			foreach (var p in prefabs)
			{
				string color = "white";
				if (p.name.Contains("RED"))
				{
					color = "red";
				} else if (p.name.Contains("Yellow"))
				{
					color = "yellow";
				} else if (p.name.Contains("DiscCompass"))
				{
					color = "disc";
				}

				var newLine = p.GetComponent<MapMarkerLineUI>();
				Vector2 origin = newLine.OriginLocal;
				Vector2 target = newLine.TipLocalPosition; //TipLocalPosition is a Vector3 but the z axis should always be zero anyway
				target += origin;
				results.Add(new LineData{color = color, origin = origin, target = target});
			}

			return results;
		}
	}
}