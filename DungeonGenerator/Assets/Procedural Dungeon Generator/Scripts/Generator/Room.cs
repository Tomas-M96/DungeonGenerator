using UnityEngine;

namespace PDG
{
	public class Room : MonoBehaviour
	{
		[Header("Matrix")]
		[SerializeField] private int rows = 40;
		[SerializeField] private int columns = 40;
		[SerializeField] private IntRange roomWidth = new IntRange(3, 7);
		public IntRange roomHeight = new IntRange(3, 7);

		[Header("Tiles")]
		[SerializeField] private GameObject floor_1;
		[SerializeField] private GameObject wall_mid;
		[SerializeField] private GameObject wall_left;
		[SerializeField] private GameObject wall_right;
		[SerializeField] private GameObject wall_side_front_left;
		[SerializeField] private GameObject wall_side_front_right;
		[SerializeField] private GameObject wall_top_mid;
		[SerializeField] private GameObject wall_top_left;
		[SerializeField] private GameObject wall_top_right;
		[SerializeField] private GameObject wall_corner_bottom_left;
		[SerializeField] private GameObject wall_corner_bottom_right;
		[SerializeField] private GameObject wall_side_mid_left;
		[SerializeField] private GameObject wall_side_mid_right;
		[SerializeField] private GameObject wall_side_top_left;
		[SerializeField] private GameObject wall_side_top_right;

		private int width, height;
		private GameObject[,] matrix;

		private void Start()
		{
			rows += 2;
			columns += 2;
			Create();
		}

		private void Create()
		{
			Clear();

			Base();
			Borders();

			Generate();
		}

		private void Base()
		{
			width = Random.Range(roomWidth.m_Min, roomHeight.m_Max + 1);
			height = Random.Range(roomHeight.m_Min, roomHeight.m_Max + 1);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					matrix[x, y] = floor_1;
				}
			}
		}
		private void Borders()
		{
			for (int x = 0; x < width; x++)
			{
				Instantiate(wall_mid, new Vector3(x, -1, 0), Quaternion.identity);
				Instantiate(wall_mid, new Vector3(x, height, 0), Quaternion.identity);

				Instantiate(wall_top_mid, new Vector3(x, height + 1, 0), Quaternion.identity);
				Instantiate(wall_top_mid, new Vector3(x, 0, 0), Quaternion.identity);
			}
			for (int y = 0; y < height + 1; y++)
			{
				Instantiate(wall_side_mid_left, new Vector3(-1, y, 0), Quaternion.identity);
				Instantiate(wall_side_mid_right, new Vector3(width, y, 0), Quaternion.identity);
			}
			Instantiate(wall_side_front_left, new Vector3(-1, -1, 0), Quaternion.identity);
			Instantiate(wall_side_front_right, new Vector3(width, -1, 0), Quaternion.identity);
			Instantiate(wall_side_top_left, new Vector3(-1, height + 1, 0), Quaternion.identity);
			Instantiate(wall_side_top_right, new Vector3(width, height + 1, 0), Quaternion.identity);
		}

		private void Generate()
		{
			for (int x = 0; x < rows; x++)
			{
				for (int y = 0; y < columns; y++)
				{
					if (matrix[x, y] != null)
						Instantiate(matrix[x, y], new Vector3(x, y, 0), Quaternion.identity);
				}
			}
		}
		private void Clear()
		{
			matrix = new GameObject[rows, columns];
			for (int x = 0; x < rows; x++)
			{
				for (int y = 0; y < columns; y++)
				{
					matrix[x, y] = null;
				}
			}

			SpriteRenderer[] tiles = FindObjectsOfType<SpriteRenderer>();
			for (int i = 0; i < tiles.Length; i++)
			{
				Destroy(tiles[i].gameObject);
			}
		}

		private bool InBounds(int x, int y)
		{
			return !(x > rows - 1 || x < 0 || y > columns - 1 || y < 0);
		}
		private bool Find(int x, int y, GameObject tile)
		{
			return InBounds(x, y) && matrix[x, y] == tile;
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
	}
}