using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class HouseModelBuilder : MonoBehaviour
{
	public Texture2D heightMap;
	public float heightMultiplier;
	public int sampleSize;
	public float threshold;
	public float maxSmallClumpHeight;

	public Texture2D flatTexture;
	public Texture2D raisedTexture;

	void Start()
	{
		HouseManager.SetHeightMap(heightMap);
		InvokeRepeating("ChangeHeightMap", 0f, 1f);  // Calls ModifyTerrain every 3 seconds
	}

	public async void ChangeHeightMap()
	{
		if (HouseManager.GetHeightMap() == null)
		{
			Debug.LogError("Heightmap was null!");
			return;
		}


		if (HouseManager.GetHeightMap() == heightMap)
		{
			return;
		}
		else
		{
			this.heightMap = HouseManager.GetHeightMap();
			Debug.Log("New heightmap loaded!");
			await ModifyTerrain();
		}
	}

	async Task<bool> ModifyTerrain()
	{
		if (heightMap == null)
		{
			Debug.LogError("Heightmap is not assigned!");
			return false;
		}

		Terrain terrain = GetComponent<Terrain>();
		TerrainData terrainData = terrain.terrainData;

		Debug.LogError(heightMap.height);
		Debug.LogError(heightMap.width);
		// Set terrain size based on the PNG image dimensions
		terrainData.size = new Vector3(heightMap.height / 43, heightMultiplier, heightMap.width / 43);

		int width = terrainData.heightmapResolution;
		int height = terrainData.heightmapResolution;

		float[,] heights = new float[width, height];

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Fix for mirrored terrain
				float normalizedX = 1 - (x / (float)width);
				float normalizedY = y / (float)height;

				float averageGrayscaleValue = 0f;
				int sampleCount = 0;

				for (int i = -sampleSize; i <= sampleSize; i++)
				{
					for (int j = -sampleSize; j <= sampleSize; j++)
					{
						float sampleX = Mathf.Clamp(normalizedX + (i / (float)width), 0, 1);
						float sampleY = Mathf.Clamp(normalizedY + (j / (float)height), 0, 1);

						averageGrayscaleValue += heightMap.GetPixelBilinear(sampleX, sampleY).grayscale;
						sampleCount++;
					}
				}

				averageGrayscaleValue /= sampleCount;

				// Inverting the grayscale value so black becomes the highest point
				averageGrayscaleValue = 1 - averageGrayscaleValue;

				// If the average grayscale value is below the threshold, set the height to zero
				if (averageGrayscaleValue < threshold)
				{
					averageGrayscaleValue = 0;
				}
				// If the average grayscale value is above the threshold but considered a small clump, cap the height
				else if (averageGrayscaleValue < maxSmallClumpHeight)
				{
					//averageGrayscaleValue = maxSmallClumpHeight;
				}
				else
				{
					averageGrayscaleValue = 0.4f;
				}

				heights[x, y] = averageGrayscaleValue * heightMultiplier;
			}
		}

		terrainData.SetHeights(0, 0, heights);

		ApplyTextures(terrainData);
		return true;
	}
	void ApplyTextures(TerrainData terrainData)
	{
		TerrainLayer[] terrainLayers = new TerrainLayer[2];

		terrainLayers[0] = new TerrainLayer();
		terrainLayers[0].diffuseTexture = flatTexture;

		terrainLayers[1] = new TerrainLayer();
		terrainLayers[1].diffuseTexture = raisedTexture;

		terrainData.terrainLayers = terrainLayers;

		float[,,] map = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 2];

		for (int y = 0; y < terrainData.alphamapHeight; y++)
		{
			for (int x = 0; x < terrainData.alphamapWidth; x++)
			{
				float height = terrainData.GetHeight(y, x) / heightMultiplier;

				if (height > 0.1f)  // Adjust this threshold as needed
				{
					map[x, y, 0] = 0;
					map[x, y, 1] = 1;
				}
				else
				{
					map[x, y, 0] = 1;
					map[x, y, 1] = 0;
				}
			}
		}

		terrainData.SetAlphamaps(0, 0, map);
	}
}
