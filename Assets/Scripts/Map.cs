using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<GameObject> hexPrefabs;
    public float hexSize = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateHexMap(MapSettings.mapRadius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateHexMap(int radius)
    {
        List<Vector3> hexPositions = new List<Vector3>();
        
        // Iterate through the hex grid coordinates
        for (int q = -radius; q <= radius; q++)
        {
            for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
            {
                // Calculate the axial coordinates (q, r)
                Vector3 hexPosition = AxialToWorld(q, r);
                hexPositions.Add(hexPosition);
                
                // Instantiate the hex tile at the calculated position
                Instantiate(hexPrefabs[0], hexPosition, Quaternion.identity, transform);
            }
        }
    }

    // Convert axial coordinates to world position (in Unity space)
    private Vector3 AxialToWorld(int q, int r)
    {
        // Assuming a flat-top hex grid and a unit hex size, this calculation is based on axial to world conversion
        float x = hexSize * 3f / 2f * q;
        float z = hexSize * Mathf.Sqrt(3) * (r + q / 2f);
        return new Vector3(x, 0f, z);  // We ignore the y-axis for 2D
    }


}

public static class MapSettings {
    public static int mapRadius { get; set;} = 5;
    public static List<HexTile> hexTiles = new List<HexTile>();
}

enum TileType
{
    Path,
    Water,
    Forest
}