using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private Player playerPrefab;
    [SerializeField] private float keptCoinPercentage;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        //a slow call. Will check everything in this scene and get back everything with this componente on it
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player player in players)
        {
            HandlePlayerSpawned(player);
        }

        Player.OnPlayerSpawned += HandlePlayerSpawned;
        Player.OnPlayerDespawned += HandlePlayerDeSpawned;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsServer) { return; }

        Player.OnPlayerSpawned -= HandlePlayerSpawned;
        Player.OnPlayerDespawned -= HandlePlayerDeSpawned;
    }

    private void HandlePlayerSpawned(Player player)
    {   
        //a work around because OnDie doesn't pass through anything and the Handler needs a parameter
        //So, we use the lambda to execute the Handle method, thus allowing a parameter directly
        //the "health" does nothing, it's just a means to an end
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDeSpawned(Player player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(Player player){
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));

        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins){
        //waits a single frame
        yield return null;

        Player playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.Wallet.TotalCoins.Value += keptCoins;
    }
}
