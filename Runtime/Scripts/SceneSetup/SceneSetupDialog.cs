
using UnityEngine;
using System.Collections;
using Holypastry.Bakery.Flow;



namespace Bakery.Dialogs
{
    public class SceneSetupDialog : SceneSetupScript
    {
        [SerializeField] string inkKnot = string.Empty;
        [SerializeField] bool _waitUntilDialogEnds = false;

        public override IEnumerator Routine()
        {
            DialogServices.Start(inkKnot);
            if (_waitUntilDialogEnds)
                yield return DialogServices.WaitUntilDialogEnds();
        }
    }
}