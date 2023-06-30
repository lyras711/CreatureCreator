using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;
using UnityEngine.UI;
public class BakeStart : MonoBehaviour
{

    public Baker GenBaker;

    public Animator Anim;

    public bool StartRecord;

    Rigidbody[] rbs;

    public float Force;

    public InputField input;
    public Transform StopRecBtn;

    // Start is called before the first frame update
    void Start()
    {
        Anim = GetComponent<Animator>();
        rbs = GetComponentsInChildren<Rigidbody>();

        foreach(Rigidbody rb in rbs)
        {
            rb.isKinematic = true;

        }

    }

    // Update is called once per frame
    void Update()
    {

        CameraRaycast();

        if (Input.GetKeyDown(KeyCode.R))
        {
            GenBaker.StartBaking();
        }
        
    }

    void CameraRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, 100) )
            {
             
                if (hit.transform.GetComponent<Rigidbody>())
                {
                    AddForceToPoint(hit.transform.GetComponent<Rigidbody>(),10);
                    Debug.Log(hit.transform.name);
                    StopRecBtn.gameObject.SetActive(true);
                    Debug.Log("hit");

                }

            }
        }

    }

    void AddForceToPoint(Rigidbody part,float force)
    {
        Anim.enabled = false;
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;

        }
        GenBaker.StartBaking();
        part.AddForce((part.position - Camera.main.transform.position).normalized * Force, ForceMode.Impulse);



    }

    private void OnApplicationQuit()
    {
        GenBaker.StopBaking();
    }


    public void ChangeAnimationName()
    {
        input.gameObject.SetActive(false);
        GenBaker.saveName = input.text;

    }
    public void StopRec()
    {
        GenBaker.StopBaking();
    }


}
