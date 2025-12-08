
using UnityEngine;
using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Holypastry.Bakery;

namespace Bakery
{

    internal class DialogManager : MonoBehaviour, IDialogManager
    {

        [SerializeField] private TextAsset _inkJSON;

        [SerializeField] private float _charPerSeconds = 50;
        [SerializeField] private float _defaultDelayBefore = 0f;
        [SerializeField] private float _defaultDelayAfter = 0f;
        [SerializeField] private float _defaultWaitTime = 3f;
        [SerializeField] private EnumPlayMode _playMode = EnumPlayMode.Mixed;

        public WaitUntil WaitUntilReady => new(() => true);

        public bool IsDialogInProgress => _isDialogInProgress;
        public WaitUntil WaitUntilDialogEnds => new(() => !_isDialogInProgress);
        public Story StoryRef => _story;

        public EnumPlayMode PlayMode
        { get => _playMode; set => _playMode = value; }
        public float NarrationSpeed
        { get => _charPerSeconds; set => _charPerSeconds = value; }



        private Story _story;

        private readonly List<TagProcessor> _tagProcessors = new();

        private float _delayBefore;
        private float _overlapDuration;
        private float _delayAfter;
        private bool _isDialogInProgress;
        private bool _skipOneLine;
        private bool _skipToNextChoice;
        private CountdownTimer _delayTimer;

        void Awake()
        {

            _story = new Story(_inkJSON.text);
            _tagProcessors.AddRange(GetComponentsInChildren<TagProcessor>());
        }

        void OnDisable()
        {
            Dialogs.Manager = Dialogs.UnregisterManager;

        }

        void OnEnable()
        {
            Dialogs.Manager = () => this;
        }



        void Update()
        {
            _delayTimer?.Tick(Time.deltaTime);
        }

        public void SkipToNextChoice() => _skipToNextChoice = true;

        public void SkipOneLine() => _skipOneLine = true;

        public void AddDelay(EnumDelayType type, float delay)
        {
            switch (type)
            {
                case EnumDelayType.BeforeLine:
                    _delayBefore = delay;
                    break;
                case EnumDelayType.AfterLine:
                    _delayAfter = delay;
                    break;
                case EnumDelayType.OverlapLine:
                    _overlapDuration = delay;
                    break;
                default:
                    break;
            }

        }

        bool TryAndSetStoryPath(string path)
        {
            try
            {
                _story.ChoosePathString(path);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void Play(string Knot)
        {

            if (string.IsNullOrEmpty(Knot))
            {
                Debug.LogWarning("Dialog knot is empty, ignoring dialog request");
                return;
            }
            if (!TryAndSetStoryPath(Knot))
            {
                Debug.LogWarning($"Knot {Knot} not found in ink story, ignoring dialog request");
                return;
            }
            _isDialogInProgress = true;
            StartCoroutine(PlayStoryRoutine());
        }


        private void EndDialog()
        {
            Dialogs.VoiceOver().Stop();
            StopAllCoroutines();
            _isDialogInProgress = false;
            Dialogs.Events.OnDialogEnd?.Invoke();
        }


        public void MakeChoice(int index)
        {
            foreach (var processor in _tagProcessors)
            {
                processor.ProcessTags(Dialogs.Thespians().TalkingCharacter,
                     _story.currentChoices[index].tags);
            }
            _story.ChooseChoiceIndex(index);
        }

        private IEnumerator PlayStoryRoutine()
        {
            Dialogs.NarrativeState().UpdateState();
            Dialogs.Events.OnDialogStart?.Invoke();
            _skipOneLine = false;
            _skipToNextChoice = false;
            while (true)
            {
                while (_story.canContinue)
                {
                    string line = _story.Continue();
                    if (!Valid(line)) continue;
                    (Dialogs.Thespians().TalkingCharacter, line) =
                                    Dialogs.Thespians().Extract(line);
                    _delayAfter = _defaultDelayAfter;
                    _delayBefore = _defaultDelayBefore;

                    ProcessTags(TagProcessor.EnumStep.BeforeLine,
                             _story.currentTags,
                              Dialogs.Thespians().TalkingCharacter);

                    Dialogs.Events.BeforeNewLine.Invoke();

                    yield return Wait(_delayBefore, extraTimer: true);

                    float lineDuration = line.Length / _charPerSeconds;

                    yield return Dialogs.VoiceOver().LoadLine(Dialogs.Thespians().TalkingCharacter,
                                 line);

                    if (Dialogs.VoiceOver().LineDuration > 0)
                    {
                        lineDuration = Dialogs.VoiceOver().LineDuration;
                        Dialogs.VoiceOver().SayLine();
                    }

                    Dialogs.Events.OnStoryNextLine.Invoke(Dialogs.Thespians().TalkingCharacter, line, _story.currentTags, lineDuration);

                    if (lineDuration > 0)
                        yield return Wait(Mathf.Max(0, lineDuration - _overlapDuration),
                                         extraTimer: false);
                    else
                        yield return Wait(_defaultWaitTime, extraTimer: false);

                    ProcessTags(TagProcessor.EnumStep.AfterLine,
                                 new(_story.currentTags),
                                    Dialogs.Thespians().TalkingCharacter);

                    Dialogs.NarrativeState().UpdateState();
                    yield return Wait(_delayAfter, extraTimer: true);
                    Dialogs.VoiceOver().Stop();

                    _skipOneLine = false;
                }

                if (_story.currentChoices.Count > 0)
                {
                    _skipToNextChoice = false;
                    Dialogs.Events.OnChoiceAvailable?.Invoke(GetChoices());
                    yield return new WaitUntil(() => _story.canContinue);
                    _skipOneLine = false;
                }
                else
                {
                    break;
                }
            }
            yield return null;

            EndDialog();
        }

        private WaitUntil Wait(float delay, bool extraTimer)
        {
            if (_playMode == EnumPlayMode.Manual)
            {
                if (extraTimer) return null;
                return new WaitUntil(() => _skipOneLine || _skipToNextChoice);
            }

            if (_playMode == EnumPlayMode.Automatic)
            {
                _delayTimer = new CountdownTimer(delay);
                _delayTimer.Start();
                return new WaitUntil(() => !_delayTimer.IsRunning);
            }

            _delayTimer = new CountdownTimer(delay);
            _delayTimer.Start();

            return new WaitUntil(() => _skipOneLine || _skipToNextChoice || !_delayTimer.IsRunning);
        }

        private bool Valid(string line)
        {
            line = line.Trim();
            line = line.Replace("\n", "");
            line = line.Replace("\r", "");

            return !string.IsNullOrEmpty(line);

        }

        private List<DialogChoice> GetChoices()
        {
            List<DialogChoice> choices = new();
            foreach (var choice in _story.currentChoices)
            {
                choices.Add(new(choice));
            }
            return choices;
        }

        private void ProcessTags(TagProcessor.EnumStep step, List<string> tags, ThespianData character)
        {
            if (tags == null) return;
            if (tags.Count == 0) return;

            foreach (var processor in _tagProcessors)
            {
                if (processor.Step != step) continue;
                processor.ProcessTags(character, tags);
            }
        }

        public void Interrupt() => EndDialog();



    }
}