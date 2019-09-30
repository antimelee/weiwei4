using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Input : MonoBehaviour
{
    // Start is called before the first frame update
    private SteamVR_Action_Boolean m_Teleport = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");

    private SteamVR_Action_Vector2 m_Touch = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TouchpadTouch");

    private GameObject Testgameobjct;

    // Update is called once per frame
    void Update()
    {
        if(m_Teleport.GetStateDown(SteamVR_Input_Sources.Any)&&Testgameobjct!=null)
        {
            Debug.Log(m_Touch.GetAxis(SteamVR_Input_Sources.Any));
            Testgameobjct.transform.parent = transform;
            //Testgameobjct.transform.position = transform.position;
            Rigidbody rig = Testgameobjct.GetComponent<Rigidbody>();
            rig.useGravity = false;
            rig.isKinematic = true;
        }

        if (m_Teleport.GetStateUp(SteamVR_Input_Sources.Any) && Testgameobjct != null)
        {
            //Debug.Log(m_Touch.GetAxis(SteamVR_Input_Sources.Any));
            Testgameobjct.transform.parent = null;
           // Testgameobjct.transform.position = new Vector3(0, 0, 0);
            Rigidbody rig = Testgameobjct.GetComponent<Rigidbody>();
            rig.useGravity = true;
            rig.isKinematic = false;
        }
    }

}
