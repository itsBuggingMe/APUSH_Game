using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APUSH_Game.Helpers;
using APUSH_Game.Interface;

namespace APUSH_Game.GameState
{
    internal class StateManager
    {
        #region Eh
        private RegionObject[] allRegions;
        private RegionObject currentRegion;
        private GameWorld world;
        private bool HasSelection => currentRegion != null;
        private State[] states;
        public StateManager(RegionObject[] region, GameWorld world)
        {
            this.world = world;
            allRegions = region;
            states = new State[]
            {
                new MoveTroopState()
                {
                    World = world,
                    Parent = this,
                },
                new PlaceTroopState()
                {
                    World = world,
                    Parent = this,
                },
                new AttackTroopState()
                {
                    World = world,
                    Parent = this,
                },
            };
        }

        private RegionObject Primary;
        private RegionObject Secondary;

        //public methods
        public void Update() => states[(int)world.SelectedAction].Update();
        public void Draw() => states[(int)world.SelectedAction].Draw();

        public bool TryRegionSelected(RegionObject region) => states[(int)world.SelectedAction].TrySelect(region);
        public bool TryRegionDeselected(RegionObject region) => states[(int)world.SelectedAction].TryDeselect(region);
        public bool IsPotenialSelection(RegionObject region) => states[(int)world.SelectedAction].IsPotenialSelection(region);

        private abstract class State
        {
            internal GameWorld World { get; init; }
            internal StateManager Parent { get; init; }
            protected bool NoSelect => !HasPrimary;
            protected bool HasPrimary => Parent.Primary != null;
            protected bool HasOnlyPrimary => Parent.Primary != null && !HasSecondary;
            protected bool HasSecondary => Parent.Secondary != null;
            protected RegionObject Primary { 
                get => Parent.Primary; 
                set => Parent.Primary = value; }
            protected RegionObject Secondary { get => Parent.Secondary; set => Parent.Secondary = value; }
            protected bool MatchesSide(RegionObject r) => (r.Data.TerritoryType != TerrioryType.ConfederateState) == World.Current;
            internal virtual void Update() { }
            internal virtual void Draw() { }
            internal abstract bool TrySelect(RegionObject selected);
            internal abstract bool TryDeselect(RegionObject deselected);
            internal abstract bool IsPotenialSelection(RegionObject region);
        }
        #endregion

        /*
        private class MoveTroopState1 : State
        {
            bool SupressDelete = false;
            private Action Deletion;
            private bool HasActiveMovement;
            private int[][] borders;
            private ScrollSelector ss;
            public MoveTroopState()
            {
                string s = "0|8|14|11|5|2|3\r\n1|5|64|2|4|31|9|26\r\n2|1|4|5|3|6|0\r\n3|0|6|2|4\r\n4|6|2|1\r\n5|13|0|2|1|64|11\r\n6|3|2|4\r\n7|8|14|15\r\n8|0|7|14\r\n9|1|26|31|13|10|12\r\n10|9|13|17|24|12\r\n11|0|5|14|13|17|53|52|16\r\n12|9|10\r\n13|10|9|5|11|64|31|17\r\n14|0|7|8|11|15|16\r\n15|14|7|38|16\r\n16|15|14|11|52|20\r\n17|10|13|53|62|56|24\r\n18|20|19\r\n19|18|20|34|41|37\r\n20|18|19|16|52|34\r\n21|35\r\n22|51\r\n23|53|83|32|30|46|56|62\r\n24|46|56|90|17|10\r\n25|35|55|48|50|60|80\r\n26|1|9\r\n27|49|58|68|47|39\r\n28|54|40|33|45|52|53|83\r\n29|36|57|40|44\r\n30|55|87|48|94|32|18\r\n31|64|1|19|9\r\n32|83|30|48|50|54|18|94\r\n33|44|88|61|42|45|28|40\r\n34|45|19|41|42|52|20\r\n35|21|25|55\r\n36|63|57|29\r\n37|41|19|39|47|66|59\r\n38|15\r\n39|37|27|47\r\n40|57|50|54|28|33|44|29\r\n41|42|70|81|59|37|19|34\r\n42|41|70|88|33|61|34\r\n43|80|63|57|50|89|60\r\n44|29|40|33\r\n45|28|52|34|33\r\n46|76|56|23|24\r\n47|69|65|66|37|68|39|27\r\n48|87|55|25|94|30|32\r\n49|51|58|27\r\n50|54|32|89|60|25|57|40|43\r\n51|49|58|22\r\n52|53|28|45|34|20|16|11\r\n53|52|83|28|62|17|11\r\n54|83|28|40|50|32\r\n55|30|48|25|35\r\n56|46|23|62|24|17|90\r\n57|29|40|50|43|63|29\r\n58|51|49|27|67|82\r\n59|91|95|66|37|81|41\r\n60|80|43|89|50|25\r\n61|70|88|33|42|78\r\n62|53|23|56|17\r\n63|80|43|36|57\r\n64|31|5|13|1\r\n65|93|75|71|72|66|47|69\r\n66|91|95|59|72|47|65\r\n67|82|58|68\r\n68|67|77|69|47|27\r\n69|68|65|77|71|47\r\n70|84|73|41|33|61|78\r\n71|69|65|93|75\r\n72|86|79|84|78|92|74|75\r\n73|70|81|79|84\r\n74|85|75|72\r\n75|74|72|71|93|65\r\n76|46\r\n77|69|68\r\n78|72|84|92|70|61\r\n79|72|86|81|73\r\n80|63|60|43|25\r\n81|79|73|41|59\r\n82|67|58\r\n83|23|54|53|32|28\r\n84|78|73|72|70|92\r\n85|74\r\n86|91|79|72\r\n87|48|30\r\n88|61|42|33\r\n89|60|50|43\r\n90|56|24\r\n91|86|66|59|95\r\n92|84|78|72\r\n93|75|71|65\r\n94|48|32|30\r\n95|91|66|59";
                borders = s.Split("\r\n").Select(s1 => s1.Split('|').Skip(1).Select(c => int.Parse(c)).ToArray()).ToArray();
            }
            internal override void Update()
            {
                if(!SupressDelete)
                {
                    Deletion?.Invoke();
                }
                SupressDelete = false;
                Deletion = null;
            }
            
            internal override void Select(RegionObject selected)
            {
                if (HasActiveMovement)
                    return;
                if(Parent.Primary == null)
                {
                    if(selected.CurrentTroops.TroopCount == 0)
                        return;
                    SupressDelete = true;
                    Parent.Primary = selected;
                    foreach (var region in Parent.allRegions)
                        region.LightUp = false;
                    foreach (var region in Parent.allRegions)
                    {
                        if ((selected.Data.TerritoryType != TerrioryType.ConfederateState) == World.Current && CanMoveTo(region, selected) && selected != region)
                            region.LightUp = true;
                    }
                }
                else if (CanMoveTo(Parent.Primary, selected))
                {
                    Parent.Secondary = selected;
                    HasActiveMovement = true;
                    ss = new ScrollSelector("                        ", s =>
                    {
                        Parent.Primary.CurrentTroops.TroopCount -= s;
                        Parent.Secondary.CurrentTroops.TroopCount += s;

                        HasActiveMovement = false;
                        Parent.Primary = null;
                        Parent.Secondary = null;

                        World.CurrentGameGUI.NumPoliticalCapital--;
                    }, 0, Parent.Primary.CurrentTroops.TroopCount, World);
                    World.GameObjects.Add(ss);
                    SupressDelete = true;
                }
            }
            internal override void Deselect(RegionObject deselected)
            {
                Deletion = () =>
                {
                    foreach (var region in Parent.allRegions)
                        region.LightUp = false;
                    Parent.Primary = null;
                    Parent.Secondary = null;
                };
            }
            private bool CanMoveTo(RegionObject A, RegionObject b)
            {
                int[] possibles = borders[A.ID];
                return possibles.Contains(b.ID);
            }
            internal override bool AllowSelect(RegionObject selected) => 
                (selected.Data.TerritoryType != TerrioryType.ConfederateState) == World.Current && 
                (selected.CurrentTroops.TroopCount != 0 || Parent.Primary != null);
        }*/

        private class MoveTroopState : State
        {
            static int[][] borders;
            Texture2D arrow = GameRoot.Game.Content.Load<Texture2D>("arrow");
            ScrollSelector ss;
            static MoveTroopState()
            {
                string s = "0|8|14|11|5|2|3\r\n1|5|64|2|4|31|9|26\r\n2|1|4|5|3|6|0\r\n3|0|6|2|4\r\n4|6|2|1\r\n5|13|0|2|1|64|11\r\n6|3|2|4\r\n7|8|14|15\r\n8|0|7|14\r\n9|1|26|31|13|10|12\r\n10|9|13|17|24|12\r\n11|0|5|14|13|17|53|52|16\r\n12|9|10\r\n13|10|9|5|11|64|31|17\r\n14|0|7|8|11|15|16\r\n15|14|7|38|16\r\n16|15|14|11|52|20\r\n17|10|13|53|62|56|24\r\n18|20|19\r\n19|18|20|34|41|37\r\n20|18|19|16|52|34\r\n21|35\r\n22|51\r\n23|53|83|32|30|46|56|62\r\n24|46|56|90|17|10\r\n25|35|55|48|50|60|80\r\n26|1|9\r\n27|49|58|68|47|39\r\n28|54|40|33|45|52|53|83\r\n29|36|57|40|44\r\n30|55|87|48|94|32|18\r\n31|64|1|19|9\r\n32|83|30|48|50|54|18|94\r\n33|44|88|61|42|45|28|40\r\n34|45|19|41|42|52|20\r\n35|21|25|55\r\n36|63|57|29\r\n37|41|19|39|47|66|59\r\n38|15\r\n39|37|27|47\r\n40|57|50|54|28|33|44|29\r\n41|42|70|81|59|37|19|34\r\n42|41|70|88|33|61|34\r\n43|80|63|57|50|89|60\r\n44|29|40|33\r\n45|28|52|34|33\r\n46|76|56|23|24\r\n47|69|65|66|37|68|39|27\r\n48|87|55|25|94|30|32\r\n49|51|58|27\r\n50|54|32|89|60|25|57|40|43\r\n51|49|58|22\r\n52|53|28|45|34|20|16|11\r\n53|52|83|28|62|17|11\r\n54|83|28|40|50|32\r\n55|30|48|25|35\r\n56|46|23|62|24|17|90\r\n57|29|40|50|43|63|29\r\n58|51|49|27|67|82\r\n59|91|95|66|37|81|41\r\n60|80|43|89|50|25\r\n61|70|88|33|42|78\r\n62|53|23|56|17\r\n63|80|43|36|57\r\n64|31|5|13|1\r\n65|93|75|71|72|66|47|69\r\n66|91|95|59|72|47|65\r\n67|82|58|68\r\n68|67|77|69|47|27\r\n69|68|65|77|71|47\r\n70|84|73|41|33|61|78\r\n71|69|65|93|75\r\n72|86|79|84|78|92|74|75\r\n73|70|81|79|84\r\n74|85|75|72\r\n75|74|72|71|93|65\r\n76|46\r\n77|69|68\r\n78|72|84|92|70|61\r\n79|72|86|81|73\r\n80|63|60|43|25\r\n81|79|73|41|59\r\n82|67|58\r\n83|23|54|53|32|28\r\n84|78|73|72|70|92\r\n85|74\r\n86|91|79|72\r\n87|48|30\r\n88|61|42|33\r\n89|60|50|43\r\n90|56|24\r\n91|86|66|59|95\r\n92|84|78|72\r\n93|75|71|65\r\n94|48|32|30\r\n95|91|66|59";
                borders = s.Split("\r\n").Select(s1 => s1.Split('|').Skip(1).Select(c => int.Parse(c)).ToArray()).ToArray();
            }

            internal override bool TrySelect(RegionObject selected)
            {
                if(NoSelect)
                {
                    Primary = selected;
                    return true;
                }
                if(HasOnlyPrimary)
                {
                    Secondary = selected;
                    ss = new ScrollSelector("                        ", s =>
                    {
                        Parent.Primary.CurrentTroops.TroopCount -= s;
                        Parent.Secondary.CurrentTroops.TroopCount += s;

                        Primary.IsSelected = false;
                        Primary.IsSelected = false;
                        Parent.Primary = null;
                        Parent.Secondary = null;

                        if(s != 0)
                            World.CurrentGameGUI.NumPoliticalCapital--;
                        ss.Delete = true;
                        ss = null;
                    }, 0, Parent.Primary.CurrentTroops.TroopCount, World);
                    World.GameObjects.Add(ss);
                    return true;
                }
                if(HasSecondary)
                {
                    return false;
                }
                throw new InvalidOperationException();
            }

            internal override bool TryDeselect(RegionObject deselected)
            {
                if(NoSelect)
                {
                    return true;
                }
                if(HasOnlyPrimary)
                {
                    if(deselected == Primary)
                    {
                        return false;
                    }
                    Primary = null;
                    return true;
                }
                if(HasSecondary)
                {
                    return false;
                }
                throw new InvalidOperationException();
            }

            internal override bool IsPotenialSelection(RegionObject region)
            {
                if (NoSelect)
                    return MatchesSide(region) && region.CurrentTroops.TroopCount > 0;
                if (HasOnlyPrimary)
                    return MatchesSide(region) && CanMoveTo(Primary, region);
                if (HasSecondary)
                    return false;
                throw new InvalidOperationException();
            }

            private bool CanMoveTo(RegionObject A, RegionObject b)
            {
                int[] possibles = borders[A.ID];
                return possibles.Contains(b.ID);
            }

            internal override void Draw()
            {
                if (Parent.Primary != null && Parent.Secondary != null)
                {
                    Vector2 from = Parent.Primary.Position;
                    Vector2 to = Parent.Secondary.Position;
                    float distance = Vector2.Distance(from, to);
                    float sqd = MathF.Sqrt(distance);
                    float targetStride = sqd;
                    float size = sqd * 0.01f;
                    int numArrow = (int)MathF.Round(distance / targetStride);
                    float estInc = distance / numArrow;

                    Vector2 inc = Helper.UnitVectorPoint(from, to) * estInc;
                    const int speed = 20;
                    Vector2 acummulator = from + inc * ((Globals.TotalFrames % speed / (float)speed));
                    float rotation = Helper.GetAngleRad(inc);
                    for (int i = 0; i < numArrow; i++)
                    {
                        float arrowToPtDist = Math.Min(Vector2.DistanceSquared(acummulator, from), Vector2.DistanceSquared(acummulator, to));


                        Globals.SpriteBatch.Draw(
                            arrow,
                            acummulator,
                            null,
                            (arrowToPtDist < distance ? Color.Yellow * (arrowToPtDist / distance) : Color.Yellow),
                            rotation, arrow.Bounds.Center.V(), size, SpriteEffects.FlipHorizontally, Depth.OverTerritory);

                        acummulator += inc;
                    }
                }
            }
        }

        private class PlaceTroopState : State
        {
            internal override bool IsPotenialSelection(RegionObject region)
            {
                throw new NotImplementedException();
            }

            internal override bool TryDeselect(RegionObject deselected)
            {
                throw new NotImplementedException();
            }

            internal override bool TrySelect(RegionObject selected)
            {
                throw new NotImplementedException();
            }
        }

        private class AttackTroopState : State
        {
            internal override bool IsPotenialSelection(RegionObject region)
            {
                throw new NotImplementedException();
            }

            internal override bool TryDeselect(RegionObject deselected)
            {
                throw new NotImplementedException();
            }

            internal override bool TrySelect(RegionObject selected)
            {
                throw new NotImplementedException();
            }
        }
    }
}
