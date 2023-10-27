using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CameraController : MonoBehaviour
{
	// scale 10*10 기준 range 35f
	public float range = 80.0f;
	private float minDistance = 0.45f;
	private float maxDistance = 0.65f;
	
	private float maxHeight = -62f; // default -64.15 
	private float minHeight = -73.3f;

	private GameObject _player;
	private float initHeight;
	
	// Use this for initialization
    void Start ()
    {
	    _player = GameObject.FindWithTag("Player");
	    initHeight = Vector3.Distance(
		    _player.GetComponent<XROrigin>().Camera.transform.position, 
		    SphericaiWorld.Instance.transform.position);
	    
	    
	    Vector3 start = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z);
	    this.transform.position = start;
    }

    public bool CamRangeCheck()
    {
	    // x, z축의 범위를 벗어난다면
	    Vector2 dis = new Vector2(this.transform.position.x, this.transform.position.z);
	    //Debug.Log(dis.sqrMagnitude);
	    if (range * range < dis.sqrMagnitude)
	    {
		    Vector3 newPos = new Vector3(-this.transform.position.x,
			    this.transform.position.y,
			    -this.transform.position.z);
		    
		    this.transform.position = newPos;
		    //Debug.Log("Camera Pos Change");

		    return false;
	    }

	    return true;
    }

    public void CamHeight()
    {
	    float curDistance = Vector3.Distance(
		    _player.GetComponent<XROrigin>().Camera.transform.position,
		    SphericaiWorld.Instance.transform.position);

	    float ratio = (curDistance - minDistance) / (maxDistance - minDistance);
	    float height = Mathf.Lerp(minHeight, maxHeight, ratio);

	    Vector3 start = new Vector3(transform.position.x, height, transform.position.z);
	    this.transform.position = start;
    }

    // Update is called once per frame
	void Update () 
	{
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 100.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 100.0f;

        transform.Translate(x, 0, 0);
        transform.Translate(0, 0, z);
	}

	void LateUpdate()
	{
		//CamRangeCheck();
		CamHeight();
	}
}
