using System.Collections.Generic;

namespace Bakery.Dialogs
{
    public class DialogTagProcessor : TagProcessor
    {
        public override EnumStep Step => EnumStep.BeforeLine;
        public override List<string> ProcessTags(CharacterData character, List<string> tags)
        {
            if (tags == null) return new();
            var newTags = new List<string>(tags);

            if (Parse(ref newTags, "DelayBefore", out List<string> delays))
                DialogServices.AddDelay(EnumDelayType.BeforeLine, ToFloat(delays[0]));

            if (Parse(ref newTags, "DelayAfter", out List<string> delays2))
                DialogServices.AddDelay(EnumDelayType.AfterLine, ToFloat(delays2[0]));

            if (Parse(ref newTags, "Overlap", out List<string> overlaps))
                DialogServices.AddDelay(EnumDelayType.OverlapLine, ToFloat(overlaps[0]));


            return newTags;
        }

        public float ToFloat(string value)
        {
            if (float.TryParse(value, out float result))
                return result;
            else
                return 0;
        }
    }
}