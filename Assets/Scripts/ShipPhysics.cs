using UnityEngine;
using System.Collections;

public class ShipPhysics : MonoBehaviour {

	//Movement
	//public float max_accel_rate = 200;
	public float max_speed = 200;

	public float accelerationRate = 120;
	public float deccelerationRate = 80;

	//Thrust Output
	float MAX_INDIVIDUAL_THRUST = 1;
	float leftOutput;
	float rightOutput;

	float outputDrag = 1.5f;
	float outputRate = 1f;

	bool isThrusting;

	public Transform leftThruster;
	public Transform rightThruster;

	public RSInputAdapter rsInputAdapter;

	float openingSpeedBoost = 100;
	
	public Transform[] clampPoints;
	float clampIntervalSeconds = .5f;
	// Use this for initialization
	void Start () {
		StartCoroutine (CR_WaitForFirstThrust ());
	}
	
	// Update is called once per frame
	void HandleUpdate () {
		isThrusting = false;

		if (Input.GetKey (KeyCode.LeftArrow)) {
			leftOutput = Mathf.Clamp(leftOutput + outputRate * Time.deltaTime, 0, MAX_INDIVIDUAL_THRUST);
			isThrusting = true;
		}
		else if(rsInputAdapter && rsInputAdapter.leftOutputNormalized > 0)
		{
			leftOutput = rsInputAdapter.leftOutputNormalized;
			isThrusting = true;
		}
		else
			leftOutput = Mathf.Clamp(leftOutput - outputDrag * Time.deltaTime, 0, MAX_INDIVIDUAL_THRUST);
		
		if (Input.GetKey (KeyCode.RightArrow)) {
			rightOutput = Mathf.Clamp(rightOutput + outputRate * Time.deltaTime, 0, MAX_INDIVIDUAL_THRUST);
			isThrusting = true;
		}
		else if(rsInputAdapter && rsInputAdapter.leftOutputNormalized > 0)
		{
			rightOutput = rsInputAdapter.rightOutputNormalized;		
			isThrusting = true;
		}
		else 
			rightOutput = Mathf.Clamp(rightOutput - outputDrag * Time.deltaTime, 0, MAX_INDIVIDUAL_THRUST);

		//Turning
		rigidbody.AddRelativeTorque(0, ( leftOutput - rightOutput ) * 200, 0);

		//Movement
		if (isThrusting) {
			float acceleration = (leftOutput + rightOutput) * .5f * accelerationRate * Time.deltaTime;
			rigidbody.velocity += transform.forward * (acceleration );
			Debug.Log(rigidbody.velocity.ToString() + "    " + acceleration);
		} 
	}

	IEnumerator CR_UpdateLoop()
	{
		while (true) {
			HandleUpdate();
			yield return 0;
		}
	}

	IEnumerator CR_WaitForFirstThrust()
	{
		while (!isThrusting) {
#if !UNITY_EDITOR_OSX
			if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || rsInputAdapter.leftHand.isTracking || rsInputAdapter.rightHand.isTracking)
#else
			if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
#endif
					isThrusting = true;

			yield return 0;	
		}
		Debug.Log ("ADSOJNE");
		StartCoroutine (CR_UpdateLoop ());
		//StartCoroutine (CR_OpeningSpeedBurst ());
	}

	IEnumerator CR_OpeningSpeedBurst()
	{
		openingSpeedBoost = 100;
		while (openingSpeedBoost > 0) {
			openingSpeedBoost -= accelerationRate * Time.deltaTime;
			yield return 0;
		}
	}


	//TODO add masking to ground clamp check
	IEnumerator CR_GroundClamp()
	{
		RaycastHit hit;
		float x, y, z = 0;
		while (true) {
				x = 0;
				y = 0;
				z = 0;
				foreach (Transform tf in clampPoints) {
						if (Physics.Raycast (transform.position, Vector3.down, out hit, 20)) {
								Debug.Log ("Hit " + hit.transform.name + "  " + hit.normal);
								x += hit.normal.x;
								y += hit.normal.y;
								z += hit.normal.z;
						}
				}
				x /= clampPoints.Length;
				y /= clampPoints.Length;
				z /= clampPoints.Length;
	
				//TODO orient myself to the average ground normal. Smooth transition
				transform.up = new Vector3 (x, y, z);
				yield return new WaitForSeconds (clampIntervalSeconds);
		}
	}
}
	