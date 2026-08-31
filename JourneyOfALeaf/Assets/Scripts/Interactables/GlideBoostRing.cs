using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GlideBoostRing : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostSpeed = 15f;
    [SerializeField] private bool useRingForwardDirection = true; // push player through the ring's facing direction

    [Header("Cooldown (prevents re-triggering while overlapping)")]
    [SerializeField] private float reTriggerCooldown = 1f;
    private float lastTriggerTime = -999f;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastTriggerTime < reTriggerCooldown)
            return;

        Player_Glide playerGlide = other.GetComponentInParent<Player_Glide>();
        if (playerGlide == null)
            return;

        if (!playerGlide.IsGliding)
            return;

        Vector3 boostDirection = useRingForwardDirection
            ? transform.forward
            : playerGlide.transform.forward;

        playerGlide.ApplyBoost(boostDirection, boostSpeed);
        lastTriggerTime = Time.time;

        Debug.Log("Glide boost ring triggered!");
    }

    // Visualize the ring's facing direction in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
    }
}