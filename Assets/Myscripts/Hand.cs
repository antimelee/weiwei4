using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Hand : MonoBehaviour
{
    public SteamVR_Action_Boolean m_pinchAction = null;//grab things by pinching

    public SteamVR_Action_Boolean m_touchpadAction = null;//teleport

    //for teleporting
    public GameObject m_Pointer=null;
    public bool isteleporting = true;
    private bool m_HasPosition = false;
    private bool m_IsTeleporting = false;
    private float m_FadeTime = 0.5f;
    //my own staff
    public SteamVR_Action_Boolean m_gripAction = null;//press the side button to show question
    public GameObject QuestionText;//attached to the question text
    public GameObject Paper;

    
    private FixedJoint m_Joint = null;
    private SteamVR_Behaviour_Pose m_Pose = null;
    private Interactable m_CurrentInteractable = null;
    private List<Interactable> m_ContactInteractables = new List<Interactable>();
    
    //my own staff
    [HideInInspector]
    Vector3 barposition = new Vector3();//record the position of bar which you pick up
    [HideInInspector]
    Quaternion barrotation;//record the rotation of bar which you pick up

    private void Awake()
    {
        m_Pose = GetComponent<SteamVR_Behaviour_Pose>();
        m_Joint = GetComponent<FixedJoint>();
        QuestionText.GetComponent<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        //pointer function       
        if (isteleporting)
        {
            m_Pointer.SetActive(m_HasPosition);
            m_HasPosition = UpdatePointer();
        }
            //pinch Down
            if (m_pinchAction.GetStateDown(m_Pose.inputSource))
        {
            //print(m_Pose.inputSource + "Trigger Down");

            if(m_CurrentInteractable != null)
            {
                m_CurrentInteractable.Action();
                return;
            }

            Pickup();
        }
        if(m_pinchAction.GetStateUp(m_Pose.inputSource))
        {

            Drop();
        }

        if (m_touchpadAction.GetStateUp(m_Pose.inputSource)&&isteleporting)
        {

            TryTeleport();
        }
        //pinch Up


        //grip Down
        if (m_gripAction.GetStateDown(m_Pose.inputSource))
        {
            //print(m_Pose.inputSource + "grip dwon");
            showup();
        }

        //grip Up
        if (m_gripAction.GetStateUp(m_Pose.inputSource))
        {
            //print(m_Pose.inputSource + "dismiss");
            dismiss();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
        {
            return;
        }

        m_ContactInteractables.Add(other.gameObject.GetComponent<Interactable>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
        {
            return;
        }

        m_ContactInteractables.Remove(other.gameObject.GetComponent<Interactable>());
    }

    public void showup()//show question
    {
        QuestionText.GetComponent<MeshRenderer>().enabled = true;
        m_CurrentInteractable=QuestionText.gameObject.GetComponent<Interactable>();
        if (m_CurrentInteractable.m_ActiveHand)
            m_CurrentInteractable.m_ActiveHand.Drop();
        //change position,make the text with your controller
        m_CurrentInteractable.transform.position = transform.position + new Vector3((float)-0.1,0,0);
        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        m_Joint.connectedBody = targetBody;
        //Set active hand
        m_CurrentInteractable.m_ActiveHand = this;
    }

    public void dismiss()//dismiss question
    {
        QuestionText.GetComponent<MeshRenderer>().enabled = false;
        if (!m_CurrentInteractable)
            return;
        m_Joint.connectedBody = null;

        //Clear
        m_CurrentInteractable.m_ActiveHand = null;
        m_CurrentInteractable = null;
    }

    public void Pickup()
    {
            //Get nearest
        m_CurrentInteractable = GetNearestInteractable();
        //Null check
        if (!m_CurrentInteractable)
            return;
        //Already held, check
        if (m_CurrentInteractable.m_ActiveHand)
            m_CurrentInteractable.m_ActiveHand.Drop();
        //position
        //m_CurrentInteractable.transform.position = transform.position;
        
        //Attach
        Rigidbody targetBOdy = m_CurrentInteractable.GetComponent<Rigidbody>();
        targetBOdy.isKinematic = true;
        barposition = targetBOdy.transform.position;
        barrotation = targetBOdy.transform.rotation;
        //m_CurrentInteractable.ApplyOffset(transform);
        //m_Joint.connectedBody = targetBOdy;
        targetBOdy.transform.parent = transform;
        //Set active hand
        m_CurrentInteractable.m_ActiveHand = this;
        
    }

    public void Drop()
    {
        //Null check
       if (!m_CurrentInteractable)
            return;

        //Apply velocity
        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        //targetBody.velocity = m_Pose.GetVelocity();
        //targetBody.angularVelocity = m_Pose.GetAngularVelocity();
        targetBody.isKinematic = true;
        targetBody.transform.position = barposition;
        targetBody.transform.rotation = barrotation;
        //Detach
        //m_Joint.connectedBody = null;
        targetBody.transform.parent = null;
        //Clear
        m_CurrentInteractable.m_ActiveHand = null;
        m_CurrentInteractable = null;
        
    }

    private Interactable GetNearestInteractable()
    {
        Interactable nearest = null;
        float minDistance = float.MaxValue;
        float distance = 0.0f;

        foreach (Interactable interactable in m_ContactInteractables)
        {
            distance = (interactable.transform.position - transform.position).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = interactable;
            }
        }
        //print("get the nearest");//just for test
        return nearest;
    }

    private void TryTeleport()
    {
        //check for valid position, and if already teleporting
        if (!m_HasPosition || m_IsTeleporting)
            return;

        //Get camera rig,and head position
        Transform camerarig = SteamVR_Render.Top().origin;
        Vector3 headPosition = SteamVR_Render.Top().head.position;

        //Figure out translation
        Vector3 groundPosition = new Vector3(headPosition.x, camerarig.position.y, headPosition.z);
        Vector3 translateVector = m_Pointer.transform.position - groundPosition;

        //move
        StartCoroutine(MoveRig(camerarig, translateVector));
    }

    private IEnumerator MoveRig(Transform cameraRig,Vector3 translation)
    {
        //Flag
        m_IsTeleporting = true;

        //Fade to black
        SteamVR_Fade.Start(Color.black, m_FadeTime, true);

        //Apply translation
        yield return new WaitForSeconds(m_FadeTime);
        cameraRig.position += translation;

        //Fade to clear
        SteamVR_Fade.Start(Color.clear, m_FadeTime, true);

        //De-flag
        m_IsTeleporting = false;
    }

    private bool UpdatePointer()
    {
        //ray from controller
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        //if it`s a hit
        if(Physics.Raycast(ray,out hit))
        {
            m_Pointer.transform.position = hit.point;
            return true;
        }

        //if not a hit
        return false;
    }
}




