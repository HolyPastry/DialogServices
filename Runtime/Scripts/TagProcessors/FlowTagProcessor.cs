using System.Collections.Generic;



namespace Bakery.Dialogs
{
    public class FlowTagProcessor : TagProcessor
    {
        public override EnumStep Step => EnumStep.AfterLine;

        public override List<string> ProcessTags(CharacterData character, List<string> tags)
        {
            if (tags == null) return new();
            var newTags = new List<string>(tags);
            foreach (string tag in tags)
            {
                if (!tag.StartsWith("LoadNextScene")) continue;
                newTags.Remove(tag);
                FlowServices.LoadNextScene();
            }
            return newTags;
        }
    }
}