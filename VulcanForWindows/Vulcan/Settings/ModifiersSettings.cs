using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulcanTest.Vulcan.Settings
{
    public class ModifiersSettings
    {
        public ModifierSettings PlusSettings { get; } = new("Options_ValueOfPlus", 0.5m);
        public ModifierSettings MinusSettings { get; } = new("Options_ValueOfMinus", -0.25m);
    }
    public class ModifierSettings
    {
        public decimal SelectedValue { get; set; }

       public bool UsesCustomValue { get; set; }

        public ModifierSettings(string key, decimal defaultValue)
        {
            //SelectedValue = decimal.Parse(Preferences.Get(key, defaultValue.ToString(CultureInfo.InvariantCulture)),
            //    CultureInfo.InvariantCulture);

            //UsesCustomValue = Preferences.Get($"{key}IsCustom", false);

            //this.WhenAnyValue(v => v.SelectedValue)
            //    .Subscribe(value
            //        => Preferences.Set(key, value.ToString(CultureInfo.InvariantCulture)));

            //this.WhenAnyValue(vm => vm.UsesCustomValue)
            //    .Subscribe(value =>
            //    {
            //        Preferences.Set($"{key}IsCustom", value);
            //    });

            //TODO: SAVE
        }
    }
}