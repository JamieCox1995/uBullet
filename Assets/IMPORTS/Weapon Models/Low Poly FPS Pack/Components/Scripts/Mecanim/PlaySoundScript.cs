﻿using UnityEngine;
using System.Collections;

public class PlaySoundScript : StateMachineBehaviour {

	[Header("Audo Clips")]
	public AudioClip soundClip;

	override public void OnStateEnter
		(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//Play sound clip at camera
		AudioSource.PlayClipAtPoint(soundClip, 
		                            Camera.main.transform.position);
	}
}