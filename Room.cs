
namespace BluebeamMaze;

internal class Room(Feature feature, int x, int y, int width, int height)
{
	public Feature Feature => feature;
	public int X => x;
	public int Y => y;
	public int Width => width;
	public int Height => height;

	public List<Room> Connections { get; } = new List<Room>();
}
