using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class EncodingsLLMenu : IEventDisabler
    {
        #region Private fields

        public bool EventsDisabled { get; set; }

        private bool _constructed;

        private readonly MainForm _owner;

        #region Backing fields

        private int _codePage;

        private ToolStripMenuItemCustom? AutodetectMenuItem;
        private ToolStripMenuItemCustom? ArabicMenu;
        private ToolStripMenuItemCustom? BalticMenu;
        private ToolStripMenuItemCustom? CentralEuropeanMenu;
        private ToolStripMenuItemCustom? ChineseMenu;
        private ToolStripMenuItemCustom? CyrillicMenu;
        private ToolStripMenuItemCustom? EasternEuropeanMenu;
        private ToolStripMenuItemCustom? GreekMenu;
        private ToolStripMenuItemCustom? HebrewMenu;
        private ToolStripMenuItemCustom? JapaneseMenu;
        private ToolStripMenuItemCustom? KoreanMenu;
        private ToolStripMenuItemCustom? LatinMenu;
        private ToolStripMenuItemCustom? NorthernEuropeanMenu;
        private ToolStripMenuItemCustom? TaiwanMenu;
        private ToolStripMenuItemCustom? ThaiMenu;
        private ToolStripMenuItemCustom? TurkishMenu;
        private ToolStripMenuItemCustom? UnitedStatesMenu;
        private ToolStripMenuItemCustom? VietnameseMenu;
        private ToolStripMenuItemCustom? WesternEuropeanMenu;
        private ToolStripMenuItemCustom? OtherMenu;

        #endregion

        private readonly Dictionary<int, ToolStripMenuItemWithBackingField<int>> _menuItemsDict = new();

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal EncodingsLLMenu(MainForm owner) => _owner = owner;

        private void MenuItems_CheckedChanged(object sender, EventArgs e)
        {
            if (EventsDisabled) return;
            if (sender is not ToolStripMenuItemCustom { Checked: true } senderItem) return;

            using (new DisableEvents(this))
            {
                foreach (ToolStripItem item in _menu.Items)
                {
                    if (item is ToolStripMenuItemCustom menuItem && menuItem != senderItem)
                    {
                        menuItem.Checked = false;

                        foreach (ToolStripItem dropDownItem in menuItem.DropDownItems)
                        {
                            if (dropDownItem is ToolStripMenuItemCustom dropDownMenuItem && dropDownItem != senderItem)
                            {
                                dropDownMenuItem.Checked = false;
                            }
                        }
                    }
                }

                senderItem.Checked = true;
                if (senderItem.OwnerItem is ToolStripMenuItemCustom { HasDropDownItems: true } ownerItem)
                {
                    ownerItem.Checked = true;
                }
            }
        }

        #region Public methods

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                Construct();
                return _menu;
            }
        }

        private void Construct()
        {
            if (_constructed) return;

            _menu = new DarkContextMenu(_owner.GetComponents()) { Tag = LoadType.Lazy };

            #region Item init

            _menu.Items.AddRange(new ToolStripItem[]
            {
                AutodetectMenuItem = new ToolStripMenuItemWithBackingField<int>(-1),
                new ToolStripSeparator(),
                new ToolStripMenuItemWithBackingField<int>(65001),
                new ToolStripMenuItemWithBackingField<int>(1200),
                new ToolStripMenuItemWithBackingField<int>(1201),
                new ToolStripMenuItemWithBackingField<int>(12000),
                new ToolStripMenuItemWithBackingField<int>(12001),
                new ToolStripSeparator()
            });

            _menu.Items.AddRange(new ToolStripItem[]
            {
                ArabicMenu = new ToolStripMenuItemCustom(),
                BalticMenu = new ToolStripMenuItemCustom(),
                CentralEuropeanMenu = new ToolStripMenuItemCustom(),
                ChineseMenu = new ToolStripMenuItemCustom(),
                CyrillicMenu = new ToolStripMenuItemCustom(),
                EasternEuropeanMenu = new ToolStripMenuItemCustom(),
                GreekMenu = new ToolStripMenuItemCustom(),
                HebrewMenu = new ToolStripMenuItemCustom(),
                JapaneseMenu = new ToolStripMenuItemCustom(),
                KoreanMenu = new ToolStripMenuItemCustom(),
                LatinMenu = new ToolStripMenuItemCustom(),
                NorthernEuropeanMenu = new ToolStripMenuItemCustom(),
                TaiwanMenu = new ToolStripMenuItemCustom(),
                ThaiMenu = new ToolStripMenuItemCustom(),
                TurkishMenu = new ToolStripMenuItemCustom(),
                UnitedStatesMenu = new ToolStripMenuItemCustom(),
                VietnameseMenu = new ToolStripMenuItemCustom(),
                WesternEuropeanMenu = new ToolStripMenuItemCustom(),
                OtherMenu = new ToolStripMenuItemCustom()
            });

            ArabicMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1256),
                new ToolStripMenuItemWithBackingField<int>(28596),
                new ToolStripMenuItemWithBackingField<int>(720),
                new ToolStripMenuItemWithBackingField<int>(10004),
                new ToolStripMenuItemWithBackingField<int>(708),
                new ToolStripMenuItemWithBackingField<int>(864)
            });

            BalticMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1257),
                new ToolStripMenuItemWithBackingField<int>(28594),
                new ToolStripMenuItemWithBackingField<int>(775)
            });

            CentralEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1250),
                new ToolStripMenuItemWithBackingField<int>(28592),
                new ToolStripMenuItemWithBackingField<int>(852),
                new ToolStripMenuItemWithBackingField<int>(10029),
                new ToolStripMenuItemWithBackingField<int>(10082),
                new ToolStripMenuItemWithBackingField<int>(20106),
                new ToolStripMenuItemWithBackingField<int>(10010)
            });

            ChineseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(51936),
                new ToolStripMenuItemWithBackingField<int>(936),
                new ToolStripMenuItemWithBackingField<int>(20936),
                new ToolStripMenuItemWithBackingField<int>(54936),
                new ToolStripMenuItemWithBackingField<int>(52936),
                new ToolStripMenuItemWithBackingField<int>(50227),
                new ToolStripMenuItemWithBackingField<int>(10008),
                new ToolStripMenuItemWithBackingField<int>(950),
                new ToolStripMenuItemWithBackingField<int>(20000),
                new ToolStripMenuItemWithBackingField<int>(20002),
                new ToolStripMenuItemWithBackingField<int>(10002)
            });

            CyrillicMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1251),
                new ToolStripMenuItemWithBackingField<int>(28595),
                new ToolStripMenuItemWithBackingField<int>(866),
                new ToolStripMenuItemWithBackingField<int>(10007),
                new ToolStripMenuItemWithBackingField<int>(855),
                new ToolStripMenuItemWithBackingField<int>(20866),
                new ToolStripMenuItemWithBackingField<int>(21866)
            });

            EasternEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(10017)
            });

            GreekMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1253),
                new ToolStripMenuItemWithBackingField<int>(28597),
                new ToolStripMenuItemWithBackingField<int>(737),
                new ToolStripMenuItemWithBackingField<int>(869),
                new ToolStripMenuItemWithBackingField<int>(10006)
            });

            HebrewMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1255),
                new ToolStripMenuItemWithBackingField<int>(862),
                new ToolStripMenuItemWithBackingField<int>(28598),
                new ToolStripMenuItemWithBackingField<int>(38598),
                new ToolStripMenuItemWithBackingField<int>(10005)
            });

            JapaneseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(932),
                new ToolStripMenuItemWithBackingField<int>(51932),
                new ToolStripMenuItemWithBackingField<int>(50220),
                new ToolStripMenuItemWithBackingField<int>(50221),
                new ToolStripMenuItemWithBackingField<int>(50222),
                new ToolStripMenuItemWithBackingField<int>(10001),
                new ToolStripMenuItemWithBackingField<int>(20932)
            });

            KoreanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(949),
                new ToolStripMenuItemWithBackingField<int>(51949),
                new ToolStripMenuItemWithBackingField<int>(50225),
                new ToolStripMenuItemWithBackingField<int>(1361),
                new ToolStripMenuItemWithBackingField<int>(10003),
                new ToolStripMenuItemWithBackingField<int>(20949)
            });

            LatinMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(858),
                new ToolStripMenuItemWithBackingField<int>(28593),
                new ToolStripMenuItemWithBackingField<int>(28605)
            });

            NorthernEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(28603),
                new ToolStripMenuItemWithBackingField<int>(861),
                new ToolStripMenuItemWithBackingField<int>(10079),
                new ToolStripMenuItemWithBackingField<int>(865),
                new ToolStripMenuItemWithBackingField<int>(20108),
                new ToolStripMenuItemWithBackingField<int>(20107)
            });

            TaiwanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(20001),
                new ToolStripMenuItemWithBackingField<int>(20004),
                new ToolStripMenuItemWithBackingField<int>(20005)
            });

            ThaiMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(874),
                new ToolStripMenuItemWithBackingField<int>(10021)
            });

            TurkishMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1254),
                new ToolStripMenuItemWithBackingField<int>(28599),
                new ToolStripMenuItemWithBackingField<int>(857),
                new ToolStripMenuItemWithBackingField<int>(10081)
            });

            UnitedStatesMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(20127),
                new ToolStripMenuItemWithBackingField<int>(437)
            });

            VietnameseMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1258)
            });

            WesternEuropeanMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(1252),
                new ToolStripMenuItemWithBackingField<int>(28591),
                new ToolStripMenuItemWithBackingField<int>(850),
                new ToolStripMenuItemWithBackingField<int>(10000),
                new ToolStripMenuItemWithBackingField<int>(20105),
                new ToolStripMenuItemWithBackingField<int>(863),
                new ToolStripMenuItemWithBackingField<int>(860)
            });

            OtherMenu.DropDown.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItemWithBackingField<int>(29001),
                new ToolStripMenuItemWithBackingField<int>(20003),
                new ToolStripMenuItemWithBackingField<int>(20420),
                new ToolStripMenuItemWithBackingField<int>(20880),
                new ToolStripMenuItemWithBackingField<int>(21025),
                new ToolStripMenuItemWithBackingField<int>(20277),
                new ToolStripMenuItemWithBackingField<int>(1142),
                new ToolStripMenuItemWithBackingField<int>(20278),
                new ToolStripMenuItemWithBackingField<int>(1143),
                new ToolStripMenuItemWithBackingField<int>(20297),
                new ToolStripMenuItemWithBackingField<int>(1147),
                new ToolStripMenuItemWithBackingField<int>(20273),
                new ToolStripMenuItemWithBackingField<int>(1141),
                new ToolStripMenuItemWithBackingField<int>(20423),
                new ToolStripMenuItemWithBackingField<int>(875),
                new ToolStripMenuItemWithBackingField<int>(20424),
                new ToolStripMenuItemWithBackingField<int>(20871),
                new ToolStripMenuItemWithBackingField<int>(1149),
                new ToolStripMenuItemWithBackingField<int>(500),
                new ToolStripMenuItemWithBackingField<int>(1148),
                new ToolStripMenuItemWithBackingField<int>(20280),
                new ToolStripMenuItemWithBackingField<int>(1144),
                new ToolStripMenuItemWithBackingField<int>(20290),
                new ToolStripMenuItemWithBackingField<int>(20833),
                new ToolStripMenuItemWithBackingField<int>(870),
                new ToolStripMenuItemWithBackingField<int>(20284),
                new ToolStripMenuItemWithBackingField<int>(1145),
                new ToolStripMenuItemWithBackingField<int>(20838),
                new ToolStripMenuItemWithBackingField<int>(20905),
                new ToolStripMenuItemWithBackingField<int>(1026),
                new ToolStripMenuItemWithBackingField<int>(20285),
                new ToolStripMenuItemWithBackingField<int>(1146),
                new ToolStripMenuItemWithBackingField<int>(37),
                new ToolStripMenuItemWithBackingField<int>(1140),
                new ToolStripMenuItemWithBackingField<int>(1047),
                new ToolStripMenuItemWithBackingField<int>(20924),
                new ToolStripMenuItemWithBackingField<int>(57002),
                new ToolStripMenuItemWithBackingField<int>(57003),
                new ToolStripMenuItemWithBackingField<int>(57004),
                new ToolStripMenuItemWithBackingField<int>(57005),
                new ToolStripMenuItemWithBackingField<int>(57006),
                new ToolStripMenuItemWithBackingField<int>(57007),
                new ToolStripMenuItemWithBackingField<int>(57008),
                new ToolStripMenuItemWithBackingField<int>(57009),
                new ToolStripMenuItemWithBackingField<int>(57010),
                new ToolStripMenuItemWithBackingField<int>(57011),
                new ToolStripMenuItemWithBackingField<int>(20269),
                new ToolStripMenuItemWithBackingField<int>(20261)
            });

            #endregion

            void InitItem(ToolStripItem item)
            {
                if (item is ToolStripMenuItemWithBackingField<int> encItem)
                {
                    if (encItem.Field >= 0)
                    {
                        _menuItemsDict[encItem.Field] = encItem;
                        var enc = Encoding.GetEncoding(encItem.Field);
                        encItem.Text = enc.EncodingName + " (" + enc.CodePage + ")";
                        encItem.CheckOnClick = true;
                    }
                    encItem.CheckedChanged += MenuItems_CheckedChanged;
                    encItem.Click += _owner.ReadmeEncodingMenuItems_Click;
                }
                else if (item is ToolStripMenuItemCustom baseItem)
                {
                    baseItem.CheckedChanged += MenuItems_CheckedChanged;
                }
            }

            foreach (ToolStripItem baseItem in _menu.Items)
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

            _menu.DarkModeEnabled = _darkModeEnabled;

            _constructed = true;

            Localize();
        }

        internal void SetEncodingMenuItemChecked(Encoding encoding)
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

        internal void Localize()
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
