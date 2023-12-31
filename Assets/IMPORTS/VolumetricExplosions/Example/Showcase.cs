using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Showcase : MonoBehaviour {


	public float timescale = 1;
	void Start(){
		ParticleSystem[] particles;
		particles = GetComponentsInChildren<ParticleSystem> ();
		foreach (ParticleSystem p in particles) {
			var m = p.main;
			m.loop = true;
			m.simulationSpeed = timescale;
		}
	
	}

	public bool setName = false;
	// Update is called once per frame
	void OnValidate () {
		if (GetComponentInChildren<explosionScript> ()) {
			GetComponentInChildren<explosionScript> ().enabled = false;
			string n = GetComponentInChildren<explosionScript> ().gameObject.name;
			GetComponentInChildren<Text> ().text = n;
			this.gameObject.name = "Showcase: " + n;
			setName = false;
		}
		else {
			GetComponentInChildren<Text> ().text = "Empty";
			setName = false;
		}
	}




}
