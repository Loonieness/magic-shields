using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;

    private void Awake() {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient){
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanges;
            foreach(LeaderboardEntityState entity in leaderboardEntities){
                HandleLeaderboardEntitiesChanges(new NetworkListEvent<LeaderboardEntityState>{
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if(IsServer){
            Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

            foreach(Player player in players){
                HandlePlayerSpawned(player);
            }

            Player.OnPlayerSpawned += HandlePlayerSpawned;
            Player.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsClient){
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanges;
        }

        if(IsServer){
            Player.OnPlayerSpawned -= HandlePlayerSpawned;
            Player.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    private void HandleLeaderboardEntitiesChanges(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch(changeEvent.Type){
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
            break;

        }
    }

    private void HandlePlayerSpawned(Player player){
        leaderboardEntities.Add(new LeaderboardEntityState{
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0

        });
    }

    private void HandlePlayerDespawned(Player player){

        if(leaderboardEntities == null) { return; }

        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if(entity.ClientId != player.OwnerClientId) { continue; }

            leaderboardEntities.Remove(entity);
            break;
        }
        
    }
}
