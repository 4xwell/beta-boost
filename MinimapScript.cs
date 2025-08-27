using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple minimap that follows the player from above.
/// </summary>

public class MinimapScript : MonoBehaviour
{
    [SerializeField] Transform player;

    void Awake() => player ??= transform;   // fallback

    void LateUpdate()
    {
        var newPosition    = player.position;
        newPosition.y      = transform.position.y;
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
