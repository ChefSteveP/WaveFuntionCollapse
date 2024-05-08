using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bob : MonoBehaviour
{
    private float yPos;
    [SerializeField]
    private float slow;
    [SerializeField]
    private float size;
    private float totalTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        yPos = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        transform.position = new Vector3(transform.position.x, yPos + Mathf.Sin(totalTime / slow) * size, transform.position.z);
    }
}
