using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure
{

public class PlayerController : MonoBehaviour
{
    public Text log;                        // UI text for displaying to player
    public Light flashlight;                // Actual light source on the player
    public Text flashlightLabel;            // UI text for displaying battery
    public Text turnLabel;                  // UI text for displaying turn number
    public GameObject goal;                 // first goal player should reach
    public GameObject exit;                 // exit point for end of game
    public AudioSource reachGoalSound;
    public AudioSource reachExitSound;

    public float travelDistance = 2.0f;     // Distance in world space (based on prefab sizes)
    public float turnSpeed = 7.0f;
    public float moveSpeed = 5.0f;
    public float soundDistance = 20.0f;     // Max distance to hear sounds from

    public int battery = 25;                // remaining flashlight usage

    bool running = false;                   // whether game is still running or not
    bool reachedGoal = false;
    int turn = 0;

    enum Action {FORWARD = 0, STRAFE_LEFT, BACK, STRAFE_RIGHT, TURN_LEFT, TURN_RIGHT};

    Quaternion desiredRotation;
    Vector3 desiredPosition;

    void Start()
    {
        desiredRotation = transform.rotation;
        desiredPosition = Vector3.zero;
        flashlightLabel.text = battery.ToString();
        turnLabel.text = String.Format("Turn: {0}", turn);
        UpdateSounds();

        log.SendMessage("Clear");
        log.SendMessage("Print", "You wake up in a dark room ...");
        log.SendMessage("Print", "You faintly hear a stream in front of you and rain to your right ...");

        running = true;
    }

    void Update()
    {
        // Perform turning & motion through lerping
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * turnSpeed);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * moveSpeed);
    }

    // UpdateSounds modifies the volume on goal and exit based on player distance
    void UpdateSounds()
    {
        // get volume by subtracting distance from max sound distance and clamping
        float distance = Vector3.Distance(desiredPosition, goal.transform.position);
        float volume = Mathf.Clamp((soundDistance - distance) / soundDistance, 0.0f, 1.0f);

        goal.GetComponent<AudioSource>().volume = volume;
        goal.GetComponent<AudioSource>().Play();

        // repeat for exit
        distance = Vector3.Distance(desiredPosition, exit.transform.position);
        volume = Mathf.Clamp((soundDistance - distance) / soundDistance, 0.0f, 1.0f);

        exit.GetComponent<AudioSource>().volume = volume;
        exit.GetComponent<AudioSource>().Play();
    }

    // PerformAction is called by UI buttons, redirect them to desired method here
    public void PerformAction(int action)
    {
        if (!running) return;

        ++turn;

        switch ((Action) action)
        {
            case Action.FORWARD:
            case Action.STRAFE_LEFT:
            case Action.BACK:
            case Action.STRAFE_RIGHT:
                Move(action);
                break;
            case Action.TURN_LEFT:
                desiredRotation *= Quaternion.Euler(0, 0, 90);
                break;
            case Action.TURN_RIGHT:
                desiredRotation *= Quaternion.Euler(0, 0, -90);
                break;
            default:
                Debug.Log("Unknown action in PlayerController.PerformAction");
                return;
        }

        UpdateFlashlight(flashlight.enabled);   // flashlight uses battery for all actions
        turnLabel.text = String.Format("Turn: {0}", turn);
    }

    // UpdateFlashlight controls the player's light source based on battery levels
    public void UpdateFlashlight(bool enabled)
    {
        if (!running) return;

        flashlight.enabled = enabled;   // update flashlight based on toggle
        if (enabled)
        {
            battery = Mathf.Max(battery - 1, 0);        // reduce flashlight battery, limit to 0
            if (!(flashlight.enabled = battery > 0))    // check if battery reached zero & update flashlight
            {
                log.SendMessage("Print", "Your flashlight has died ...");
            }
        }

        flashlightLabel.text = battery.ToString();
    }

    void Move(int action)
    {
        Vector3 delta;      // move amount towards direction faced
        float distance;     // for calculations later
        float newDistance;

        // multiply direction by players rotation to get proper motion
        // FORWARD & BACK are actually UP and DOWN in world space
        switch ((Action) action)
        {
            case Action.FORWARD:
                delta = travelDistance * (desiredRotation * Vector3.up);
                break;
            case Action.STRAFE_LEFT:
                delta = travelDistance * (desiredRotation * Vector3.left);
                break;
            case Action.BACK:
                delta = travelDistance * (desiredRotation * Vector3.down);
                break;
            case Action.STRAFE_RIGHT:
                delta = travelDistance * (desiredRotation * Vector3.right);
                break;
            default:
                Debug.Log("Unknown action in PlayerController.Move");   // this should never occur ...
                return;
        }

        // check if a collision will occur before moving
        // using collisions instead of bounds b/c world is randomly generated
        RaycastHit hitInfo;
        if (Physics.Raycast(desiredPosition, delta, out hitInfo, travelDistance))
        {
            if (hitInfo.collider.tag == "Wall")
            {
                log.SendMessage("Print", "You hit a wall ...");
                // make sure audio source is placed on wall prefab
                hitInfo.collider.gameObject.GetComponent<AudioSource>().Play();
            }
        }
        else    // no collisions, perform movement
        {
            if (!reachedGoal)   // determine if player is moving closer to goal
            {
                distance = Vector3.Distance(desiredPosition, goal.transform.position);
                newDistance = Vector3.Distance(desiredPosition + delta, goal.transform.position);

                if (newDistance < distance)
                    log.SendMessage("Print", "The sound of a stream gets louder ...");
                else
                    log.SendMessage("Print", "The sound of a stream gets softer ...");
            }
            else                // determine if moving closer to exit
            {
                distance = Vector3.Distance(desiredPosition, exit.transform.position);
                newDistance = Vector3.Distance(desiredPosition + delta, exit.transform.position);

                if (newDistance < distance)
                    log.SendMessage("Print", "The sound of rain gets louder ...");
                else
                    log.SendMessage("Print", "The sound of rain gets softer ...");
            }

            // update player position
            desiredPosition += delta;
            GetComponent<AudioSource>().Play(); // footsteps sounds is component on the player
        }

        UpdateSounds();                         // new position, update goal & exit sounds

        // check players position with respect to the goal for clear conditions
        distance = Vector3.Distance(desiredPosition, goal.transform.position);
        if (!reachedGoal && distance < travelDistance)
        {
            reachedGoal = true;
            goal.GetComponent<AudioSource>().mute = true;   // mute goal sounds
            reachGoalSound.Play();

            log.SendMessage("Print", "You find a key!");
        }

        // now check exit
        distance = Vector3.Distance(desiredPosition, exit.transform.position);
        if (distance < travelDistance)
        {
            if (reachedGoal)    // player already has the key
            {
                exit.GetComponent<AudioSource>().mute = true;
                reachExitSound.Play();

                running = false;    // end the game

                log.SendMessage("Print", "You escaped!");
                log.SendMessage("Print", String.Format("Turns taken: {0}", turn));
            }
            else                // player doesn't have key yet
            {
                log.SendMessage("Print", "The exit is locked ... ");
                log.SendMessage("Print", "Maybe the key is by the stream?");
            }
        }
    }
}

} // namespace Adventure