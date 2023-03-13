using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Avalanche
{

// EnemyController defines and controls the behaviour of the attached enemy.
// The Enemy is only able to move left and right and is bounded within the center
// 90% of the Main Camera's Viewport. "Drops" are instantiated at a semi-random pace.
//
// As time progresses, Enemy movement speed and Drop instantiation rate will increase.
public class EnemyController : MonoBehaviour
{
    // referenced GameObjects/Components
    [SerializeField] GameObject dropPrefab = null;
    Camera cam;

    // game variables
    [SerializeField] float speed = 3.0f;
    [SerializeField] float speedGrowth = 1.05f;
    [SerializeField] float maxSpeed = 15.0f;

    [SerializeField] float dropRate = 2.0f;
    [SerializeField] float rateGrowth = .96f;
    [SerializeField] float rateVariation = .3f;
    [SerializeField] float minRate = 0.5f;

    Vector3 heading;

    // Awake finds referenced GameObjects & components
    void Awake()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Start with the Enemy heading right and the first drop being 2 seconds later
    void Start()
    {
        heading = Vector3.right;

        Invoke("DropItem", 2.0f);
        Invoke("ReverseHeading", Random.Range(0.0f, 2 * maxSpeed / speed));
    }

    // Update Enemy position based on the current heading. If Enemy position is out
    // of bounds, set heading such that Enemy will head back into bounds.
    void Update()
    {
        transform.Translate(speed * heading * Time.deltaTime);

        // check if position is outside of center 90% of Viewport, rely on center of
        // World position being (0, 0) to determine new heading based on x position
        if (Mathf.Abs(cam.WorldToViewportPoint(transform.position).x - 0.5f) > 0.45f)
            heading = Mathf.Sign(transform.position.x) * Vector3.left;
    }

    // ReverseHeading more often as speed increases
    void ReverseHeading()
    {
        heading *= -1;
        Invoke("ReverseHeading", Random.Range(0.0f, 2 * maxSpeed / speed));
    }

    // DropItem instantiates an item, speeds up the Enemy and drop rate, and reinvokes itself.
    void DropItem()
    {
        Instantiate(dropPrefab, transform.position, Quaternion.identity);

        speed = Mathf.Min(speed * speedGrowth, maxSpeed);
        dropRate *= rateGrowth;

        // calculate next drop time with some variation, limit to minRate
        float nextDrop = dropRate * Random.Range(1-rateVariation, 1+rateVariation);
        Invoke("DropItem", Mathf.Max(nextDrop, minRate));
    }
}

} // namespace Avalanche