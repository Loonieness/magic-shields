using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    //to check the aim position
    [SerializeField] private InputReader inputReader;

    //to modify the turrets object
    [SerializeField] private Transform turretTransform;

    //happens after normal updates so it won't jitter or have the bullet shot at weird angles
    private void LateUpdate() {
        if( !IsOwner ) { return; }

        //reads from the AimPosition, usually mouse X Y coordinates on screen
        Vector2 aimScreenPosition = inputReader.AimPosition;   

        //a method from Camera. Shows where the cursor in the world is using the imputs from inputReader 
        Vector2 aimWoldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);

        //from the edges of the world to where the turret is, subtrack, the middle is where the cursor is
        turretTransform.up = new Vector2(
            aimWoldPosition.x - turretTransform.position.x,
            aimWoldPosition.y - turretTransform.position.y);
    }
}
