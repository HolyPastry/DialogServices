
using Holypastry.Bakery;
using UnityEngine;

namespace Bakery
{
    [CreateAssetMenu(fileName = "Thespian", menuName = "Bakery/Dialogs/Thespian", order = 0)]
    public class ThespianData : ContentTag
    {
        public bool HideName;
        public string ActorName = string.Empty;
    }
}

