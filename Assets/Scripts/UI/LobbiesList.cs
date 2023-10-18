using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    //from where the various lobbies are being created. The prefabs and the children
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    //this is false by default
    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable() {
        RefreshList();
    }

    public async void RefreshList(){
        if(isRefreshing){ return; }
        isRefreshing = true;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            options.Filters = new List<QueryFilter>(){

                new QueryFilter(
                    //check the avaiable slots and make sure they are Greater Than zero
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                new QueryFilter(
                    //check if it's locked, then equals it to 0, false, not showing. If not locked, then show it
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0") 
        };

        //take the options we just made above and put it on an object
        QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

        //destroy all existing lobbies and destroy them, just so they are not created multiple times
        foreach (Transform child in lobbyItemParent)
        {
            Destroy(child.gameObject);
        }

        //then, create new lobbies from the result of the filter above
        foreach (Lobby lobby in lobbies.Results)
        {
            LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
            lobbyItem.Initialise(this, lobby);
        }

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isRefreshing = false;

    }

    //this lobby has only the name and the maxCount of players. Just visual stuff
    public async void JoinAsync(Lobby lobby){
        if(isJoining){ return; }
        isJoining = true;

        try
        {   //this lobby actually has the Data, and is created by the system, not me
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isJoining = false;
    }
}
