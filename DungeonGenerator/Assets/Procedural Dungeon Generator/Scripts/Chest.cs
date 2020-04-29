using UnityEngine;

namespace PDG
{
	public class Chest : MonoBehaviour
	{
		private Animator animator;
		private AudioSource source;

		private bool mimic = false;
		private bool opened = false;


		private void Awake()
		{
			animator = GetComponent<Animator>();
			source = GetComponent<AudioSource>();

			mimic = Random.Range(0, 5) == 0 ? true : false;
		}

		public void Open()
		{
			if (opened) return;

			opened = true;

			source.Play();

			if (mimic)
			{
				animator.Play("Mimic");

			}
			else
			{
				animator.Play("Open");

			}
		}
	}
}