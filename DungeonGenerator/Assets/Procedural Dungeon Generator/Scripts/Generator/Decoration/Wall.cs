using UnityEngine;

namespace PDG
{
	public class Wall : MonoBehaviour
	{
		[SerializeField] private Sprite[] holes;
		[SerializeField] private Sprite[] banners;

		private Dungeon dungeon;
		private Matrix matrix;


		private void Start()
		{
			dungeon = FindObjectOfType<Dungeon>();
			matrix = dungeon.GetComponent<Matrix>();
			int x = (int)transform.position.x;
			int y = (int)transform.position.y;

			Sprite tmpSprite;
			if (Ground && Random.Range(0, 10) == 0)
			{
				GetComponent<SpriteRenderer>().sprite = banners[Random.Range(0, banners.Length)];
			}

			if (Random.Range(0, 8) == 0)
			{
				tmpSprite = holes[Random.Range(0, holes.Length)];

				if ((tmpSprite.name != "wall_goo") || (Ground && tmpSprite.name == "wall_goo"))
				{
					GetComponent<SpriteRenderer>().sprite = tmpSprite;
				}
			}
		}

		private bool Ground
		{
			get
			{
				int x = (int)transform.position.x;
				int y = (int)transform.position.y;

				return matrix.map[x, y - 1] == dungeon.floor_1;
			}
		}
	}
}