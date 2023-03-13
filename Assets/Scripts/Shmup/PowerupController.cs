using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shmup
{

public class PowerupController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.GrantRandomBuff();
            Destroy(this.gameObject);
        }
    }
}


} // namespace Shmup