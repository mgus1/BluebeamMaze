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

			var maze = new Maze(mazeBitmap);
			var startRoom = maze.FindRoom(Feature.Start);
			var endRoom = maze.FindRoom(Feature.End);
			if (startRoom == null || endRoom == null)
				throw new Exception("Could not find both a starting and ending room");

			var path = maze.FindPath(startRoom, endRoom);
			if (path == null)
				throw new Exception("Could not find a path between starting and ending rooms");

			DrawPathOnBitmap(mazeBitmap, path, Color.Green);

			Console.WriteLine($"Saving solved maze to \"{destFile}\"");
			mazeBitmap.Save(destFile);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: Maze failed with exception: {ex.Message}");
		}
	}

	/// <summary>
	/// Draw the given path on the maze's bitmap
	/// </summary>
	/// <param name="bmp">The in memory bitmap. This is drawn onto.</param>
	/// <param name="path">A list of rooms to draw the path from</param>
	/// <param name="color">The color of the path to draw</param>
	private static void DrawPathOnBitmap(Bitmap bmp, List<Room> path, Color color)
	{
		using var pen = new Pen(color, 3);
		using var g = Graphics.FromImage(bmp);

		Room? prevRoom = null;
		foreach (var room in path)
		{
			if (prevRoom != null)
			{
				// draw green line from center of previous room to center of current room
				var start = new Point(prevRoom.X + prevRoom.Width / 2, prevRoom.Y + prevRoom.Height / 2);
				var end = new Point(room.X + room.Width / 2, room.Y + room.Height / 2);
				g.DrawLine(pen, start, end);
			}
			prevRoom = room;
		}
	}
}
