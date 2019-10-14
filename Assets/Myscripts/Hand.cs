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

    //related to showing question
    private Questions.QuestionSet active_q;
    private Questions q = new Questions(); // Get all the question strings
    private char[] Order;
    private string[] SortedQuestions = new string[6];
    public SteamVR_Action_Boolean m_gripAction = null;//press the side button to show question
    public GameObject QuestionText;//attached to the question text
    public GameObject Paper;
    public int counter;
    public string filename;
    public StudyTracker Tracker;

    private FixedJoint m_Joint = null;
    private SteamVR_Behaviour_Pose m_Pose = null;
    private Interactable m_CurrentInteractable = null;
    private List<Interactable> m_ContactInteractables = new List<Interactable>();
    private bool isCountry = false;
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
        //keyboard input
        if (Input.GetButtonDown("Fire3"))
        {
            ChangeQuestionText();
        }
    }

    void Start()
    {
        counter = 0;
        filename = Tracker.filename;
        /*Paper.GetComponent<MeshRenderer>().enabled = true;
        QuestionText.GetComponent<MeshRenderer>().enabled = true;*/
    }
    void ChangeQuestionText()
    {
        Debug.Log(SortedQuestions[counter]);
        QuestionText.GetComponent<TextMesh>().text = SortedQuestions[counter]; // change text from string[]
        counter = counter < 5 ? counter + 1 : 0; //change counter to cycle through questions
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
        Paper.GetComponent<MeshRenderer>().enabled = true;
        /*m_CurrentInteractable =QuestionText.gameObject.GetComponent<Interactable>();
        if (m_CurrentInteractable.m_ActiveHand)
            m_CurrentInteractable.m_ActiveHand.Drop();
        //change position,make the text with your controller
        m_CurrentInteractable.transform.position = transform.position + new Vector3((float)-0.1,0,0);
        Rigidbody targetBody = m_CurrentInteractable.GetComponent<Rigidbody>();
        m_Joint.connectedBody = targetBody;
        //Set active hand
        m_CurrentInteractable.m_ActiveHand = this;*/
    }

    public void dismiss()//dismiss question
    {
        QuestionText.GetComponent<MeshRenderer>().enabled = false;
        Paper.GetComponent<MeshRenderer>().enabled = false;
        /*if (!m_CurrentInteractable)
            return;
        m_Joint.connectedBody = null;

        //Clear
        m_CurrentInteractable.m_ActiveHand = null;
        m_CurrentInteractable = null;*/
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

        Transform[] ts = m_CurrentInteractable.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts)
        {
        if (t.gameObject.name == "FloatCountries")
            {
                isCountry = true;
            }
            else if (t.gameObject.name == "FloatYears")
            {
                isCountry = false;
            }
        }
        GameObject.Find("Vis_Barchart").GetComponent<Vis>().createVis.Showfloatelements(true, isCountry);
        GameObject.Find("Vis_Barchart").GetComponent<Vis>().createVis.attachbars(m_CurrentInteractable.name);
        //Attach
        Rigidbody targetBOdy = m_CurrentInteractable.GetComponent<Rigidbody>();

        //below is the method that I want to freeze the specific motion axis of targetbody, but I failed
        //targetBOdy.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
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
        GameObject.Find("Vis_Barchart").GetComponent<Vis>().createVis.Showfloatelements(false, isCountry);
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

    void ParseOrder(string input)
    {
        if (input.Length == 6)
        {
            Order = input.ToCharArray();
            for (int i = 0; i < 6; i++)
            {
                if ((Order[i] == 'R') | (Order[i] == 'r'))
                {
                    SortedQuestions[i] = (i < 3) ? active_q.r1 : active_q.r2;
                }
                else if ((Order[i] == 'O') | (Order[i] == 'o'))
                {
                    SortedQuestions[i] = (i < 3) ? active_q.o1 : active_q.o2;
                }
                else if ((Order[i] == 'C') | (Order[i] == 'c'))
                {
                    SortedQuestions[i] = (i < 3) ? active_q.c1 : active_q.c2;
                }
            }
        }
        else
        {
            Debug.LogWarning("Order is not equal to 6");
        }
    }

    /// <summary> Sets the questions and filename, and finds the Question Board GameObjects
    /// <remarks> Far from the best way to accomplish this, but it did the job</remarks>
    /// </summary>
	public void setQuestionTrigger(string name)
    {
        filename = name;//set the filename
        GameObject Paper = GameObject.Find("Paper"); //get the paper object
        GameObject Question = GameObject.Find("Question");//get the question text
        Debug.Log(Paper);
        switch (filename)//Just grab the one string[] based on filename
        {
            case "co2":
                active_q = q.co2;
                break;
            case "education":
                active_q = q.education;
                break;
            case "grosscapital":
                active_q = q.grosscapital;
                break;
            case "health":
                active_q = q.health;
                break;
            case "homicide":
                active_q = q.homicide;
                break;
            case "suicide":
                active_q = q.suicide;
                break;
            case "agriculturalland":
                active_q = q.agriculturalland;
                break;
            case "military":
                active_q = q.military;
                break;
            case "carmortality":
                active_q = q.carmortality;
                break;
            default:
                Debug.Log("No filename/ filename doesn't correspond to any 'Questions'.");
                break;
        }
        Debug.Log("QO " + Tracker.QuestionOrder);
        ParseOrder(Tracker.QuestionOrder);

    }
}




