using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    [SerializeField] List<GameObject> hunters;
    [SerializeField] List<GameObject> powerups;

    public Vector3 avoidAngle;
    float moveSpeed = 2f;
    float rotateSpeed = 1000f;

    void Start()
    {

    }

    void Update()
    {
        AngleFinder();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (rb.tag == "Hunter")
            {
                hunters.Add(other.gameObject);
            }
            if (rb.tag == "PowerUp")
            {
                powerups.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (rb.tag == "Hunter")
            {
                for (int i = 0; i < hunters.Count; i++)
                {
                    if (other.gameObject == hunters[i])
                    {
                        hunters.RemoveAt(i);
                    }
                }
            }
            if (rb.tag == "Collectible")
            {
                for (int i = 0; i < powerups.Count; i++)
                {
                    if (other.gameObject == powerups[i])
                    {
                        powerups.RemoveAt(i);
                    }
                }
            }
        }
    }

    void AngleFinder()
    {
        Vector3 combinedThreatVector = Vector3.zero;

        foreach (var hunter in hunters)
        {
            Vector3 threatVector = transform.position - hunter.transform.position;
            combinedThreatVector += threatVector.normalized;
        }

        Vector3 escapeVector = -combinedThreatVector.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(escapeVector, transform.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        transform.Translate(-escapeVector * Time.deltaTime * moveSpeed, Space.World);
    }

    Vector3 CalculateAttractionVector()
    {
        GameObject nearestCollectible = GetNearestCollectible();

        if (nearestCollectible != null)
        {
            Vector3 attractionVector = nearestCollectible.transform.position - transform.position;
            return attractionVector.normalized;
        }

        return Vector3.zero;
    }

    GameObject GetNearestCollectible()
    {
        GameObject nearestCollectible = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject collectible in powerups)
        {
            float distance = Vector3.Distance(transform.position, collectible.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCollectible = collectible;
            }
        }

        return nearestCollectible;
    }
}
