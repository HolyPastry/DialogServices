namespace Bakery
{
    public interface IThespianManager
    {
        ThespianData TalkingCharacter { get; set; }
        bool Exists(string character);
        (ThespianData, string) Extract(string line);
    }
}