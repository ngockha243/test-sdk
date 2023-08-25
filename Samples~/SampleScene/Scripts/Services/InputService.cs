using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace Services
{

	public class InputService
	{
#if UNITY_EDITOR
		public List<GameObject> FakeTouchs { get; set; }
		public List<Image> FakeTouchImages { get; set; }
		private List<KeyCode> keys = new List<KeyCode>() { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
#endif
		private Touch touch = new Touch();
		private List<Touch> touches = new List<Touch>();

		private EventSystem eventSystem;
		public InputService()
		{

		}
		public bool IsClickEscape()
		{
			if(Application.platform == RuntimePlatform.Android)
			{
				if(Input.GetKey(KeyCode.Escape))
				{
					return true;
				}
			}
			return false;
		}
		public Touch GetTouch(int id)
		{
			// ID
			touch.Id = id;
			// Reset touch
			touch.Position = Vector2.zero;
			touch.Phase = TouchPhase.None;
#if UNITY_EDITOR
			if (id > 5)
			{
				Logger.Error("Your input id is invalid.");
				return touch;
			}
#endif
			if (id < 0)
			{
				Logger.Error("Your input id is invalid.");
				return touch;
			}
			if (Input.touchSupported == true)
			{
				UnityEngine.Touch unityTouch = Input.GetTouch(id);
				// Position
				touch.Position = unityTouch.position;
				// Phase
				if (unityTouch.phase == UnityEngine.TouchPhase.Began)
				{
					touch.Phase = TouchPhase.Down;
				}
				else if (unityTouch.phase == UnityEngine.TouchPhase.Ended || unityTouch.phase == UnityEngine.TouchPhase.Canceled)
				{
					touch.Phase = TouchPhase.Up;
				}
				else if (unityTouch.phase == UnityEngine.TouchPhase.Moved || unityTouch.phase == UnityEngine.TouchPhase.Stationary)
				{
					touch.Phase = TouchPhase.Move;
				}
			}
			else
			{
				if (id == 0)
				{
					// Position
					touch.Position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
					// Phase
					if (Input.GetMouseButtonDown(0))
					{
						touch.Phase = TouchPhase.Down;
					}
					else if (Input.GetMouseButtonUp(0))
					{
						touch.Phase = TouchPhase.Up;
					}
					else if (Input.GetMouseButton(0))
					{
						touch.Phase = TouchPhase.Move;
					}
				}
#if UNITY_EDITOR
				else
				{
					if (FakeTouchs == null)
					{
						Logger.Debug("There is no fake touch.");
					}
					else
					{
						for (int i = 0; i < keys.Count; i++)
						{
							if (id == i + 1)
							{
								Transform touchPosition = FakeTouchs[i].transform;
								// Position
								touch.Position = new Vector2(touchPosition.position.x, touchPosition.position.y);
								// Phase
								if (Input.GetKeyDown(keys[i]))
								{
									touch.Phase = TouchPhase.Down;
									FakeTouchImages[i].color = new Color(1f, 1f, 1f, 1f);
								}
								else if (Input.GetKey(keys[i]))
								{
									touch.Phase = TouchPhase.Move;
									FakeTouchImages[i].color = new Color(1f, 1f, 1f, 1f);
								}
								else if (Input.GetKeyUp(keys[i]))
								{
									touch.Phase = TouchPhase.Up;
									FakeTouchImages[i].color = new Color(1f, 1f, 1f, 0.5f);
								}
							}
						}
					}
				}
#endif
			}
			return touch;
		}
		public List<Touch> GetTouches()
		{
			touches.Clear();

			if (Input.touchSupported == true)
			{
				foreach (var unityTouch in Input.touches)
				{
					// Reset touch
					touch.Position = Vector2.zero;
					touch.Phase = TouchPhase.None;

					// Id
					touch.Id = unityTouch.fingerId;
					// Position
					touch.Position = unityTouch.position;
					// Phase
					if (unityTouch.phase == UnityEngine.TouchPhase.Began)
					{
						touch.Phase = TouchPhase.Down;
					}
					else if (unityTouch.phase == UnityEngine.TouchPhase.Ended || unityTouch.phase == UnityEngine.TouchPhase.Canceled)
					{
						touch.Phase = TouchPhase.Up;
					}
					else if (unityTouch.phase == UnityEngine.TouchPhase.Moved || unityTouch.phase == UnityEngine.TouchPhase.Stationary)
					{
						touch.Phase = TouchPhase.Move;
					}
					// Add
					if (touch.Phase != TouchPhase.None)
					{
						touches.Add(touch);
					}
				}
			}
			else
			{
				touch.Position = Vector2.zero;
				touch.Phase = TouchPhase.None;

				// Id
				touch.Id = 0;
				// Position
				touch.Position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				// Phase
				if (Input.GetMouseButtonDown(0))
				{
					touch.Phase = TouchPhase.Down;
				}
				else if (Input.GetMouseButtonUp(0))
				{
					touch.Phase = TouchPhase.Up;
				}
				else if (Input.GetMouseButton(0))
				{
					touch.Phase = TouchPhase.Move;
				}
				// Add
				if (touch.Phase != TouchPhase.None)
				{
					touches.Add(touch);
				}
#if UNITY_EDITOR
				if (FakeTouchs == null)
				{
					Logger.Debug("There is no fake touch.");
				}
				else
				{
					for (int i = 0; i < keys.Count; i++)
					{
						touch.Position = Vector2.zero;
						touch.Phase = TouchPhase.None;

						Transform touchPosition = FakeTouchs[i].transform;
						// Id
						touch.Id = i + 1;
						// Position
						touch.Position = new Vector2(touchPosition.position.x, touchPosition.position.y);
						// Phase
						if (Input.GetKeyDown(keys[i]))
						{
							touch.Phase = TouchPhase.Down;
							FakeTouchImages[i].color = new Color(1f, 1f, 1f, 1f);
						}
						else if (Input.GetKey(keys[i]))
						{
							touch.Phase = TouchPhase.Move;
							FakeTouchImages[i].color = new Color(1f, 1f, 1f, 1f);
						}
						else if (Input.GetKeyUp(keys[i]))
						{
							touch.Phase = TouchPhase.Up;
							FakeTouchImages[i].color = new Color(1f, 1f, 1f, 0.5f);
						}
						// Add
						if (touch.Phase != TouchPhase.None)
						{
							touches.Add(touch);
						}
					}
				}
#endif
			}
			return touches;
		}
		public int GetTouchCount()
		{
			if (Input.touchSupported == true)
			{
				return Input.touchCount;
			}
			else
			{
				int count = 0;
				if (GetTouch(0).Phase != TouchPhase.None)
				{
					count++;
				}
#if UNITY_EDITOR
				for (int i = 0; i < keys.Count; i++)
				{
					if (GetTouch(i + 1).Phase != TouchPhase.None)
					{
						count++;
					}
				}
#endif
				return count;
			}
		}
		public bool IsClickOnUI(Touch touch)
		{
			if (eventSystem == null)
			{
				eventSystem = EventSystem.current;
			}
			if (eventSystem.IsPointerOverGameObject() == true || eventSystem.IsPointerOverGameObject(touch.Id))
			{
				return true;
			}
			return false;
		}
	}
	public enum TouchPhase
	{
		None,
		Down,
		Move,
		Up
	}
	public struct Touch
	{
		public int Id { get; set; }
		public Vector2 Position { get; set; }
		public TouchPhase Phase { get; set; }

	}
}
