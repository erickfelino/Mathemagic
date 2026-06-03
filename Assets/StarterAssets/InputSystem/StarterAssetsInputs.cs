using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		private bool inputEnabled = true;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			if (!inputEnabled) return;
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			if (!inputEnabled) return;
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			if (!inputEnabled) return;
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			if (!inputEnabled) return;
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		// Enable or disable player input (used when opening/closing puzzles)
		public void SetInputEnabled(bool enabled)
		{
			inputEnabled = enabled;
			cursorInputForLook = enabled;
			cursorLocked = enabled;

			if (!enabled)
			{
				move = Vector2.zero;
				look = Vector2.zero;
				jump = false;
				sprint = false;

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				SetCursorState(cursorLocked);
				Cursor.visible = !cursorLocked;
			}
		}
	}
	
}