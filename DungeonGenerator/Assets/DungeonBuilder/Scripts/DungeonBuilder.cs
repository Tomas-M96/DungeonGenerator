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
                Vector3Int floorLayerTileLocation = new Vector3Int(x, y, 0);
                floorLayer.SetTile(floorLayerTileLocation, biomeTiles.FloorTile);
            }
        }
    }

    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int min = Math.Min(x1, x2);
        int max = Math.Max(x1, x2);

        for (int x = min; x <= max; x++)
        {
            Vector3Int floorLayerTileLocation = new Vector3Int(x, y, 0);
            floorLayer.SetTile(floorLayerTileLocation, biomeTiles.FloorTile);
        }
    }

    private void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int min = Math.Min(y1, y2);
        int max = Math.Max(y1, y2);

        for (int y = min; y <= max; y++)
        {
            Vector3Int floorLayerTileLocation = new Vector3Int(x, y, 0);
            floorLayer.SetTile(floorLayerTileLocation, biomeTiles.FloorTile);
        }
    }

    private void BuildWalls()
    {
        Vector3Int initialWallLocation = new Vector3Int(rooms[0].x1, rooms[0].y1 - 1, 0);
        floorLayer.SetTile(initialWallLocation, biomeTiles.BottomWall);
    }
}
