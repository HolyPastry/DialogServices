using System.Collections.Generic;
using Ink.Runtime;

namespace Bakery.Dialogs
{
    public struct DialogChoice
    {
        public List<string> Tags;
        public string Text;

        public DialogChoice(Choice choice)
        {
            if (choice.tags == null)
                Tags = new();
            else
                Tags = new(choice.tags);
            Text = choice.text;
        }
    }
}