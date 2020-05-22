public class Room
{
    public Room(int xPos, int yPos, int xWidth, int yHeight)
    {
        x1 = xPos;
        y1 = yPos;
        x2 = xPos + xWidth;
        y2 = yPos + yHeight;
    }

    public int x1 { get; set; }
    public int x2 { get; set; }
    public int y1 { get; set; }
    public int y2 { get; set; }


    //Function to get the center x and y values for the room
    public int[] center()
    {
        int[] centers = new int[2];
        centers[0] = (x1 + x2) / 2;
        centers[1] = (y1 + y2) / 2;
        return centers;
    }

    //Function to check if the room intersects with another room
    public bool intersect(Room otherRoom)
    {
        return (x1 <= otherRoom.x2 + 2 && x2 >= otherRoom.x1 - 2 &&
                y1 <= otherRoom.y2 + 2 && y2 >= otherRoom.y1 - 2);
    }
}