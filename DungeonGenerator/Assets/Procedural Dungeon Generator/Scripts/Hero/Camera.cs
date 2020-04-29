using UnityEngine;

namespace PDG
{
	public class Camera : MonoBehaviour
	{
		public float smoothTime = 0.3F;
		public float zOffSet = -5f;

		[HideInInspector] public Transform target;

		private Vector2 velocity = Vector2.zero;


		private void Update()
		{
			if (target == null) return;

			Vector2 targetPosition = target.TransformPoint(new Vector2());

			transform.position = Vector2.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

			transform.position = new Vector3(transform.position.x, transform.position.y, zOffSet);
		}
	}
}