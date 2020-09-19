using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurePlease.Addon
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
