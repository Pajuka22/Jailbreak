using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerBase toFollow;
    public PlayerBase otherPlayer;
    public Vector3 vectorToPlayer = new Vector3(1, -1, 0);
    public float distanceToPlayer = 10;
    [Range(0.1f, 1)]
    public float smoothFactor;

    private void Start()
    {
        transform.rotation = Quaternion.LookRotation(vectorToPlayer);
        transform.position = toFollow.transform.position - vectorToPlayer / vectorToPlayer.magnitude * distanceToPlayer;
    }
    public void SwapPlayers()
    {
        PlayerBase placeholder = toFollow;
        toFollow = otherPlayer;
        otherPlayer = placeholder;
    }
    private void LateUpdate()
    {
        Vector3 newPos = toFollow.transform.position - vectorToPlayer.normalized * distanceToPlayer;
        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor);
    }
}
