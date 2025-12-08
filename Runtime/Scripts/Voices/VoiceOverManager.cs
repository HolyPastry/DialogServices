using UnityEngine;

namespace Bakery
{
    public abstract class VoiceOverManager : MonoBehaviour
    {
        public abstract float LineDuration { get; }
        public abstract Coroutine LoadLine(CharacterData data, string line);
        public abstract void SayLoadedLine();
        public abstract void Stop();

    }
}