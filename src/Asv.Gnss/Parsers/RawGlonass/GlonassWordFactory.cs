using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asv.Gnss
{
    public static class GlonassWordFactory
    {
        public static GlonassWordBase Create(uint[] navBits)
        {
            var wordId = GlonassRawHelper.GetWordId(navBits);
            var data = GlonassRawHelper.GetRawData(navBits);
            GlonassWordBase subframe;
            switch (wordId)
            {
                case 1:
                    subframe = new GlonassWord1();
                    break;
                case 2:
                    subframe = new GlonassWord2();
                    break;
                case 3:
                    subframe = new GlonassWord3();
                    break;
                case 4:
                    subframe = new GlonassWord4();
                    break;
                case 5:
                    subframe = new GlonassWord5();
                    break;
                case 6:
                case 8:
                case 10:
                case 12:
                case 14:
                    subframe = new GlonassWordCn1();
                    break;
                case 7:
                case 9:
                case 11:
                case 13:
                case 15:
                    subframe = new GlonassWordCn2();
                    break;
                default:
                    throw new Exception($"Unknown Glonass word ID:{Convert.ToString(wordId, 2).PadRight(8)}");
            }
            subframe.Deserialize(data);
            return subframe;
        }
    }
}
