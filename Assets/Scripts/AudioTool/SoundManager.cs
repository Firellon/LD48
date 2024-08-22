using AudioTools.Sound;
using Utilities.Prefabs;
using Utilities.RandomService;

namespace LD48.AudioTool
{
    public class SoundManager : SoundManager<SoundType>
    {
        public SoundManager(IPrefabPool prefabPool, IRandomService randomService) : base(prefabPool, randomService, SoundType.Unspecified)
        {
        }
    }
}