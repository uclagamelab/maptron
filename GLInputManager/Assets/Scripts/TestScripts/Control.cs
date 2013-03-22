using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		
		transform.position += -NewCustomInputManager.Get().GetAxis( "Horizontal" ) * transform.right;
		transform.position += NewCustomInputManager.Get().GetAxis( "Vertical" ) * transform.forward;
		
		
	}
}
