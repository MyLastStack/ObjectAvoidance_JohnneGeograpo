using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    [SerializeField] List<GameObject> hunters;
    [SerializeField] List<GameObject> powerups;
    [SerializeField] List<GameObject> collectible;

    Vector3 finalVector;
    float moveSpeed = 2f;
    float rotateSpeed = 1000f;

    void Start()
    {

    }

    void Update()
    {
        Vector3 attractionVector = CalculateAttractionVector();

        if (attractionVector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(attractionVector, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            transform.Translate(attractionVector.normalized * Time.deltaTime * moveSpeed, Space.World);
        }
        else
        {
            Vector3 combinedThreatVector = CalculateCombinedThreatVector();

            if (hunters.Count != 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(combinedThreatVector, transform.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            }

            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        }
    }

    #region OnCollision
    private void OnCollisionEnter(Collision collision)
    {
        
    }
    #endregion

    #region OnTrigger
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
    #endregion

    Vector3 CalculateCombinedThreatVector()
    {
        Vector3 combinedThreatVector = Vector3.zero;

        foreach (GameObject hunter in hunters)
        {
            Vector3 threatVector = transform.position - hunter.transform.position;
            combinedThreatVector += threatVector.normalized;
        }

        return combinedThreatVector.normalized;
    }

    Vector3 CalculateAttractionVector()
    {
        GameObject nearestPU = GetNearestPU();

        if (nearestPU != null)
        {
            Vector3 attractionVector = nearestPU.transform.position - transform.position;
            return attractionVector.normalized;
        }

        return Vector3.zero;
    }

    GameObject GetNearestPU()
    {
        GameObject nearestPU = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject powerup in powerups)
        {
            float distance = Vector3.Distance(transform.position, powerup.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPU = powerup;
            }
        }

        return nearestPU;
    }
}
