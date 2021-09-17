using System.Collections.Generic;
using UnityEngine;
using Yashlan.util;

namespace Yashlan.audio
{
    public class AudioPlayer : SingletonBehaviour<AudioPlayer>
    {
        public const string HIT_ENEMY_SFX = "hit-enemy";
        public const string ENEMY_DIE_SFX = "enemy-die";

        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private List<AudioClip> _audioClips;

        public void PlaySFX(string name)
        {
            AudioClip sfx = _audioClips.Find(s => s.name == name);
            if (sfx == null) return;

            _audioSource.PlayOneShot(sfx);
        }
    }
}
