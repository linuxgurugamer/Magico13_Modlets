using System;
using System.Linq;

namespace NotInMyBackYard
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class NIMBYScenario : ScenarioModule
    {
        public override void OnLoad(ConfigNode node)
        {
            NIMBYEvent.Instance.taggedVessels.Clear();
            string[] taggedVessels = node.GetValues("taggedVessel");
            for (int i = 0; i < taggedVessels.Length; i++)
            {
                string s = taggedVessels.ElementAt(i);
                NIMBYEvent.Instance.taggedVessels.Add(new Guid(s));
            }
        }

        public override void OnSave(ConfigNode node)
        {
            for (int i = 0; i < NIMBYEvent.Instance.taggedVessels.Count; i++)
            {
                node.AddValue("taggedVessel", NIMBYEvent.Instance.taggedVessels.ElementAt(i));
            }
        }
    }
}