using UnityEngine;

[ExecuteInEditMode]
public class WaterTile : MonoBehaviour
{
	public PlanarReflection reflection;

	public WaterBase waterBase;

	public void Start()
	{
		AcquireComponents();
	}

	private void AcquireComponents()
	{
		if (!reflection)
		{
			if ((bool)transform.parent)
			{
				reflection = transform.parent.GetComponent<PlanarReflection>();
			}
			else
			{
				reflection = transform.GetComponent<PlanarReflection>();
			}
		}
	}

	public void OnWillRenderObject()
	{
		Camera current = Camera.current;
		if ((bool)reflection)
		{
			reflection.WaterTileBeingRendered(transform, current);
		}
		if ((bool)waterBase)
		{
			waterBase.WaterTileBeingRendered(transform, current);
		}
	}
}
