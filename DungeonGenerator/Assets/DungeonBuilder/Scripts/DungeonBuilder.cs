using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonBuilder : MonoBehaviour
{
    [Header("Map Size Variables")]
    [SerializeField] int mapWidth = 100;
    [SerializeField] int mapHeight = 100;
    [SerializeField] int maxRoomSize = 10;
    [SerializeField] int minRoomSize = 6;
    [SerializeField] int maxRooms = 30;

    [Header("Player Object")]
    [SerializeField] GameObject player;

    [Header("Dungeon Biome")]
    [SerializeField] private Biome biomeTiles;

    //Internal Variables
    int w, h, x, y;
    bool failed = false;
    List<Room> rooms = new List<Room>();
    int numRooms = 0;

    //Tilemap Variables
    Tilemap baseLayer;
    Tilemap floorLayer;
    Tilemap objectLayer;
    Tilemap wallLayer;
    Tilemap doorLayer;

    private void Start()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        baseLayer = tilemaps[0];
        floorLayer = tilemaps[2];
        objectLayer = tilemaps[1];
        wallLayer = tilemaps[3];
        doorLayer = tilemaps[4];

        CreateMap();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ClearMap();
        }
    }

    private void ClearMap()
    {
        baseLayer.ClearAllTiles();
        floorLayer.ClearAllTiles();
        objectLayer.ClearAllTiles();
        wallLayer.ClearAllTiles();
        doorLayer.ClearAllTiles();
        failed = false;
        rooms.Clear();
        numRooms = 0;
        CreateMap();
    }

    private void CreateMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int baseLayerTileLocation = new Vector3Int(x, y, 1);
                baseLayer.SetTile(baseLayerTileLocation, biomeTiles.BaseTile);
            }
        }
        GenerateRooms();
    }

    private void GenerateRooms()
    {

        for (int r = 0; r <= maxRooms; r++)
        {
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

            Room newRoom = new Room(x, y, w, h);

            foreach (Room otherRoom in rooms)
            {
                if (newRoom.intersect(otherRoom))
                {
                    failed = true;
                    break;
                }
            }

            if (!failed)
            {
                CreateRoom(newRoom);
                int[] newCenters = newRoom.center();

                if (numRooms == 0)
                {
                    print("Player Spawn Point");
                    // Vector3 spawnPoint = new Vector3(newCenters[0] + 0.5f, newCenters[1], -1);
                    // Instantiate(player, spawnPoint, Quaternion.identity);
                }
                else
                {

                    int[] prevCenters = rooms[numRooms - 1].center();


                    if (UnityEngine.Random.Range(0, 1) == 1)
                    {
                        CreateHorizontalCorridor(prevCenters[0], newCenters[0], prevCenters[1]);
                        CreateVerticalCorridor(prevCenters[1], newCenters[1], newCenters[0]);
                    }
                    else
                    {
                        CreateVerticalCorridor(prevCenters[1], newCenters[1], prevCenters[0]);
                        CreateHorizontalCorridor(prevCenters[0], newCenters[0], newCenters[1]);
                    }
                }
                rooms.Add(newRoom);
                numRooms += 1;
            }
            failed = false;
        }
        BuildWalls();
    }

    private void CreateRoom(Room room)
    {
        for (int x = room.x1; x < room.x2; x++)
        {
            for (int y = room.y1; y < room.y2; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                floorLayer.SetTile(currentTile, biomeTiles.FloorTile);
            }
        }
    }

    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int min = Math.Min(x1, x2);
        int max = Math.Max(x1, x2);

        for (int x = min; x <= max; x++)
        {
            Vector3Int currentTile = new Vector3Int(x, y, 0);
            Vector3Int upTile = new Vector3Int(x, y + 1, 0);
            Vector3Int downTile = new Vector3Int(x, y - 1, 0);

            floorLayer.SetTile(currentTile, biomeTiles.FloorTile);
        }
    }

    private void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int min = Math.Min(y1, y2);
        int max = Math.Max(y1, y2);

        for (int y = min; y <= max; y++)
        {
            Vector3Int currentTile = new Vector3Int(x, y, 0);
            Vector3Int leftTile = new Vector3Int(x - 1, y, 0);
            Vector3Int rightTile = new Vector3Int(x + 1, y, 0);

            floorLayer.SetTile(currentTile, biomeTiles.FloorTile);
        }
    }

    private void BuildWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
                Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
                Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);
                Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
                Vector3Int upRightCheck = new Vector3Int(x + 1, y + 1, 0);
                Vector3Int upLeftCheck = new Vector3Int(x - 1, y + 1, 0);
                Vector3Int downRightCheck = new Vector3Int(x + 1, y - 1, 0);
                Vector3Int downLeftCheck = new Vector3Int(x - 1, y - 1, 0);

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
        PaintTiles();
    }

    private void PaintTiles()
    {
        bool secondPass = false;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
                Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
                Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
                Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);

                if (floorLayer.GetTile(currentTile) == biomeTiles.PlaceholderWall)
                {
                    int tileAssignment = 0;

                    if (floorLayer.GetSprite(upCheck) != null)
                        tileAssignment += 1;
                    if (floorLayer.GetSprite(rightCheck) != null)
                        tileAssignment += 2;
                    if (floorLayer.GetSprite(downCheck) != null)
                        tileAssignment += 4;
                    if (floorLayer.GetSprite(leftCheck) != null)
                        tileAssignment += 8;

                    ChooseTile(tileAssignment, x, y, secondPass);
                }
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
                Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
                Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
                Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);

                if (floorLayer.GetTile(currentTile) == biomeTiles.PlaceholderWall)
                {
                    int tileAssignment = 0;

                    if (wallLayer.GetSprite(upCheck) != null)
                        tileAssignment += 1;
                    if (wallLayer.GetSprite(rightCheck) != null)
                        tileAssignment += 2;
                    if (wallLayer.GetSprite(downCheck) != null)
                        tileAssignment += 4;
                    if (wallLayer.GetSprite(leftCheck) != null)
                        tileAssignment += 8;

                    ChooseTile(tileAssignment, x, y, secondPass);
                }
            }
        }

        secondPass = true;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                Vector3Int upCheck = new Vector3Int(x, y + 1, 0);
                Vector3Int rightCheck = new Vector3Int(x + 1, y, 0);
                Vector3Int downCheck = new Vector3Int(x, y - 1, 0);
                Vector3Int leftCheck = new Vector3Int(x - 1, y, 0);

                if (floorLayer.GetTile(currentTile) == biomeTiles.PlaceholderWall)
                {
                    int tileAssignment = 0;

                    if (floorLayer.GetTile(upCheck) == biomeTiles.PlaceholderWall)
                        tileAssignment += 1;
                    if (floorLayer.GetTile(rightCheck) == biomeTiles.PlaceholderWall)
                        tileAssignment += 2;
                    if (floorLayer.GetTile(downCheck) == biomeTiles.PlaceholderWall)
                        tileAssignment += 4;
                    if (floorLayer.GetTile(leftCheck) == biomeTiles.PlaceholderWall)
                        tileAssignment += 8;

                    ChooseTile(tileAssignment, x, y, secondPass);
                }
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0);
                if (wallLayer.GetTile(currentTile) != null)
                {
                    floorLayer.SetTile(currentTile, null);
                }
            }
        }
        CreateStairs();
    }


    private void ChooseTile(int tileAssignment, int x, int y, bool secondPass)
    {
        Vector3Int currentTile = new Vector3Int(x, y, 0);
        if (!secondPass)
        {
            switch (tileAssignment)
            {
                case 3:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
                    break;
                case 6:
                    wallLayer.SetTile(currentTile, biomeTiles.TopLeftCornerWall);
                    break;
                case 7:
                    wallLayer.SetTile(currentTile, biomeTiles.RightWall);
                    break;
                case 9:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomRightCornerWall);
                    break;
                case 11:
                    wallLayer.SetTile(currentTile, biomeTiles.TopWall);
                    break;
                case 12:
                    wallLayer.SetTile(currentTile, biomeTiles.TopRightCornerWall);
                    break;
                case 13:
                    wallLayer.SetTile(currentTile, biomeTiles.LeftWall);
                    break;
                case 14:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomWall);
                    break;
                default:
                    Debug.Log("No Tile to paint");
                    break;
            }
        }
        else
        {
            switch (tileAssignment)
            {
                case 1:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
                    break;
                case 2:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
                    break;
                case 3:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
                    break;
                case 4:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
                    break;
                case 5:
                    wallLayer.SetTile(currentTile, biomeTiles.LeftWall);
                    break;
                case 6:
                    wallLayer.SetTile(currentTile, biomeTiles.TopLeftCornerWall);
                    break;
                case 7:
                    wallLayer.SetTile(currentTile, biomeTiles.RightJoinWall);
                    break;
                case 8:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomLeftCornerWall);
                    break;
                case 9:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomRightCornerWall);
                    break;
                case 10:
                    wallLayer.SetTile(currentTile, biomeTiles.BottomWall);
                    break;
                case 11:
                    wallLayer.SetTile(currentTile, biomeTiles.UpJoinWall);
                    break;
                case 12:
                    wallLayer.SetTile(currentTile, biomeTiles.TopRightCornerWall);
                    break;
                case 13:
                    wallLayer.SetTile(currentTile, biomeTiles.LeftJoinWall);
                    break;
                case 14:
                    wallLayer.SetTile(currentTile, biomeTiles.DownJoinWall);
                    break;
                default:
                    Debug.Log("No Tile to paint");
                    break;
            }
        }
    }
    private void CreateStairs()
    {
        int i = UnityEngine.Random.Range(0, rooms.Count);

        int x = UnityEngine.Random.Range(rooms[i].x1, rooms[i].x2);
        int y = UnityEngine.Random.Range(rooms[i].y1, rooms[i].y2);

        Vector3Int stairsTile = new Vector3Int(x, y, 0);
        wallLayer.SetTile(stairsTile, biomeTiles.Stairs);
    }
}
