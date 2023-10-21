using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BountyCoin : Coin
{
    public override int Collect() {
        //if it's a client, just hides it and make sure it can't be collected again
        if(!IsServer) {
            Show(false);
            return 0;
        }

        if(alreadyCollected) { return 0; }

        //if the cases where it's collected doesn't happen, collect it 
        alreadyCollected = true;

        //the interrogation mark makes sure it doesn't show a null. REMEMBER THIS JOAB PLS
        Destroy(gameObject);

        return coinValue;
    }
}
