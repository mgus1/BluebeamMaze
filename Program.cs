
namespace BluebeamMaze;

internal static class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		// give usage message if incorrect number of arguments
		if (args.Length != 2)
		{
			Console.WriteLine("Usage: maze \"source.[bmp,png,jpg]\" \"destination.[bmp,png,jpg]\"");
			return;
		}

		// this will standardize the source and destination filenames (such as remove quotes).
		var sourceFile = Path.GetFullPath(args[0]);
		var destFile = Path.GetFullPath(args[1]);

		// check for existence of source file
		if (!File.Exists(sourceFile))
		{
			Console.WriteLine($"Error: File \"{sourceFile}\" not found");
			return;
		}

		try
		{
			var mazeBitmap = new Bitmap(sourceFile);
			Console.WriteLine($"Maze size is {mazeBitmap.Width} x {mazeBitmap.Height}");

			var features = Maze.TranslateBitmap(mazeBitmap);

			Console.WriteLine($"Saving solved maze to \"{destFile}\"");
			mazeBitmap.Save(destFile);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: Maze failed with exception: {ex.Message}");
		}
	}
}
