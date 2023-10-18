using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;


//remove monobehavior because this is only going to be a C# class that we create an instance of in the host singleton
public class HostGameManager
{
    private string joinCode;
    private string lobbyId;

    private Allocation allocation;

    //number of connections, or players
    private const int MaxConnections = 20;

    private const string GameSceneName = "Game";

    public async Task StartHostAsync(){
        try
        {
            //please give me an allocation with this many connections
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
            
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        try
        {
            //then, give me the code for it
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

//and here is all the info you need. The IP, the Port, the connection type DTLS(Datagram Transport Layer Security)
        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            //Data is everything that everyone on the lobby can read. In this case, the joinCode
            lobbyOptions.Data = new Dictionary<string, DataObject>(){
                {
                    "JoinCode", new DataObject(
                        //they will join the lobby, and then be able to read the code as a member
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )
                }

            };
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(
                "My Lobby", MaxConnections, lobbyOptions);

            lobbyId = lobby.Id;      
            //We will call the HostSingleton, which is a monobehavior, and make it run the heartbeat every 15 seconds
            //coroutines happen in monobehaviors, and this class isn't one. 
            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15));      
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        NetworkManager.Singleton.StartHost();
        
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    //IEnumerator is a coroutine, which will happen, pause, then happen again from where it stopped
    private IEnumerator HeartBeatLobby(float waitTimeSeconds){
        //this is an infinite loop
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while(true){
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;

        }

    }
}
