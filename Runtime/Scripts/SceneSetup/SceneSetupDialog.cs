
using UnityEngine;
using System.Collections;

namespace Bakery
{
    public class SceneSetupDialog : SceneSetupScript
    {
        [SerializeField] string inkKnot = string.Empty;
        [SerializeField] bool _waitUntilDialogEnds = false;

        public override IEnumerator Routine()
        {
            Dialogs.Manager().Play(inkKnot);
            if (_waitUntilDialogEnds)
                yield return Dialogs.Manager().WaitUntilDialogEnds;
        }
    }
}