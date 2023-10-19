using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


//IDispose is to have a Dispose method that cleans up things when the class is used. To shutdown cleanly
public class NetworkClient : IDisposable
{
     //so we can restore locally whatever comes in the NetworkServer parameter
    private NetworkManager networkManager;

    private const string MenySceneName = "Menu";

   public NetworkClient(NetworkManager networkManager){
    this.networkManager = networkManager;

    //this happens everytime someone enters the server
    networkManager.OnClientDisconnectCallback += OnClientDisconnect;

   }


    private void OnClientDisconnect(ulong clientId)
    {
        //this makes sure that it all doesn't shut down if a client disconnects, and not a host
        //hosts id is 0 by default
        if(clientId != 0 && clientId != networkManager.LocalClientId){ return; }

        //if the scene we enter after disconnecting isn't the main menu, then goes to menu
        if(SceneManager.GetActiveScene().name != MenySceneName){
            SceneManager.LoadScene(MenySceneName);
        }
        
        //if we are still connected after disconnecting, shutdowns
        if(networkManager.IsConnectedClient){
            networkManager.Shutdown();
        }


    }

    //cleans up whatever this class does every time it is called
    public void Dispose()
    {   //unsubscribes the networkManager, not leaving traces when shuts down. The ClientGameManager uses this
        if(networkManager != null){
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}
