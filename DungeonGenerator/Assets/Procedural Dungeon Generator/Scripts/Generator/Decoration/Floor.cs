using UnityEngine;

namespace PDG
{
	public class Floor : MonoBehaviour
	{
		[SerializeField] private Sprite[] decoration;

		private GameObject skull;


		private void Awake()
		{
			skull = Resources.Load("Floor/skull") as GameObject;
		}

		private void Start()
		{
			int x = (int)transform.position.x;
			int y = (int)transform.position.y;

			if (Random.Range(0, 6) == 0)
			{
				GetComponent<SpriteRenderer>().sprite = decoration[Random.Range(0, decoration.Length)];
			}

			if (Random.Range(0, 25) == 0)
			{
				Instantiate(skull, new Vector3(x, y, 0), Quaternion.identity);
			}
		}
	}
}
