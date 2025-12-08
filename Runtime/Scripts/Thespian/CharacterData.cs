
using Holypastry.Bakery;
using UnityEngine;

namespace Bakery
{
    [CreateAssetMenu(fileName = "Thespian", menuName = "Bakery/Dialogs/Thespian", order = 0)]
    public class ThespianData : ScriptableObject
    {
        public bool HideName;
        public string Name = string.Empty;
        public string VoiceId;
        public Sprite Portrait;
    }
}

