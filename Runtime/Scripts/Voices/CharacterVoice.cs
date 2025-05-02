
using UnityEngine;
using KBCore.Refs;
using System.Collections;
using System;

namespace Bakery.Dialogs
{
    [RequireComponent(typeof(AudioSource))]
    public class CharacterVoice : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private AudioSource _audioSource;

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
            VoiceOverManager.AddVoice(this);
        }

        void OnDisable()
        {
            VoiceOverManager.RemoveVoice(this);
        }

        IEnumerator Start()
        {
            yield return FlowServices.WaitUntilReady();
            yield return DialogServices.WaitUntilReady();

            VoiceOverManager.AddVoice(this);
            _initialized = true;
        }

        void OnDestroy()
        {
            if (_initialized)
                VoiceOverManager.RemoveVoice(this);
        }

        public void Say(AudioClip currentLineClip)
        {
            _audioSource.PlayOneShot(currentLineClip);
        }
    }
}