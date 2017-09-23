using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers.PatchManager;

namespace PluginVR
{
    [Plugin("VirtualReality", typeof(VirtualReality), "34d7de14-8621-41ad-984b-7d7d4b8d7ff3")]
    public class VirtualReality : TorchPluginBase
    {
        public override void Init(ITorchBase torch)
        {
            Console.WriteLine("Plugin init");
            base.Init(torch);
            if (Torch.Managers.GetManager<PatchManager>() == null)
                Torch.Managers.AddManager(new PatchManager(torch));
            Torch.Managers.AddManager(new VRManager(torch));
        }
    }
}
