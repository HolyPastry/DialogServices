
using UnityEngine;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using Bakery.Saves;


namespace Bakery.Dialogs
{
    [Serializable]
    public struct NarrativeBool
    {
        public string Key;
        public bool Value;
    }


    [Serializable]
    public class SerialNarrative : SerialData
    {
        public const string KeyName = "NarrativeState";
        public override string Key() => "NarrativeState";
        public List<NarrativeBool> NarrativeBools;

        public SerialNarrative(SerialNarrative serialNarrative)
        {
            NarrativeBools = new List<NarrativeBool>(serialNarrative.NarrativeBools);
        }

        public SerialNarrative()
        {
            NarrativeBools = new();
        }

        internal void Clear()
        {
            NarrativeBools.Clear();
        }
    }


    public class NarrativeState
    {
        SerialNarrative _serialNarrative;
        private readonly Story _storyRef;

        public NarrativeState(Story story)
        {
            _storyRef = story;

            if (SaveServices.IsEnabled())
                LoadNarrativeVariables();
            else
                InitNarrativeVariables();
            _storyRef.variablesState.variableChangedEvent += UpdateStateFromInk;

        }

        public void Disable()
        {
            _storyRef.variablesState.variableChangedEvent -= UpdateStateFromInk;
        }


        ~NarrativeState()
        {
            Disable();
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
            _serialNarrative = SaveServices.Load<SerialNarrative>(SerialNarrative.KeyName);
        }

        private void SaveNarrativeVariables()
        {
            SaveServices.Save(_serialNarrative);
        }

        public void SetNarrativeFlag(string flag, bool isTrue)
        {
            _serialNarrative.NarrativeBools.RemoveAll(x => x.Key == flag);
            _serialNarrative.NarrativeBools.Add(new NarrativeBool { Key = flag, Value = isTrue });
            SaveNarrativeVariables();
        }


        public void UpdateInkState()
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
                SetNarrativeFlag(variableName, boolValue.value);
        }

        public bool CheckNarrativeFlag(string condition)
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