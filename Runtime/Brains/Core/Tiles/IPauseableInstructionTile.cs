namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IPauseableInstructionTile
    {
        void Pause();
        bool Resume();
    }
}