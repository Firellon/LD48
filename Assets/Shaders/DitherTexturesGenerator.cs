using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VolFx
{
	public class PaletteCash
	{
		public  Texture2D _palette;
		public  Texture2D _quant;
		public  Texture2D _measure;
	}

	public static class LutGenerator
	{
		private static Texture2D _lut16;
		private static Texture2D _lut32;
		private static Texture2D _lut64;

		// =======================================================================
		[Serializable]
		public enum LutSize
		{
			x16,
			x32,
			x64
		}

		[Serializable]
		public enum Gamma
		{
			rec601,
			rec709,
			rec2100,
			average,
		}

		// =======================================================================
		public static PaletteCash Generate(Texture2D _palette, LutSize lutSize = LutSize.x16, Gamma gamma = Gamma.rec601)
		{
			var clean  = _getLut(lutSize);
			var lut    = clean.GetPixels();
			var colors = _palette.GetPixels();
			
			var _lutPalette = new Texture2D(clean.width, clean.height, TextureFormat.ARGB32, false);
			var _lutQuant   = new Texture2D(clean.width, clean.height, TextureFormat.ARGB32, false);
			var _lutMeasure = new Texture2D(clean.width, clean.height, TextureFormat.ARGB32, false);

			// grade colors from lut to palette by rgb 
			var palette = lut.Select(lutColor => colors.Select(gradeColor => (grade: compare(lutColor, gradeColor), color: gradeColor)).OrderBy(n => n.grade).First())
							.Select(n => n.color)
							.ToArray();
			
			var quant = lut.Select(lutColor =>
						   {
							   var set = colors.Select(gradeColor => (grade: compare(lutColor, gradeColor), color: gradeColor)).OrderBy(n => n.grade).ToArray();
							   // var a   = set[0];
							   var b   = set[1];

							   return b.color;
						   })
						   .ToArray();
			
			colors = _palette.GetPixels().Select(_lutAt).ToArray();
			var measure = lut.Select(lutColor =>
							{
								var set = colors.Select(gradeColor => (grade: compare(lutColor, gradeColor), color: gradeColor)).OrderBy(n => n.grade).ToArray();
								var a   = set[0];
								var b   = set[1];

								var measure = 1f - a.grade / b.grade;

								return new Color(measure, measure, measure);
							})
							.ToArray();
			
			_lutPalette.SetPixels(palette);
			_lutPalette.filterMode = FilterMode.Point;
			_lutPalette.wrapMode   = TextureWrapMode.Clamp;
			_lutPalette.Apply();
			
			_lutQuant.SetPixels(quant);
			_lutQuant.filterMode = FilterMode.Point;
			_lutQuant.wrapMode   = TextureWrapMode.Clamp;
			_lutQuant.Apply();
			
			_lutMeasure.SetPixels(measure);
			_lutMeasure.filterMode = FilterMode.Bilinear;
			_lutMeasure.wrapMode   = TextureWrapMode.Clamp;
			_lutMeasure.Apply();

			var result = new PaletteCash()
			{
				_palette  = _lutPalette,
				_measure  = _lutMeasure,
				_quant    = _lutQuant,
			};
			
			return result;

			// -----------------------------------------------------------------------
			float compare(Color a, Color b)
			{
				// compare colors by grayscale distance
				var weight = gamma switch
				{
					Gamma.rec601  => new Vector3(0.299f, 0.587f, 0.114f),
					Gamma.rec709  => new Vector3(0.2126f, 0.7152f, 0.0722f),
					Gamma.rec2100 => new Vector3(0.2627f, 0.6780f, 0.0593f),
					Gamma.average => new Vector3(0.33333f, 0.33333f, 0.33333f),
					_             => throw new ArgumentOutOfRangeException()
				};

				// var c = a.ToVector3().Mul(weight) - b.ToVector3().Mul(weight);
				var c = new Vector3(a.r * weight.x, a.g * weight.y, a.b * weight.z) - new Vector3(b.r * weight.x, b.g * weight.y, b.b * weight.z);
				
				return c.magnitude;
			}
			
			Color _lutAt(Color c)
			{
				if (c.r >= 1f) c.r = 0.999f;
				if (c.g >= 1f) c.g = 0.999f;
				if (c.b >= 1f) c.b = 0.999f;
				
				var _lutSize = _getLutSize(lutSize);
				var scale   = (_lutSize - 1f) / _lutSize;
				var offset  = .5f * (1f / _lutSize);
				var step    = 1f / _lutSize;
				// y / (lutSize - 1f)
				var x = Mathf.FloorToInt((c.r * scale + offset) / step);
				var y = Mathf.FloorToInt((c.g * scale + offset) / step);
				var z = Mathf.FloorToInt((c.b * scale + offset) / step);

				return lutAt(x, y, z);
				
				// -----------------------------------------------------------------------
				Color lutAt(int x, int y, int z)
				{
					return new Color(x / (_lutSize - 1f), y / (_lutSize - 1f), z / (_lutSize - 1f), 1f);
				}
			}
		}

		// =======================================================================
		internal static int _getLutSize(LutSize lutSize)
		{
			return lutSize switch
			{
				LutSize.x16 => 16,
				LutSize.x32 => 32,
				LutSize.x64 => 64,
				_           => throw new ArgumentOutOfRangeException()
			};
		}
		
		internal static Texture2D _getLut(LutSize lutSize)
		{
			var size = _getLutSize(lutSize);
			var _lut = lutSize switch
			{
				LutSize.x16 => _lut16,
				LutSize.x32 => _lut32,
				LutSize.x64 => _lut64,
				_           => throw new ArgumentOutOfRangeException(nameof(lutSize), lutSize, null)
			};
			
			if (_lut != null && _lut.height == size)
				 return _lut;
			
			_lut            = new Texture2D(size * size, size, TextureFormat.RGBA32, 0, false);
			_lut.filterMode = FilterMode.Point;

			for (var y = 0; y < size; y++)
			for (var x = 0; x < size * size; x++)
				_lut.SetPixel(x, y, _lutAt(x, y));
			
			_lut.Apply();
			return _lut;

			// -----------------------------------------------------------------------
			Color _lutAt(int x, int y)
			{
				return new Color((x % size) / (size - 1f), y / (size - 1f), Mathf.FloorToInt(x / (float)size) * (1f / (size - 1f)), 1f);
			}
		}
	}

    public class DitherTextureGenerator : EditorWindow
    {
        private Texture2D paletteTex;

        [MenuItem("Nebulate.me/DitherTextureGenerator")]
        public static void Show()
        {
            DitherTextureGenerator wnd = GetWindow<DitherTextureGenerator>();
            wnd.titleContent = new GUIContent("Dither Texture Generator");
        }
		
        private static Texture2D TextureField(string name, Texture2D texture)
        {
            EditorGUILayout.BeginHorizontal();
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fixedWidth = 70;
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            EditorGUILayout.EndHorizontal();
            return result;
        }
		
        private void SaveTexture(Texture2D texture, string filename, string outputFolder = "/RenderOutput")
        {
            var bytes = texture.EncodeToPNG();
            var dirPath = Application.dataPath + outputFolder;

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            File.WriteAllBytes(dirPath + "/" + filename + ".png", bytes);
            Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath + "/" + filename + ".png");

            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }

        private LutGenerator.LutSize lutSize = LutGenerator.LutSize.x16;
        private LutGenerator.Gamma gamma = LutGenerator.Gamma.rec601;

        private void OnGUI()
        {
	        paletteTex = TextureField("Palette", paletteTex);

	        lutSize = (LutGenerator.LutSize)EditorGUI.EnumPopup(
		        new Rect(0, 20, position.width - 6, 15),
		        "LutSize:",
		        lutSize
		    );

	        gamma = (LutGenerator.Gamma)EditorGUI.EnumPopup(
		        new Rect(0, 40, position.width - 6, 15),
		        "Gamma:",
		        gamma
	        );

	        if (GUILayout.Button("GenerateTextures"))
            {
                var paletteObj = LutGenerator.Generate(paletteTex, lutSize, gamma);

                var palette = paletteObj._palette;
                var quant   = paletteObj._quant;
                var measure = paletteObj._measure;

                SaveTexture(palette, "PaletteTex", "/Dithering/Textures");
                SaveTexture(quant, "QuantTex", "/Dithering/Textures");
                SaveTexture(measure, "MeasureTex", "/Dithering/Textures");
            }
        }
    }
}