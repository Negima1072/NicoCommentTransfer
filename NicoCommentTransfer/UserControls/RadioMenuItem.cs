using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NicoCommentTransfer.UserControls
{
    class RadioMenuItem : MenuItem
    {
        public string GroupName { get; set; }
        protected override void OnClick()
        {
            var ic = Parent as ItemsControl;
            if(null != ic)
            {
                var rmi = ic.Items.OfType<RadioMenuItem>().FirstOrDefault(i => i.GroupName == GroupName && i.IsChecked);
                if (null != rmi) rmi.IsChecked = false;
                IsChecked = true;
            }
            base.OnClick();
        }
    }
}
