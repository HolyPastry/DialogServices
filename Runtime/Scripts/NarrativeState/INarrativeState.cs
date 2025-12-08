namespace Bakery
{
    public interface INarrativeState
    {
        void UpdateState();
        bool CheckFlag(string condition);
        void SetFlag(string flag, bool isTrue);
    }
}