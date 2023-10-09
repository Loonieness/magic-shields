using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinCollector : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire;
    }

    private void OnTriggerEnter2D(Collider2D col) {
        //if it can't get the component
        if(!col.TryGetComponent<Coin>(out Coin coin)) { return; }

        int coinValue = coin.Collect();
        
        if(!IsServer) { return; }

        TotalCoins.Value += coinValue;
    }
}
