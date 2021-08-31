using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeUp : MonoBehaviour
{
    public float multiplier = 1.5f;

    //for if we want particles 
    //public GameObject pickupEffect;

    void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup(other);
        }
    }

    void Pickup(Collider player)
    {
        //for if we want particles 
        //Instantiate(pickupEffect, transform.position, transform.rotation);
        
        Debug.Log("ur mom");

        player.transform.localScale *= multiplier;

        Destroy(gameObject);
    }
}
