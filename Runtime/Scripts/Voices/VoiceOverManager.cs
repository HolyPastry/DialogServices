using System;

using System.Collections.Generic;
using Holypastry.Bakery;

using UnityEngine;

namespace Bakery.Dialogs
{
    internal class VoiceOverManager : MonoBehaviour
    {
        internal static Action<CharacterVoice> AddVoice = delegate { };
        internal static Action<CharacterVoice> RemoveVoice = delegate { };

        internal float LineDuration => _currentLineClip != null ? _currentLineClip.length : -1;
        private readonly List<CharacterVoice> _characterVoices = new();


        private AudioClip _currentLineClip;
        private bool _loaded;
        private CharacterVoice _currentVoice;


        void OnEnable()
        {
            AddVoice = (voice) => _characterVoices.AddUnique(voice);
            RemoveVoice = (voice) => _characterVoices.Remove(voice);
        }

        void OnDisable()
        {
            AddVoice = delegate { };
            RemoveVoice = delegate { };
        }

        public WaitUntil LoadLine(CharacterData data, string line)
        {
            if (!Valid(data, line, out _currentVoice))
                return new WaitUntil(() => true);
            _loaded = false;
            _currentLineClip = null;
            LoadLineAsync(line);

            return new WaitUntil(() => _loaded);
        }

        protected async void LoadLineAsync(string line)
        {
            if (string.IsNullOrEmpty(_currentVoice.CharacterData.ActorName))
            {
                Debug.LogWarning("ActorName is null or empty", this);
                return;
            }
            var filename = VoiceFileUtils.TextToFileName(line, _currentVoice.CharacterData.ActorName);
            _currentLineClip = await VoiceFileUtils.LoadAudioClipFromLocal(filename);

            if (_currentLineClip == null)
                Debug.LogWarning($"Audio clip not found for {filename}", this);

            _loaded = true;
        }

        private bool Valid(CharacterData data, string line, out CharacterVoice characterVoice)
        {
            characterVoice = null;
            if (string.IsNullOrEmpty(line))
            {
                Debug.LogWarning("Line is null or empty", this);
                return false;
            }
            if (data == null)
            {
                Debug.LogWarning("CharacterData is null", this);
                return false;
            }
            var voices = _characterVoices.FindAll(v => v.CharacterData == data);
            if (voices.Count == 0)
            {
                Debug.LogWarning($"No voices found for {data}", this);
                return false;
            }
            if (voices.Count > 1)
            {
                Debug.LogWarning($"Multiple voices found for {data}", this);
                return false;
            }

            characterVoice = voices[0];
            return characterVoice != null;
        }


        internal void SayLoadedLine()
        {
            if (_currentVoice == null)
            {
                Debug.LogWarning("No voice loaded", this);
                return;
            }
            _currentVoice.Say(_currentLineClip);
        }
    }
}