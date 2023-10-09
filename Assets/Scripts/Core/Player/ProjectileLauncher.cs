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
    [SerializeField] private CoinCollector wallet;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool shouldFire;
    private float timer;
    private float muzzleFlashTimer;

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
        //needs to be done before checking if owner, everyone should see it
        if(muzzleFlashTimer > 0f){
            muzzleFlashTimer -= Time.deltaTime;
            
            if(muzzleFlashTimer <= 0f){
            muzzleFlash.SetActive(false);
            }
        }

        if( !IsOwner ) { return; }

        //Ex: timer is 1. this method makes timer do a countdown, so 1 second, then turn timer into 0. 
        //on the end of this function, timer resets and receives a number of seconds again
        if(timer > 0) {
            timer -= Time.deltaTime;
        }

   

        if( !shouldFire ) { return; }

        if(timer > 0){ return ;}

        if(wallet.TotalCoins.Value < costToFire) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        
        //this updates the time every time a bullet is fired
        timer = 1 / fireRate;
    }

    private void HandlePrimaryFire(bool shouldFire){
        this.shouldFire = shouldFire;
    }

    //create an invisible object exactly like the bullet on the client, but this is where all the logic will go
    //Remote Procedural Call. Sends a message to server, server responds to client, continues
    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction){
        if(wallet.TotalCoins.Value < costToFire) { return; }

        wallet.SpendCoins(costToFire);

        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab, 
            spawnPos, 
            Quaternion.identity);

            projectileInstance.transform.up = direction;

            //so the player can't collide with it's own bullet
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

            if(projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage)){
                dealDamage.SetOwner(OwnerClientId);
            }

            //just to check if the bullet exists, so it can receive movement and speed
            if(projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb)){
                
                rb.velocity = rb.transform.up * projectileSpeed;
                
            }

            //the serverRpc calls the ClientRpc to send the information to every client
            SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

        //sends a message to all clients
        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction){
            //shows the explosion on the barrel
            muzzleFlash.SetActive(true);
            //times how much time the muzzle should be active, but a fixed time that will be countted down on update
            muzzleFlashTimer = muzzleFlashDuration;

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

            //so the player can't collide with it's own bullet
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

            //just to check if the bullet exists, so it can receive movement and speed
            if(projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb)){
                
                rb.velocity = rb.transform.up * projectileSpeed;

            }
    }
}
