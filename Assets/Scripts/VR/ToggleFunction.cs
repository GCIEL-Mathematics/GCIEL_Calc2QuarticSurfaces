using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ToggleFunction : MonoBehaviour
{
    public InputActionProperty AButton;

    bool AButtonPressed;

    public ChunkVolume chkVol;

    void Update(){
        if(AButton.action.triggered && !AButtonPressed) {
            AButtonPressed = true;
            chkVol.function = (chkVol.function + 1) % 8;
            chkVol.Render();
        } else{
            AButtonPressed = false;
        }
    }
}
