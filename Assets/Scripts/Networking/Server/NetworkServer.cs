using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer
{
    //so we can restore locally whatever comes in the NetworkServer parameter
    private NetworkManager networkManager;

    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

   public NetworkServer(NetworkManager networkManager){
    this.networkManager = networkManager;

    //this happens everytime someone enters the server
    networkManager.ConnectionApprovalCallback += ApprovalCheck;
    networkManager.OnServerStarted += OnNetworkReady;

   }


    //gives information about said connection above
    private void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        //this casts the Payload, which would come into a byte[], into a string
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        //then, cast it to a usable object of type UserData
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        //from the payload, we store the Ids along the userData, having the name ready for display later
        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        //can gather the userData by using the id
        authIdToUserData[userData.userAuthId] = userData;

        //this will let this all finish the connection with the server. If set False, it wouldn't connect
        response.Approved = true;
        response.CreatePlayerObject = true;
    }

        private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        //when client disconects, and this functions finds a clientId and a authId, throw them away
        if(clientIdToAuth.TryGetValue(clientId, out string authId)){
            clientIdToAuth.Remove(clientId);

            authIdToUserData.Remove(authId);
        }
    }
}
