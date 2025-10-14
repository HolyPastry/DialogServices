
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SocialPlatforms;

namespace Bakery.Dialogs
{

    public class CharacterVoice : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        public WaitUntil WaitUntilLoaded => new(() => !_isLoaded);

        [SerializeField] private CharacterData _characterData;
        private bool _isLoaded;
        private bool _initialized;

        public CharacterData CharacterData => _characterData;

        public float Length => _audioSource.clip != null ? _audioSource.clip.length : -1;

        void OnEnable()
        {
            if (_characterData == null)
            {
                Debug.LogWarning("CharacterData is not set in CharacterVoice:" + gameObject.name);
                return;
            }
            if (!_initialized) return;
            LocalVoiceOverManager.AddVoice(this);
        }

        void OnDisable()
        {
            LocalVoiceOverManager.RemoveVoice(this);
        }

        IEnumerator Start()
        {
            yield return FlowServices.WaitUntilReady();
            yield return DialogServices.WaitUntilReady();

            LocalVoiceOverManager.AddVoice(this);
            _initialized = true;
        }

        void OnDestroy()
        {
            if (_initialized)
                LocalVoiceOverManager.RemoveVoice(this);
        }

        public void Say(AudioClip currentLineClip)
        {
            _audioSource.PlayOneShot(currentLineClip);
        }

        internal void Interrupt()
        {
            _audioSource.Stop();
        }
    }
}