
using UnityEngine;
using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Holypastry.Bakery;
using Holypastry.Bakery.Flow;
using Unity.VisualScripting;

namespace Bakery.Dialogs
{
    internal class DialogManager : Service
    {

        [SerializeField] private TextAsset _inkJSON;
        [SerializeField] private string _collectionFolder = "Characters";
        [SerializeField] private float _charPerSeconds = 50;

        private VoiceOverManager _voiceOverManager;
        private Story _story;
        private NarrativeState _narrativeState;
        private DataCollection<CharacterData> _dataCollection;
        private readonly List<TagProcessor> _tagProcessors = new();

        private float _delayBefore;
        private float _overlapDuration;
        private float _delayAfter;
        private CharacterData _talkingCharacter;
        private bool _isDialogInProgress;



        void Awake()
        {
            _dataCollection = new DataCollection<CharacterData>(_collectionFolder);
            _story = new Story(_inkJSON.text);
            _tagProcessors.AddRange(GetComponentsInChildren<TagProcessor>());
            _narrativeState = new NarrativeState(_story);
            _voiceOverManager = GetComponent<VoiceOverManager>();

        }

        void OnDisable()
        {
            DialogServices.WaitUntilReady = () => new WaitUntil(() => true);

            DialogServices.MakeChoice = delegate { };

            DialogServices.IsDialogInProgress = delegate { return false; };
            DialogServices.WaitUntilDialogEnds = () => new WaitUntil(() => true);

            DialogServices.Start = delegate { };
            DialogServices.Exists = (character) => false;
            DialogServices.InterruptDialog = delegate { };

            DialogServices.SetNarrativeFlag = delegate { };
            DialogServices.ExtractCharacter = (line) => (null, "");
            DialogServices.CheckNarrativeFlag = delegate { return false; };
            DialogServices.SetTextNarrationSpeed = delegate { };
            DialogServices.AddDelay = delegate { };

        }

        void OnEnable()
        {
            DialogServices.WaitUntilReady = () => WaitUntilReady;

            DialogServices.MakeChoice = MakeChoice;

            DialogServices.IsDialogInProgress = () => _isDialogInProgress;
            DialogServices.WaitUntilDialogEnds = () => new WaitUntil(() => !_isDialogInProgress);

            DialogServices.Start = StartDialog;
            DialogServices.Exists = TryAndSetStoryPath;
            DialogServices.InterruptDialog = EndDialog;

            DialogServices.ExtractCharacter = (line) => ExtractCharacterData(line);
            DialogServices.SetNarrativeFlag = _narrativeState.SetNarrativeFlag;
            DialogServices.CheckNarrativeFlag = _narrativeState.CheckNarrativeFlag;

            DialogServices.SetTextNarrationSpeed = (speed) => _charPerSeconds = speed;

            DialogServices.AddDelay = AddDelay;

        }

        private void AddDelay(EnumDelayType type, float delay)
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

        private bool TryAndSetStoryPath(string path)
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

        private void StartDialog(string Knot)
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
            StopAllCoroutines();
            _isDialogInProgress = false;
            DialogEvents.OnDialogEnd?.Invoke();
        }


        private void MakeChoice(int index)
        {
            foreach (var processor in _tagProcessors)
            {
                processor.ProcessTags(_talkingCharacter, _story.currentChoices[index].tags);
            }
            _story.ChooseChoiceIndex(index);
        }

        private IEnumerator PlayStoryRoutine()
        {

            _narrativeState.UpdateInkState();
            DialogEvents.OnDialogStart?.Invoke();

            while (true)
            {
                while (_story.canContinue)
                {
                    string line = _story.Continue();
                    if (!Valid(line)) continue;
                    (_talkingCharacter, line) = ExtractCharacterData(line);
                    _delayAfter = 0;
                    _delayBefore = 0;

                    ProcessTags(TagProcessor.EnumStep.BeforeLine, _story.currentTags, _talkingCharacter);

                    DialogEvents.BeforeNewLine.Invoke();

                    yield return new WaitForSeconds(_delayBefore);

                    float lineDuration = -1;

                    if (_voiceOverManager != null)
                    {
                        yield return _voiceOverManager.LoadLine(_talkingCharacter, line);
                        lineDuration = _voiceOverManager.LineDuration;
                        _voiceOverManager.SayLoadedLine();
                    }
                    else
                    {
                        lineDuration = line.Length / _charPerSeconds;
                    }
                    DialogEvents.OnStoryNextLine.Invoke(_talkingCharacter, line, _story.currentTags, lineDuration);

                    if (lineDuration > 0)
                        yield return new WaitForSeconds(Mathf.Max(0, lineDuration - _overlapDuration));
                    else
                        yield return new WaitForSeconds(3f);

                    ProcessTags(TagProcessor.EnumStep.AfterLine, new(_story.currentTags), _talkingCharacter);
                    _narrativeState.UpdateInkState();
                    yield return new WaitForSeconds(_delayAfter);
                }

                if (_story.currentChoices.Count > 0)
                {
                    DialogEvents.OnChoiceAvailable?.Invoke(GetChoices());
                    yield return new WaitUntil(() => _story.canContinue);
                }
                else
                {
                    break;
                }
            }
            yield return null;

            EndDialog();
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

        private void ProcessTags(TagProcessor.EnumStep step, List<string> tags, CharacterData character)
        {
            if (tags == null) return;
            if (tags.Count == 0) return;

            foreach (var processor in _tagProcessors)
            {
                if (processor.Step != step) continue;
                processor.ProcessTags(character, tags);
            }
        }

        private (CharacterData, string) ExtractCharacterData(string line)
        {

            if (!line.Contains(":")) return (null, line);

            string[] split = line.Split(':');
            string characterStr = split[0];
            line = split[1];

            CharacterData character = _dataCollection.Find(x => x.name == characterStr);
            if (character == null)
            {
                Debug.LogWarning($"Character {characterStr} not found in data collection\n{line}");
                return (null, line);
            }
            return (character, line);
        }
    }
}