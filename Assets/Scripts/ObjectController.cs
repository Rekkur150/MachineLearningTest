using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    [Tooltip("The Object that will be cloned")]
    public GameObject masterObject;

    [Tooltip("The max number of objects")]
    public int numberOfObjects = 100;

    [Tooltip("This collider will be the range of which the objects will be dispersed")]
    public Collider coll;

    [Tooltip("The hierarchy location objects will be cloned to, default is this GameObject")]
    public GameObject cloneRepository = null;

    //List of objects
    private List<GameObject> objectList = new List<GameObject>();

    private int ActiveObjects = 0;

    // Start is called before the first frame update
    void Start()
    {

        if (cloneRepository == null)
        {
            cloneRepository = gameObject;
        }


        GenerateObjects();
        DistributeObjects();
    }

    public GameObject GetClosestObject(Transform trans)
    {
        GameObject closest = objectList[0];
        float closestDistance = Vector3.Distance(objectList[0].transform.position, trans.position);
        for (int i = 1; i < numberOfObjects; i++)
        {
            if (objectList[i].activeInHierarchy) {
                float tempDistance = Vector3.Distance(objectList[i].transform.position, trans.position);
                if (tempDistance < closestDistance)
                {
                    closestDistance = tempDistance;
                    closest = objectList[i];
                }
            }
        }

        return closest;
    }

    public void CollectedObject(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        obj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        obj.SetActive(false);
        ActiveObjects--;

        //RespawnObject();
    }

    public void MoveRandomObjectToLocation(Vector3 pos)
    {
        objectList[0].transform.position = pos;
    }

    private void RespawnObject()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            if (!objectList[i].activeInHierarchy)
            {
                DistributeObject(objectList[i]);
            }
        }
    }

    private void GenerateObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            objectList.Add(Instantiate(masterObject, cloneRepository.transform));
        }
    }

    public void DistributeObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            DistributeObject(objectList[i]);
        }
    }

    private void DistributeObject(GameObject obj)
    {

        obj.transform.position = new Vector3(
            Random.Range(coll.bounds.min.x, coll.bounds.max.x),
            Random.Range(coll.bounds.min.y, coll.bounds.max.y),
            Random.Range(coll.bounds.min.z, coll.bounds.max.z));
        obj.SetActive(true);
        ActiveObjects++;
    }

}

