using System;
using System.Collections;
using System.Collections.Generic;
using Holypastry.Bakery;

using UnityEngine;

namespace Bakery
{
    public class LocalVoiceOverManager : MonoBehaviour, IVoiceOverManager
    {
        internal static Action<CharacterVoice> AddVoice = delegate { };
        internal static Action<CharacterVoice> RemoveVoice = delegate { };

        protected readonly List<CharacterVoice> _characterVoices = new();

        protected AudioClip _currentLineClip;
        protected bool _loaded;
        protected CharacterVoice _currentVoice;

        public float LineDuration => _currentLineClip != null ? _currentLineClip.length : -1;
        protected virtual void OnEnable()
        {
            AddVoice = (voice) => _characterVoices.AddUnique(voice);
            RemoveVoice = (voice) => _characterVoices.Remove(voice);
            Dialogs.VoiceOver = () => this;
        }

        protected virtual void OnDisable()
        {
            AddVoice = delegate { };
            RemoveVoice = delegate { };
            Dialogs.VoiceOver = Dialogs.UnregisterVoiceOverManager;
        }

        public Coroutine LoadLine(ThespianData data, string line)
        {
            if (!Valid(data, line, out _currentVoice))
                return null;

            _loaded = false;
            _currentLineClip = null;
            return StartCoroutine(LoadLineCoroutine(line));

        }
        private IEnumerator LoadLineCoroutine(string line)
        {
            LoadLineAsync(line);
            yield return new WaitUntil(() => _loaded);
        }

        protected bool Valid(ThespianData data, string line, out CharacterVoice characterVoice)
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
        protected virtual async void LoadLineAsync(string line)
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

        public void SayLine()
        {
            if (_currentVoice == null)
            {
                Debug.LogWarning("No voice loaded", this);
                return;
            }
            _currentVoice.Say(_currentLineClip);
        }

        public void Stop()
        {
            _currentVoice.Interrupt();
        }
    }
}