using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;

// This class shows and hides doors (aka portals) when you walk into them. It listens for all OnPortalTransition events
// and manages the active portal.
public class DoorManager : MonoBehaviour
{
    public delegate void DoorAction(Transform door);
    public static event DoorAction OnDoorOpen;

    public Vector3 cameraPos;

    public Quaternion rotationFlip1;
    public Quaternion rotationFlip2;

    public GameObject[] doorToVirtual;
    public GameObject[] desitnations;
    public GameObject doorToReality;
    public GameObject scanText;
    public GameObject planeText;
    public GameObject ballThrower;
    public GameObject ball;
    public GameObject particle;
    private GameObject currDoor;

    private AudioSource audioSourceBall;
    private AudioSource audioSourcePortal;
    public AudioClip throwSound;
    public AudioClip portalSound;

    public Camera mainCamera;

    public int i = 0;
    public int doorCount;
    public float j = 0;
    public int k = 0;
    public float currDoorYRotation;

    public bool ballInAir = false;
    private bool isCurrDoorOpen = false;
    private bool isRealityDoorOpen = false;
    private bool isNextDoorVirtual = true;
    private bool setDoorAnimation = false;
    private bool setDoorRise = false;



    void Start()
    {
        PortalTransition.OnPortalTransition += OnDoorEntrance;
        rotationFlip1 = Quaternion.Euler(0, 0, 0);
        rotationFlip2 = Quaternion.Euler(0, 180, 0);
        audioSourceBall = ball.GetComponent<AudioSource>();
        audioSourcePortal = GetComponent<AudioSource>();

    }

    void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            if(isRealityDoorOpen == false)
            OpenDoorInFront(Input.mousePosition);
        }


        GameObject[] planeObjects = GameObject.FindGameObjectsWithTag("plane");
        for (k = 0; k < planeObjects.Length; k++)
        {
            if (planeObjects[k].activeInHierarchy && isCurrDoorOpen == false)
            {
                scanText.SetActive(false);
                planeText.SetActive(true);
            }

            else
            {
                planeText.SetActive(false);
            }
        }

      

        cameraPos = mainCamera.transform.position;


        // ANIMATE DOOR //

        if (setDoorAnimation == true)
        {
            currDoor.transform.localScale *= 1.075f;
        }

        if (setDoorRise == true)
        {
            Vector3 risePos = currDoor.transform.position;
            risePos.y += .045f;
            currDoor.transform.position = risePos;
        }


        // SET ACTIVE UI //`

        if (isRealityDoorOpen == true)
        {
            scanText.SetActive(false);
            planeText.SetActive(false);
            ballThrower.SetActive(false);
        }



        if (isCurrDoorOpen == true && ballInAir == false)
        {
            ballThrower.SetActive(true);
        }
        else
        {
            ballThrower.SetActive(false);
        }
    }

    public void OpenDoorInFront(Vector2 point)
    {
        RaycastHit hit;

        if (Physics.Raycast(mainCamera.ScreenPointToRay(point), out hit))
        {
            if (!isCurrDoorOpen)
            {
                if (i < doorToVirtual.Length)
                {
                    currDoor = doorToVirtual[i];
                    i++;
                }

                else
                {
                    i = 0;
                    currDoor = doorToVirtual[i];
                }

                spawnDoor();
                StartCoroutine(growDoor());
                currDoor.transform.position = hit.point;
                currDoor.transform.rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(currDoor.transform.position - mainCamera.transform.position, Vector3.up));
                Vector3 yAdjusted = new Vector3(currDoor.transform.position.x, currDoor.transform.position.y + .5f, currDoor.transform.position.z);
                currDoor.GetComponentInParent<Portal>().Source.transform.localPosition = yAdjusted;  //currDoor.transform.position;
                StartCoroutine(levitate());
                isCurrDoorOpen = true;

                if (OnDoorOpen != null)
                {
                    OnDoorOpen(currDoor.transform);
                }
            }
        }
    }




    // Respond to the player walking into the doorway. Since there are only two portals, we don't need to pass which
    // portal was entered.
    private void OnDoorEntrance()
    {
        if (doorCount == 0)
        {
            Vector3 realDoorPos = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + .5f, mainCamera.transform.position.z);
            Quaternion matchAngles = currDoor.transform.localRotation;
            currDoor.SetActive(false);
            isRealityDoorOpen = true;
            doorToReality.SetActive(true);
            Collider sc = mainCamera.GetComponentInChildren<SphereCollider>();
            sc.enabled = false;
            doorToReality.transform.position = realDoorPos;
            doorToReality.transform.localRotation = matchAngles;
            StartCoroutine((turnOnColliderLong(mainCamera)));
            GameObject poof = GameObject.FindWithTag("poof");
            Destroy(poof);
            isCurrDoorOpen = false;
            doorCount = 1;
        }

        else
        {
            isRealityDoorOpen = false;
            doorToReality.SetActive(false);
            doorCount = 0;
        }
       // ResetScene();

    }

    public void throwBall()
    {
        audioSourceBall.PlayOneShot(throwSound);
        Collider sc = mainCamera.GetComponentInChildren<SphereCollider>();
        ballInAir = true;
        sc.enabled = false;
        ball.SetActive(true);
        ball.transform.position = cameraPos;
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.mass = 1.1f;
        rb.velocity = mainCamera.transform.forward * 5;
        rb.angularVelocity = mainCamera.transform.forward * 5;
        StartCoroutine((turnOnCollider(mainCamera)));
    }

    void spawnDoor()
    {
        audioSourcePortal.PlayOneShot(portalSound);
        currDoor.SetActive(true);
        GameObject newParticle = Instantiate(particle, currDoor.transform.position, Quaternion.identity);
        newParticle.tag = "poof";
    }

  /*  public void ResetScene()
    {
        ARKitWorldTrackingSessionConfiguration sessionConfig = new ARKitWorldTrackingSessionConfiguration(UnityARAlignment.UnityARAlignmentGravity, UnityARPlaneDetection.Horizontal);
       UnityARSessionNativeInterface.GetARSessionNativeInterface().RunWithConfigAndOptions(sessionConfig, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking);   
    }*/

    IEnumerator turnOnCollider(Camera camera)
    {
        Collider sc = camera.GetComponentInChildren<SphereCollider>();
        yield return new WaitForSeconds(.25f);
        sc.enabled = true;
        ballInAir = false;
    }

    IEnumerator turnOnColliderLong(Camera camera)
    {
        Collider sc = camera.GetComponentInChildren<SphereCollider>();
        yield return new WaitForSeconds(1);
        sc.enabled = true;
    }

    IEnumerator growDoor()
    {
        setDoorAnimation = true;
        yield return new WaitForSeconds(.5f);
        setDoorAnimation = false;
    }

    IEnumerator levitate()
    {
        setDoorRise = true;
        yield return new WaitForSeconds(.5f);
        setDoorRise = false;
    }
}