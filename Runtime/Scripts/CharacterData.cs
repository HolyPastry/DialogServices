
using Holypastry.Bakery;
using UnityEngine;

namespace Bakery
{
    [CreateAssetMenu(fileName = "DialogCharacter", menuName = "Dialogs/Character", order = 0)]
    public class CharacterData : ContentTag
    {
        public bool HideName;
        public string ActorName = string.Empty;
    }
}

