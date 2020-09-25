namespace CurePlease.App.Addon
{
    public interface IAddon
    {
        void Load();
        void Verify();
        void Unload();
        void BindHotKeys();
        void UnbindHotKeys();
    }
}
