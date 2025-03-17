using UnityEngine;
using System;

public class HexTile : MonoBehaviour
{
    private TileType tileType;
    private Vector3 location;
    private string UID;

    void Start()
    {
        location = transform.position;

        string prefabName = gameObject.name.Replace("(Clone)", "").Trim();
        switch (prefabName)
        {
            case "Hex32":
                tileType = TileType.Path;
                break;
            case "Lake":
                tileType = TileType.Water;
                break;
            case "Forest":
                tileType = TileType.Forest;
                break;
            default:
                Debug.LogWarning($"Unknown prefab name: {prefabName}");
                break;
        }

        UID = Guid.NewGuid().ToString();

        MapSettings.hexTiles.Add(this);
    }

    void Update()
    {
        
    }
}
