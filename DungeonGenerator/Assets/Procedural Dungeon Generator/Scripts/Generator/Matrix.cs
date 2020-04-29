using UnityEngine;

namespace PDG
{
	public class Matrix : MonoBehaviour
	{
		[HideInInspector] public int rows = 40;
		[HideInInspector] public int columns = 40;

		public GameObject[,] map;
	}
}