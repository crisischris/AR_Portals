using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class PlanesManager : MonoBehaviour {

    public DoorManager doorManager;
    public UnityARGeneratePlane genPlane;

    // Update is called once per frame
    void Update()
    {
            if (doorManager.doorCount == 0)
            {
            genPlane.planePrefab.SetActive(true);
            }
            else
            {
            genPlane.planePrefab.SetActive(false);
            }
        }
}
