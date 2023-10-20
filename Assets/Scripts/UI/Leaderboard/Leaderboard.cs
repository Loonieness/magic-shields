using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    private List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();

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
            //if there's not any entityDisplays where it has a ClientId where it is equal to this new ClientId comming
            //spawn a new one. If match, don't spawn
            if(!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId)){
                LeaderboardEntityDisplay leaderboardEntity = 
                    Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                leaderboardEntity.Initialise(
                    changeEvent.Value.ClientId, 
                    changeEvent.Value.PlayerName,
                    changeEvent.Value.Coins);
                entityDisplays.Add(leaderboardEntity); 
            }
            break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
            LeaderboardEntityDisplay displayToRemove = 
                entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

            if(displayToRemove != null){
                //if we just destroy it, it can cause some issues. So first, dettach it from leaderboard
                displayToRemove.transform.SetParent(null);
                Destroy(displayToRemove.gameObject);
                entityDisplays.Remove(displayToRemove);
            }
            break;

            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
            LeaderboardEntityDisplay displayToUpdate = 
                entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);

            if(displayToUpdate != null){
                displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
            }
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
