using Region = APUSH_Game.GameState.Region;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.Xna.Framework;
using APUSH_Game.GameState;
using System.Drawing;
using FastBitmapLib;
using APUSH_Game.Helpers;

namespace Tools
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            const int thickAmt = 4;
            Vector2[] vectors = Enumerable.Range(0, 8).Select(i => Helper.RotateVector(Vector2.UnitX * thickAmt, i * 45) + Vector2.One * thickAmt).ToArray();

            const string path = "C:\\Users\\Jason\\source\\repos\\APUSH_Game\\APUSH_Game\\Content\\PCicon - Copy.png";
            const string paths = "C:\\Users\\Jason\\source\\repos\\APUSH_Game\\APUSH_Game\\Content\\PCicon.png";
            Bitmap bmp = new Bitmap(path);

            Bitmap final = new Bitmap(bmp.Width + 2 * thickAmt, bmp.Height + 2 * thickAmt);
            using (var g = Graphics.FromImage(final))
            {
                for(int i = 0; i < vectors.Length; i++)
                {
                    g.DrawImage(bmp, new PointF(vectors[i].X, vectors[i].Y));
                }
            }

            FastBitmap fast = new FastBitmap(final);
            fast.Lock();
            fast.ModPixel(c => Color.FromArgb(c.A, 255, 255, 255));
            fast.Unlock();
            final.Save(paths);
            /*
            //passes

            const string dir = "C:\\Users\\Jason\\source\\repos\\APUSH_Game\\Tools";
            foreach (var (i, minfo) in typeof(MapProcessor).GetMethods().Where(mi => mi.IsStatic && mi.IsPublic).Select((i, j) => (j + 1, i)))
            {
                string potPath = Path.Combine(dir, $"MapV{i + 1}.png");
                if (!File.Exists(potPath))
                {
                    Bitmap bmp = new Bitmap(Path.Combine(dir, $"MapV{i}.png"));
                    FastBitmap fastBitmap = new FastBitmap(bmp);
                    fastBitmap.Lock();
                    minfo.Invoke(null, new object[] { fastBitmap });
                    fastBitmap.Unlock();
                    bmp.Save(potPath);
                }

                Console.WriteLine($"Done {i}");
            }*/

                return;
            /*
            File.WriteAllText("questions.json", JsonConvert.SerializeObject(Enumerable.Range(0, 25).Select(c => new Question()).ToArray()));


            Bitmap bmp = new Bitmap("C:\\Users\\Jason\\source\\repos\\APUSH_Game\\APUSH_Game\\Content\\Packed - Copy.png");
            FastBitmap fbmp = new FastBitmap(bmp);
            fbmp.Lock();

            P(fbmp);

            fbmp.Unlock();
            bmp.Save("C:\\Users\\Jason\\source\\repos\\APUSH_Game\\APUSH_Game\\Content\\Packed.png");

            static void P(FastBitmap bmp)
            {
                Stack<HashSet<Point>> groups = new();

                bmp.ModPixel((x, y, c) =>
                {
                    if (c.EqualsValue(Color.White) && !groups.Any(h => h.Contains(new Point(x, y))))
                        groups.Push(MapProcessor.FloodFill(bmp, x, y));
                    return c;
                });

                var sort = groups.OrderByDescending(s => s.Count).ToArray();
                Region[] regions = JsonConvert.DeserializeObject<Region[]>(File.ReadAllText("terr.json"));

                for (int i = 0; i < sort.Length; i++)
                {
                    double avgx = sort[i].Average(p => p.X);
                    double avgy = sort[i].Average(p => p.Y);
                    regions[i].CenterX = (float)avgx;
                    regions[i].CenterY = (float)avgy;
                }

                File.WriteAllText("terr.json", JsonConvert.SerializeObject(regions.ToArray()));
            }*/

            /*
            //bitfields
            Region[] regions = JsonConvert.DeserializeObject<Region[]>(File.ReadAllText("terr.json"));
            FastBitmap fastBitmap = new FastBitmap(new Bitmap("C:\\Users\\Jason\\source\\repos\\APUSH_Game\\APUSH_Game\\Content\\Packed.png"));
            fastBitmap.Lock(); 

            Color bg = fastBitmap.GetPixel(0,0);
            foreach(var region in regions)
            {
                Rectangle bmpRect = new Rectangle(region.TextureSource.X, region.TextureSource.Y, region.TextureSource.Width, region.TextureSource.Height);
                BitArray bits = new BitArray(bmpRect.Width * bmpRect.Height);

                int ctr = 0;
                for (int i = 0; i < bmpRect.Width; i++)
                {
                    int x = i + bmpRect.X;
                    for(int j = 0; j < bmpRect.Height; j++, ctr++)
                    {
                        int y = j + bmpRect.Y;
                        bits[ctr] = fastBitmap.GetPixel(x, y).EqualsValue(bg);
                    }
                }
                region.BitField = Convert.ToBase64String(bits.ToBytes().ToArray());
            }
            fastBitmap.Unlock();*/
            //parse to json
            string raw = "0|0|1862|371|1501|1179|0|0|1501|1179\r\n1|0|1379|1683|1294|802|1196|1179|1294|802\r\n2|0|1185|1058|1278|790|1501|0|1278|790\r\n3|0|1078|211|1196|1038|0|1179|1196|1038\r\n4|1|852|960|741|1262|1196|1981|741|1262\r\n5|0|2284|1505|1153|396|1937|1981|1153|396\r\n6|0|916|484|743|632|0|2217|743|632\r\n7|1|3213|459|601|683|1937|2377|601|683\r\n8|0|2781|484|492|774|2490|1179|492|774\r\n9|2|2615|2134|511|634|2779|0|511|634\r\n10|2|3000|2252|556|519|607|2849|556|519\r\n11|1|3335|1472|607|531|0|2849|607|531\r\n12|2|2799|2573|626|622|2538|2377|626|622\r\n13|0|2928|1892|529|403|2538|2999|529|403\r\n14|1|3255|1132|560|366|1501|790|560|366\r\n15|1|3560|713|487|530|2779|634|487|530\r\n16|1|3703|1228|365|648|2982|1179|365|648\r\n17|2|3438|1945|459|418|1937|3060|459|418\r\n18|1|4094|821|361|487|3067|2999|361|487\r\n19|1|4269|1211|387|439|3164|2377|387|439\r\n20|1|4026|1290|282|490|3290|0|282|490\r\n21|2|4652|2680|330|477|1196|3243|330|477\r\n22|1|5318|377|347|534|743|2217|347|534\r\n23|2|3843|2101|224|571|1526|3243|224|571\r\n24|2|3499|2356|300|401|3347|1179|300|401\r\n25|2|4347|2268|422|303|3290|872|422|303\r\n26|2|2268|2426|382|382|3290|490|382|382\r\n27|1|4928|745|290|386|2538|3402|290|386\r\n28|2|4025|1846|600|217|2061|957|600|217\r\n29|2|4709|1881|450|235|607|3368|450|235\r\n30|2|4042|2283|226|369|350|3380|226|369\r\n31|2|2644|1935|293|242|3572|0|293|242\r\n32|2|4044|2082|301|220|3572|242|301|220\r\n33|2|4460|1703|688|167|2061|790|688|167\r\n34|1|4147|1590|433|212|3428|2999|433|212\r\n35|2|4411|2514|413|221|1937|3478|413|221\r\n36|2|4686|2024|282|282|3551|2609|282|282\r\n37|1|4628|1143|350|327|0|3380|350|327\r\n38|1|3757|649|529|273|3090|1981|529|273\r\n39|1|4688|965|343|232|3551|2377|343|232\r\n40|2|4404|1817|444|249|3347|1580|444|249\r\n41|1|4512|1367|264|377|3428|3211|264|377\r\n42|2|4575|1580|404|177|3428|3588|404|177\r\n43|2|4409|2092|346|268|3067|3486|346|268\r\n44|2|4791|1759|402|145|607|3603|402|145\r\n45|1|4147|1757|420|141|3347|1829|420|141\r\n46|2|3739|2576|204|222|2828|3652|204|222\r\n47|1|4863|1125|272|184|3619|1981|272|184\r\n48|2|4223|2297|171|251|3672|490|171|251\r\n49|1|5148|708|157|292|1750|3559|157|292\r\n50|2|4285|2051|235|250|2828|3402|235|250\r\n51|1|5267|669|164|316|1750|3243|164|316\r\n52|1|3931|1746|232|189|2227|3699|232|189\r\n53|2|3856|1928|186|189|3692|3211|186|189\r\n54|2|4066|1986|378|115|3090|2254|378|115\r\n55|2|4136|2533|290|160|1937|3699|290|160\r\n56|2|3737|2320|164|266|1009|3603|164|266\r\n57|2|4501|2006|238|176|0|3707|238|176\r\n58|1|5216|934|277|168|3619|2165|277|168\r\n59|1|4713|1271|184|192|2350|3478|184|192\r\n60|2|4373|2214|355|168|3164|2816|355|168\r\n61|2|4890|1566|218|174|1196|3720|218|174\r\n62|2|3768|2114|135|212|2396|3060|135|212\r\n63|2|4622|2115|202|243|3647|1179|202|243\r\n64|0|2659|1876|284|75|743|2751|284|75\r\n65|1|4985|1282|180|131|3672|741|180|131\r\n66|1|4858|1267|143|156|3712|872|143|156\r\n67|1|5219|1051|159|155|3692|3400|159|155\r\n68|1|5065|1080|176|157|3647|1422|176|157\r\n69|1|5108|1185|119|142|607|3748|119|142\r\n70|2|4725|1515|260|103|2982|1827|260|103\r\n71|1|5104|1324|126|145|1057|3368|126|145\r\n72|1|4939|1393|149|115|3468|2254|149|115\r\n73|2|4776|1468|194|97|3551|2891|194|97\r\n74|1|5053|1412|129|190|2396|3272|129|190\r\n75|1|5084|1373|98|161|1090|2217|98|161\r\n76|2|3903|2676|86|112|1090|2378|86|112\r\n77|1|5210|1157|173|106|3712|1028|173|106\r\n78|1|4981|1500|98|96|1090|2490|98|96\r\n79|2|4821|1436|129|57|1057|3513|129|57\r\n80|2|4722|2339|64|73|2661|1033|64|73\r\n81|1|4764|1435|111|76|2661|957|111|76\r\n82|1|5361|1043|50|88|1131|2586|50|88\r\n83|2|4003|2059|66|45|1090|2768|66|45\r\n84|2|4948|1495|51|75|1090|2693|51|75\r\n85|1|5121|1581|41|107|1090|2586|41|107\r\n86|1|4886|1423|63|53|2661|1106|63|53\r\n87|2|4240|2399|40|43|1027|2751|40|43\r\n88|2|4971|1672|40|46|2725|1033|40|46\r\n89|2|4390|2189|44|42|2724|1106|44|42\r\n90|2|3778|2410|47|47|1141|2693|47|47\r\n91|1|4883|1407|52|26|2490|1953|52|26\r\n92|1|4982|1497|28|27|2574|1953|28|27\r\n93|1|5103|1351|32|26|2542|1953|32|26\r\n94|2|4198|2281|35|21|1501|1156|35|21\r\n95|1|4890|1400|22|17|1536|1156|22|17";

            List<Region> regions = new List<Region>();

            foreach(var ss in raw.Split(Environment.NewLine))
            {
                var parts = ss.Split('|');

                regions.Add(new Region()
                {
                    MapBounds = new Rectangle(int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5])),
                    TextureSource = new Rectangle(int.Parse(parts[6]), int.Parse(parts[7]), int.Parse(parts[8]), int.Parse(parts[9])),
                    TerritoryType = (TerrioryType)(int.Parse(parts[1])),
                    RegionName = "Fill This in",
                    BitField = string.Empty,
                    ID = int.Parse(parts[0])
                });
            }
            File.WriteAllText("terr.json", JsonConvert.SerializeObject(regions.ToArray()));
           
        }

        public static IEnumerable<byte> ToBytes(this BitArray bits, bool MSB = false)
        {
            int bitCount = 7;
            int outByte = 0;

            foreach (bool bitValue in bits)
            {
                if (bitValue)
                    outByte |= MSB ? 1 << bitCount : 1 << (7 - bitCount);
                if (bitCount == 0)
                {
                    yield return (byte)outByte;
                    bitCount = 8;
                    outByte = 0;
                }
                bitCount--;
            }
            // Last partially decoded byte
            if (bitCount < 7)
                yield return (byte)outByte;
        }
    }
}
