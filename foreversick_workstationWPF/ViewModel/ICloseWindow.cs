using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foreversick_workstationWPF.ViewModel
{
    interface ICloseWindow
    {
        public Action Close { get; set; }
        public bool CanClose();
    }
}
