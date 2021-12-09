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

                Menu.DarkModeEnabled = _darkModeEnabled;
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

            Menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };

            #region Item init

            Menu.Items.AddRange(new ToolStripItem[]
            {
                AutodetectMenuItem = new ToolStripMenuItemWithBackingField<int>(-1) { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(65001) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1200) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1201) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(12000) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(12001) { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy }
            });

            Menu.Items.AddRange(new ToolStripItem[]
            {
                ArabicMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                BalticMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                CentralEuropeanMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                ChineseMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                CyrillicMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                EasternEuropeanMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                GreekMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                HebrewMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                JapaneseMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                KoreanMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                LatinMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                NorthernEuropeanMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                TaiwanMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                ThaiMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                TurkishMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                UnitedStatesMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                VietnameseMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                WesternEuropeanMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                OtherMenu = new ToolStripMenuItemCustom { Tag = LoadType.Lazy }
            });

            ArabicMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1256) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28596) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(720) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10004) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(708) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(864) { Tag = LoadType.Lazy }
            });

            BalticMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1257) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28594) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(775) { Tag = LoadType.Lazy }
            });

            CentralEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1250) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28592) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(852) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10029) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10082) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20106) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10010) { Tag = LoadType.Lazy }
            });

            ChineseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(51936) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(936) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20936) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(54936) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(52936) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(50227) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10008) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(950) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20000) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20002) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10002) { Tag = LoadType.Lazy }
            });

            CyrillicMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1251) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28595) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(866) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10007) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(855) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20866) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(21866) { Tag = LoadType.Lazy }
            });

            EasternEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(10017) { Tag = LoadType.Lazy }
            });

            GreekMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1253) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28597) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(737) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(869) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10006) { Tag = LoadType.Lazy }
            });

            HebrewMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1255) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(862) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28598) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(38598) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10005) { Tag = LoadType.Lazy }
            });

            JapaneseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(932) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(51932) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(50220) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(50221) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(50222) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10001) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20932) { Tag = LoadType.Lazy }
            });

            KoreanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(949) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(51949) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(50225) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1361) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10003) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20949) { Tag = LoadType.Lazy }
            });

            LatinMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(858) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28593) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28605) { Tag = LoadType.Lazy }
            });

            NorthernEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(28603) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(861) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10079) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(865) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20108) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20107) { Tag = LoadType.Lazy }
            });

            TaiwanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(20001) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20004) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20005) { Tag = LoadType.Lazy }
            });

            ThaiMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(874) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10021) { Tag = LoadType.Lazy }
            });

            TurkishMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1254) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28599) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(857) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10081) { Tag = LoadType.Lazy }
            });

            UnitedStatesMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(20127) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(437) { Tag = LoadType.Lazy }
            });

            VietnameseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1258) { Tag = LoadType.Lazy }
            });

            WesternEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1252) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(28591) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(850) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(10000) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20105) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(863) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(860) { Tag = LoadType.Lazy }
            });

            OtherMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(29001) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20003) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20420) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20880) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(21025) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20277) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1142) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20278) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1143) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20297) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1147) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20273) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1141) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20423) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(875) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20424) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20871) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1149) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(500) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1148) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20280) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1144) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20290) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20833) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(870) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20284) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1145) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20838) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20905) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1026) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20285) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1146) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(37) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1140) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(1047) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20924) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57002) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57003) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57004) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57005) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57006) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57007) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57008) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57009) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57010) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(57011) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20269) { Tag = LoadType.Lazy },
                new ToolStripMenuItemWithBackingField<int>(20261) { Tag = LoadType.Lazy }
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

            AutodetectMenuItem!.Text = LText.CharacterEncoding.AutodetectNow;
            ArabicMenu!.Text = LText.CharacterEncoding.Category_Arabic;
            BalticMenu!.Text = LText.CharacterEncoding.Category_Baltic;
            CentralEuropeanMenu!.Text = LText.CharacterEncoding.Category_CentralEuropean;
            ChineseMenu!.Text = LText.CharacterEncoding.Category_Chinese;
            CyrillicMenu!.Text = LText.CharacterEncoding.Category_Cyrillic;
            EasternEuropeanMenu!.Text = LText.CharacterEncoding.Category_EasternEuropean;
            GreekMenu!.Text = LText.CharacterEncoding.Category_Greek;
            HebrewMenu!.Text = LText.CharacterEncoding.Category_Hebrew;
            JapaneseMenu!.Text = LText.CharacterEncoding.Category_Japanese;
            KoreanMenu!.Text = LText.CharacterEncoding.Category_Korean;
            LatinMenu!.Text = LText.CharacterEncoding.Category_Latin;
            NorthernEuropeanMenu!.Text = LText.CharacterEncoding.Category_NorthernEuropean;
            TaiwanMenu!.Text = LText.CharacterEncoding.Category_Taiwan;
            ThaiMenu!.Text = LText.CharacterEncoding.Category_Thai;
            TurkishMenu!.Text = LText.CharacterEncoding.Category_Turkish;
            UnitedStatesMenu!.Text = LText.CharacterEncoding.Category_UnitedStates;
            VietnameseMenu!.Text = LText.CharacterEncoding.Category_Vietnamese;
            WesternEuropeanMenu!.Text = LText.CharacterEncoding.Category_WesternEuropean;
            OtherMenu!.Text = LText.CharacterEncoding.Category_Other;
        }

        #endregion
    }
}
