using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AIMovement : MonoBehaviour
{
    [SerializeField] float forwardDist = 1.0f, sideDist = 3.0f;

    [SerializeField] List<GameObject> hunters;
    [SerializeField] List<GameObject> powerups;
    [SerializeField] List<GameObject> collectables;
    [SerializeField] GameObject target;

    bool isLeft, isRight;
    RaycastHit hit;

    Vector3 finalVector;
    float moveSpeed = 7.0f;
    float rotateSpeed = 1000f;
    float buffUptime = 3.0f;

    void Start()
    {

    }

    void Update()
    {
        Vector3 attractionVector = CalculateAttractionVector();

        CheckTargets();
        AvoidWalls();

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
        if (collision.gameObject.tag == "Hunter")
        {
            gameObject.SetActive(false);
        }
        if (collision.gameObject.tag == "PowerUps")
        {
            if (collision.gameObject.name == "SpeedBoost")
            {
                moveSpeed = 12.0f;
                collision.gameObject.SetActive(false);
                SpeedBoosting();
            }
            else if (collision.gameObject.name == "Invisible")
            {

            }
        }
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

    void AvoidWalls()
    {
        if (Physics.BoxCast(transform.position, new Vector3(0.5f, 0.5f, 0.5f), transform.forward, out hit, Quaternion.identity, forwardDist))
        {
            if (hit.transform.gameObject.tag == "Wall" || hit.transform.gameObject.tag == "Prey")
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
    void CheckTargets()
    {
        float distance = 10000.0f;

        for (int i = 0; i < collectables.Count; i++)
        {
            if (Vector3.Distance(transform.position, collectables[i].transform.position) < distance)
            {
                distance = Vector3.Distance(transform.position, collectables[i].transform.position);
                target = collectables[i];
            }
        }

        if (target != null)
        {
            if (target.gameObject.activeSelf)
            {
                if (Physics.BoxCast(transform.position, new Vector3(0.5f, 0.5f, 0.5f), target.transform.position - transform.position, out hit, Quaternion.identity))
                {
                    if (hit.transform.tag != "Wall" || hit.transform.tag != "Prey")
                    {
                        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, target.transform.position - transform.position, 1, 1));
                    }

                    if (Vector3.Distance(target.transform.position, transform.position) < 2.0f)
                    {
                        target.SetActive(false);
                        target = null;
                        for (int i = 0; i < collectables.Count; i++)
                        {
                            if (target == collectables[i])
                            {
                                collectables.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            else
            {
                target = null;
            }
        }
    }

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

    IEnumerator SpeedBoosting()
    {
        yield return new WaitForSeconds(buffUptime);
        moveSpeed = 7.0f;
    }
}
