using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    //public because the UI needs to read this, but the get private set makes it only settable in private
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    //only the server can change this variable. Public because we will show it on screen via different scripts 
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    private bool isDead;

    public Action<Health> OnDie;
    public override void OnNetworkSpawn() {
        if(!IsServer) { return; }

        CurrentHealth.Value = MaxHealth;
   }

    public void TakeDamage(int damageValue) {
        ModifyHealth(-damageValue);
   }

    public void RestoreHealth(int healValue) {
        ModifyHealth(healValue);
   }

    public void ModifyHealth(int value) {
        if(isDead) { return; }

        int newHealth = CurrentHealth.Value + value;
        //sets the maximum and minimum value of newHealth
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

        if(CurrentHealth.Value == 0){
            //question mark is a null checker. This summons the OnDie event
            OnDie?.Invoke(this);
            isDead = true;
        }


   }
}
