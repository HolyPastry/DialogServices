using Ink.Runtime;
using UnityEngine;

namespace Bakery
{
    public interface IDialogManager
    {
        WaitUntil WaitUntilReady { get; }
        WaitUntil WaitUntilDialogEnds { get; }

        Bakery.EnumPlayMode PlayMode { get; set; }
        float NarrationSpeed { get; set; }

        bool IsDialogInProgress { get; }

        public Story StoryRef { get; }

        void MakeChoice(int choiceIndex);
        void Play(string inkKnot);
        void Interrupt();

        void AddDelay(EnumDelayType delayType, float delay);
        void SkipOneLine();
        void SkipToNextChoice();


    }
}