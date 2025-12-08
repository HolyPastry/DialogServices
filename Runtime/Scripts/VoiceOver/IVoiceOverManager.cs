using UnityEngine;

namespace Bakery
{
    public interface IVoiceOverManager
    {
        float LineDuration { get; }
        Coroutine LoadLine(ThespianData talkingCharacter, string line);
        void SayLine();
        void Stop();
    }
}