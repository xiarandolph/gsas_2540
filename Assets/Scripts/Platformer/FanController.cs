using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{

public class FanController : MonoBehaviour
{
    Animator animator;

    [SerializeField] bool on = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator.SetBool("on", on);
    }
}

} // namespace Platformer