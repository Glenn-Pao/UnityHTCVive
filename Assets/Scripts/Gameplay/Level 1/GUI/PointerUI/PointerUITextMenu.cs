﻿using UnityEngine;
using System.Collections;

public class PointerUITextMenu : MonoBehaviour
{
    public string message = "Hello World";
    public float f_Time = 3f;
    public Vector3 v3_Offset = new Vector3(0f, 0.065f, 0f);
    public PointerUIReceiver UI_InteractTrigger;
    public HologramTextWorld HTW_HologramTextWorld;
    

    // Use this for initialization
    void Start()
    {
        if (UI_InteractTrigger == null)
        {
            if (this.GetComponent<PointerUIReceiver>() != null)
            {
                UI_InteractTrigger = this.GetComponent<PointerUIReceiver>();
            }
            else
            {
                UI_InteractTrigger = this.gameObject.AddComponent<PointerUIReceiver>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (UI_InteractTrigger.Interacted())
        {
            HTW_HologramTextWorld.setMessage(this.transform.position + v3_Offset, message, f_Time);
        }
    }
}
