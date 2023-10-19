using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

//remove monobehavior because this is only going to be a C# class that we create an instance of in the client singleton
public class ClientGameManager
{
    private JoinAllocation allocation;

    private NetworkClient networkClient;

    private const string MenuSceneName = "Menu";

    public async Task<bool> InitAsync() {
        await UnityServices.InitializeAsync();

        //this hooks up NetworkClient to when the host disconnects and the player is now a client obligatoryly
        //this makes a client be able to go into a new game without having to reopen the whole build
        networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if(authState == AuthState.Authenticated) {
            return true;
        }

        return false;
    }

    public void GoToMenu(){
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            //take the joinCode, which is a allocation code, created by the host
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
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

        UserData userData = new UserData{
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };

        //transform the player's name into json, then into byte array, them send it to the server when connecting
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
        //the client doesn't need to start a scene, because the server will do it for them
        
    }
}
