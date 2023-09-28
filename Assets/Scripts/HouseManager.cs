using UnityEngine;

public static class HouseManager
{
	public static Texture2D heightMap;

	public static Texture2D GetHeightMap()
	{
		return heightMap;
	}

	public static void SetHeightMap(Texture2D newHeightMap)
	{
		heightMap = newHeightMap;
	}
}