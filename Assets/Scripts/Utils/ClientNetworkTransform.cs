using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

//the 'base' makes the function on the class always being called

public class ClientNetworkTransform : NetworkTransform
{
    //makes the player be assigned to the respective client on spawn and permission to move it
    public override void OnNetworkSpawn(){
        base.OnNetworkSpawn();
        CanCommitToTransform = IsOwner;
    }

    protected override void Update() {
        //makes sure that every frame we are still the owner
        CanCommitToTransform = IsOwner;
        base.Update();
        if(NetworkManager != null){
            if(NetworkManager.IsConnectedClient || NetworkManager.IsListening){
                if(CanCommitToTransform){
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }

            }
        }

    }
    //the server trusts the client now and everything that he does. Can lead to hacks
   protected override bool OnIsServerAuthoritative(){
    return false;
   }
}
