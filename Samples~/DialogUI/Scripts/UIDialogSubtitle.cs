using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bakery.Dialogs.UI
{

    public class UIDialogSubtitle : MonoBehaviour
    {
        public static Action ForceHide = delegate { };
        public static Action CancelForceHide = delegate { };


        [SerializeField] private GameObject _hiddable;
        [SerializeField] private TMPro.TextMeshProUGUI _text;
        [SerializeField] private List<LayoutGroup> _layoutGroups;


        private readonly List<UIChoiceButton> _choiceButtonPool = new();
        private bool _choiceAvailable;
        private bool _forceHide;



        void Awake()
        {
            GetComponentsInChildren(true, _choiceButtonPool);
            GetComponentsInChildren(true, _layoutGroups);

            foreach (var button in _choiceButtonPool)
                button.gameObject.SetActive(false);

            _hiddable.SetActive(false);

        }

        void OnEnable()
        {
            foreach (var button in _choiceButtonPool)
                button.OnChoiceSelected += OnChoiceSelected;

            DialogEvents.OnStoryNextLine += OnStoryNextLine;
            DialogEvents.OnChoiceAvailable += OnChoiceAvailable;
            DialogEvents.OnDialogStart += Show;
            DialogEvents.OnDialogEnd += Hide;
            DialogEvents.BeforeNewLine += Hide;

            ForceHide = OnForceHide;
            CancelForceHide = OnCancelForceHide;
        }

        void OnDisable()
        {
            foreach (var button in _choiceButtonPool)
                button.OnChoiceSelected -= OnChoiceSelected;

            DialogEvents.OnStoryNextLine -= OnStoryNextLine;
            DialogEvents.OnChoiceAvailable -= OnChoiceAvailable;
            DialogEvents.OnDialogStart -= Show;
            DialogEvents.OnDialogEnd -= Hide;
            DialogEvents.BeforeNewLine -= Hide;

            ForceHide = delegate { };
            CancelForceHide = delegate { };
        }

        private void OnForceHide()
        {
            _forceHide = true;
            _hiddable.SetActive(false);
        }
        private void OnCancelForceHide()
        {
            _forceHide = false;
        }

        private void Show()
        {
            if (_forceHide) return;
            _hiddable.SetActive(true);
            UpdateLayoutGroupRoutine();
        }

        private void Hide()
        {
            _hiddable.SetActive(false);
        }

        private void OnChoiceAvailable(List<DialogChoice> list)
        {
            _choiceAvailable = true;
            for (int i = 0; i < list.Count; i++)
            {
                _choiceButtonPool[i].Init(i, list[i].Text);
            }
            StartCoroutine(UpdateLayoutGroupRoutine());
        }

        private void OnChoiceSelected(int index)
        {
            if (!_choiceAvailable) return;
            _choiceAvailable = false;

            HideAllChoiceButtons();
            DialogServices.MakeChoice(index);
            _hiddable.SetActive(false);
        }

        private void HideAllChoiceButtons()
        {

            foreach (var button in _choiceButtonPool)
                button.gameObject.SetActive(false);

        }

        void OnStoryNextLine(CharacterData character, string line, List<string> tags, float lineDuration)
        {
            if (_forceHide) return;
            _hiddable.SetActive(true);

            if (character == null || character.HideName)
                _text.text = line;
            else
            {
                _text.text = $"<b>{character.MasterText}</b>: {line}";
            }
            StartCoroutine(UpdateLayoutGroupRoutine());

            //  UpdateLayoutGroupRoutine();

        }

        private IEnumerator UpdateLayoutGroupRoutine()
        {
            yield return null;

            for (int i = _layoutGroups.Count - 1; i >= 0; i--)
            {
                _layoutGroups[i].enabled = false;
                _layoutGroups[i].enabled = true;


            }

        }
    }
}