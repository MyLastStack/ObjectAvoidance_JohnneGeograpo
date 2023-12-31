using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    public List<GameObject> preys;

    [SerializeField] float movementSpeed = 7.0f, forwardDist = 1.0f, sideDist = 3.0f;
    float rotateSpeed = 1000f;

    bool isLeft, isRight;

    RaycastHit hit;

    public bool hasTriggered = false;

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 attractionVector = CalculateAttractionVector();

        AvoidWalls();

        if (attractionVector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(attractionVector, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            transform.Translate(attractionVector.normalized * Time.deltaTime * movementSpeed * 2.5f, Space.World);
        }
        else
        {
            transform.Translate(Vector3.forward * Time.deltaTime * movementSpeed * 2.5f);
        }
    }

    #region OnCollision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Prey")
        {
            hasTriggered = false;
        }
    }
    #endregion

    #region OnTrigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Prey"))
        {
            if (LineOfSight(other.transform))
            {
                if (!hasTriggered)
                {
                    preys.Add(other.gameObject);
                    hasTriggered = true;
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (rb.tag == "Prey")
            {
                for (int i = 0; i < preys.Count; i++)
                {
                    preys.RemoveAt(i);
                    hasTriggered = false;
                }
            }
        }
    }
    #endregion

    void AvoidWalls()
    {
        if (Physics.BoxCast(transform.position, new Vector3(0.5f, 0.5f, 0.5f), transform.forward, out hit, Quaternion.identity, forwardDist))
        {
            if (hit.transform.gameObject.tag == "Wall" || hit.transform.gameObject.tag == "Hunter")
            {

                // Rotate based on what is to the sides
                isLeft = Physics.Raycast(transform.position, -transform.right, sideDist);
                isRight = Physics.Raycast(transform.position, transform.right, sideDist);

                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, -hit.normal, 1, 1));

                if (isLeft && isRight)
                {
                    transform.Rotate(Vector3.up, 180);
                }
                else if (isLeft && !isRight)
                {
                    transform.Rotate(Vector3.up, 90);
                }
                else if (!isLeft && isRight)
                {
                    transform.Rotate(Vector3.up, -90);
                }
                else
                {
                    if (Random.Range(1, 3) == 1)
                    {
                        transform.Rotate(Vector3.up, 90);
                    }
                    else
                    {
                        transform.Rotate(Vector3.up, -90);
                    }
                }
            }
        }
    }

    Vector3 CalculateAttractionVector()
    {
        GameObject nearestPrey = GetNearestPrey();

        if (nearestPrey != null)
        {
            Vector3 attractionVector = nearestPrey.transform.position - transform.position;
            return attractionVector.normalized;
        }

        return Vector3.zero;
    }

    GameObject GetNearestPrey()
    {
        GameObject nearestPU = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject prey in preys)
        {
            float distance = Vector3.Distance(transform.position, prey.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPU = prey;
            }
        }

        return nearestPU;
    }

    bool LineOfSight(Transform target)
    {
        RaycastHit hit;
        Vector3 direction = target.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out hit, 5.0f))
        {
            if (hit.transform.CompareTag("Prey") && !hit.transform.GetComponent<AIMovement>().invis)
            {
                return true;
            }
        }

        return false;
    }
}
