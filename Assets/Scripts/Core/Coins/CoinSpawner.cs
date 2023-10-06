using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class CoinSpawner : NetworkBehaviour {
    
    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins = 50;
    [SerializeField] private int coinValue = 10;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
//used to check which layer the object is in, so it doesn't spawn on a wall or anything
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];

    private float coinRadius;

    public override void OnNetworkSpawn(){
        if(!IsServer) { return; }
        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }

    }

    private void SpawnCoin(){
        //creates a coin on a random coordinate with a default rotation
        RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

        coinInstance.SetValue(coinValue);
        //it exists as a server side object, and owned by no client
        coinInstance.GetComponent<NetworkObject>().Spawn();

        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        //updates the coin position
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    //will create a circle on the map and, if there isn't a wall in it, spawns a coin in it
    private Vector2 GetSpawnPoint(){
        float x = 0;
        float y = 0;

        //basically a infinite loop until break
        while(true){
            //gets random x and y coordinates
            x = Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = Random.Range(ySpawnRange.x, ySpawnRange.y);

            //the random coordinate generated
            Vector2 spawnPoint = new Vector2(x, y);

            //it doesn't alocate memory, more performative, just uses the same array it receives
            //checks if there is anything on the random space 
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if(numColliders == 0){
                return spawnPoint;
            }
        }
    }
}
