using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

            GameObject mycube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mycube.transform.localScale = new Vector3((float)(1), (float)0.6, (float)(1 ));
            mycube.transform.localPosition = new Vector3((float)0.4, (float)0.4, 0);
            //mycube.tag = "Interactable";
            //Rigidbody gameObjectsRigidBody = mycube.AddComponent<Rigidbody>(); // Add the rigidbody.
            //gameObjectsRigidBody.mass = 5; // Set the mass to 5 via the Rigidbody.
            //mycube.AddComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
