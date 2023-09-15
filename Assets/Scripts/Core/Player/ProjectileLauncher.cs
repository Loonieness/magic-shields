using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{  
    [Header("References")]
    //hooked into the 'fire' action
    [SerializeField] private InputReader inputReader;
    //the bullet spawns' position at the end of the barrel, and it's decided its rotation to go forth. All is Transform
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;

    private bool shouldFire;

    public override void OnNetworkSpawn(){
        if( !IsOwner ) { return; }
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn(){
        if( !IsOwner ) { return; }
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update()
    {
        if( !IsOwner ) { return; }
        if( !shouldFire ) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
    }

    private void HandlePrimaryFire(bool shouldFire){
        this.shouldFire = shouldFire;
    }

    //create an invisible object exactly like the bullet on the client, but this is where all the logic will go
    //Remote Procedural Call. Sends a message to server, server responds to client, continues
    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction){
        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab, 
            spawnPos, 
            Quaternion.identity);

            projectileInstance.transform.up = direction;

            //the serverRpc calls the ClientRpc to send the information to every client
            SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

        //sends a message to all clients
        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction){
            //if we are the owner, there's no need to see the same object twice
            if( IsOwner ) { return; }

             SpawnDummyProjectile(spawnPos, direction);



        }

    //spawns the bullet on the client, but only to be visible. There will be no logic here such as collision
    //Dummy = not real
    //it's used on the Update method
     private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        //create a new object. First, the original, then the position of the original, and the rotation
        //identity is to not cast from quaternion to Vector3
        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab, 
            spawnPos, 
            Quaternion.identity);

            projectileInstance.transform.up = direction;
    }
}
