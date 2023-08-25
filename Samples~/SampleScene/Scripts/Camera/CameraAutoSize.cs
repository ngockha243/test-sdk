using UnityEngine;

using Extensions;

public class CameraAutoSize : MonoBehaviour
{
	[SerializeField] protected Camera mainCamera;
	[SerializeField] protected Transform background;
	[SerializeField] protected float minSize = 11.7f;
	[SerializeField] protected float normalSize = 9.6f;
	[SerializeField] protected float scale = 1.146f;
	[SerializeField] protected float minRatio = 0.45f;
	[SerializeField] protected float normalRatio = 0.5625f;

	private float maxPosX;
	private float currentSize;
	public float MaxSizeX { get; private set; }
	private void Awake()
	{
		mainCamera.ThrowIfNull();
		background.ThrowIfNull();
		background.localScale = Vector3.one * scale;
		AutoSize();
	}

	public void AutoSize()
	{
		float currentRatio = (float)Screen.width / (float)Screen.height;
		float size = normalSize;
		if (currentRatio < normalRatio)
		{
			size = minSize * minRatio / currentRatio;
		}
		mainCamera.orthographicSize = size;

		maxPosX = size * currentRatio;
		currentSize = size;
	}

	public float GetMaxPosX()
	{
		return maxPosX;
	}

	public float GetCurrentSize()
	{
		return currentSize;
	}
}
