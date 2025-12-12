using System;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

namespace Bakery
{
    public static class Dialogs
    {
        #region Events 
        public static class Events
        {
            public static Action OnDialogStart = delegate { };
            public static Action<ThespianData, string, List<string>, float> OnStoryNextLine =
                (character, text, tags, lineDuration) => { };

            public static Action<List<DialogChoice>> OnChoiceAvailable = delegate { };

            public static Action OnDialogEnd = delegate { };

            public static Action OnDialogInRangeUpdate = delegate { };

            public static Action BeforeNewLine = delegate { };
        }
        #endregion  

        #region Interfaces
        public static Func<IDialogManager> Manager = UnregisterManager;
        public static Func<IVoiceOverManager> VoiceOver = UnregisterVoiceOverManager;
        public static Func<INarrativeState> NarrativeState = UnregisterNarrativeState;
        public static Func<IThespianManager> Thespians = UnregisterThespianManager;



        #endregion


        #region Mock Implementations
        private static IDialogManager _cachedMockManager;
        private static IVoiceOverManager _cachedMockVoiceOverManager;
        private static INarrativeState _cachedMockNarrativeState;
        private static IThespianManager _cachedMockThespianManager;
        internal static IDialogManager UnregisterManager()
        {
            Debug.LogWarning("No Dialog Manager registered, using mock implementation.");
            _cachedMockManager ??= new MockDialogManager();
            Manager = () => _cachedMockManager;
            return Manager();
        }

        public static IVoiceOverManager UnregisterVoiceOverManager()
        {
            Debug.LogWarning("No Voice Over Manager registered, using mock implementation.");
            _cachedMockVoiceOverManager ??= new MockVoiceOverManager();
            VoiceOver = () => _cachedMockVoiceOverManager;
            return VoiceOver();
        }
        public static INarrativeState UnregisterNarrativeState()
        {
            Debug.LogWarning("No Narrative State registered, using mock implementation.");
            _cachedMockNarrativeState ??= new MockNarrativeState();
            NarrativeState = () => _cachedMockNarrativeState;
            return NarrativeState();
        }

        public static IThespianManager UnregisterThespianManager()
        {
            Debug.LogWarning("No Thespian Manager registered, using mock implementation.");
            _cachedMockThespianManager ??= new MockThespianManager();
            Thespians = () => _cachedMockThespianManager;
            return Thespians();
        }

        private class MockThespianManager : IThespianManager
        {
            public ThespianData TalkingCharacter
            {
                get => null;
                set { }
            }

            public bool Exists(string character) => false;

            public (ThespianData, string) Extract(string line)
            => (null, "");
        }

        private class MockNarrativeState : INarrativeState
        {
            public bool CheckFlag(string condition) => false;

            public void SetFlag(string flag, bool isTrue) { }

            public void UpdateState() { }
        }

        private class MockDialogManager : IDialogManager
        {
            public WaitUntil WaitUntilReady => new(() => true);
            public WaitUntil WaitUntilDialogEnds => new(() => true);

            public bool IsDialogInProgress => false;
            public Story StoryRef => null;

            public EnumPlayMode PlayMode { get; set; } = EnumPlayMode.Automatic;
            public float NarrationSpeed { get; set; } = 50f;

            public void AddDelay(EnumDelayType delayType, float delay) { }

            public void Interrupt() { }
            public void MakeChoice(int choiceIndex) { }

            public void SkipOneLine() { }
            public void SkipToNextChoice() { }
            public void Play(string inkKnot) { }


        }
        private class MockVoiceOverManager : IVoiceOverManager
        {
            public float LineDuration => -1;

            public Coroutine LoadLine(ThespianData talkingCharacter, string line)
            => null;


            public void SayLine()
            { }

            public void Stop() { }
        }
        #endregion


        #region Cleaning up statics
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Manager = UnregisterManager;
            VoiceOver = UnregisterVoiceOverManager;
            NarrativeState = UnregisterNarrativeState;
            Thespians = UnregisterThespianManager;

            Events.BeforeNewLine = delegate { };
            Events.OnChoiceAvailable = delegate { };
            Events.OnDialogEnd = delegate { };
            Events.OnDialogInRangeUpdate = delegate { };
            Events.OnDialogStart = delegate { };
            Events.OnStoryNextLine = (character, text, tags, lineDuration) => { };


#if UNITY_EDITOR
            Debug.Log("[Save] Static fields reset (domain reload skipped)");
#endif
        }
        #endregion
    }
}