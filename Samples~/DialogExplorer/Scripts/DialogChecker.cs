
using System;
using System.Collections;
using System.Collections.Generic;

namespace Bakery.Dialogs
{
    public class DialogChecker : DialogExplorer
    {


        public event Action<string, string, string> OnCharacterNotFound = (character, line, knot) => { };
        public event Action<string, string, string> OnTagNotFound = (tag, line, Knot) => { };
        public event Action<string, string> OnLineWithoutCharacter = (line, knot) => { };

        public event Action<string> OnLineProcessed = (line) => { };

        protected void OnEnable()
        {

            OnExplorationEnded += OnEnded;
        }

        protected void OnDisable()
        {

            OnExplorationEnded -= OnEnded;
        }

        protected override void Start()
        {
            base.Start();


            StartCoroutine(CheckSpecificKnotRoutine());
        }

        private void OnEnded()
        {
        }


        protected override IEnumerator ProcessRoutine(string line, string knot)
        {

            (var character, var newLine) = DialogServices.ExtractCharacter(line);
            if (character == null)
            {
                OnLineWithoutCharacter?.Invoke(line, knot);
            }

            CheckTags(character, line, knot);

            OnLineProcessed?.Invoke(line);
            _processed = true;
            yield break;
        }

        private void CheckTags(CharacterData character, string line, string knot)
        {
            List<string> tags = new(_story.currentTags);
            if (tags == null || tags.Count == 0) return;

            foreach (var processor in _tagProcessors)
                tags = processor.ProcessTags(character, tags);

            _knownNotFoundTags.AddRange(tags);

            foreach (var tag in tags)
                OnTagNotFound?.Invoke(tag, line, knot);

        }

        private List<string> RemoveKnownTags(List<string> tags)
        {
            List<string> result = new();
            foreach (var tag in tags)
            {
                if (_knownNotFoundTags.Contains(tag)) continue;
                result.Add(tag);
            }
            return result;
        }


    }
}