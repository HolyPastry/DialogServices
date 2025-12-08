using System;
using System.Collections.Generic;


namespace Bakery
{
    [Serializable]
    public class SerialNarrative : ISerialData
    {
        public const string KeyName = "NarrativeState";
        public List<NarrativeBool> NarrativeBools;

        public SerialNarrative(SerialNarrative serialNarrative)
        {
            NarrativeBools = new List<NarrativeBool>(serialNarrative.NarrativeBools);
        }

        public SerialNarrative()
        {
            NarrativeBools = new();
        }

        internal void Clear()
        {
            NarrativeBools.Clear();
        }

        public void Deserialize()
        { }

        public void Serialize()
        {
        }
    }

}