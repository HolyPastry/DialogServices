using System;
using Bakery.Dialogs;
using UnityEngine;


public static partial class DialogServices
{
    public static Func<WaitUntil> WaitUntilReady = () => new WaitUntil(() => true);

    public static Action<int> MakeChoice = delegate { };

    public static Func<bool> IsDialogInProgress = delegate { return false; };
    public static Func<WaitUntil> WaitUntilDialogEnds = () => new WaitUntil(() => true);

    public static Action<string> Start = (inkKnot) => { };
    public static Func<string, bool> Exists = (character) => false;
    public static Action InterruptDialog = delegate { };

    public static Action<float> SetTextNarrationSpeed = (character) => { };

    public static Func<string, (CharacterData, string)> ExtractCharacter = (line) => { return (null, ""); };

    public static Func<string, bool> CheckNarrativeFlag = (condition) => false;
    public static Action<string, bool> SetNarrativeFlag = (flag, isTrue) => { };

    public static Action<EnumDelayType, float> AddDelay = (delayType, delay) => { };
}
