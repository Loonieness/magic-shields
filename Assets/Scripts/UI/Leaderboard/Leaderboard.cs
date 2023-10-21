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
    [SerializeField] private int entitiesToDisplay = 8;

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

        //makes sure the ones with more coins go to the top. If I wanted a ascending order, I'd swap x and y
        entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

        for (int i = 0; i < entityDisplays.Count; i++)
        {
            //already shows the correct descending order, but doesn't updates the text
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            //-1 because, counting the 0, it would be 9 names instead of 8
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay -1);
        }

        //references ourselves. The child representing us
        LeaderboardEntityDisplay myDisplay =
            entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if(myDisplay != null){
            //if I am beyond the eighth place, get the eighth place child and hides it. Put me instead
            if(myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay){
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void HandlePlayerSpawned(Player player){
        leaderboardEntities.Add(new LeaderboardEntityState{
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0

        });

        //OnValueChanged always passes the old and new value after said change
        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) =>
            HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    private void HandlePlayerDespawned(Player player){

        if(leaderboardEntities == null) { return; }

        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if(entity.ClientId != player.OwnerClientId) { continue; }

            leaderboardEntities.Remove(entity);
            break;
        }

        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) =>
            HandleCoinsChanged(player.OwnerClientId, newCoins);
        
    }

    private void HandleCoinsChanged(ulong clientId, int newCoins){
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if(leaderboardEntities[i].ClientId != clientId) { continue; }

            leaderboardEntities[i] = new LeaderboardEntityState{
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                Coins = newCoins
            };
            
            return;
        }

    }
}
