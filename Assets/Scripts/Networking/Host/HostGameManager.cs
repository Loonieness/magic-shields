using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

//remove monobehavior because this is only going to be a C# class that we create an instance of in the client singleton
public class HostGameManager
{
    private string joinCode;

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

        //and here is all the info you need. The IP, the Port, the connection type UDP(user data protocol)
        RelayServerData relayServerData = new RelayServerData(allocation, "udp");
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
        
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }
}
