
using UnityEngine;
using System.Collections.Generic;




namespace Bakery.Dialogs
{

    public abstract class TagProcessor : MonoBehaviour
    {
        public enum EnumStep
        {
            BeforeLine,
            AfterLine
        }
        public virtual EnumStep Step => EnumStep.AfterLine;
        public abstract List<string> ProcessTags(CharacterData character, List<string> tags);

        public static bool ParseInt(ref List<string> tags, string key, out int value)
        {
            if (tags == null)
            {
                value = -1;
                return false;
            }
            int i = 0;
            while (i < tags.Count)
            {
                if (ParseInt(tags[i], key, out value))
                {
                    tags.RemoveAt(i);
                    return true;
                }
                i++;
            }

            value = -1;
            return false;
        }

        public static bool Parse(ref List<string> tags, string key, out List<string> values)
        {
            if (tags == null)
            {
                values = new();
                return false;
            }
            int i = 0;

            values = new List<string>();
            while (i < tags.Count)
            {
                if (Parse(tags[i], key, out string value))
                {
                    tags.RemoveAt(i);
                    values.Add(value);
                    continue;
                }
                i++;
            }


            return values.Count > 0;
        }

        public static bool ParseInt(string tag, string key, out int value)
        {
            if (tag.StartsWith(key))
            {
                try
                {
                    value = int.Parse(tag[key.Length..]);
                }
                catch
                {
                    value = -1;
                    return false;
                }

                return true;
            }
            value = -1;
            return false;
        }

        public static bool Parse(string tag, string key, out string value)
        {
            if (tag.StartsWith(key))
            {

                value = tag[key.Length..];
                return true;
            }
            value = "";
            return false;
        }
    }
}