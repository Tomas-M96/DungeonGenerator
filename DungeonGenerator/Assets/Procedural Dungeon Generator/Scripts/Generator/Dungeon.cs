using UnityEngine;

namespace PDG
{
	[RequireComponent(typeof(Matrix))]
	public class Dungeon : MonoBehaviour
	{
		#region Variables
		public enum GenerationMode { Horizontal, Vertical }

		[Header("Map")]
		[SerializeField] [Range(0, 100)] private int width = 45;
		[SerializeField] [Range(0, 100)] private int height = 45;
		[SerializeField] private GenerationMode mode = GenerationMode.Horizontal;

		[Header("Rooms")]
		[SerializeField] private IntRange numRooms = new IntRange(13, 15);
		[SerializeField] private IntRange roomWidth = new IntRange(3, 7);
		[SerializeField] private IntRange roomHeight = new IntRange(3, 7);

		[Header("Chests")]
		[SerializeField] private int maxChests = 10;
		[SerializeField] private GameObject chestPrefab;

		[Header("Characters")]
		[SerializeField] private GameObject hero;

		[HideInInspector] public GameObject floor_1;
		[HideInInspector] public GameObject wall_mid;
		private GameObject wall_left;
		private GameObject wall_right;
		private GameObject wall_side_front_left;
		private GameObject wall_side_front_right;
		private GameObject wall_top_mid;
		private GameObject wall_corner_top_left;
		private GameObject wall_corner_top_right;
		private GameObject wall_corner_bottom_left;
		private GameObject wall_corner_bottom_right;
		private GameObject wall_side_mid_left;
		private GameObject wall_side_mid_right;
		private GameObject wall_side_top_left;
		private GameObject wall_side_top_right;
		private GameObject wall_column_top;
		private GameObject wall_column_mid;
		private GameObject wall_column_base;
		private GameObject fountain_blue;
		private GameObject fountain_red;
		private GameObject exit;

		private int rooms = 0;
		private int chestsCount = 0;
		private Matrix matrix;
		private Matrix board;
		private Vector2[] pointsList;

		private Transform itemParent;
		#endregion Variables

		#region Methods
		private void Awake()
		{
			floor_1 = Resources.Load("Floor/floor_1") as GameObject;
			wall_mid = Resources.Load("Wall/Base/wall_mid") as GameObject;
			wall_left = Resources.Load("Wall/Base/wall_left") as GameObject;
			wall_right = Resources.Load("Wall/Base/wall_right") as GameObject;
			wall_side_front_left = Resources.Load("Wall/Base/wall_side_front_left") as GameObject;
			wall_side_front_right = Resources.Load("Wall/Base/wall_side_front_right") as GameObject;
			wall_top_mid = Resources.Load("Wall/Base/wall_top_mid") as GameObject;
			wall_corner_top_left = Resources.Load("Wall/Base/wall_corner_top_left") as GameObject;
			wall_corner_top_right = Resources.Load("Wall/Base/wall_corner_top_right") as GameObject;
			wall_corner_bottom_left = Resources.Load("Wall/Base/wall_corner_bottom_left") as GameObject;
			wall_corner_bottom_right = Resources.Load("Wall/Base/wall_corner_bottom_right") as GameObject;
			wall_side_mid_left = Resources.Load("Wall/Base/wall_side_mid_left") as GameObject;
			wall_side_mid_right = Resources.Load("Wall/Base/wall_side_mid_right") as GameObject;
			wall_side_top_left = Resources.Load("Wall/Base/wall_side_top_left") as GameObject;
			wall_side_top_right = Resources.Load("Wall/Base/wall_side_top_right") as GameObject;
			wall_column_top = Resources.Load("Wall/Decoration/Column/wall_column_top") as GameObject;
			wall_column_mid = Resources.Load("Wall/Decoration/Column/wall_column_mid") as GameObject;
			wall_column_base = Resources.Load("Wall/Decoration/Column/wall_column_base") as GameObject;
			fountain_blue = Resources.Load("Wall/Decoration/Fountains/fountain_blue") as GameObject;
			fountain_red = Resources.Load("Wall/Decoration/Fountains/fountain_red") as GameObject;
			exit = Resources.Load("Floor/exit") as GameObject;
		}

		private void Start()
		{
			Create();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space)) Create();
		}

		#region Initialization
		public void Create()
		{
			Clear();
			Rooms();
			Connect();
			Smooth();
			Walls();
			Structure();
			Columns();
			Fountains();
			InsertExit();
			Chests();
			SpawnHero();
			Generate();
		}

		public void Clear()
		{
			matrix = GetComponent<Matrix>();
			matrix.rows = width;
			matrix.columns = height;
			matrix.map = new GameObject[matrix.rows + 2, matrix.columns + 2];

			board = GameObject.Find("Board").GetComponent<Matrix>();
			board.rows = matrix.rows;
			board.columns = matrix.columns;
			board.map = new GameObject[board.rows + 2, board.columns + 2];

			rooms = Random.Range(numRooms.m_Min, numRooms.m_Max);
			chestsCount = 0;

			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					matrix.map[x, y] = null;
					board.map[x, y] = null;
				}
			}

			SpriteRenderer[] tiles = FindObjectsOfType<SpriteRenderer>();
			for (int i = 0; i < tiles.Length; i++)
			{
				Destroy(tiles[i].gameObject);
			}

			GameObject lastDungeon = GameObject.Find("Dungeon");
			if (lastDungeon != null) Destroy(lastDungeon);

			GameObject map = new GameObject();
			map.name = "Dungeon";
			itemParent = map.transform;
		}
		private void Generate()
		{
			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] != null)
					{
						GameObject go = Instantiate(matrix.map[x, y], new Vector3(x, y, 0), Quaternion.identity);
						go.transform.SetParent(itemParent);
					}

					if (board.map[x, y] == chestPrefab)
					{
						board.map[x, y] = floor_1;
						GameObject go = Instantiate(chestPrefab, new Vector3(x, y + 0.25F, 0), Quaternion.identity);
						go.transform.SetParent(itemParent);
					}
				}
			}
		}
		#endregion Initialization

		#region Generation
		private void Rooms()
		{
			int attemps = 0;

			for (int i = 0; i < rooms; i++)
			{
				int X, Y, horizontal, vertical;
				do
				{
					X = Random.Range(0, matrix.rows);
					Y = Random.Range(0, matrix.columns);
					horizontal = Random.Range(roomWidth.m_Min, roomWidth.m_Max);
					vertical = Random.Range(roomHeight.m_Min, roomHeight.m_Min);

					attemps++;
					if (attemps > 1000)
					{
						LauncError("Insufficient space!");
						break;
					}

				} while (!Avaiable(X, Y, horizontal, vertical));
				for (int x = X; x < X + horizontal; x++) for (int y = Y; y < Y + vertical; y++) matrix.map[x, y] = floor_1;

				switch (Random.Range(0, 4))
				{
					case 0: matrix.map[Random.Range(X, X + horizontal - 1), Y] = exit; break;
					case 1: matrix.map[Random.Range(X, X + horizontal - 1), Y + vertical - 1] = exit; break;
					case 2: matrix.map[X, Random.Range(Y, Y + vertical - 1)] = exit; break;
					case 3: matrix.map[X + horizontal - 1, Random.Range(Y, Y + vertical - 1)] = exit; break;
				}
			}
			SavePoints();
		}
		private void Connect()
		{
			int x, y;
			Vector2 current, target;
			Vector2 direction = Vector2.zero;

			for (int web = 0; web < 2; web++)
			{
				for (int i = 0; i < rooms - 1; i++)
				{
					current = pointsList[i];
					target = pointsList[i + 1];
					do
					{
						if (target.x > current.x) direction = Vector2.right;
						else if (target.x < current.x) direction = Vector2.left;
						else if (target.y > current.y) direction = Vector2.up;
						else if (target.y < current.y) direction = Vector2.down;
						else direction = Vector2.zero;

						x = (int)current.x + (int)direction.x;
						y = (int)current.y + (int)direction.y;
						if (Find(x, y, null)) matrix.map[x, y] = floor_1;
						current = new Vector2(x, y);
					} while (direction != Vector2.zero);
				}

				Vector2[] tmpList = pointsList;
				for (int i = 0; i < tmpList.Length; i++)
				{
					pointsList[Random.Range(0, pointsList.Length)] = tmpList[i];
				}
			}
		}
		private void Smooth()
		{
			for (int i = 0; i < 3; i++)
			{
				for (int x = 0; x < matrix.rows; x++)
				{
					for (int y = 0; y < matrix.columns; y++)
					{
						if (matrix.map[x, y] == null)
						{
							if (Find(x + 1, y, floor_1) && Find(x - 1, y, floor_1)
								&& Find(x, y + 1, floor_1) && Find(x, y - 1, floor_1))
								matrix.map[x, y] = floor_1;

							if (Find(x + 1, y, floor_1) && Find(x - 1, y, floor_1))
								matrix.map[x, y] = floor_1;

							if (Find(x, y + 1, floor_1) && Find(x, y - 1, floor_1))
								matrix.map[x, y] = floor_1;

							if (Find(x, y + 1, null) && Find(x, y + 2, floor_1) && Find(x, y - 1, floor_1))
							{
								matrix.map[x, y + 1] = floor_1;
								matrix.map[x, y] = floor_1;
							}
						}
					}
				}
			}
		}
		private void Walls()
		{
			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] == floor_1)
					{
						if (Find(x, y + 1, null)) matrix.map[x, y + 1] = wall_mid;
						if (Find(x, y - 1, null)) matrix.map[x, y - 1] = wall_mid;
					}
				}
			}
		}
		private void Structure()
		{
			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] == wall_mid)
					{
						if (Find(x - 1, y, floor_1)
							&& Find(x - 1, y - 1, floor_1)
							&& Find(x, y - 1, floor_1))
							matrix.map[x, y] = wall_left;
						if (Find(x + 1, y, floor_1)
							&& Find(x + 1, y - 1, floor_1)
							&& Find(x, y - 1, floor_1))
							matrix.map[x, y] = wall_right;

						if (Find(x - 1, y, null)
							&& Find(x - 1, y - 1, null)
							&& Find(x, y - 1, null))
						{
							GameObject go = Instantiate(wall_side_front_left, new Vector3(x - 1, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						if (Find(x + 1, y, null)
							&& Find(x + 1, y - 1, null)
							&& Find(x, y - 1, null))
						{
							GameObject go = Instantiate(wall_side_front_right, new Vector3(x + 1, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
					}
				}
			}

			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] == wall_mid)
					{
						if (Find(x - 1, y, null) && !Find(x, y - 1, null))
						{
							GameObject go = Instantiate(wall_side_mid_left, new Vector3(x - 1, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						if (Find(x + 1, y, null) && !Find(x, y - 1, null))
						{
							GameObject go = Instantiate(wall_side_mid_right, new Vector3(x + 1, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}

						if (Find(x, y - 1, null) && (Find(x - 1, y - 1, floor_1) || Find(x - 1, y - 1, wall_mid)))
						{
							GameObject go = Instantiate(wall_side_mid_right, new Vector3(x, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						if (Find(x, y - 1, null) && (Find(x + 1, y - 1, floor_1) || Find(x + 1, y - 1, wall_mid)))
						{
							GameObject go = Instantiate(wall_side_mid_left, new Vector3(x, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}

						if (!Find(x - 1, y, floor_1) && !Find(x + 1, y, floor_1))
						{
							GameObject go = Instantiate(wall_top_mid, new Vector3(x, y + 1, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						else if (Find(x - 1, y, floor_1))
						{
							GameObject go = Instantiate(wall_corner_top_left, new Vector3(x, y + 1, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						else if (Find(x + 1, y, floor_1))
						{
							GameObject go = Instantiate(wall_corner_top_right, new Vector3(x, y + 1, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
					}
					else if (matrix.map[x, y] == wall_left)
					{
						if (Find(x + 1, y, null))
						{
							GameObject go_ = Instantiate(wall_side_mid_right, new Vector3(x + 1, y, 0), Quaternion.identity);
							go_.transform.SetParent(itemParent);
						}

						GameObject go = Instantiate(wall_corner_bottom_left, new Vector3(x, y + 1, 0), Quaternion.identity);
						go.transform.SetParent(itemParent);
					}
					else if (matrix.map[x, y] == wall_right)
					{
						if (Find(x - 1, y, null))
						{
							GameObject go_ = Instantiate(wall_side_mid_left, new Vector3(x - 1, y, 0), Quaternion.identity);
							go_.transform.SetParent(itemParent);
						}

						GameObject go = Instantiate(wall_corner_bottom_right, new Vector3(x, y + 1, 0), Quaternion.identity);
						go.transform.SetParent(itemParent);
					}
					else if (matrix.map[x, y] == floor_1)
					{
						if (Find(x - 1, y, null))
						{
							GameObject go = Instantiate(wall_side_mid_left, new Vector3(x - 1, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						if (Find(x + 1, y, null))
						{
							GameObject go = Instantiate(wall_side_mid_right, new Vector3(x + 1, y, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}

						if (Find(x - 1, y + 2, null) && (Find(x, y + 1, wall_mid) || Find(x, y + 1, wall_left) || Find(x, y + 1, wall_right)))
						{
							GameObject go = Instantiate(wall_side_top_left, new Vector3(x - 1, y + 2, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
						if (Find(x + 1, y + 2, null) && (Find(x, y + 1, wall_mid) || Find(x, y + 1, wall_left) || Find(x, y + 1, wall_right)))
						{
							GameObject go = Instantiate(wall_side_top_right, new Vector3(x + 1, y + 2, 0), Quaternion.identity);
							go.transform.SetParent(itemParent);
						}
					}
				}
			}
		}
		private void Columns()
		{
			for (int x = matrix.rows - 1; x > 0; x--)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] == wall_mid)
					{
						if (!Find(x - 1, y, wall_column_mid) && !Find(x + 1, y, wall_column_mid) && Find(x, y - 1, floor_1))
						{
							matrix.map[x, y + 1] = wall_column_top;
							matrix.map[x, y] = wall_column_mid;
							matrix.map[x, y - 1] = wall_column_base;
						}
					}
				}
			}
		}
		#endregion Generation

		#region Extras
		private void Fountains()
		{
			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] == wall_mid)
					{
						if (Random.Range(0, 60) == 0 && Find(x, y - 1, floor_1))
						{
							matrix.map[x, y] = fountain_blue;
						}
						else if (Random.Range(0, 150) == 0 && Find(x, y - 1, floor_1))
						{
							matrix.map[x, y] = fountain_red;
						}
					}
				}
			}
		}
		private void InsertExit()
		{
			int x = 0; int y = 0;
			do
			{
				x = Random.Range(0, matrix.rows);
				y = Random.Range(0, matrix.columns);
			} while (matrix.map[x, y] != floor_1
			|| !Find(x + 1, y, floor_1) || !Find(x - 1, y, floor_1)
			|| !Find(x, y + 1, floor_1) || !Find(x, y - 1, floor_1)
			|| !Find(x + 1, y + 1, floor_1) || !Find(x - 1, y - 1, floor_1)
			|| !Find(x + 1, y - 1, floor_1) || !Find(x - 1, y + 1, floor_1));

			matrix.map[x, y] = exit;
		}
		private void Chests()
		{
			for (int x = 0; x < matrix.rows; x++)
			{
				for (int y = 0; y < matrix.columns; y++)
				{
					if (matrix.map[x, y] == floor_1 && (Find(x, y + 1, wall_mid) || Find(x, y + 1, wall_left) || Find(x, y + 1, wall_right)) && Random.Range(0, 3) == 0
						&& matrix.map[x, y - 2] == floor_1 && matrix.map[x + 1, y - 2] == floor_1 && matrix.map[x - 1, y - 2] == floor_1)
					{
						if (chestsCount < maxChests)
						{
							chestsCount++;
							board.map[x, y] = chestPrefab;
						}
					}
				}
			}
		}
		private void SpawnHero()
		{
			if (hero == null) return;

			int x = 0; int y = 0;
			do
			{
				x = Random.Range(0, matrix.rows);
				y = Random.Range(0, matrix.columns);
			} while (matrix.map[x, y] != floor_1
			|| !Find(x + 1, y, floor_1) || !Find(x - 1, y, floor_1)
			|| !Find(x, y + 1, floor_1) || !Find(x, y - 1, floor_1)
			|| !Find(x + 1, y + 1, floor_1) || !Find(x - 1, y - 1, floor_1)
			|| !Find(x + 1, y - 1, floor_1) || !Find(x - 1, y + 1, floor_1)
			|| board.map[x, y] != null);

			board.map[x, y - 1] = hero;

			GameObject h = Instantiate(hero, new Vector3(x, y - 0.25F, 0), Quaternion.identity);
			h.name = "Hero";

			FindObjectOfType<Camera>().target = h.transform;
		}
		#endregion Extras

		#region Tools
		private bool InBounds(int x, int y)
		{
			return !(x > matrix.rows - 1 || x < 0 || y > matrix.columns - 1 || y < 0);
		}
		public bool Find(int x, int y, GameObject tile)
		{
			return InBounds(x, y) && matrix.map[x, y] == tile;
		}
		private bool Avaiable(int X, int Y, int width, int height)
		{
			for (int x = X; x < X + width; x++)
			{
				for (int y = Y; y < Y + height; y++)
				{
					if (!Find(x, y, null)) return false;

					#region Around
					if (!Find(X - 1, y, null)) return false;
					if (!Find(x, Y - 1, null)) return false;
					if (!Find(X + width, y, null)) return false;
					if (!Find(x, Y + height, null)) return false;
					if (!Find(X + width, Y + height, null)) return false;
					if (!Find(X - 1, Y - 1, null)) return false;
					if (!Find(X + width, Y - 1, null)) return false;
					if (!Find(X - 1, Y + height, null)) return false;

					if (!Find(X - 2, y, null)) return false;
					if (!Find(x, Y - 2, null)) return false;
					if (!Find(X + width + 1, y, null)) return false;
					if (!Find(x, Y + height + 1, null)) return false;
					if (!Find(X + width + 1, Y + height + 1, null)) return false;
					if (!Find(X - 2, Y - 2, null)) return false;
					if (!Find(X + width + 1, Y - 2, null)) return false;
					if (!Find(X - 2, Y + height + 1, null)) return false;

					if (!Find(X - 3, y, null)) return false;
					if (!Find(x, Y - 3, null)) return false;
					if (!Find(X + width + 2, y, null)) return false;
					if (!Find(x, Y + height + 2, null)) return false;
					if (!Find(X + width + 2, Y + height + 2, null)) return false;
					if (!Find(X - 3, Y - 3, null)) return false;
					if (!Find(X + width + 2, Y - 3, null)) return false;
					if (!Find(X - 3, Y + height + 2, null)) return false;
					#endregion
				}
			}
			return true;
		}

		private void SavePoints()
		{
			pointsList = new Vector2[rooms];
			RemovePoints();

			switch (mode)
			{
				case GenerationMode.Horizontal:
					for (int y = 0; y < matrix.columns; y++)
					{
						for (int x = 0; x < matrix.rows; x++)
						{
							SetPoints(x, y);
						}
					}
					break;
				case GenerationMode.Vertical:
					for (int x = 0; x < matrix.rows; x++)
					{
						for (int y = 0; y < matrix.columns; y++)
						{
							SetPoints(x, y);
						}
					}
					break;
			}
		}
		private void SetPoints(int x, int y)
		{
			if (matrix.map[x, y] == exit)
			{
				for (int i = 0; i < pointsList.Length; i++)
				{
					if (pointsList[i] == Vector2.zero)
					{
						pointsList[i] = new Vector2(x, y);
						break;
					}
				}
				matrix.map[x, y] = floor_1;
			}
		}
		private void RemovePoints()
		{
			for (int i = 0; i < pointsList.Length; i++)
			{
				pointsList[i] = Vector2.zero;
			}
		}
		private void LauncError(string message)
		{
			Debug.LogError(message);
			UnityEditor.EditorApplication.isPlaying = false;
		}
		#endregion Tools
		#endregion Methods
	}
}
