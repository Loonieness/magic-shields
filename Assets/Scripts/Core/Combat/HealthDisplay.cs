using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage;

    public override void OnNetworkSpawn(){
        if(!IsClient) { return; }

        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
        //manually changes health because otherwise you would just see the changed version after damage or heal
        HandleHealthChanged(0, health.CurrentHealth.Value);

    }

    public override void OnNetworkDespawn(){
        if(!IsClient) { return; }
        //unsubscribe  health just so it doesn't linger after you exit the room or disconects
        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth){
        //this will give a float percentage, because MaxHealth is 1, not 100
        healthBarImage.fillAmount = (float)newHealth / health.MaxHealth;

    }

}
