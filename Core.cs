using UnityEngine;
using UnityEngine.InputSystem;
using MelonLoader;

[assembly: MelonInfo(typeof(MapDrawer.Core), "MapDrawer", "1.2.0", "Wrothmonk", null)]

namespace MapDrawer
{
	public class Core : MelonMod
	{
		private static int _galleryIndex = 0;

		public override void OnInitializeMelon()
		{
			IOHelper.Init();
			MelonLogger.Msg("Initialized");
		}
		public override void OnUpdate()
		{
			if (Keyboard.current == null){
				return;}
			// if (Keyboard.current.numpad0Key.wasPressedThisFrame){
			// 	TestFunc();}
			if (Keyboard.current.numpad9Key.wasPressedThisFrame){
				DrawHelper.ClearLines();}
			if (Keyboard.current.numpad8Key.wasPressedThisFrame){
				IOHelper.SaveDrawing(DrawHelper.GetLines());}
			// if (Keyboard.current.numpad7Key.wasPressedThisFrame){
			// 	IOHelper.LoadDrawing(Path.Combine(IOHelper.gallery, "0.draw"));}
			if (Keyboard.current.numpad7Key.wasPressedThisFrame){
				IOHelper.Init();} // Update gallery list
			if (Keyboard.current.numpad6Key.wasPressedThisFrame){
				NextImage();}
			if (Keyboard.current.numpad5Key.wasPressedThisFrame){
				CurrImage();}
			if (Keyboard.current.numpad4Key.wasPressedThisFrame){
				PrevImage();}
			if (Keyboard.current.numpad3Key.wasPressedThisFrame){
				DrawHelper.SetLabelVisibility(true);}
			if (Keyboard.current.numpad2Key.wasPressedThisFrame){
				DrawHelper.SetLabelVisibility(false);}
			if (Keyboard.current.numpad1Key.wasPressedThisFrame){
				IOHelper.ConvertJson();}
		}

		public static void CurrImage()
		{
			if (IOHelper.galleryFiles.Count <= 0)
			{
				MelonLogger.Error("No images in gallery folder!");
				return;
			}
			DrawHelper.ClearLines();
			IOHelper.LoadDrawing(IOHelper.galleryFiles[_galleryIndex]);
		}

		public static void NextImage()
		{
			if (IOHelper.galleryFiles.Count <= 0)
			{
				MelonLogger.Error("No images in gallery folder!");
				return;
			}
			_galleryIndex++;
			if (_galleryIndex >= IOHelper.galleryFiles.Count) {_galleryIndex = 0;}
			DrawHelper.ClearLines();
			IOHelper.LoadDrawing(IOHelper.galleryFiles[_galleryIndex]);
		}

		public static void PrevImage()
		{
			if (IOHelper.galleryFiles.Count <= 0)
			{
				MelonLogger.Error("No images in gallery folder!");
				return;
			}
			_galleryIndex--;
			if (_galleryIndex < 0) {_galleryIndex = IOHelper.galleryFiles.Count-1;}
			DrawHelper.ClearLines();
			IOHelper.LoadDrawing(IOHelper.galleryFiles[_galleryIndex]);
		}

		// public static void TestFunc()
		// {
		// 	// MelonLogger.Msg("Drawing lines");
		// 	// for (double x = 0; x <= (_lineCount-1)*0.01; x+=0.01)
		// 	// {
		// 	// 	DrawHelper.DrawLine("white", new Vector2((float)x, 0), new Vector2((float)x, 5));
		// 	// }
		// 	var lines = DrawHelper.GetLines();
		// 	foreach (var line in lines)
		// 	{
		// 		MelonLogger.Msg(line);
		// 	}
		// }
	}
}