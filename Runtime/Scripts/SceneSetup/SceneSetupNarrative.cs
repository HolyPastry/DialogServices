
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holypastry.Bakery.Flow;



namespace Bakery.Dialogs
{

    public class SceneSetupNarrative : SceneSetupScript
    {
        [SerializeField] List<NarrativeBool> _narrativeBool = new();
        public override IEnumerator Routine()
        {
            foreach (var narrative in _narrativeBool)
                DialogServices.SetNarrativeFlag(narrative.Key, narrative.Value);
            yield break;
        }
    }
}