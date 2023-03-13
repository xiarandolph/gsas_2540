using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{

public class CameraController : MonoBehaviour
{
    // referenced objs
    Camera cam;
    Rigidbody2D player;

    // logic variables, modified in functions
    float smoothTime = 0.3f;                // SmoothDamp: rough time it takes camera to follow player
    Vector3 camVelocity = Vector3.zero;     // used with SmoothDamp

    void Awake()
    {
        cam = GetComponent<Camera>();
        player = GameObject.Find("Player").GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        //GameObject obj = GameObject.Find("Border");
        //Debug.Log(obj.GetComponent<CompositeCollider2D>().bounds);
    }

    void Update()
    {
        Vector3 pos = new Vector3(player.position.x, player.position.y, cam.transform.position.z);
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, pos, ref camVelocity, smoothTime);
    }
}


}

