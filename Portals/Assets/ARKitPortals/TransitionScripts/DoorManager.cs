using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject portalOpener;
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
    public float currDoorYRotation;
   
    public bool ballInAir = false;
    private bool isCurrDoorOpen = false;
    private bool isRealityDoorOpen = false;
    private bool isNextDoorVirtual = true;
    private bool setDoorAnimation = false;


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
        cameraPos = mainCamera.transform.position;

        if (setDoorAnimation == true)
        {
            currDoor.transform.localScale *= 1.075f;
        }

        if (isRealityDoorOpen == true)
        {
            portalOpener.SetActive(false);
        }

        else
        {
            portalOpener.SetActive(true);
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

    // This method is called from the Spawn Portal button in the UI. It spawns a portal in front of you.
    public void OpenDoorInFront()
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

            StartCoroutine((spawnDoor()));
            currDoor.transform.position = (Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up)).normalized
                + mainCamera.transform.position;
            currDoor.transform.rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(currDoor.transform.position - mainCamera.transform.position, Vector3.up));
            currDoor.GetComponentInParent<Portal>().Source.transform.localPosition = currDoor.transform.position;
            isCurrDoorOpen = true;

            if (OnDoorOpen != null)
            {
                OnDoorOpen(currDoor.transform);
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
            //Quaternion adjustMatchAngles = Quaternion.Euler(matchAngles.x, matchAngles.y-180, matchAngles.z);
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

    IEnumerator spawnDoor()
    {
        audioSourcePortal.PlayOneShot(portalSound);
        currDoor.SetActive(true);
        GameObject newParticle = Instantiate(particle, currDoor.transform.position, Quaternion.identity);
        newParticle.tag = "poof";
        setDoorAnimation = true;
        yield return new WaitForSeconds(.5f);
        setDoorAnimation = false;
    }
}