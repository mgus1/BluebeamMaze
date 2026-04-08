namespace BluebeamMaze;

/// <summary>
/// This class represents a maze as a set of interconnected rooms. Each room
/// points back to the original maze in terms of X,Y pixel coordinates.
/// </summary>
internal class Maze
{
	#region public API

	public Maze(Bitmap mazeBitmap) =>
		CreateRooms(TranslateBitmap(mazeBitmap));

	/// <summary>
	/// Find a path from the start room to the end room
	/// </summary>
	/// <param name="start">The starting room</param>
	/// <param name="end">The ending room</param>
	/// <returns>List of rooms starting with start room and ending with end room or null if no solution</returns>
	public List<Room>? FindPath(Room start, Room end)
	{
		var path = new List<Room>();
		var hashSet = new HashSet<Room>();	// also all rooms in path, but keyed for quickly checking if a room is already in the path (i.e. Contains uses a hash rather than a scan)

		var room = start;
		var nextIx = 0;		// the index of the next room to select

		// while room not the ending room
		while (room != end)
		{
			if (nextIx < room.Connections.Count)
			{
				var nextRoom = room.Connections[nextIx];
				if (hashSet.Contains(nextRoom))
				{
					nextIx++;	// if this is the room we just came from, then skip. Could also be circular path (example doesn't have that though)
				}
				else
				{
					path.Add(room);
					hashSet.Add(room);
					room = nextRoom;
					nextIx = 0;
				}
			}
			else
			{
				// need to back up and try next connection/passage
				if (path.Count == 0) return null;	// if can't then no solution

				var prevRoom = path[path.Count - 1];
				path.Remove(prevRoom);
				hashSet.Remove(prevRoom);

				nextIx = prevRoom.Connections.IndexOf(room) + 1;
				room = prevRoom;
			}
		}
		path.Add(room);

		return path;
	}

	/// <summary>
	/// Find the first room with the given feature
	/// </summary>
	/// <param name="feature">What feature to look for (i.e. Start or End)</param>
	/// <returns>The first room with the given feature or null if not found</returns>
	public Room? FindRoom(Feature feature)
		=> _rooms.FirstOrDefault(room => room.Feature == feature);

	#endregion
	#region Create the room list

	/// <summary>
	/// Create the list of rooms and all their connections
	/// </summary>
	/// <param name="features">The maze bitmap converted to 2D feature array</param>
	private void CreateRooms(Feature[,] features)
	{
		var roomGrid = CreateRoomGrid(features);
		var gridWidth = roomGrid.GetLength(0);
		var gridHeight = roomGrid.GetLength(1);
		for (var x = 0; x != gridWidth; x++)
		{
			for (var y = 0; y != gridHeight; y++)
			{
				var room1 = roomGrid[x, y];
				_rooms.Add(room1);

				// connected to room to the right?
				if (x < gridWidth-1)
				{
					var room2 = roomGrid[x + 1, y];
					if (AreRoomsConnected(room1, room2, features))
					{
						room1.Connections.Add(room2);
						room2.Connections.Add(room1);
					}
				}

				// connected to room below?
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
	}

	/// <summary>
	/// Create a grid of rooms from the feature array.
	/// </summary>
	/// <param name="features">Describes the maze bitmap</param>
	/// <returns>A 2D array of rooms representation of the original maze bitmap</returns>
	private static Room[,] CreateRoomGrid(Feature[,] features)
	{
		// Note: it is assumed the maze is in a regular grid pattern. I don't assume the size of the rooms
		//	or even that they are square (although that is true of the test maze). It is assumed that
		//	walls are a single pixel in thickness and perfectly vertical or horizontal. This algorithm
		//	could be easily tweaked if necessary to accommodate imperfect walls.

		const int minWallLength = 2;			// minimum # pixels required to constitute a wall

		var width = features.GetLength(0);
		var height = features.GetLength(1);

		// determine where the vertical walls are
		var vertWalls = new List<int>();    // list of X coordinates from left to right of where the vertical walls are
		for (var x = 0; x != width; x++)
		{
			var wallCount = 0;
			for (var y = 0; y != height; y++)
			{
				if (features[x,y] != Feature.Wall)
					wallCount = 0;
				else if (++wallCount == minWallLength)
				{
					vertWalls.Add(x);
					break;
				}
			}
		}

		// determine where the horizontal walls are
		var horzWalls = new List<int>();    // list of Y coordinates from top to bottom of where the horizontal walls are
		for (var y = 0; y != height; y++)
		{
			var wallCount = 0;
			for (var x = 0; x != width; x++)
			{
				if (features[x, y] != Feature.Wall)
					wallCount = 0;
				else if (++wallCount == minWallLength)
				{
					horzWalls.Add(y);
					break;
				}
			}
		}

		// get # of rooms in each dimension and make sure we have some
		var numRoomsX = vertWalls.Count - 1;
		var numRoomsY = horzWalls.Count - 1;
		if (numRoomsX < 1 || numRoomsY < 1)
			throw new Exception("No maze rooms found");

		// now, using the wall locations, calculate room coordinates,
		//  determine what type it is, and add to the room grid
		var rooms = new Room[numRoomsX, numRoomsY];
		for (var x = 0; x != numRoomsX; x++)
		{
			for (var y = 0; y != numRoomsY; y++)
			{
				// get coordinates and size of room based on wall locations
				var roomX = vertWalls[x] + 1;
				var roomY = horzWalls[y] + 1;
				var roomWidth = vertWalls[x + 1] - roomX;
				var roomHeight = horzWalls[y + 1] - roomY;

				// look at pixel in center of the room to determine if start, end, or normal room
				var feature = features[roomX + roomWidth/2, roomY + roomHeight/2];
				var room = new Room(feature, roomX, roomY, roomWidth, roomHeight);
				rooms[x,y] = room;
			}
		}
		return rooms;
	}

	/// <summary>
	/// Checks if two rooms are connected or if a wall is between them. This assumes
	/// the two rooms are either in the same column or row
	/// </summary>
	/// <param name="room1">The first of the two rooms</param>
	/// <param name="room2">The second of the two rooms</param>
	/// <returns>true if no wall between them</returns>
	private static bool AreRoomsConnected(Room room1, Room room2, Feature[,] features)
	{
		// get coordinates of room centers
		var x1 = room1.X + room1.Width / 2;
		var y1 = room1.Y + room1.Height / 2;
		var x2 = room2.X + room2.Width / 2;
		var y2 = room2.Y + room2.Height / 2;

		// Determine the path we're stepping as we're scanning for the wall.
		// Note: this could be extended for arbitrary slopes (using fractional stepping in shorter dimension)
		var stepX = x1 == x2 ? 0 : x1 < x2 ? 1 : -1;
		var stepY = y1 == y2 ? 0 : y1 < y2 ? 1 : -1;

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
		for (var x = 0; x != bitmap.Width; x++)
			for (var y = 0; y != bitmap.Height; y++)
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

	private List<Room> _rooms = new();

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
