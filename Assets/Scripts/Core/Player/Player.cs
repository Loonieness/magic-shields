using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Collections;
using System;

public class Player : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    //the "field" makes it appear on the editor. Without it, properties don't appear on the editor
    [field: SerializeField] public Health Health { get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    //string is not a valid type to sync over the network, so we use this monstrosity instead. Works as a string
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if(IsServer){
            UserData userData =
                HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);  

            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
        }

        if(IsOwner){
            virtualCamera.Priority = ownerPriority;
        }
    }

    public override void OnNetworkDespawn(){
        if(IsServer){
            OnPlayerDespawned?.Invoke(this);
        }

    }
}
