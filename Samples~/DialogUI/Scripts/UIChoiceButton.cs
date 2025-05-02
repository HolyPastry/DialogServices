
using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bakery.Dialogs.UI
{

    public class UIChoiceButton : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;

        private int _index;

        public Action<int> OnChoiceSelected;

        public void Init(int index, string text)
        {

            _index = index;
            _text.text = text;

            _button.onClick.AddListener(OnButtonClicked);
            gameObject.SetActive(true);
        }

        void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            OnChoiceSelected?.Invoke(_index);
        }
    }
}