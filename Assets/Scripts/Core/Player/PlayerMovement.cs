using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{   
    //just to separate things on the editor
    [Header("References")]
    //references the InputReader thing and allows this to read input (WASD and arrows mostly, I think)
    [SerializeField] private InputReader inputReader;

    //references the Transform, used to make changes on the player's body
    [SerializeField] private Transform bodyTransform;

    //references the body of the player, which is a rigidBody2d
    [SerializeField] private Rigidbody2D rb;



    [Header("Settings")]
    //references the speed of the player
    [SerializeField] private float movementSpeed = 4f;

    //references the speed in which the player can turn
    [SerializeField] private float turningRate = 30f;


    public override void OnNetworkSpawn(){
        if (!IsOwner) { return; }
        inputReader.MoveEvent += HandleMove;

    } 

    private Vector2 previousMovementInput;

    public override void OnNetworkDespawn(){
        if (!IsOwner) { return; }
        inputReader.MoveEvent -= HandleMove;

    } 


    private void Update()
    {
        if (!IsOwner) { return; }

        //the x is because we only wanna use the horizontal movement A and D
        //the negative is just because it naturally will make you go left pressing D and right pressing A
        //Time.deltaTime makes it framerate independent. The velocity won't depend on the computer running, but the time-
        //-on the game itself.
        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    //doesn't happen every frame, only when physics are calculated. It goes in sync with it
    private void FixedUpdate() {
        if (!IsOwner) { return; }

        //rigidBody already comes with velocity
        //up is to not really move up in corelation with the scenario, but with the object. To wherever it's 'up' is
        //casting as Vector2 because up is Vector3 but we don't need the Z component, just X and Y
        //don't need to multiplicate with deltaTime, because 'velocity' tells how far to move per second, not frame
        rb.velocity = (Vector2)bodyTransform.up * previousMovementInput.y * movementSpeed;
    }

    //created just because the Input is not to be changed or handled directly. So we only take the information
    private void HandleMove(Vector2 movementInput){
        previousMovementInput = movementInput;
    }
}
