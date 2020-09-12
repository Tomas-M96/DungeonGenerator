using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonBuilder : MonoBehaviour
{
    //Editor Variables
    [Header("Map Size Variables")]
    [Tooltip("Maximum width of the map")]
    [SerializeField] 
    private int mapWidth = 100;
    [Tooltip("Maximum height of the map")]
    [SerializeField]
    private int mapHeight = 100;
    [Tooltip("Maximum room size")]
    [SerializeField]
    private int maxRoomSize = 10;
    [Tooltip("Minimum room size")]
    [SerializeField]
    private int minRoomSize = 6;
    [Tooltip("Maximum number of rooms to attempt")]
    [SerializeField]
    private int maxRooms = 30;

    [Header("Player")]
    [Tooltip("Player Object")]
    [SerializeField] 
    GameObject player;

    [Header("Dungeon Biome")]
    [Tooltip("Biome of the dungeon")]
    [SerializeField] 
    private Biome biomeTiles;

    //Internal Variables
    private int w, h, x, y;
    private bool failed = false;
    private List<Room> rooms = new List<Room>();
    private List<Room> hiddenRooms = new List<Room>();
    private int numRooms = 0;

    //Tilemap Layer Variables
    private Tilemap baseLayer;
    private Tilemap floorLayer;
    private Tilemap objectLayer;
    private Tilemap wallLayer;
    private Tilemap doorLayer;

    private void Start()
    {
        //Find the tilemaps and assign them to their respective variable
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        baseLayer = tilemaps[0];
        floorLayer = tilemaps[2];
        objectLayer = tilemaps[1];
        wallLayer = tilemaps[3];
        doorLayer = tilemaps[4];

        //Call the CreateMap function
        CreateMap();
    }

    private void Update()
    {
        //If space is pressed, regen the map
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ClearMap();
        }
    }

    //Function to regen the map
    private void ClearMap()
    {
        //Clears all tilemaps
        baseLayer.ClearAllTiles();
        floorLayer.ClearAllTiles();
        objectLayer.ClearAllTiles();
        wallLayer.ClearAllTiles();
        doorLayer.ClearAllTiles();
        //Sets the failed bool to false
        failed = false;
        //Clears the room array
        rooms.Clear();
        hiddenRooms.Clear();
        //Resets numRooms
        numRooms = 0;
        //Calls CreateMap again
        CreateMap();
    }

    private void CreateMap()
    {
        //Loops through each tile of the room width and height
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //Spawns a base tile for every tile
                Vector3Int baseLayerTileLocation = new Vector3Int(x, y, 1);
                baseLayer.SetTile(baseLayerTileLocation, biomeTiles.BaseTile);
            }
        }

        //Calls GenerateRooms
        GenerateRooms();
    }

    private void GenerateRooms()
    {
        //Loops until the maximum number of rooms has been attempted
        for (int r = 0; r <= maxRooms; r++)
        {
            //Gets a random number for width, height, xpos and ypos
            w = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
            h = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
            x = UnityEngine.Random.Range(0, mapWidth - w);
            y = UnityEngine.Random.Range(0, mapHeight - h);

            if (x == 0)
            {
                x += 1;
            }
            else if (x == mapWidth - 1)
            {
                x -= 1;
            }

            if (y == 0)
            {
                y += 1;
            }
            else if (y == mapHeight - 1)
            {
                y -= 1;
            }

            //Creates a new room
            Room newRoom = new Room(x, y, w, h);

            //Loop to check if the created room intersects with any rooms in the room list
            foreach (Room otherRoom in rooms)
            {
                if (newRoom.intersect(otherRoom))
                {
                    //If it does set failed to true and break the loop
                    failed = true;
                    break;
                }
            }

            //If it does not intersect
            if (!failed)
            {
                //Call the CreateRoom method, passing in the new room
                CreateRoom(newRoom);
                //Get the center positions of the new room
                int[] newCenters = newRoom.center();

                //If this is the first room
                if (numRooms == 0)
                {
                    //Spawn the player
                    print("Player Spawn Point");
                    // Vector3 spawnPoint = new Vector3(newCenters[0] + 0.5f, newCenters[1], -1);
                    // Instantiate(player, spawnPoint, Quaternion.identity);
                }
                else
                {
                    //Get the center values of the previous room
                    int[] prevCenters = rooms[numRooms - 1].center();

                    ////randomise choice of what corridors to create first
                    //if (UnityEngine.Random.Range(0, 1) == 1)
                    //{
                    //    //Creates the corridors using the center values
                    //    CreateHorizontalCorridor(prevCenters[0], newCenters[0], prevCenters[1]);
                    //    CreateVerticalCorridor(prevCenters[1], newCenters[1], newCenters[0]);
                    //}
                    //else
                    //{
                    //    CreateVerticalCorridor(prevCenters[1], newCenters[1], prevCenters[0]);
                    //    CreateHorizontalCorridor(prevCenters[0], newCenters[0], newCenters[1]);
                    //}
                }

                //Add the room to the rooms list
                rooms.Add(newRoom);
                //Increment the numRooms variable
                numRooms += 1;
            }

            //Set failed to false
            failed = false;
        }

        CreateHiddenRooms();
        //Call the BuildWalls method
        //BuildWalls();
    }

    private void CreateRoom(Room room)
    {
        //Loop through the width and xpos of the room
        for (int x = room.x1; x < room.x2; x++)
        {
            //Loop through the height and ypos of the room
            for (int y = room.y1; y < room.y2; y++)
            {
                //Spawn a floor tile for each position
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                floorLayer.SetTile(currentTile, biomeTiles.FloorTile);
            }
        }
    }

    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        //Gets the smallest value of the two rooms xpos
        int min = Math.Min(x1, x2);
        //Gets the largest value of the two rooms xpos
        int max = Math.Max(x1, x2);

        //Loop from the smallest value to the largest value
        for (int x = min; x <= max; x++)
        {
            //Spawn a floor tile at the current xpos
            Vector3Int currentTile = new Vector3Int(x, y, 0);

            floorLayer.SetTile(currentTile, biomeTiles.FloorTile);
        }
    }

    private void CreateVerticalCorridor(int y1, int y2, int x)
    {
        //Gets the smallest value of the two rooms ypos
        int min = Math.Min(y1, y2);
        //Gets the largest value of the two rooms ypos
        int max = Math.Max(y1, y2);

        //Loop from the smallest value to the largest value
        for (int y = min; y <= max; y++)
        {
            //Spawn a floor tile at the current ypos
            Vector3Int currentTile = new Vector3Int(x, y, 0);

            floorLayer.SetTile(currentTile, biomeTiles.FloorTile);
        }
    }

    private void BuildWalls()
    {
        //Loops through the map
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //Sets variables for checking the tiles surrounding the current tile
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
                Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
                Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);
                Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
                Vector3Int upRightCheck = new Vector3Int(x + 1, y + 1, 0);
                Vector3Int upLeftCheck = new Vector3Int(x - 1, y + 1, 0);
                Vector3Int downRightCheck = new Vector3Int(x + 1, y - 1, 0);
                Vector3Int downLeftCheck = new Vector3Int(x - 1, y - 1, 0);

                //If statements to set placeholder walls
                if (floorLayer.GetTile(currentTile) == biomeTiles.FloorTile)
                {
                    if (floorLayer.GetTile(upCheck) == null)
                    {
                        floorLayer.SetTile(upCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(downCheck) == null)
                    {
                        floorLayer.SetTile(downCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(leftCheck) == null)
                    {
                        floorLayer.SetTile(leftCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(rightCheck) == null)
                    {
                        floorLayer.SetTile(rightCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(upRightCheck) == null)
                    {
                        floorLayer.SetTile(upRightCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(upLeftCheck) == null)
                    {
                        floorLayer.SetTile(upLeftCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(downRightCheck) == null)
                    {
                        floorLayer.SetTile(downRightCheck, biomeTiles.PlaceholderWall);
                    }
                    if (floorLayer.GetTile(downLeftCheck) == null)
                    {
                        floorLayer.SetTile(downLeftCheck, biomeTiles.PlaceholderWall);
                    }
                }
            }
        }
        
        
        //Calls PaintTiles
        //TODO - THIS NEEDS REWORKING
        //PaintTiles();
    }

    //TODO - THIS NEEDS REWORKING
    //private void PaintTiles()
    //{
    //    //Sets secondPass to false
    //    bool secondPass = false;


    //    //Loops through the map
    //    for (int x = 0; x < mapWidth; x++)
    //    {
    //        for (int y = 0; y < mapHeight; y++)
    //        {
    //            //Sets up variables for checking tiles surrounding the current tile
    //            Vector3Int currentTile = new Vector3Int(x, y, 0);
    //            Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
    //            Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
    //            Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
    //            Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);

    //            //If the current tile is a placeholder wall
    //            if (floorLayer.GetTile(currentTile) == biomeTiles.PlaceholderWall)
    //            {
    //                //Reset the tileAssignment variable
    //                int tileAssignment = 0;

    //                //If statements to bitmask the tile value
    //                if (floorLayer.GetSprite(upCheck) != null)
    //                    tileAssignment += 1;
    //                if (floorLayer.GetSprite(rightCheck) != null)
    //                    tileAssignment += 2;
    //                if (floorLayer.GetSprite(downCheck) != null)
    //                    tileAssignment += 4;
    //                if (floorLayer.GetSprite(leftCheck) != null)
    //                    tileAssignment += 8;

    //                //Calls the ChooseTile method passing in the bitmask value and x and y positions
    //                ChooseTile(tileAssignment, x, y, secondPass);
    //            }
    //        }
    //    }

    //    //Loops through the map
    //    for (int x = 0; x < mapWidth; x++)
    //    {
    //        for (int y = 0; y < mapHeight; y++)
    //        {
    //            Vector3Int currentTile = new Vector3Int(x, y, 0);
    //            Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
    //            Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
    //            Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
    //            Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);

    //            if (floorLayer.GetTile(currentTile) == biomeTiles.PlaceholderWall)
    //            {
    //                int tileAssignment = 0;

    //                if (wallLayer.GetSprite(upCheck) != null)
    //                    tileAssignment += 1;
    //                if (wallLayer.GetSprite(rightCheck) != null)
    //                    tileAssignment += 2;
    //                if (wallLayer.GetSprite(downCheck) != null)
    //                    tileAssignment += 4;
    //                if (wallLayer.GetSprite(leftCheck) != null)
    //                    tileAssignment += 8;

    //                ChooseTile(tileAssignment, x, y, secondPass);
    //            }
    //        }
    //    }

    //    //Sets secondPass to true
    //    secondPass = true;

    //    //Loops through the map
    //    for (int x = 0; x < mapWidth; x++)
    //    {
    //        for (int y = 0; y < mapHeight; y++)
    //        {
    //            Vector3Int currentTile = new Vector3Int(x, y, 0);
    //            Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
    //            Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
    //            Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
    //            Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);

    //            if (floorLayer.GetTile(currentTile) == biomeTiles.PlaceholderWall)
    //            {
    //                int tileAssignment = 0;

    //                if (floorLayer.GetTile(upCheck) == biomeTiles.PlaceholderWall)
    //                    tileAssignment += 1;
    //                if (floorLayer.GetTile(rightCheck) == biomeTiles.PlaceholderWall)
    //                    tileAssignment += 2;
    //                if (floorLayer.GetTile(downCheck) == biomeTiles.PlaceholderWall)
    //                    tileAssignment += 4;
    //                if (floorLayer.GetTile(leftCheck) == biomeTiles.PlaceholderWall)
    //                    tileAssignment += 8;

    //                ChooseTile(tileAssignment, x, y, secondPass);
    //            }
    //        }
    //    }

    //    //Loops through the map
    //    for (int x = 0; x < mapWidth; x++)
    //    {
    //        for (int y = 0; y < mapHeight; y++)
    //        {
    //            //Clears all remaining placeholder tiles
    //            Vector3Int currentTile = new Vector3Int(x, y, 0);
    //            if (wallLayer.GetTile(currentTile) != null)
    //            {
    //                floorLayer.SetTile(currentTile, null);
    //            }
    //        }
    //    }
    //    CreateStairs();
    //}


    //private void ChooseTile(int tileAssignment, int x, int y, bool secondPass)
    //{
    //    Vector3Int currentTile = new Vector3Int(x, y, 0);
    //    if (!secondPass)
    //    {
    //        switch (tileAssignment)
    //        {
    //            case 3:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
    //                break;
    //            case 6:
    //                wallLayer.SetTile(currentTile, biomeTiles.TopLeftCornerWall);
    //                break;
    //            case 7:
    //                wallLayer.SetTile(currentTile, biomeTiles.RightWall);
    //                break;
    //            case 9:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomRightCornerWall);
    //                break;
    //            case 11:
    //                wallLayer.SetTile(currentTile, biomeTiles.TopWall);
    //                break;
    //            case 12:
    //                wallLayer.SetTile(currentTile, biomeTiles.TopRightCornerWall);
    //                break;
    //            case 13:
    //                wallLayer.SetTile(currentTile, biomeTiles.LeftWall);
    //                break;
    //            case 14:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomWall);
    //                break;
    //            default:
    //                Debug.Log("No Tile to paint");
    //                break;
    //        }
    //    }
    //    else
    //    {
    //        switch (tileAssignment)
    //        {
    //            case 1:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
    //                break;
    //            case 2:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
    //                break;
    //            case 3:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
    //                break;
    //            case 4:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
    //                break;
    //            case 5:
    //                wallLayer.SetTile(currentTile, biomeTiles.LeftWall);
    //                break;
    //            case 6:
    //                wallLayer.SetTile(currentTile, biomeTiles.TopLeftCornerWall);
    //                break;
    //            case 7:
    //                wallLayer.SetTile(currentTile, biomeTiles.RightJoinWall);
    //                break;
    //            case 8:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
    //                break;
    //            case 9:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomRightCornerWall);
    //                break;
    //            case 10:
    //                wallLayer.SetTile(currentTile, biomeTiles.BottomWall);
    //                break;
    //            case 11:
    //                wallLayer.SetTile(currentTile, biomeTiles.UpJoinWall);
    //                break;
    //            case 12:
    //                wallLayer.SetTile(currentTile, biomeTiles.TopRightCornerWall);
    //                break;
    //            case 13:
    //                wallLayer.SetTile(currentTile, biomeTiles.LeftJoinWall);
    //                break;
    //            case 14:
    //                wallLayer.SetTile(currentTile, biomeTiles.DownJoinWall);
    //                break;
    //            default:
    //                Debug.Log("No Tile to paint");
    //                break;
    //        }
    //    }
    //}

    private void CreateStairs()
    {
        //Get a random room
        int i = UnityEngine.Random.Range(0, rooms.Count);

        //Uses the random room to get a random x and y value from it
        int x = UnityEngine.Random.Range(rooms[i].x1, rooms[i].x2);
        int y = UnityEngine.Random.Range(rooms[i].y1, rooms[i].y2);

        //Spawns a staircase at the random position
        Vector3Int stairsTile = new Vector3Int(x, y, 0);
        wallLayer.SetTile(stairsTile, biomeTiles.Stairs);
    }

    private void CreateHiddenRooms()
    {
        while (hiddenRooms.Count < 1)
        {
            x = UnityEngine.Random.Range(0, mapWidth - w);
            y = UnityEngine.Random.Range(0, mapHeight - h);

            failed = false;

            Room hiddenRoom = new Room(x, y, 6, 6);

            foreach (Room otherRoom in rooms)
            {
                if (hiddenRoom.intersect(otherRoom))
                {
                    //If it does set failed to true and break the loop
                    failed = true;
                    break;
                }
            }

            //If the previous loop fails, try loop again
            if (failed)
            {
                continue;
            }
            else
            {
                hiddenRooms.Add(hiddenRoom);
                //Loop through the width and xpos of the room
                for (int x = hiddenRoom.x1; x < hiddenRoom.x2; x++)
                {
                    //Loop through the height and ypos of the room
                    for (int y = hiddenRoom.y1; y < hiddenRoom.y2; y++)
                    {
                        //Spawn a floor tile for each position
                        Vector3Int currentTile = new Vector3Int(x, y, 0);
                        floorLayer.SetTile(currentTile, biomeTiles.Stairs);
                    }
                }
            }
        }


    }
}
