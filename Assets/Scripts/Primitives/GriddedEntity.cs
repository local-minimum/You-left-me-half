using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class for things that exists on the level grid
/// </summary>
[ExecuteInEditMode]
public class GriddedEntity : MonoBehaviour
{
    [SerializeField]
    Vector3Int gridPosition;

    private void Awake()
    {
        var movable = GetComponent<MovingEntity>();
        if (movable) { 
            movable.OnMove += GriddedEntity_OnMove;
        }
    }

    private void OnDestroy()
    {
        var movable = GetComponent<MovingEntity>();
        if (movable)
        {
            movable.OnMove -= GriddedEntity_OnMove;
        }
    }

    private void GriddedEntity_OnMove(string id, Vector3Int position, FaceDirection lookDirection)
    {
        gridPosition = position;
    }


    private void Update()
    {
        if (!Application.isPlaying)
        {
            transform.position = Level.AsWorldPosition(gridPosition);
        }
    }
}
