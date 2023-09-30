using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//abstract means this class does nothing, only its children. And they are obligated to have everything the parent has
public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    protected int coinValue = 10; 
    protected bool alreadyCollected;

    public abstract int Collect();

    public void SetValue(int value){
        coinValue = value;
    }

    //it hides or shows the coin on map, or else the lag from the server will make it junky
    protected void Show(bool show) {
        spriteRenderer.enabled = show;
    }
    
}
