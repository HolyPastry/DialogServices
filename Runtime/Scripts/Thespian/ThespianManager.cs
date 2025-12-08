using Holypastry.Bakery;
using UnityEngine;

namespace Bakery
{
    public class ThespianManager : MonoBehaviour, IThespianManager
    {
        [SerializeField] private string _collectionFolder = "Characters";
        private DataCollection<ThespianData> _dataCollection;

        public ThespianData TalkingCharacter { get; set; }
        void Awake()
        {
            _dataCollection = new DataCollection<ThespianData>(_collectionFolder);
        }

        void OnEnable()
        {
            Dialogs.Thespians = () => this;
        }
        void OnDisable()
        {
            Dialogs.Thespians = Dialogs.UnregisterThespianManager;
        }

        public (ThespianData, string) Extract(string line)
        {

            if (!line.Contains(":")) return (null, line);

            string[] split = line.Split(':');
            string characterStr = split[0];
            line = split[1];

            ThespianData character = _dataCollection.Find(x => x.name == characterStr);
            if (character == null)
            {
                Debug.LogWarning($"Character {characterStr} not found in data collection\n{line}");
                return (null, line);
            }
            return (character, line);
        }

        public bool Exists(string thespianName)
         => _dataCollection.Exists(x => x.name == thespianName);
    }
}