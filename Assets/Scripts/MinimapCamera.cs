using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private float yoffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = PlayerTransform.position;
        targetPosition.y += yoffset;
        transform.position = targetPosition;
    }
}
