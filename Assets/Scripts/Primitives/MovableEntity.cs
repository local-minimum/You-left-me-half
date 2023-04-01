using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MoveEvent(string id, Vector3Int position, FaceDirection lookDirection);

public class MovableEntity : MonoBehaviour
{
    [SerializeField, Tooltip("Leave empty to use game object name")]
    string id;

    public event MoveEvent OnMove;

    public string Id
    {
        get
        {
            return string.IsNullOrEmpty(id) ? name : id;
        }
    }
    public void SetNewGridPosition(Vector3Int position, FaceDirection lookDirection)
    {
        OnMove?.Invoke(Id, position, lookDirection);
    }
}
