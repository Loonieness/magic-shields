using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
   [SerializeField] private int damage = 10;
   
   private ulong ownerClientId;

    //ulong = big int. This function is to set the damage to a owner and not affect them
   public void SetOwner(ulong ownerClientId) {
        this.ownerClientId = ownerClientId;
   }

    private void OnTriggerEnter2D(Collider2D col) {
        if(col.attachedRigidbody == null) { return; }

        if(col.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj)){
            //if the local ID is the same as the collided objects ID
            if(ownerClientId == netObj.OwnerClientId){
                return;
            }
        }

        //out picks the component from the other object and do something with it
        if(col.attachedRigidbody.TryGetComponent<Health>(out Health health)){
            health.TakeDamage(damage);
        }
    
   } 
}
