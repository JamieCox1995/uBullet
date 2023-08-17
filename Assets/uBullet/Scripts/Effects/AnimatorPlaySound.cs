using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.Effects
{
    public class AnimatorPlaySound : StateMachineBehaviour
    {

        [Header("Audio To Play:")]
        public AudioClip audioClip;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
        }
    }
}
