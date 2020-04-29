using UnityEngine;

namespace PDG
{
	public enum Direction
	{
		None, Up, Down, Right, Left
	}

	public class Hero : MonoBehaviour
	{
		[SerializeField] private float speed = 3f;
		[SerializeField] private LayerMask obstacles;

		private Animator animator;
		private Vector2 targetPosition;

		private Direction direction = Direction.None;


		private void Awake()
		{
			animator = GetComponent<Animator>();

			targetPosition = transform.position;
		}

		private void Update()
		{
			float horizontal = Input.GetAxisRaw("Horizontal");
			float vertical = Input.GetAxisRaw("Vertical");

			Vector2 axisDirection = new Vector2(horizontal, vertical);

			animator.SetInteger("Direction", (int)direction);

			if (axisDirection != Vector2.zero && targetPosition == (Vector2)transform.position)
			{
				if (Mathf.Abs(axisDirection.x) > Mathf.Abs(axisDirection.y))
				{
					if (axisDirection.x > 0)
					{
						ChangeDirection(Direction.Right);
					}
					else
					{
						ChangeDirection(Direction.Left);
					}
				}
				else
				{
					if (axisDirection.y > 0)
					{
						ChangeDirection(Direction.Up);
					}
					else
					{
						ChangeDirection(Direction.Down);
					}
				}
			}

			if ((Vector2)transform.position == targetPosition)
				ChangeDirection(Direction.None);

			transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

			Flip();
		}

		private void ChangeDirection(Direction dir)
		{
			direction = dir;

			if (CheckCollision) return;

			targetPosition += GetDirection(direction);
		}

		private bool CheckCollision
		{
			get
			{
				RaycastHit2D hit;

				hit = Physics2D.Raycast(transform.position, GetDirection(direction), 1, obstacles);

				if (hit)
				{
					//Debug.Log(hit.collider.tag);

					switch (hit.collider.tag)
					{
						case "Exit":
							FindObjectOfType<Dungeon>().Create();
							break;
						case "Chest":
							hit.collider.GetComponent<Chest>().Open();
							break;
					}
				}

				return hit.collider != null;
			}
		}

		private Vector2 GetDirection(Direction dir)
		{
			Vector2 v = Vector2.zero;

			switch (dir)
			{
				default:
				case Direction.None: v = Vector2.zero; break;
				case Direction.Up: v = Vector2.up; break;
				case Direction.Down: v = Vector2.down; break;
				case Direction.Right: v = Vector2.right; break;
				case Direction.Left: v = Vector2.left; break;
			}

			return v;
		}

		private void Flip()
		{
			Vector2 theScale = transform.localScale;

			switch (direction)
			{
				case Direction.Right: theScale.x = 1; break;
				case Direction.Left: theScale.x = -1; break;
			}

			transform.localScale = theScale;
		}
	}
}