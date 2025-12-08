
using UnityEngine;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using Bakery;
using System.Collections;


namespace Bakery
{

    public class NarrativeState : MonoBehaviour, INarrativeState
    {
        private Story _storyRef;
        SerialNarrative _serialNarrative;
        private bool _saveEnabled;

        void OnEnable()
        {
            Dialogs.NarrativeState = () => this;
        }
        void OnDisable()
        {
            Dialogs.NarrativeState = Dialogs.UnregisterNarrativeState;

        }

        IEnumerator Start()
        {
            yield return Flow.Manager().WaitUntilReady;
            yield return Dialogs.Manager().WaitUntilReady;

            _storyRef = Dialogs.Manager().StoryRef;
            _saveEnabled = Persistence.Manager().IsEnabled;

            if (_saveEnabled)
                LoadNarrativeVariables();

            if (_serialNarrative == null)
                InitNarrativeVariables();
            _storyRef.variablesState.variableChangedEvent += UpdateStateFromInk;

        }

        void OnDestroy()
        {
            _storyRef.variablesState.variableChangedEvent -= UpdateStateFromInk;
        }

        private void InitNarrativeVariables()
        {
            _serialNarrative = new();

            foreach (var key in _storyRef.variablesState)
            {
                Ink.Runtime.Object value = _storyRef.variablesState.TryGetDefaultVariableValue(key);

                if (value is BoolValue boolValue)
                    _serialNarrative.NarrativeBools.Add(new NarrativeBool { Key = key, Value = boolValue.value });
            }
        }
        private void LoadNarrativeVariables()
        {
            _serialNarrative = Persistence.Manager().LoadOrCreate<SerialNarrative>(SerialNarrative.KeyName);
        }

        private void SaveNarrativeVariables()
        {
            if (!_saveEnabled) return;
            Persistence.Manager().Cache(SerialNarrative.KeyName, _serialNarrative);
        }

        public void SetFlag(string flag, bool isTrue)
        {
            _serialNarrative.NarrativeBools.RemoveAll(x => x.Key == flag);
            _serialNarrative.NarrativeBools.Add(new NarrativeBool { Key = flag, Value = isTrue });
            SaveNarrativeVariables();
        }


        public void UpdateState()
        {
            _storyRef.variablesState.variableChangedEvent -= UpdateStateFromInk;
            var narrativeBools = new List<NarrativeBool>(_serialNarrative.NarrativeBools);
            foreach (var variable in narrativeBools)
            {
                try
                {
                    BoolValue value = new(variable.Value);
                    _storyRef.variablesState.SetGlobal(variable.Key, value);
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Variable {variable.Key} not found in ink story");
                }
            }

            _storyRef.variablesState.variableChangedEvent += UpdateStateFromInk;

        }

        private void UpdateStateFromInk(string variableName, Ink.Runtime.Object newValue)
        {
            if (newValue is Ink.Runtime.BoolValue boolValue)
                SetFlag(variableName, boolValue.value);
        }

        public bool CheckFlag(string condition)
        {

            if (!_serialNarrative.NarrativeBools.Exists(x => x.Key == condition))
            {
                Debug.LogWarning($"Narrative condition {condition} not found");
                return false;
            }
            return _serialNarrative.NarrativeBools.Find(x => x.Key == condition).Value;

        }
    }

}