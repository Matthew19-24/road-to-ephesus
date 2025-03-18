using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<GameObject> hexPrefabs;
    public float hexSize = 1.0f;
    private Dictionary<Vector2Int, TileType> tileMap = new Dictionary<Vector2Int, TileType>();
    private float noiseSeed;

    void Start()
    {
        noiseSeed = Random.Range(0f, 100f); // Generate a random seed
        GenerateHexMap(MapSettings.mapRadius);
    }

    public void GenerateHexMap(int radius)
    {
        List<Vector2Int> hexPositions = new List<Vector2Int>();
        
        // Generate terrain with noise-based grouping
        foreach (var pos in GenerateHexGrid(radius))
        {
            TileType tileType = DetermineTileType(pos);
            tileMap[pos] = tileType;
        }
        
        // Ensure paths connect by carving a main path through the map
        ConnectPaths(hexPositions);
        
        // Instantiate hex tiles in Unity
        foreach (var pos in tileMap.Keys)
        {
            Vector3 worldPos = AxialToWorld(pos.x, pos.y);
            GameObject hexPrefab = hexPrefabs[(int)tileMap[pos]];
            Instantiate(hexPrefab, worldPos, Quaternion.identity, transform);
        }
    }

    private IEnumerable<Vector2Int> GenerateHexGrid(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
            {
                yield return new Vector2Int(q, r);
            }
        }
    }

    private TileType DetermineTileType(Vector2Int pos)
    {
        float noise = Mathf.PerlinNoise((pos.x + noiseSeed) * 0.1f, (pos.y + noiseSeed) * 0.1f);
        if (noise < 0.3f) return TileType.Water;
        if (noise < 0.6f) return TileType.Forest;
        return TileType.Path;
    }

    private void ConnectPaths(List<Vector2Int> positions)
    {
        Vector2Int start = new Vector2Int(-MapSettings.mapRadius, 0);
        Vector2Int end = new Vector2Int(MapSettings.mapRadius, 0);
        List<Vector2Int> path = AStarPathfinding(start, end);
        foreach (var pos in path)
        {
            tileMap[pos] = TileType.Path;
        }
    }

    private List<Vector2Int> AStarPathfinding(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> openSet = new HashSet<Vector2Int> { start };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, end) };
        
        while (openSet.Count > 0)
        {
            Vector2Int current = GetLowestFScore(openSet, fScore);
            if (current == end)
                return ReconstructPath(cameFrom, current);
            
            openSet.Remove(current);
            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                float tentativeGScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
                    openSet.Add(neighbor);
                }
            }
        }
        return new List<Vector2Int>();
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector2Int GetLowestFScore(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int best = default;
        float bestScore = float.MaxValue;
        foreach (var pos in openSet)
        {
            if (fScore.TryGetValue(pos, out float score) && score < bestScore)
            {
                best = pos;
                bestScore = score;
            }
        }
        return best;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y),
            new Vector2Int(pos.x, pos.y + 1),
            new Vector2Int(pos.x, pos.y - 1),
            new Vector2Int(pos.x + 1, pos.y - 1),
            new Vector2Int(pos.x - 1, pos.y + 1)
        };
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private Vector3 AxialToWorld(int q, int r)
    {
        float x = hexSize * Mathf.Sqrt(3) * (q + r / 2f);
        float z = hexSize * 3f / 2f * r;
        return new Vector3(x, 0f, z);
    }
}

public static class MapSettings
{
    public static int mapRadius { get; set; } = 20;
}

enum TileType
{
    Path = 0,
    Water = 1,
    Forest = 2
}
