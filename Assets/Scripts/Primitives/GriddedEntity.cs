using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GriddedEntity : MonoBehaviour
{
    [SerializeField]
    Vector3Int gridPosition;

    private void Awake()
    {
        var movable = GetComponent<MovableEntity>();
        if (movable) { 
            movable.OnMove += GriddedEntity_OnMove;
        }
    }

    private void OnDestroy()
    {
        var movable = GetComponent<MovableEntity>();
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
