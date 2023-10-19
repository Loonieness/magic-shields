using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TMP_Text playerNameText;

    private void Start()
    {
        //forces the name to be read even before any change
        HandlePlayerNameChanged(string.Empty, player.PlayerName.Value);

        //whenever the playerName changes, does this function
        player.PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        playerNameText.text = newName.ToString();
    }

    private void OnDestroy(){
        player.PlayerName.OnValueChanged -= HandlePlayerNameChanged;

    }

    
}
