
using System.Collections.Generic;

namespace BluebeamMaze;

/// <summary>
/// This class represents a maze as a set of interconnected rooms. Each room
/// points back to the original maze in terms of coordinates.
/// </summary>
internal class Maze
{
	public Maze(Bitmap mazeBitmap) =>
		_rooms = CreateRooms(TranslateBitmap(mazeBitmap));

	/// <summary>
	/// Find a path from the start room to the end room
	/// </summary>
	/// <param name="start">The starting room</param>
	/// <param name="end">The ending room</param>
	/// <returns>List of rooms starting with start room and ending with end room or null if no solution</returns>
	public List<Room>? FindPath(Room start, Room end)
	{
		var path = new List<Room>();
		var hashSet = new HashSet<Room>();	// also all rooms in path, but keyed for quick check

		var room = start;
		var nextIx = 0;		// the index of the next room to select

		// while room not the ending room
		while (room != end)
		{
			if (nextIx < room.Connections.Count)
			{
				var next = room.Connections[nextIx];
				if (hashSet.Contains(next))
				{
					nextIx++;	// if this is the room we just came from, then skip. Could also be circular path (example doesn't have that though)
				}
				else
				{
					path.Add(room);
					hashSet.Add(room);
					room = next;
					nextIx = 0;
				}
			}
			else
			{
				// need to back up
				if (path.Count == 0) return null;	// if can't then no solution

				var prev = path[path.Count - 1];
				path.Remove(prev);
				hashSet.Remove(room);

				nextIx = prev.Connections.IndexOf(room) + 1;
				room = prev;
			}
		}
		path.Add(room);

		return path;
	}

	/// <summary>
	/// Find the first room with the given feature
	/// </summary>
	/// <param name="feature"></param>
	/// <returns></returns>
	public Room? FindRoom(Feature feature)
		=> _rooms.FirstOrDefault(room => room.Feature == feature);

	#region Create the room list

	/// <summary>
	/// Create the list of rooms and all their connections
	/// </summary>
	/// <param name="features">The maze bitmap converted to 2D feature array</param>
	/// <returns>List of all rooms</returns>
	private List<Room> CreateRooms(Feature[,] features)
	{
		List<Room> rooms = new List<Room>();

		var roomGrid = CreateRoomGrid(features);
		int gridWidth = roomGrid.GetLength(0);
		int gridHeight = roomGrid.GetLength(1);
		for (int x = 0; x != gridWidth; x++)
		{
			for (int y = 0; y != gridHeight; y++)
			{
				var room1 = roomGrid[x, y];
				rooms.Add(room1);

				if (x < gridWidth-1)
				{
					var room2 = roomGrid[x + 1, y];
					if (AreRoomsConnected(room1, room2, features))
					{
						room1.Connections.Add(room2);
						room2.Connections.Add(room1);
					}
				}

				if (y < gridHeight-1)
				{
					var room2 = roomGrid[x, y + 1];
					if (AreRoomsConnected(room1, room2, features))
					{
						room1.Connections.Add(room2);
						room2.Connections.Add(room1);
					}
				}
			}
		}
		return rooms;
	}

	/// <summary>
	/// Create a grid of rooms from the feature array.
	/// TODO: Make this not require fixed size rooms.
	/// </summary>
	/// <param name="features">Describes the maze bitmap</param>
	/// <returns>An array of rooms</returns>
	private Room[,] CreateRoomGrid(Feature[,] features)
	{
		var rooms = new Room[10,10];
		for (int x = 0; x != 10; x++)
		{
			for (int y = 0; y != 10; y++)
			{
				var feature = features[x * 44 + 21, y * 44 + 21];
				var room = new Room(feature, x * 44 + 1, y * 44 + 1, 43, 43);
				rooms[x,y] = room;
			}
		}
		return rooms;
	}

	/// <summary>
	/// Checks if two rooms are connected or if a wall is between them. This assumes
	/// the two rooms are either in the same column or row
	/// </summary>
	/// <param name="a">The first of the two rooms</param>
	/// <param name="b">The second of the two rooms</param>
	/// <returns>true if no wall between them</returns>
	private bool AreRoomsConnected(Room a, Room b, Feature[,] features)
	{
		// get coordinates of room centers
		int x1 = a.X + a.Width / 2;
		int y1 = a.Y + a.Height / 2;
		int x2 = b.X + b.Width / 2;
		int y2 = b.Y + b.Height / 2;

		// Determine the path we're stepping as we're scanning for the wall.
		// Note: this could be extended for arbitrary slopes (using fractional stepping in shorter dimension)
		int stepX = x1 == x2 ? 0 : x1 < x2 ? 1 : -1;
		int stepY = y1 == y2 ? 0 : y1 < y2 ? 1 : -1;

		while (y1 != y2 || x1 != x2)
		{
			if (features[x1, y1] == Feature.Wall)
				return false;
			y1 += stepY;
			x1 += stepX;
		}
		return true;
	}

	#endregion
	#region Bitmap to Feature array conversion

	/// <summary>
	/// Converts an RGB bitmap into a more convenient 2D array of features
	/// </summary>
	/// <param name="bitmap">The raw pixel bitmap</param>
	/// <returns>A two dimensional array of features</returns>
	private static Feature[,] TranslateBitmap (Bitmap bitmap)
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
	#region Data

	private List<Room> _rooms;

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
