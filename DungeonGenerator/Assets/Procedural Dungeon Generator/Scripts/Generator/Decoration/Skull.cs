using UnityEngine;

namespace PDG
{
	public class Skull : MonoBehaviour
	{
		private void Start()
		{
			transform.SetParent(GameObject.Find("Dungeon").transform);

			if (Random.Range(0, 3) == 0)
				GetComponent<SpriteRenderer>().flipX = true;
		}
	}
}
