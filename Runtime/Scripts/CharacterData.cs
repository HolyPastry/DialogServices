
using UnityEngine;

namespace Bakery.Dialogs
{
    [CreateAssetMenu(fileName = "DialogCharacter", menuName = "Dialogs/Character", order = 0)]
    public class CharacterData : ScriptableObject
    {
        public string Name;
        public bool HideName;
        public string ActorName = string.Empty;
    }
}

