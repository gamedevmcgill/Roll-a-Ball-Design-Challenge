using UnityEngine;
using System.Collections;

public class BobAndRotate : MonoBehaviour {

    private float bobTime = 0;
    [Tooltip(
        "How much the object bobs. Greater values mean more bobbing."
    )]
    public float bobMultiplier = 1f;
    float yPos;

    [Tooltip(
        "If true, the object spins."
    )]
    [SerializeField] bool spin = true;
    [Tooltip(
        "If true, the object bobs."
    )]
    [SerializeField] bool bob = true;

    void Start()
    {
        if (spin)
        {        
        // transform.Rotate(new Vector3 (0, Random.Range(0, 360), 0), Space.World);
        transform.Rotate(new Vector3 (0, Mathf.Sin((gameObject.transform.localPosition[0] + gameObject.transform.localPosition[2]) / 5f) * 180, 0), Space.World); // I like it when things are nice and geometric looking
        }

        if (bob)
        {
        yPos = transform.localPosition[1];
        bobTime = Mathf.Sin((gameObject.transform.localPosition[0] + gameObject.transform.localPosition[2]) / 5f); // lines of collectibles have their bob offset by their location so they appear to bob in a wave pattern
        }
    
    }
	// Before rendering each frame..
	void Update () 
	{
        if (spin)
        {
		transform.Rotate(new Vector3 (0, 90, 0) * Time.deltaTime, Space.World);
        }
        
        if (bob)
        {
        transform.localPosition = new Vector3(transform.localPosition[0], yPos + bobMultiplier * ( 0.4f * Mathf.Sin(bobTime) ), transform.localPosition[2]);
        bobTime += Time.deltaTime;
        }
	}
}	