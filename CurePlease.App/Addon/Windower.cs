using System.Threading;
using EliteMMO.API;

namespace CurePlease.App.Addon
{
    public class Windower : IAddon
    {
        private readonly EliteAPI eliteApi;

        public Windower(EliteAPI eliteApi)
        {
            this.eliteApi = eliteApi;
        }

        public void BindHotKeys()
        {
            eliteApi.ThirdParty.SendString("//bind ^!F1 cureplease toggle");
            eliteApi.ThirdParty.SendString("//bind ^!F2 cureplease start");
            eliteApi.ThirdParty.SendString("//bind ^!F3 cureplease pause");
        }

        public void Load()
        {
            eliteApi.ThirdParty.SendString("//lua load CurePlease_addon");
            Thread.Sleep(1500);
            eliteApi.ThirdParty.SendString("//cpaddon settings " + OptionsForm.config.ipAddress + " " + OptionsForm.config.listeningPort);
            Thread.Sleep(1000);
            Verify();
        }

        public void UnbindHotKeys()
        {
            eliteApi.ThirdParty.SendString("//unbind ^!F1");
            eliteApi.ThirdParty.SendString("//unbind ^!F2");
            eliteApi.ThirdParty.SendString("//unbind ^!F3");
        }

        public void Unload()
        {
            eliteApi.ThirdParty.SendString("//lua unload CurePlease_addon");
        }

        public void Verify()
        {
            eliteApi.ThirdParty.SendString(string.Format("//cpaddon verify"));
        }
    }
}
