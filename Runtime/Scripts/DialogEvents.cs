using System;
using System.Collections.Generic;



namespace Bakery.Dialogs
{
    public static class DialogEvents
    {
        public static Action OnDialogStart = delegate { };
        public static Action<CharacterData, string, List<string>, float> OnStoryNextLine =
            (character, text, tags, lineDuration) => { };

        public static Action<List<DialogChoice>> OnChoiceAvailable = delegate { };

        public static Action OnDialogEnd = delegate { };

        public static Action OnDialogInRangeUpdate = delegate { };

        public static Action BeforeNewLine = delegate { };
    }
}