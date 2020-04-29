using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.AI;

public class Room {
	public int x = 0;
	public int y = 0;
	public int w = 0;
	public int h = 0;
	public Room connectedTo = null;
	public int branch = 0;
	public string relative_positioning = "x";
	public bool dead_end = false;
	public int room_id = 0;
}

public class SpawnList {
	public int x;
	public int y;
	public bool byWall;
    public string wallLocation;
    public bool inTheMiddle;
	public bool byCorridor;

	public int asDoor = 0;
	public Room room = null;
	public bool spawnedObject;
}

[System.Serializable]
public class SpawnOption {
	public int minSpawnCount;
	public int maxSpawnCount;
	public bool spawnByWall;
    public bool spawmInTheMiddle;
    public bool spawnRotated;
    //public bool byCorridor;
    public float heightFix = 0;

    public GameObject gameObject;
	[Tooltip("Use 0 for random room, make sure spawn room isnt bigger than your room count")]
	public int spawnRoom = 0;
}

[System.Serializable]
public class CustomRoom {
	[Tooltip("make sure room id isnt bigger than your room count")]
	public int roomId = 1;
	public GameObject floorPrefab;
	public GameObject wallPrefab;
	public GameObject doorPrefab;
	public GameObject cornerPrefab;
}

public class MapTile {
	public int type = 0; //Default = 0 , Room Floor = 1, Wall = 2, Corridor Floor 3, Room Corners = 4, 5, 6 , 7
    public int orientation = 0;
	public Room room = null;   
}


public class Dungeonizer : MonoBehaviour {
	public GameObject startPrefab;
	public GameObject exitPrefab;
	public List<SpawnList> spawnedObjectLocations = new List<SpawnList>();
	public GameObject floorPrefab;
	public GameObject wallPrefab;
	public GameObject doorPrefab;
	//public GameObject doorCorners;
	public GameObject corridorFloorPrefab;

	public GameObject cornerPrefab;
	public bool cornerRotation = false;

    public int maximumRoomCount = 10;

    [Tooltip("Maximum gap between rooms. Also affects corridor lengths ")]
    public int roomMargin = 3;
    [Tooltip("If Checked: makes dungeon reset on every time level loads.")]
    public bool generate_on_load = true;
	public int minRoomSize = 5;
	public int maxRoomSize = 10;
	public float tileScaling = 1f;
	public List<SpawnOption> spawnOptions = new List<SpawnOption>();
	public List<CustomRoom> customRooms = new List<CustomRoom> ();
	public bool makeIt3d = false;

    //public NavMeshSurface surface;

    class Dungeon {
		public static int map_size;
		public static int map_size_x;
		public static int map_size_y;


		public static MapTile[,] map;

		public static List<Room> rooms = new List<Room>();
		
		public static Room goalRoom;
		public static Room startRoom;
		
		public int min_size;
		public int max_size;
		
		public int maximumRoomCount;
		public int roomMargin;
		public int roomMarginTemp;

		//tile types for ease
		public static List<int> roomsandfloors = new List<int> { 1, 3 };
		public static List<int> corners = new List<int> {4,5,6,7};
		public static List<int> walls = new List<int> {8,9,10,11};
		private static List<string> directions = new List<string> {"x","y","-y","-x"}; //,"-y"};
		
		public void Generate() {
			int room_count = this.maximumRoomCount;
			int min_size = this.min_size;
			int max_size = this.max_size;
            if(roomMargin < 2)
            {
                map_size = room_count * max_size * 2;
            }
            else
            {
                map_size = (room_count * (max_size + (roomMargin * 2))) + (room_count * room_count * 2);
            }
            map = new MapTile[map_size, map_size];

			for (var x = 0; x < map_size; x++) {
				for (var y = 0; y < map_size; y++) {
					map [x, y] = new MapTile ();
					map [x, y].type = 0;
				}
			}
			rooms = new List<Room> ();
			


			int collision_count = 0;
			string direction = "set";
            string oldDirection = "set";
			Room lastRoom;


			for (var i = 0; i < room_count; i++) {
				Room room = new Room (); 
				if (rooms.Count == 0) {
					//first room
					room.x = (int)Mathf.Floor (map_size / 2f);
					room.y = (int)Mathf.Floor (map_size / 2f); //Random.Range(10,20);
					room.w = Random.Range (min_size, max_size);
                    if (room.w % 2 == 0) room.w += 1;
					room.h = Random.Range (min_size, max_size);
                    if (room.h % 2 == 0) room.h += 1;
					room.branch = 0;
					lastRoom = room;
				} else {
					int branch = 0;
					if (collision_count == 0) {
						branch = Random.Range (5, 20); //complexity
					}
					room.branch = branch;

					lastRoom = rooms [rooms.Count - 1];
					int lri = 1;

					while (lastRoom.dead_end) {
						lastRoom = rooms [rooms.Count - lri++];
					}


                    if (direction == "set") {
                        string newRandomDirection = directions[Random.Range(0, directions.Count)];
                        direction = newRandomDirection;
                        while (direction == oldDirection)
                        {
                            newRandomDirection = directions[Random.Range(0, directions.Count)];
                            direction = newRandomDirection;
                        }

                    }
                    this.roomMarginTemp = Random.RandomRange(0, this.roomMargin - 1);

                    if (direction == "y") {
						room.x = lastRoom.x + lastRoom.w + Random.Range (3, 5) + this.roomMarginTemp;
						room.y = lastRoom.y;
					} else if (direction == "-y") {
						room.x = lastRoom.x - lastRoom.w - Random.Range (3, 5) - this.roomMarginTemp;
						room.y = lastRoom.y;
					} else if (direction == "x") {
						room.y = lastRoom.y + lastRoom.h + Random.Range (3, 5) + this.roomMarginTemp;
						room.x = lastRoom.x;
					} else if (direction == "-x") {
						room.y = lastRoom.y - lastRoom.h - Random.Range (3, 5) - this.roomMarginTemp;
						room.x = lastRoom.x;
					}

					room.w = Random.Range (min_size, max_size);
                    if (room.w % 2 == 0) room.w += 1;

                    room.h = Random.Range (min_size, max_size);
                    if (room.h % 2 == 0) room.h += 1;

                    room.connectedTo = lastRoom;
				}

				bool doesCollide = this.DoesCollide (room, 0);				
				if (doesCollide) {
					i--;
					collision_count += 1;
					if (collision_count > 3) {
						lastRoom.branch = 1;
						lastRoom.dead_end = true;
						collision_count = 0;
					} else {
                        oldDirection = direction;
						direction = "set";
					}
				} else {
					room.room_id = i;
					rooms.Add (room);
                    oldDirection = direction;
					direction = "set";
				}
			}

			//room making
			for (int i = 0; i < rooms.Count; i++) {
				Room room = rooms [i];
				for (int x = room.x; x < room.x + room.w; x++) {                    
                    for (int y = room.y; y < room.y + room.h; y++) {                        
                        map[x, y].type = 1;
						map [x, y].room = room;

					}
				}
			}

			//corridor making
			for (int i = 1; i < rooms.Count; i++) {
				Room roomA = rooms [i];
				Room roomB = rooms [i].connectedTo;

				if (roomB != null) {
					var pointA = new Room (); //start
					var pointB = new Room ();

					pointA.x = roomA.x + (int)Mathf.Floor (roomA.w / 2);
					pointB.x = roomB.x + (int)Mathf.Floor (roomB.w / 2);

					pointA.y = roomA.y + (int)Mathf.Floor (roomA.h / 2);
					pointB.y = roomB.y + (int)Mathf.Floor (roomB.h / 2);

					if (Mathf.Abs (pointA.x - pointB.x) > Mathf.Abs (pointA.y - pointB.y)) {
						//yatay
						if (roomA.h > roomB.h) {
							pointA.y = pointB.y;

						} else {
							pointB.y = pointA.y;

						}
					} else {
						//dikey
						if (roomA.w > roomB.w) {
							pointA.x = pointB.x;
						} else {
							pointB.x = pointA.x;
						}					
					}

					while ((pointB.x != pointA.x) || (pointB.y != pointA.y)) {
						if (pointB.x != pointA.x) {
							if (pointB.x > pointA.x) {
								pointB.x--;
							} else {
							

								pointB.x++;
							}
						} else if (pointB.y != pointA.y) {
							if (pointB.y > pointA.y) {
								pointB.y--;
							} else {
								pointB.y++;
							}
						}



						if (map [pointB.x, pointB.y].room == null) {
                            map[pointB.x, pointB.y].type = 3;
						}

					} 

				}
			}



            //x crop; because map created in the middle of array and we are pushing it to bottom left edge.
            int row = 1;
			int min_crop_x = map_size;
			for (int x = 0; x < map_size -1; x++) {
				bool x_empty = true;
				for (int y = 0; y < map_size -1; y++) {
					if (map[x,y].type != 0){
						x_empty = false;
						if(x < min_crop_x){
							min_crop_x = x;
						}
						break;
					}
				}
				if (!x_empty){
					for (int y=0;y < map_size -1;y++){
						map[row,y] = map[x,y];
						map[x,y] = new MapTile();
					}
					row += 1;
				}
			}
			
			
			//y crop
			row = 1;
			int min_crop_y = map_size;
			for (int y = 0; y < map_size -1; y++) {
				bool y_empty = true;
				for (int x = 0; x < map_size -1; x++) {
					if (map[x,y].type != 0){
						y_empty = false;
						if(y < min_crop_y){
							min_crop_y = y;
						}
						break;
					}
				}
				if (!y_empty){
					for (int x=0;x < map_size -1;x++){
						map[x,row] = map[x,y];
						map[x,y] = new MapTile();
					}
					row += 1;
				}
			}
			foreach (Room room in rooms) {
				room.x -= min_crop_x;
				room.y -= min_crop_y;
			}

			//test map size
			int final_map_size_y = 0;
			for (int y = 0; y < map_size -1; y++) {
				for (int x = 0; x < map_size -1; x++) {
					if (map[x,y].type != 0){
						final_map_size_y += 1;
						break;
					}
				}
			}

			int final_map_size_x = 0;
			for (int x = 0; x < map_size -1; x++) {
				for (int y = 0; y < map_size -1; y++) {
					if (map[x,y].type != 0){
						final_map_size_x += 1;
						break;
					}
				}
			}

			final_map_size_x += 5;
			final_map_size_y += 5;

			MapTile[,] new_map = new MapTile[final_map_size_x + 1, final_map_size_y + 1];
			for (int x= 0; x < final_map_size_x; x++) {
				for(int y=0;y < final_map_size_y; y++){
					new_map[x,y] = map[x,y];
				}
			}
			map = new_map;
			map_size_x = final_map_size_x;
			map_size_y = final_map_size_y;

			//walls 
			for (int x = 0; x < map_size_x -1; x++) {
				for (int y = 0; y < map_size_y -1; y++) {
					if (map [x, y].type == 0) {
						if (map [x + 1, y].type == 1 || map [x + 1, y].type == 3) { //west
							map [x, y].type = 11;
							map [x, y].room = map [x + 1, y].room;
						}
						if (x > 0) {
							if (map [x - 1, y].type == 1 || map [x - 1, y].type == 3) { //east
								map [x, y].type = 9;
								map [x, y].room = map [x - 1, y].room;

							}
						}
						
						if (map [x, y + 1].type == 1 || map [x, y + 1].type == 3) { //south
							map [x, y].type = 10;
							map [x, y].room = map [x, y + 1].room;

						}
						
						if (y > 0) {
							if (map [x, y - 1].type == 1 || map [x, y - 1].type == 3) { //north
								map [x, y].type = 8;
								map [x, y].room = map [x, y - 1].room;

							}
						}
					}
				}
			}
			
			//corners
			for (int x = 0; x < map_size_x -1; x++) {
				for (int y = 0; y < map_size_y -1; y++) {
					if (walls.Contains (map [x, y + 1].type) && walls.Contains (map [x + 1, y].type) && roomsandfloors.Contains (map [x + 1, y + 1].type)) { //north
						map [x, y].type = 4;
						map [x, y].room = map [x + 1, y + 1].room;
					}
					if (y > 0) {
						if (walls.Contains (map [x + 1, y].type) && walls.Contains (map [x, y - 1].type) && roomsandfloors.Contains (map [x + 1, y - 1].type)) { //north
							map [x, y].type = 5;
							map [x, y].room = map [x + 1, y - 1].room;

						}
					}
					if (x > 0) {
						if (walls.Contains (map [x - 1, y].type) && walls.Contains (map [x, y + 1].type) && roomsandfloors.Contains (map [x - 1, y + 1].type)) { //north
							map [x, y].type = 7;
							map [x, y].room = map [x - 1, y + 1].room;

						}
					}
					if (x > 0 && y > 0) {
						if (walls.Contains (map [x - 1, y].type) && walls.Contains (map [x, y - 1].type) && roomsandfloors.Contains (map [x - 1, y - 1].type)) { //north
							map [x, y].type = 6;
							map [x, y].room = map [x - 1, y - 1].room;

						}
					}
					/* door corners --- a bit problematic in this version */
					if (map [x, y].type == 3) { 
						if (map [x + 1, y].type == 1) {
							map [x, y + 1].type = 11;
							map [x, y - 1].type = 11;
						} else if (Dungeon.map [x - 1, y].type == 1) {
							map [x, y + 1].type = 9;
							map [x, y - 1].type = 9;
						}
					}

				}
			}

            for (int x = 0; x < map_size_x - 1; x++)
            {
                for (int y = 0; y < map_size_y - 1; y++)
                {
                    if(map[x, y].type == 3)
                    {
                        bool cw = map[x, y + 1].type == 3;
                        bool ce = map[x, y - 1].type == 3;
                        bool cn = map[x + 1, y].type == 3;
                        bool cs = map[x - 1, y].type == 3;
                        if(cw || ce)
                        {
                            map[x, y].orientation = 1;
                        }
                        else if(cn || cs)
                        {
                            map[x, y].orientation = 2;
                        }
                    }
                }
            }
 
            //find far far away room
            goalRoom = rooms[rooms.Count -1 ];
			if (goalRoom != null) {
				goalRoom.x = goalRoom.x + (goalRoom.w / 2);
				goalRoom.y = goalRoom.y + (goalRoom.h / 2);
			}
			//starting point
			startRoom = rooms[0];
			startRoom.x = startRoom.x + (startRoom.w / 2);
			startRoom.y = startRoom.y + (startRoom.h / 2);
			
		}

		private bool DoesCollide (Room room, int ignore) {
			int random_blankliness = 0;

			for (int i = 0; i < rooms.Count; i++) {
				//if (i == ignore) continue;
				var check = rooms[i];
				if (!((room.x + room.w + random_blankliness < check.x) ||
                     (room.x > check.x + check.w + random_blankliness) || 
                     (room.y + room.h + random_blankliness < check.y) || 
                     (room.y > check.y + check.h + random_blankliness)))
                    return true;
			}
			
			return false;
		}
 

		private float lineDistance( Room point1, Room point2 )
		{
			var xs = 0;
			var ys = 0;
			
			xs = point2.x - point1.x;
			xs = xs * xs;
			
			ys = point2.y - point1.y;
			ys = ys * ys;
			
			return Mathf.Sqrt( xs + ys );
		}



	}

	public void ClearOldDungeon(bool immediate = false)
	{
		int childs = transform.childCount;
		for (var i = childs - 1; i >= 0; i--)
		{
			if(immediate){
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
			else {
				Destroy(transform.GetChild(i).gameObject);
			}
		}
	}


	public void Generate()
	{
		Dungeon dungeon = new Dungeon ();
		
		dungeon.min_size = minRoomSize;
		dungeon.max_size = maxRoomSize;
		dungeon.maximumRoomCount = maximumRoomCount;
		dungeon.roomMargin = roomMargin;
		
		dungeon.Generate ();
		
		//Dungeon.map = floodFill(Dungeon.map,1,1); //old test code,i am keeping this as a trophy
		
		for (var y = 0; y < Dungeon.map_size_y; y++) {
			for (var x = 0; x < Dungeon.map_size_x; x++) {
				int tile = Dungeon.map [x, y].type;
                int orientation = Dungeon.map[x, y].orientation;
				GameObject created_tile;
				Vector3 tile_location;
				if (!makeIt3d) {
					tile_location = new Vector3 (x * tileScaling, y * tileScaling, 0);
				} else {
					tile_location = new Vector3 (x * tileScaling, 0, y * tileScaling);
				}

				created_tile = null;
				if (tile == 1) {
					GameObject floorPrefabToUse = floorPrefab;
					Room room = Dungeon.map[x,y].room;
					if(room != null){
						foreach(CustomRoom customroom in customRooms){
							if(customroom.roomId == room.room_id){
								floorPrefabToUse = customroom.floorPrefab;
								break;
							}
						}
					}

					created_tile = GameObject.Instantiate (floorPrefabToUse, tile_location, Quaternion.identity) as GameObject;
				}
				
				if ( Dungeon.walls.Contains(tile)) {
					GameObject wallPrefabToUse = wallPrefab;
					Room room = Dungeon.map[x,y].room;
					if(room != null){
						foreach(CustomRoom customroom in customRooms){
							if(customroom.roomId == room.room_id){
								wallPrefabToUse = customroom.wallPrefab;
								break;
							}
						}
					}

					created_tile = GameObject.Instantiate (wallPrefabToUse, tile_location, Quaternion.identity) as GameObject;
					if(!makeIt3d){
						created_tile.transform.Rotate(Vector3.forward  * (-90 * (tile -4)));
					}
					else{
						created_tile.transform.Rotate(Vector3.up  * (-90 * (tile -4)));
					}
				}
				
				if (tile == 3) {
                    if (corridorFloorPrefab)
                    {
                        created_tile = GameObject.Instantiate(corridorFloorPrefab, tile_location, Quaternion.identity) as GameObject;
                    }
                    else
                    {
                        created_tile = GameObject.Instantiate(floorPrefab, tile_location, Quaternion.identity) as GameObject;
                    }

                    if (orientation == 1 && makeIt3d)
                    {
                        created_tile.transform.Rotate(Vector3.up * (-90));
                    }

                }

                if (Dungeon.corners.Contains(tile)) {
					GameObject cornerPrefabToUse = cornerPrefab;
					Room room = Dungeon.map[x,y].room;
					if(room != null){
						foreach(CustomRoom customroom in customRooms){
							if(customroom.roomId == room.room_id){
								cornerPrefabToUse = customroom.cornerPrefab;
								break;
							}
						}
					}


					if(cornerPrefabToUse){ //there was a bug in this line. A good man helped for fix.
						created_tile = GameObject.Instantiate (cornerPrefabToUse, tile_location, Quaternion.identity) as GameObject;
						if(cornerRotation){
							if(!makeIt3d){
								created_tile.transform.Rotate(Vector3.forward  * (-90 * (tile -4)));
							}
							else{
								created_tile.transform.Rotate(Vector3.up  * (-90 * (tile -4)));
							}
						}
					}
					else{
						created_tile = GameObject.Instantiate (wallPrefab, tile_location, Quaternion.identity) as GameObject;
					}
				}
				
				if (created_tile) {
					created_tile.transform.parent = transform;
				}
			}
		}

		GameObject end_point;
		GameObject start_point;
		if (!makeIt3d) {
			end_point = GameObject.Instantiate (exitPrefab, new Vector3 (Dungeon.goalRoom.x * tileScaling, Dungeon.goalRoom.y * tileScaling, 0), Quaternion.identity) as GameObject;
			start_point = GameObject.Instantiate (startPrefab, new Vector3 (Dungeon.startRoom.x * tileScaling, Dungeon.startRoom.y * tileScaling, 0), Quaternion.identity) as GameObject;
			
		} else {
			end_point = GameObject.Instantiate (exitPrefab, new Vector3 (Dungeon.goalRoom.x * tileScaling, 0, Dungeon.goalRoom.y * tileScaling), Quaternion.identity) as GameObject;
			start_point = GameObject.Instantiate (startPrefab, new Vector3 (Dungeon.startRoom.x * tileScaling, 0, Dungeon.startRoom.y * tileScaling), Quaternion.identity) as GameObject;
		}

		
		end_point.transform.parent = transform;
		start_point.transform.parent = transform;
		
		//Spawn Objects;
		List<SpawnList> spawnedObjectLocations = new List<SpawnList> ();

		//OTHERS
		for (int x = 0; x < Dungeon.map_size_x; x++) {
			for (int y = 0; y < Dungeon.map_size_y; y++) {
				if (Dungeon.map [x, y].type == 1 &&
				    	((Dungeon.startRoom != Dungeon.map [x, y].room && Dungeon.goalRoom != Dungeon.map [x, y].room) || maximumRoomCount <= 3)) {
					var location = new SpawnList ();

					location.x = x;
					location.y = y;
                    if (Dungeon.walls.Contains(Dungeon.map[x + 1, y].type)) {
                        location.byWall = true;
                        location.wallLocation = "S";
                    }
                    else if (Dungeon.walls.Contains(Dungeon.map[x - 1, y].type))
                    {
                        location.byWall = true;
                        location.wallLocation = "N";
                    }
                    else if (Dungeon.walls.Contains(Dungeon.map[x, y + 1].type)) {
                        location.byWall = true;
                        location.wallLocation = "W";
                    }
                    else if (Dungeon.walls.Contains(Dungeon.map [x, y - 1].type)) {
						location.byWall = true;
                        location.wallLocation = "E";
                    }

                    if (Dungeon.map [x + 1, y].type == 3 || Dungeon.map [x - 1, y].type == 3 || Dungeon.map [x, y + 1].type == 3 || Dungeon.map [x, y - 1].type == 3) {
						location.byCorridor = true;
					}
					if (Dungeon.map [x + 1, y + 1].type == 3 || Dungeon.map [x - 1, y - 1].type == 3 || Dungeon.map [x - 1, y + 1].type == 3 || Dungeon.map [x + 1, y - 1].type == 3) {
						location.byCorridor = true;
					}
					location.room = Dungeon.map[x,y].room;

                    int roomCenterX = (int)Mathf.Floor(location.room.w / 2) + location.room.x;
                    int roomCenterY = (int)Mathf.Floor(location.room.h / 2) + location.room.y;

                    if(x == roomCenterX + 1 && y == roomCenterY + 1 )
                    {
                        location.inTheMiddle = true;
                    } 
                    spawnedObjectLocations.Add (location);
				}
				else if (Dungeon.map [x, y].type == 3) {
					var location = new SpawnList ();
					location.x = x;
					location.y = y;	

					if (Dungeon.map [x + 1, y].type == 1 ) {
						location.byCorridor = true;
						location.asDoor = 4;
						location.room = Dungeon.map[x + 1,y].room;

						spawnedObjectLocations.Add (location);
					}
					else if(Dungeon.map [x - 1, y].type == 1){
						location.byCorridor = true;
						location.asDoor = 2;
						location.room = Dungeon.map[x - 1,y].room;

						spawnedObjectLocations.Add (location);	
					}
					else if (Dungeon.map [x, y + 1].type == 1 ){
						location.byCorridor = true;
						location.asDoor = 1;
						location.room = Dungeon.map[x,y + 1].room;

						spawnedObjectLocations.Add (location);
					}
					else if (Dungeon.map [x, y - 1].type == 1){
						location.byCorridor = true;
						location.asDoor = 3;
						location.room = Dungeon.map[x,y - 1].room;

						spawnedObjectLocations.Add (location);					
					}
				}
			}
		}
		
		for (int i = 0; i < spawnedObjectLocations.Count; i++) {
			SpawnList temp = spawnedObjectLocations [i];
			int randomIndex = Random.Range (i, spawnedObjectLocations.Count);
			spawnedObjectLocations [i] = spawnedObjectLocations [randomIndex];
			spawnedObjectLocations [randomIndex] = temp;
		}
		
		int objectCountToSpawn = 0;


        //you will need below 2 lines if you are going to use dynamic pathfinding. And a NavMeshSurface component attached to same gameobject.
        //surface = GetComponent<NavMeshSurface>();
        //surface.BuildNavMesh();

        //DOORS
        if (doorPrefab) {
			for (int i = 0; i < spawnedObjectLocations.Count; i++) {
				if (spawnedObjectLocations[i].asDoor > 0){
					GameObject newObject;
					SpawnList spawnLocation = spawnedObjectLocations[i];

					GameObject doorPrefabToUse = doorPrefab;
					Room room = spawnLocation.room;
					if(room != null){
						foreach(CustomRoom customroom in customRooms){
							if(customroom.roomId == room.room_id){
								doorPrefabToUse = customroom.doorPrefab;
								break;
							}
						}
					}

					if (!makeIt3d){
						newObject = GameObject.Instantiate(doorPrefabToUse,new Vector3(spawnLocation.x * tileScaling ,spawnLocation.y * tileScaling,0),Quaternion.identity) as GameObject;
					}
					else {
						newObject = GameObject.Instantiate(doorPrefabToUse,new Vector3(spawnLocation.x * tileScaling ,0,spawnLocation.y * tileScaling),Quaternion.identity) as GameObject;
					}

					if(!makeIt3d){
						newObject.transform.Rotate(Vector3.forward  * (-90 * ( spawnedObjectLocations[i].asDoor - 1)));
					}
					else{
						newObject.transform.Rotate(Vector3.up  * (-90 * ( spawnedObjectLocations[i].asDoor - 1)));
					}

					newObject.transform.parent = transform;
					spawnedObjectLocations[i].spawnedObject = newObject;

				}
			}
		}



        //OTHERS
        foreach (SpawnOption objectToSpawn in spawnOptions){
			objectCountToSpawn = Random.Range(objectToSpawn.minSpawnCount,objectToSpawn.maxSpawnCount);
			while (objectCountToSpawn > 0){
                bool created = false;

                for (int i = 0;i < spawnedObjectLocations.Count;i++){
					bool createHere= false;
				
					if (!spawnedObjectLocations[i].spawnedObject && !spawnedObjectLocations[i].byCorridor){
                        
						if(objectToSpawn.spawnRoom > maximumRoomCount){
							objectToSpawn.spawnRoom = 0;
						}
						if(objectToSpawn.spawnRoom == 0){
							if (objectToSpawn.spawnByWall){
								if (spawnedObjectLocations[i].byWall){
									createHere = true;
								}
							}
                            else if (objectToSpawn.spawmInTheMiddle)
                            {
                                if (spawnedObjectLocations[i].inTheMiddle)
                                {
                                    createHere = true;
                                }
                            }
							else {
								createHere = true;
							}
						}
						else {
							if (spawnedObjectLocations[i].room.room_id == objectToSpawn.spawnRoom){
								if (objectToSpawn.spawnByWall){
									if (spawnedObjectLocations[i].byWall){
										createHere = true;
									}
								}
								else {
									createHere = true;
								}
							}
						}
					}


					if (createHere){ //means dungeonizer found a suitable place to put object.
						SpawnList spawnLocation = spawnedObjectLocations[i];
						GameObject newObject;
                        Quaternion spawnRotation = Quaternion.identity;

                        if (!makeIt3d){
                            newObject = GameObject.Instantiate(objectToSpawn.gameObject,new Vector3(spawnLocation.x * tileScaling ,spawnLocation.y * tileScaling,0),spawnRotation) as GameObject;
						}
						else {
                            if (spawnLocation.byWall)
                            {
                                if (spawnLocation.wallLocation == "S")
                                {
                                    spawnRotation = Quaternion.Euler(new Vector3(0, 270, 0));
                                }
                                else if (spawnLocation.wallLocation == "N")
                                {
                                    spawnRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                                }
                                else if (spawnLocation.wallLocation == "W")
                                {
                                    spawnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                                }
                                else if (spawnLocation.wallLocation == "E")
                                {
                                    spawnRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                                }

                            }
                            else
                            {
                                if (objectToSpawn.spawnRotated)
                                {
                                    spawnRotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
                                }
                                else
                                {
                                    spawnRotation = Quaternion.Euler(new Vector3(0, Random.Range(0,2) * 90, 0));
                                }
                            }

                            newObject = GameObject.Instantiate(objectToSpawn.gameObject,new Vector3(spawnLocation.x * tileScaling ,0 + objectToSpawn.heightFix ,spawnLocation.y * tileScaling),spawnRotation) as GameObject;
						}
						
						newObject.transform.parent = transform;
						spawnedObjectLocations[i].spawnedObject = newObject; 
						objectCountToSpawn--;
                        created = true;
						break;
					}
				}
                if (!created)
                {
                    objectCountToSpawn--;
                }
                //if cant find anywhere to put, dont put. (prevents endless loops)
            }
		}
	}



    // Use this for initialization
    void Start () {
		if (generate_on_load){
			ClearOldDungeon();
			Generate();

        }
	}





}
