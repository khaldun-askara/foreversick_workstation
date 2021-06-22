using System;

namespace foreversick_workstationWPF.ViewModel
{
    interface ICloseWindow
    {
        public Action Close { get; set; }
        public bool CanClose();
    }
}
