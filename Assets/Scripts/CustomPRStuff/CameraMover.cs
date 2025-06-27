using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private GameObject camToMove;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector2 moveDirection;

    private void Update()
    {
        if(camToMove != null)
        {
            Vector2 move = moveDirection * moveSpeed * Time.deltaTime;
            camToMove.transform.position = new Vector3(camToMove.transform.position.x + move.x, camToMove.transform.position.y + move.y, camToMove.transform.position.z);
        }
    }
}
