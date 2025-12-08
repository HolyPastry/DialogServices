
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bakery
{

    public class SceneSetupNarrative : SceneSetupScript
    {
        [SerializeField] List<NarrativeBool> _narrativeBool = new();
        public override IEnumerator Routine()
        {
            foreach (var narrative in _narrativeBool)
                Dialogs.NarrativeState().SetFlag(narrative.Key, narrative.Value);
            yield break;
        }
    }
}