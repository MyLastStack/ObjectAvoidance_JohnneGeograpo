using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HunterMovement : MonoBehaviour
{
    [SerializeField] GameObject prey;

    [SerializeField] float movementSpeed = 2.0f, forwardDist = 1.0f, sideDist = 3.0f;

    bool isLeft, isRight;

    RaycastHit hit;

    void Start()
    {
        
    }

    void Update()
    {
        transform.LookAt(prey.transform.position);
        transform.Translate(Vector3.forward * Time.deltaTime * movementSpeed);
    }
    void AvoidWalls()
    {
        if (Physics.BoxCast(transform.position, new Vector3(0.5f, 0.5f, 0.5f), transform.forward, out hit, Quaternion.identity, forwardDist))
        {
            if (hit.transform.gameObject.tag == "Wall")
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
}
