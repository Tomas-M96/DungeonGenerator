using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Biome Tiles")]
public class Biome : ScriptableObject
{
    [Header("Floor Tiles")]
    [SerializeField] private Tile baseTile;
    [SerializeField] private Tile floorTile;

    [SerializeField] private Tile leftWallTile;
    [SerializeField] private Tile rightWallTile;
    [SerializeField] private Tile topWallTile;
    [SerializeField] private Tile bottomWallTile;

    [Header("Outer Corner Wall Tiles")]
    [SerializeField] private Tile topLeftCornerTile;
    [SerializeField] private Tile topRightCornerTile;
    [SerializeField] private Tile bottomRightCornerTile;
    [SerializeField] private Tile bottomLeftCornerTile;

    [Header("Single Wall Tiles")]
    [SerializeField] private Tile singleWallTile;


    public Tile BaseTile { get { return baseTile; } }
    public Tile FloorTile { get { return floorTile; } }

    public Tile LeftWall { get { return leftWallTile; } }
    public Tile RightWall { get { return rightWallTile; } }
    public Tile TopWall { get { return topWallTile; } }
    public Tile BottomWall { get { return bottomWallTile; } }

    public Tile TopLeftCornerWall { get { return topLeftCornerTile; } }
    public Tile TopRightCornerWall { get { return topRightCornerTile; } }
    public Tile BottomLeftCornerWall { get { return bottomLeftCornerTile; } }
    public Tile BottomRightCornerWall { get { return bottomRightCornerTile; } }

    public Tile SingleWall { get { return singleWallTile; } }
}
