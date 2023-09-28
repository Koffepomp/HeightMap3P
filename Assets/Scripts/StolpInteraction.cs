using StarterAssets;
using UnityEngine;

public class StolpInteraction : MonoBehaviour
{
	public Texture2D heightMap;
	private Transform playerTransform;
	public float interactionDistance = 2.0f;
	public StarterAssetsInputs playerInputs;
	// Start is called before the first frame update
	void Start()
	{
		playerTransform = playerInputs.transform;
	}

	// Update is called once per frame
	void Update()
	{
		float distance = Vector3.Distance(transform.position, playerTransform.position);

		if (distance <= interactionDistance && playerInputs.interact)
		{
			Debug.Log("'" + heightMap.name + "'" + " set as new heightMap.");
			HouseManager.SetHeightMap(heightMap);
		}
	}
}
