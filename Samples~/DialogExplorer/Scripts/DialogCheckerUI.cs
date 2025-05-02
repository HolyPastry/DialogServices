using System;
using System.Collections;
using System.Collections.Generic;
using Holypastry.Bakery;

using TMPro;
using UnityEngine;

namespace Bakery.Dialogs
{
    public class DialogCheckerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _numberBranch;
        [SerializeField] private TextMeshProUGUI _log;

        [SerializeField] private TextMeshProUGUI _lineProcessed;


        [SerializeReference] private DialogChecker _dialogChecker;




        private List<string> _lineWithoutCharacterList = new();
        private List<string> _characterNotFound = new();
        private List<string> _tagNotFound = new();

        void Awake()
        {
            _numberBranch.text = "0";
        }

        void OnEnable()
        {
            _dialogChecker.OnTotalBranchesExploredCountChanged += UpdateBranches;
            _dialogChecker.OnCharacterNotFound += UpdateCharacters;
            _dialogChecker.OnTagNotFound += UpdateTags;
            _dialogChecker.OnNewKnotExplored += UpdateKnots;
            _dialogChecker.OnLineWithoutCharacter += UpdateLineWithoutCharacter;
            _dialogChecker.OnLineProcessed += UpdateLineProcessed;

        }



        void OnDisable()
        {
            _dialogChecker.OnTotalBranchesExploredCountChanged -= UpdateBranches;
            _dialogChecker.OnCharacterNotFound -= UpdateCharacters;
            _dialogChecker.OnTagNotFound -= UpdateTags;
            _dialogChecker.OnNewKnotExplored -= UpdateKnots;
            _dialogChecker.OnLineWithoutCharacter -= UpdateLineWithoutCharacter;
            _dialogChecker.OnLineProcessed -= UpdateLineProcessed;


        }




        private void UpdateLineProcessed(string line)
        {
            _lineProcessed.text = line;
        }

        private void UpdateLineWithoutCharacter(string line, string knot)
        {
            if (_lineWithoutCharacterList.Contains(line)) return;
            _lineWithoutCharacterList.Add(line);
            line = line.Trim('\n').Trim('\r');
            _log.text += $"\nNo Characters: {knot}: {line}";
        }

        private void UpdateKnots(string knotName)
        {
            _log.text += "\nNEW KNOT: " + knotName; ;
        }

        private void UpdateTags(string tag, string line, string knot)
        {
            if (_tagNotFound.Contains(tag)) return;
            _tagNotFound.Add(tag);
            line = line.Trim('\n').Trim('\r');
            _log.text += $"\nTag Missing: <b>{tag}</b>, {knot}:{line}";
        }

        private void UpdateCharacters(string character, string line, string knot)
        {
            if (_characterNotFound.Contains(character)) return;
            _characterNotFound.Add(character);
            line = line.Trim('\n').Trim('\r');
            _log.text += $"\nCharacter Missing: <b>{character}</b>, {knot}:{line}";

        }

        private void UpdateBranches(int obj)
        {
            _numberBranch.text = obj.ToString();
        }
    }
}
