using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer
{
    //so we can restore locally whatever comes in the NetworkServer parameter
    private NetworkManager networkManager;

   public NetworkServer(NetworkManager networkManager){
    this.networkManager = networkManager;

    //this happens everytime someone enters the server
    networkManager.ConnectionApprovalCallback += ApprovalCheck;

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

        Debug.Log(userData.userName);

        //this will let this all finish the connection with the server. If set False, it wouldn't connect
        response.Approved = true;
        response.CreatePlayerObject = true;
    }
}
