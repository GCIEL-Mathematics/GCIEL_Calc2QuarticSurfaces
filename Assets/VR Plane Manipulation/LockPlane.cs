using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LockPlane : MonoBehaviour
{
    public enum LockState
    {
        Off,
        XY,
        XZ,
        YZ
    };

    private XRGrabInteractable grabInteractable;
    private LockState current = LockState.Off;

    public void Start()
    {
        grabInteractable = this.gameObject.GetComponent<XRGrabInteractable>();
    }

    public void Lock()
    {
        int state = (int)current;
        state++;
        if (state > 3) state = 0;

        current = (LockState)state;

        if (current == LockState.Off)
        {
            grabInteractable.trackRotation = true;
            return;
        }

        grabInteractable.trackRotation = false;
        // todo: lock properly
    }
}
