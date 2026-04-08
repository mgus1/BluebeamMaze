
namespace BluebeamMaze;

/// <summary>
/// This class represents a maze as a set of interconnected rooms. Each room
/// points back to the original maze in terms of coordinates.
/// </summary>
internal class Maze
{
	public void Load(Feature[,] map)
	{

	}

	#region Bitmap to Feature array conversion

	/// <summary>
	/// Converts an RGB bitmap into a more convenient 2D array of features
	/// </summary>
	/// <param name="bitmap">The raw pixel bitmap</param>
	/// <returns>A two dimensional array of features</returns>
	public static Feature[,] TranslateBitmap (Bitmap bitmap)
	{
		var features = new Feature[bitmap.Width, bitmap.Height];
		for (int x = 0; x != bitmap.Width; x++)
			for (int y = 0; y != bitmap.Height; y++)
				features[x, y] = ConvertPixelToFeature(bitmap.GetPixel(x, y));
		return features;
	}

	/// <summary>
	/// Using fixed color values since the input maze is very specific, but this could
	/// be expanded for other features and could be built dynamically by scanning colors
	/// in ranges so we wouldn't be stuck with just pure colors.
	/// </summary>
	private static Feature ConvertPixelToFeature(Color color)
	{
		if (!_pixelFeatureMap.TryGetValue(color, out var feature))
			throw new Exception("Maze contains an unsupported pixel color");

		return feature;
	}

	private static Dictionary<Color, Feature> _pixelFeatureMap = new()
	{
		[Color.FromArgb(255, 0, 0, 255)] = Feature.Start,		// Blue
		[Color.FromArgb(255, 255, 0, 0)] = Feature.End,			// Red
		[Color.FromArgb(255, 255, 255, 255)] = Feature.None,	// White
		[Color.FromArgb(255, 0, 0, 0)] = Feature.Wall,			// Black
	};

	#endregion
}

/// <summary>
/// This is the translation from color to what maze feature it represents. This
/// makes the code cleaner when determining the room connections.
/// </summary>
internal enum Feature
{
	None,
	Wall,
	Start,
	End,
}