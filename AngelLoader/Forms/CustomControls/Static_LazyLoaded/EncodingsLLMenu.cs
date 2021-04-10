using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class EncodingsLLMenu
    {
        #region Private fields

        private static bool _constructed;

        private static MainForm _owner = null!;

        #region Backing fields

        private static int _codePage;

        private static ToolStripMenuItemCustom? AutodetectMenuItem;
        private static ToolStripMenuItemCustom? ArabicMenu;
        private static ToolStripMenuItemCustom? BalticMenu;
        private static ToolStripMenuItemCustom? CentralEuropeanMenu;
        private static ToolStripMenuItemCustom? ChineseMenu;
        private static ToolStripMenuItemCustom? CyrillicMenu;
        private static ToolStripMenuItemCustom? EasternEuropeanMenu;
        private static ToolStripMenuItemCustom? GreekMenu;
        private static ToolStripMenuItemCustom? HebrewMenu;
        private static ToolStripMenuItemCustom? JapaneseMenu;
        private static ToolStripMenuItemCustom? KoreanMenu;
        private static ToolStripMenuItemCustom? LatinMenu;
        private static ToolStripMenuItemCustom? NorthernEuropeanMenu;
        private static ToolStripMenuItemCustom? TaiwanMenu;
        private static ToolStripMenuItemCustom? ThaiMenu;
        private static ToolStripMenuItemCustom? TurkishMenu;
        private static ToolStripMenuItemCustom? UnitedStatesMenu;
        private static ToolStripMenuItemCustom? VietnameseMenu;
        private static ToolStripMenuItemCustom? WesternEuropeanMenu;
        private static ToolStripMenuItemCustom? OtherMenu;

        #endregion

        private static readonly Dictionary<int, ToolStripMenuItemWithBackingField<int>> _menuItemsDict = new();

        #endregion

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        private static void MenuItems_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItemWithBackingField<int> menuItem) return;
            if (!menuItem.Checked) return;

            foreach (var item in _menuItemsDict)
            {
                var curMenuItem = item.Value;

                if (!curMenuItem.CheckOnClick) continue;

                if (curMenuItem != menuItem)
                {
                    curMenuItem.Checked = false;
                }
                else
                {
                    if (!curMenuItem.Checked) curMenuItem.Checked = true;
                }
            }
        }

        #region Public methods

        internal static DarkContextMenu Menu = null!;

        internal static void Construct(MainForm owner)
        {
            if (_constructed) return;

            _owner = owner;

            Menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LazyLoaded.True };

            #region Item init

            Menu.Items.AddRange(new ToolStripItem[]
            {
                AutodetectMenuItem = new ToolStripMenuItemWithBackingField<int>(-1) { Tag = LazyLoaded.True },
                new ToolStripSeparator { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(65001) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1200) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1201) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(12000) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(12001) { Tag = LazyLoaded.True },
                new ToolStripSeparator { Tag = LazyLoaded.True }
            });

            Menu.Items.AddRange(new ToolStripItem[]
            {
                ArabicMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                BalticMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                CentralEuropeanMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                ChineseMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                CyrillicMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                EasternEuropeanMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                GreekMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                HebrewMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                JapaneseMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                KoreanMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                LatinMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                NorthernEuropeanMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                TaiwanMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                ThaiMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                TurkishMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                UnitedStatesMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                VietnameseMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                WesternEuropeanMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                OtherMenu = new ToolStripMenuItemCustom { Tag = LazyLoaded.True }
            });

            ArabicMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1256) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28596) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(720) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10004) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(708) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(864) { Tag = LazyLoaded.True }
            });

            BalticMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1257) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28594) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(775) { Tag = LazyLoaded.True }
            });

            CentralEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1250) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28592) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(852) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10029) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10082) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20106) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10010) { Tag = LazyLoaded.True }
            });

            ChineseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(51936) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(936) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20936) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(54936) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(52936) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(50227) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10008) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(950) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20000) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20002) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10002) { Tag = LazyLoaded.True }
            });

            CyrillicMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1251) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28595) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(866) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10007) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(855) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20866) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(21866) { Tag = LazyLoaded.True }
            });

            EasternEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(10017) { Tag = LazyLoaded.True }
            });

            GreekMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1253) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28597) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(737) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(869) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10006) { Tag = LazyLoaded.True }
            });

            HebrewMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1255) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(862) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28598) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(38598) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10005) { Tag = LazyLoaded.True }
            });

            JapaneseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(932) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(51932) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(50220) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(50221) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(50222) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10001) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20932) { Tag = LazyLoaded.True }
            });

            KoreanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(949) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(51949) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(50225) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1361) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10003) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20949) { Tag = LazyLoaded.True }
            });

            LatinMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(858) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28593) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28605) { Tag = LazyLoaded.True }
            });

            NorthernEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(28603) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(861) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10079) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(865) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20108) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20107) { Tag = LazyLoaded.True }
            });

            TaiwanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(20001) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20004) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20005) { Tag = LazyLoaded.True }
            });

            ThaiMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(874) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10021) { Tag = LazyLoaded.True }
            });

            TurkishMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1254) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28599) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(857) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10081) { Tag = LazyLoaded.True }
            });

            UnitedStatesMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(20127) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(437) { Tag = LazyLoaded.True }
            });

            VietnameseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1258) { Tag = LazyLoaded.True }
            });

            WesternEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1252) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(28591) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(850) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(10000) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20105) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(863) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(860) { Tag = LazyLoaded.True }
            });

            OtherMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(29001) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20003) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20420) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20880) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(21025) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20277) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1142) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20278) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1143) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20297) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1147) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20273) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1141) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20423) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(875) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20424) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20871) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1149) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(500) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1148) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20280) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1144) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20290) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20833) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(870) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20284) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1145) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20838) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20905) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1026) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20285) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1146) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(37) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1140) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(1047) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20924) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57002) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57003) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57004) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57005) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57006) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57007) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57008) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57009) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57010) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(57011) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20269) { Tag = LazyLoaded.True },
                new ToolStripMenuItemWithBackingField<int>(20261) { Tag = LazyLoaded.True }
            });

            #endregion

            static void InitItem(ToolStripItem item)
            {
                if (item is ToolStripMenuItemWithBackingField<int> encItem)
                {
                    if (encItem.Field >= 0)
                    {
                        _menuItemsDict[encItem.Field] = encItem;
                        var enc = Encoding.GetEncoding(encItem.Field);
                        encItem.Text = enc.EncodingName + " (" + enc.CodePage + ")";
                        encItem.CheckOnClick = true;
                        encItem.CheckedChanged += MenuItems_CheckedChanged;
                    }
                    encItem.Click += _owner.ReadmeEncodingMenuItems_Click;
                }
            }

            foreach (ToolStripItem baseItem in Menu.Items)
            {
                InitItem(baseItem);
                if (baseItem is ToolStripMenuItem baseMenuItem && baseMenuItem.DropDownItems.Count > 0)
                {
                    foreach (ToolStripItem item in baseMenuItem.DropDownItems)
                    {
                        InitItem(item);
                    }
                }
            }

            if (_menuItemsDict.TryGetValue(_codePage, out var menuItem))
            {
                menuItem.Checked = true;
            }

            _constructed = true;

            Localize();
        }

        internal static void SetEncodingMenuItemChecked(Encoding encoding)
        {
            if (_constructed && _menuItemsDict.TryGetValue(encoding.CodePage, out var menuItem))
            {
                menuItem.Checked = true;
            }
            else
            {
                _codePage = encoding.CodePage;
            }
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            AutodetectMenuItem!.Text = LText.Global.Autodetect;
            ArabicMenu!.Text = LText.CharacterEncodingCategories.Arabic;
            BalticMenu!.Text = LText.CharacterEncodingCategories.Baltic;
            CentralEuropeanMenu!.Text = LText.CharacterEncodingCategories.CentralEuropean;
            ChineseMenu!.Text = LText.CharacterEncodingCategories.Chinese;
            CyrillicMenu!.Text = LText.CharacterEncodingCategories.Cyrillic;
            EasternEuropeanMenu!.Text = LText.CharacterEncodingCategories.EasternEuropean;
            GreekMenu!.Text = LText.CharacterEncodingCategories.Greek;
            HebrewMenu!.Text = LText.CharacterEncodingCategories.Hebrew;
            JapaneseMenu!.Text = LText.CharacterEncodingCategories.Japanese;
            KoreanMenu!.Text = LText.CharacterEncodingCategories.Korean;
            LatinMenu!.Text = LText.CharacterEncodingCategories.Latin;
            NorthernEuropeanMenu!.Text = LText.CharacterEncodingCategories.NorthernEuropean;
            TaiwanMenu!.Text = LText.CharacterEncodingCategories.Taiwan;
            ThaiMenu!.Text = LText.CharacterEncodingCategories.Thai;
            TurkishMenu!.Text = LText.CharacterEncodingCategories.Turkish;
            UnitedStatesMenu!.Text = LText.CharacterEncodingCategories.UnitedStates;
            VietnameseMenu!.Text = LText.CharacterEncodingCategories.Vietnamese;
            WesternEuropeanMenu!.Text = LText.CharacterEncodingCategories.WesternEuropean;
            OtherMenu!.Text = LText.CharacterEncodingCategories.Other;
        }

        #endregion
    }
}
