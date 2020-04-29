using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBuilder : MonoBehaviour
{

    public enum TileType
    {
        Wall, Floor
    }

    public int columns = 100;
    public int rows = 100;
    public IntRange numRooms = new IntRange (15,20);
    public IntRange roomWidth = new IntRange (3,20);
    public IntRange roomHeight = new IntRange (3,20);
    public IntRange corridorLength = new IntRange (6,20);
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] outerWallTiles;

    private TileType[][] tiles;
    private Room[] rooms;
    private Corridor[] corridors;
    private GameObject boardHolder;

    // Start is called before the first frame update
    void Start()
    {
        boardHolder = new GameObject("BoardHolder");

        SetupTilesArray();

        CreateRoomsAndCorridors();
        
        SetTilesValuesForRooms();
        SetTilesValuesForCorridors();

        InstantiateTiles();
        InstantiateOuterWalls();
    }


    void SetupTilesArray()
    {
        //Set the tiles jagged array to the correct width
        tiles = new TileType[columns][];

        //Go through all the tile arrays
        for (int i = 0; i < tiles.Length; i++)
        {
            //And set each tile array to the correct height
            tiles[i] = new TileType[rows];
        }
    }

    void CreateRoomsAndCorridors()
    {
        //Create the rooms array with a random size.
        rooms = new Room[numRooms.Random];

        // Create the corridors for the rooms, should always be on less than room
        corridors = new Corridor[rooms.Length - 1];

        //Create first room and corridor
        rooms[0] = new Room();
        corridors[0] = new Corridor();

        //Setup the first room, there is no previous corridor so we do not use one.
        rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

        //Setup the first corridor using the first room
        corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);

        for (int i=1; i<rooms.Length; i++)
        {
            //Create a room
            rooms[i] = new Room();

            //Setup the room based on previous corridor
            rooms[i].SetupRoom(roomWidth, roomHeight, columns, rows, corridors[i-1]);

            //If the end of corridors array hasn't been reached
            if (i < corridors.Length)
            {
                //Create a corridor
                corridors[i] = new Corridor();

                //Setup corridor based on the last created room
                corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
            }
        }
    }

    void SetTilesValuesForRooms()
    {
        //Go through all rooms
        for(int i=0; i<rooms.Length; i++)
        {
            Room currentRoom = rooms[i];
            
            //and for each room go through its width
            for (int j=0; j<currentRoom.roomWidth; j++)
            {
                int xCoord = currentRoom.xPos + j;

                //For each horizontal tile, go up veritcally through the rooms height
                for (int k = 0; k<currentRoom.roomHeight; k++)
                {
                    int yCoord = currentRoom.yPos + k;

                    //The coordinates in the jagged array are based on the room's position and it's width and height
                    tiles[xCoord][yCoord] = TileType.Floor;
                }
            }
        }
    }

    void SetTilesValuesForCorridors()
    {
        //Go through every corridor
        for (int i=0; i<corridors.Length; i++)
        {
            Corridor currentCorridor = corridors[i];

            //Go through its length
            for (int j = 0; j<currentCorridor.corridorLength; j++)
            {
                //Start the coordinates at the start of the corridor
                int xCoord = currentCorridor.startXPos;
                int yCoord = currentCorridor.startYPos;

                //Depending on the direction, add or subtract from the appropriate coordinate based on how far through the length the loop is
                switch (currentCorridor.direction)
                {
                    case Direction.North:
                        yCoord += j;
                        break;
                    case Direction.East:
                        yCoord += j;
                        break;
                    case Direction.South:
                        yCoord -= j;
                        break;
                    case Direction.West:
                        yCoord += j;
                        break;
                }
                tiles[xCoord][yCoord] = TileType.Floor;
            }
        }
    }

    void InstantiateTiles()
    {
        //Go threough all the tiles in the jagged array
        for (int i=0; i<tiles.Length; i++)
        {
            for (int j=0; j<tiles.Length; j++)
            {
                //Instantiate a floor tile for it
                InstantiateFromArray(floorTiles, i, j);

                //If the tile type is wall
                if (tiles[i][j] == TileType.Wall)
                {
                    //Instantiate a wall over the top
                    InstantiateFromArray(wallTiles, i, j);
                }
            }
        }
    }

    void InstantiateOuterWalls()
    {
        //The outer walls are one unit left, right, up and down from the board
        float leftEdgeX = -1f;
        float rightEdgeX = columns + 0f;
        float bottomEdgeY = -1f;
        float topEdgeY = rows + 0f;

        //Instantiate both vertical walls
        InstantiateVerticalOuterWall(leftEdgeX, bottomEdgeY, topEdgeY);
        InstantiateVerticalOuterWall(rightEdgeX, bottomEdgeY, topEdgeY);
    
        //Instantiate both horizontal walls
        InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
        InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
    }

    void InstantiateVerticalOuterWall(float xCoord, float startingY, float endingY)
    {
        //Start the loop at the starting value for y
        float currentY = startingY;

        //While the value for y is less than the end value
        while (currentY <= endingY)
        {
            //Instantiate an outer wall tile at the x coordinate and the current y coordinate
            InstantiateFromArray(outerWallTiles, xCoord, currentY);
            currentY++;
        }
    }

    void InstantiateHorizontalOuterWall(float yCoord, float startingX, float endingX)
    {
        //Start the loop at the starting value for x
        float currentX = startingX;

        //While the value for y is less than the end value
        while (currentX <= endingX)
        {
            //Instantiate an outer wall tile at the x coordinate and the current y coordinate
            InstantiateFromArray(outerWallTiles, yCoord, currentX);
            currentX++;
        }
    }

    void InstantiateFromArray(GameObject[] prefabs, float xCoord, float yCoord)
    {
        //Create a random index for the array
        int randomIndex = Random.Range(0, prefabs.Length);

        //The position to be instantiated at is based on the coordinates
        Vector3 position = new Vector3(xCoord, yCoord, 0f);

        //Create an instance of the prefab from the random index of array
        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        //Set the tiles parent to the board holder
        tileInstance.transform.parent = boardHolder.transform;
    }
}
