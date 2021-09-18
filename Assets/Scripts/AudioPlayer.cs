using System.Collections.Generic;
using UnityEngine;
using Yashlan.util;

namespace Yashlan.audio
{
    public class AudioPlayer : SingletonBehaviour<AudioPlayer>
    {
        public const string DROP_TOWER_SFX = "drop-tower";
        public const string HIT_ENEMY_SFX  = "hit-enemy";
        public const string HIT_TOWER_SFX  = "hit-enemy";
        public const string ENEMY_DIE_SFX  = "enemy-die";
        public const string TOWER_DIE_SFX  = "enemy-die";
        public const string GAME_WIN_SFX   = "game-win";
        public const string GAME_LOSE_SFX  = "game-lose";

        [SerializeField]
        private AudioClip _bgmSound;
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private List<AudioClip> _audioClips;

        private AudioSource _bgmSource;

        void Start()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.clip = _bgmSound;
            _bgmSource.loop = true;
            _bgmSource.volume = 0.5f;
            _bgmSource.Play();
        }

        public void StopBGM() => _bgmSource.Stop();

        public void PlaySFX(string name)
        {
            AudioClip sfx = _audioClips.Find(s => s.name == name);
            if (sfx == null) return;

            _audioSource.PlayOneShot(sfx);
        }
    }
}
