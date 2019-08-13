using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.Common.DataClasses;
using Xunit;

namespace AngelLoader.Tests
{
    public class MiscTests
    {
        [Fact]
        public async void InstallFM_Test()
        {
            var fm = new FanMission();

            // TODO:
            // -Create test fm zip file
            // -Fill out fm fields with appropriate values
            // -Either modify InstallFM so that we pass it everything it uses, or else just fill out every global
            // it uses with correct values for our test

            //bool success = await InstallAndPlay.InstallFM(fm);

            // TODO: Assert audio files have been converted properly
        }
    }
}
