using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

//right clicking and selecting "create", this object will be an option appearing now
[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]

//not monobehavior because I want other objects to access this, such as movement and firing
public class InputReader : ScriptableObject, IPlayerActions
{

     public event Action<Vector2> MoveEvent;

    //action is a trigger and sets what happens if it activates. This one turns autofire on and off (bool)
    public event Action<bool> PrimaryFireEvent;

    //since movement and shoot is just one trigger for movement, but aiming is always happening, it's best to just be a 
    //variable instead of a continous event. More performatic
    public Vector2 AimPosition { get; private set; }

    //references the Controls on the Input folder
    private Controls controls;

    //makes the game see the other methods by creating an instance
    private void OnEnable() {

        //if it hasn't been setup already
        if(controls == null) {
            controls = new Controls();
            //Player was created as a control scheme and it uses this same object on the method bellow
            controls.Player.SetCallbacks(this);
        }
        //enables the controll Scheme called Player that was created
        controls.Player.Enable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());  
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        //this is a bool, and says if the button is pressed
        //the question mark makes sure that the Invoke only occurs with a Listener, or else a null would cause issues
        if (context.performed){
            PrimaryFireEvent?.Invoke(true);
        }else{
            PrimaryFireEvent?.Invoke(false);
        }
        
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimPosition = context.ReadValue<Vector2>();
    }
}
