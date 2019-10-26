using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using Xunit;

namespace AngelLoader.Tests
{
    public class MiscTests
    {
        [Fact]
        public void ReadConfigIni_Test()
        {
            // TODO: Assert against all possible bad data that I can think of here
        }

        [Fact]
        public async void InstallFM_Test()
        {
            var fm = new FanMission();

            // TODO:
            // -Create test fm zip file
            // -Fill out fm fields with appropriate values
            // -Either modify InstallFM so that we pass it everything it uses, or else just fill out every global
            // it uses with correct values for our test

            //bool success = await FMInstallAndPlay.InstallFM(fm);

            // TODO: Assert audio files have been converted properly
        }

        [Fact]
        public void UpdateFMTagsString_Test()
        {
            var fm = new FanMission();

            var cat1 = new CatAndTags { Category = "author" };
            cat1.Tags.Add("Tannar");
            cat1.Tags.Add("Random_Taffer");
            fm.Tags.Add(cat1);

            var cat2 = new CatAndTags { Category = "contest" };
            cat2.Tags.Add("10 rooms");
            fm.Tags.Add(cat2);

            var cat3 = new CatAndTags { Category = "length" };
            cat3.Tags.Add("short");
            fm.Tags.Add(cat3);

            var cat4 = new CatAndTags { Category = "series" };
            fm.Tags.Add(cat4);

            var cat5 = new CatAndTags { Category = "misc" };
            cat5.Tags.Add("campaign");
            cat5.Tags.Add("atmospheric");
            cat5.Tags.Add("other protagonist");
            cat5.Tags.Add("water");
            cat5.Tags.Add("thing_shaped");
            fm.Tags.Add(cat5);

            FMTags.UpdateFMTagsString(fm);

            Assert.Equal(
                "author:Tannar,author:Random_Taffer,contest:10 rooms,length:short,series,misc:campaign,misc:atmospheric,misc:other protagonist,misc:water,misc:thing_shaped",
                fm.TagsString);
        }
    }
}
