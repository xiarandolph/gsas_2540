using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class HealthBarController : MonoBehaviour
{
    Transform bar;
    Quaternion rotation;
    Vector3 position;

    void Awake()
    {
        bar = transform.Find("Bar").GetComponent<Transform>();
        rotation = Quaternion.identity;
        position = transform.parent.position - transform.position;

        // make the healthbar invisible initially
        // healthbar is active initially in scene because
        // original rotation and position need to be saved
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        // make sure healthbar always stays in position and rotation above parent
        transform.rotation = rotation;
        transform.position = transform.parent.position - position;
    }

    public void UpdateBar(float percentage)
    {
        // healthbar is only visible when damaged
        gameObject.SetActive(percentage < 1.0f);

        // scene is set up such that bar can be scaled from 0 to 1
        Vector3 scale = bar.localScale;
        scale.x = percentage;
        bar.localScale = scale;
    }
}

} // namespace Shmup

