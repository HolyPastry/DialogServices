

using System;
using System.Collections;
using System.Collections.Generic;
using Holypastry.Bakery;
using Ink.Runtime;

using UnityEngine;

namespace Bakery.Dialogs
{
    public abstract class DialogExplorer : MonoBehaviour
    {
        [SerializeField] private int _numIterations = 1;
        protected DataCollection<CharacterData> _collection;

        [SerializeField] protected bool _isDebug = false;
        [SerializeField] protected TextAsset _inkFile;

        [SerializeField] private List<knot> _knotsToExplore = new();

        [SerializeField] private string _initialKnot = string.Empty;

        [Serializable]
        public class knot
        {
            public string knotName;
            public bool OnceOnly = false;

            public bool Triggered = false;
        }

        private NarrativeState _narrativeState;
        protected Story _story;
        const int MAX_ITERATION = 10000;
        protected bool _allAreExhausted;
        protected bool _checkerAvailable = true;
        protected string _previousLine;
        protected readonly List<string> _exhaustedPathes = new();
        protected readonly List<string> _knownNotFoundTags = new();

        protected List<string> _knots = new();
        protected bool _processed;

        public object WaitUntilProcessed => new WaitUntil(() => _processed);

        public Action<string> OnNewKnotExplored = delegate { };
        public Action<int> OnTotalBranchesExploredCountChanged = delegate { };

        public Action OnExplorationEnded = delegate { };

        private int _totalBranchesExploredCount;

        protected List<TagProcessor> _tagProcessors;


        void OnDestroy()
        {
            StopAllCoroutines();

            DialogServices.SetNarrativeFlag = delegate { };
        }

        protected virtual void Start()
        {
            _tagProcessors = new(GetComponents<TagProcessor>());
            _collection = new("DialogCharacters");
            _story = new Story(_inkFile.text);
            _narrativeState = new NarrativeState(_story);

            DialogServices.SetNarrativeFlag = _narrativeState.SetNarrativeFlag;

            Dictionary<string, Ink.Runtime.Object> namedContent = _story.mainContentContainer.namedOnlyContent;

            foreach (KeyValuePair<string, Ink.Runtime.Object> kv in namedContent)
                _knots.Add(kv.Key);
        }

        protected void DebugLog(string message)
        {
            if (_isDebug) Debug.Log(message);
        }

        private bool _linearStoryEnded;
        private bool _branchExploredToTheEnd;
        private string _currentKnot;

        protected IEnumerator CheckAllKnotsRoutine()
        {
            foreach (var knot in _knots)
            {
                _checkerAvailable = false;
                OnNewKnotExplored?.Invoke(knot);
                StartCoroutine(CheckKnot(knot));
                yield return new WaitUntil(() => _checkerAvailable);
            }
            OnExplorationEnded?.Invoke();
        }

        protected IEnumerator CheckSpecificKnotRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            for (var j = 0; j < _numIterations; j++)
            {

                _narrativeState = new(_story);
                _story.ResetState();
                _exhaustedPathes.Clear();
                _knownNotFoundTags.Clear();
                _totalBranchesExploredCount = 0;

                if (!string.IsNullOrEmpty(_initialKnot))
                {
                    StartCoroutine(CheckKnot(_initialKnot));
                    yield return new WaitUntil(() => _checkerAvailable);
                }



                for (var i = 0; i < MAX_ITERATION; i++)
                {
                    List<int> indexes = new();
                    for (var k = 0; k < _knotsToExplore.Count; k++)
                        indexes.Add(k);

                    while (indexes.Count > 0)
                    {
                        var randomIndex = UnityEngine.Random.Range(0, indexes.Count);
                        var randomKnotIndex = indexes[randomIndex];
                        indexes.RemoveAt(randomIndex);
                        var knot = _knotsToExplore[randomKnotIndex];

                        if (knot.OnceOnly && knot.Triggered) continue;
                        knot.Triggered = true;
                        bool isUnlocked = true;


                        if (!isUnlocked) continue;
                        StartCoroutine(CheckKnot(knot.knotName));
                        yield return new WaitUntil(() => _checkerAvailable);
                    }
                }
            }
            OnExplorationEnded?.Invoke();
        }

        protected IEnumerator CheckKnot(string knot)
        {

            OnNewKnotExplored?.Invoke(knot);
            _checkerAvailable = false;


            int iteration = 0;
            if (!ChoosePathString(knot))
            {
                _checkerAvailable = true;
                yield break;
            }

            DebugLog("\n-----------------------------");
            DebugLog("* Exploring New Knot: " + knot);

            while (
                iteration < MAX_ITERATION)
            {
                // if (_exhaustedPathes.Contains(knot))
                //     break;

                _branchExploredToTheEnd = false;
                // _story.ResetState();
                ChoosePathString(knot);
                int exaustedBranchCount = _exhaustedPathes.Count;
                StartCoroutine(ExploreBranch(knot));
                yield return new WaitUntil(() => _branchExploredToTheEnd);
                _totalBranchesExploredCount++;
                OnTotalBranchesExploredCountChanged.Invoke(_totalBranchesExploredCount);
                if (exaustedBranchCount == _exhaustedPathes.Count)
                    break;
                iteration++;
            }
            _checkerAvailable = true;
        }

        private bool ChoosePathString(string knot)
        {
            try
            {
                _currentKnot = knot;
                _story.ChoosePathString(knot);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                _checkerAvailable = true;
                return false;
            }
        }

        private IEnumerator GoThroughLinearStory()
        {
            _linearStoryEnded = false;
            while (_story.canContinue)
            {
                string line = GoToNextLine();
                if (string.IsNullOrEmpty(line)) break;
                _processed = false;

                StartCoroutine(ProcessRoutine(line, _currentKnot));
                yield return WaitUntilProcessed;
                _narrativeState.UpdateInkState();
            }
            _linearStoryEnded = true;
        }

        private CustomYieldInstruction WaitUntilLinearStoryEnds
        {
            get
            {
                StartCoroutine(GoThroughLinearStory());
                return new WaitUntil(() => _linearStoryEnded);
            }
        }

        private IEnumerator ExploreBranch(string branchId)
        {

            yield return WaitUntilLinearStoryEnds;

            string id = branchId;
            while (_story.currentChoices.Count > 0)
            {
                if (!PickNextChoice(ref id))
                    break;
                yield return WaitUntilLinearStoryEnds;
            }

            DebugLog("* Branch: " + id);
            _exhaustedPathes.AddUnique(id);
            _branchExploredToTheEnd = true;

        }

        private bool PickNextChoice(ref string id)
        {
            int i = 0;
            while (i < _story.currentChoices.Count)
            {
                DebugLog("-- Choice " + i + ": " + _story.currentChoices[i].text);

                if (_exhaustedPathes.Contains(id + i))
                {
                    i++;
                    continue;
                }
                id += i;
                _story.ChooseChoiceIndex(i);
                return true;
            }
            return false;
        }

        protected abstract IEnumerator ProcessRoutine(string line, string knot);

        protected string GoToNextLine()
        {
            try
            {
                var line = _story.Continue();
                DebugLog($"--- {line}");
                return line;
            }
            catch (Exception)
            {

                Debug.LogWarning("Couldn't continue: " + _story.state.currentPathString);
                return string.Empty;
            }

        }


    }
}