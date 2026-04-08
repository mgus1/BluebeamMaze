namespace BluebeamMaze;

/// <summary>
/// This represents a single rectangular room in the maze.
/// </summary>
/// <param name="feature">The type of room. Currently Start, End, or None (a normal room)</param>
/// <param name="x">X coordinate in maze's bitmap</param>
/// <param name="y">Y coordinate in maze's bitmap</param>
/// <param name="width">Width of the room</param>
/// <param name="height">Height of the room</param>
internal class Room(Feature feature, int x, int y, int width, int height)
{
	public Feature Feature => feature;

	public int X => x;
	public int Y => y;
	public int Width => width;
	public int Height => height;

	/// <summary>
	/// This is a list of rooms this room is connected to.
	/// </summary>
	public List<Room> Connections { get; } = new List<Room>();
}
