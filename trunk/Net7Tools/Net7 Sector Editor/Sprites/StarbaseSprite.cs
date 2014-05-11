using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

using N7.Props;
using N7.Utilities;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Event;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Util;

namespace N7.Sprites
{
    class StarbaseSprite
    {
        private StarbaseProps dp;
        private PropertyGrid _pg;
        private bool appearsInRadar;
        private PText pname;
        private DataRow dr;
        private PLayer _layer;
        private PImage starbaseImage;
        private bool dragging;
        private DataGridView _dgv;

        public StarbaseSprite(PLayer layer, DataRow r, PropertyGrid pg, DataGridView dgv)
        {
            _pg = pg;
            setupData(r);
            dr = r;
            _layer = layer;
            _dgv = dgv;

            String name = r["name"].ToString();
            float x = float.Parse(r["position_x"].ToString());
            float y = -(float.Parse(r["position_y"].ToString()));
            float sigRadius = float.Parse(r["signature"].ToString());
            float rrRadius = float.Parse(r["radar_range"].ToString());
            float ExplorationRange = float.Parse(r["exploration_range"].ToString());
            appearsInRadar = (Boolean) r["appears_in_radar"];
            int navType = int.Parse(r["nav_type"].ToString());

            float sigDia = (sigRadius * 2) / 100;
            float rrDia = (rrRadius * 2) / 100;
            float expDia = (ExplorationRange * 2) / 100;

            if (sigDia == 0)
            {
                sigDia = 5;
            }
            if (rrDia == 0)
            {
                rrDia = 5;
            }
            if (expDia == 0)
            {
                expDia = 5;
            }

            string dataDirectory = "Images";
            string filePath = Path.Combine("..", "..");
            string imageName = null;

            if (appearsInRadar == true)
            {
                imageName = "stations.gif";
            }
            else
            {
                imageName = "stations.gif";
            }

            if (File.Exists(Path.Combine(dataDirectory, imageName)))
            {
                filePath = "";
            }

            Image image = Image.FromFile(Path.Combine(filePath, Path.Combine(dataDirectory, imageName)));
            starbaseImage = new PImage(image);
            starbaseImage.X = (x - (image.Width / 2)) / 100;
            starbaseImage.Y = (y - (image.Height / 2)) / 100;

            float sigX = (x / 100) - ((sigDia / 2) - (image.Width / 2));
            float sigY = (y / 100) - ((sigDia / 2) - (image.Height / 2));
            float rrX = (x / 100) - ((rrDia / 2) - (image.Width / 2));
            float rrY = (y / 100) - ((rrDia / 2) - (image.Height / 2));
            float expX = (x / 100) - ((expDia / 2) - (image.Width / 2));
            float expY = (y / 100) - ((expDia / 2) - (image.Height / 2));

            Pen sigPen = new Pen(Color.Yellow, 3.0F);
            Pen rrPen = new Pen(Color.LightGoldenrodYellow, 2.0F);
            rrPen.DashStyle = DashStyle.Dash;
            Pen expPen = new Pen(Color.LightYellow, 1.0F);
            expPen.DashStyle = DashStyle.DashDotDot;

            PPath sigCircle = PPath.CreateEllipse(sigX, sigY, sigDia, sigDia);
            sigCircle.Pen = sigPen;
            PPath rrCircle = PPath.CreateEllipse(rrX, rrY, rrDia, rrDia);
            rrCircle.Pen = rrPen;
            PPath expCircle = PPath.CreateEllipse(expX, expY, expDia, expDia);
            expCircle.Pen = expPen;

            PNode sigNode = sigCircle;
            sigNode.Brush = Brushes.Transparent;

            PNode rrNode = rrCircle;
            rrNode.Brush = Brushes.Transparent;

            PNode expNode = expCircle;
            expNode.Brush = Brushes.Transparent;

            pname = new PText(name);
            pname.TextBrush = Brushes.White;
            pname.TextAlignment = StringAlignment.Center;
            pname.X = (x / 100) - (pname.Width / 2);
            pname.Y = (y / 100) - 20;

            starbaseImage.AddChild(sigNode);
            starbaseImage.AddChild(rrNode);
            starbaseImage.AddChild(expNode);
            starbaseImage.AddChild(pname);

            //Add placeholder nodes for nav_type visualization lookup.
            for (int i = 0; i < navType; i++)
            {
                starbaseImage.AddChild(new PNode());
            }

            starbaseImage.ChildrenPickable = false;

            //Set the tag to this class for information retrieval later.
            starbaseImage.Tag = this;

            // Attach event delegates directly to the node.
            starbaseImage.MouseDown += new PInputEventHandler(Image_MouseDown);
            starbaseImage.MouseUp += new PInputEventHandler(Image_MouseUp);
            starbaseImage.MouseDrag += new PInputEventHandler(Image_MouseDrag);

            //Display Object by adding them to its layer
            layer.AddChild(starbaseImage);
        }

        private void setupData(DataRow r)
        {
            int objectType = int.Parse(r["type"].ToString());
            String oType = "";
            switch (objectType)
            {
                case 0:
                    oType = "Mobs";
                    break;
                case 3:
                    oType = "Planets";
                    break;
                case 11:
                    oType = "Stargates";
                    break;
                case 12:
                    oType = "Starbases";
                    break;
                case 37:
                    oType = "Decorations";
                    break;
                case 38:
                    oType = "Harvestables";
                    break;
            }

            dp = new StarbaseProps();

            //Base Props
            dp.SectorID = int.Parse(r["sector_id"].ToString());
            dp.NavType = r["nav_type"].ToString();
            dp.Signature = float.Parse(r["signature"].ToString());
            dp.IsHuge = (Boolean) r["is_huge"];
            dp.BaseXP = int.Parse(r["base_xp"].ToString());
            dp.ExplorationRange = float.Parse(r["exploration_range"].ToString());

            dp.BaseAssetID = int.Parse(r["base_asset_id"].ToString());

            AdobeColors.HSL hslColor = new AdobeColors.HSL();
            hslColor.H = float.Parse(r["h"].ToString());
            hslColor.S = float.Parse(r["s"].ToString());
            hslColor.L = float.Parse(r["v"].ToString());
            Color newColor = AdobeColors.HSL_to_RGB(hslColor);
            dp.Color = newColor;

            dp.Type = oType;
            dp.Scale = float.Parse(r["scale"].ToString()); ;
            dp.PositionX = float.Parse(r["position_x"].ToString());
            dp.PositionY = float.Parse(r["position_y"].ToString());
            dp.PositionZ = float.Parse(r["position_z"].ToString()); ;

            double[] quat1 = new double[4];
            quat1[0] = double.Parse(r["orientation_z"].ToString());
            quat1[1] = double.Parse(r["orientation_u"].ToString());
            quat1[2] = double.Parse(r["orientation_v"].ToString());
            quat1[3] = double.Parse(r["orientation_w"].ToString());

            QuaternionCalc qc1 = new QuaternionCalc();
            double[] ang1 = qc1.QuatToAngle(quat1);
            if (ang1[0] == double.NaN)
            {
                ang1[0] = 0;
            }
            if (ang1[1] == double.NaN)
            {
                ang1[1] = 0;
            }
            if (ang1[2] == double.NaN)
            {
                ang1[2] = 0;
            }
            dp.Orientation_Yaw = Math.Round(ang1[0], 0);
            dp.Orientation_Pitch = Math.Round(ang1[1], 0);
            dp.Orientation_Roll = Math.Round(ang1[2], 0);

            dp.Name = r["name"].ToString();
            dp.AppearsInRadar = (Boolean) r["appears_in_radar"];
            dp.RadarRange = float.Parse(r["radar_range"].ToString());
            dp.Destination = int.Parse(r["gate_to"].ToString());
            dp.SoundEffect = int.Parse(r["sound_effect_id"].ToString());
            dp.SoundEffectRange = float.Parse(r["sound_effect_range"].ToString());

            try
            {
                //Starbase props
                dp.IsCapShip = bool.Parse(r["capShip"].ToString());
                dp.IsDockable = bool.Parse(r["dockable"].ToString());
            }
            catch (Exception)
            {
                dp.IsCapShip = false;
                dp.IsDockable = false;
            }
        }

        protected void Image_MouseDown(object sender, PInputEventArgs e)
        {
            _pg.SelectedObject = dp;
        }

        protected void Image_MouseDrag(object sender, PInputEventArgs e)
        {
            dragging = true;
        }

        protected void Image_MouseUp(object sender, PInputEventArgs e)
        {
            if (dragging == true)
            {
                dp.PositionX = e.Position.X * 100;
                dp.PositionY = -(e.Position.Y * 100);
                dr["position_x"] = e.Position.X * 100;
                dr["position_y"] = -(e.Position.Y * 100);
                dragging = false;
            }
            _pg.SelectedObject = dp;
            mainFrm.selectedObjectID = int.Parse(dr["sector_object_id"].ToString());
        }

        public Boolean getAppearsInRader()
        {
            return appearsInRadar;
        }

        public PText getText()
        {
            return pname;
        }

        public void updateChangedInfo(String propertyName, Object _changedValue)
        {
            String changedValue = _changedValue.ToString();

            //Update DataRow and Graphics (only certain props require graphical updating).
            if (propertyName == "SectorID")
            {
                dr["sector_id"] = int.Parse(changedValue);
            }
            else if (propertyName == "NavType")
            {
                dr["nav_type"] = changedValue;
            }
            else if (propertyName == "Signature")
            {
                dr["signature"] = float.Parse(changedValue);

                float imageWidth = starbaseImage.Width;
                float imageHeight = starbaseImage.Height;
                float x = starbaseImage.X;
                float y = starbaseImage.Y;
                starbaseImage.GetChild(0).X = (x + (imageWidth / 2)) - (float.Parse(changedValue) / 100);
                starbaseImage.GetChild(0).Y = (y + (imageHeight / 2)) - (float.Parse(changedValue) / 100);
                starbaseImage.GetChild(0).Width = (float.Parse(changedValue) * 2) / 100;
                starbaseImage.GetChild(0).Height = (float.Parse(changedValue) * 2) / 100;
            }
            else if (propertyName == "IsHuge")
            {
                dr["is_huge"] = bool.Parse(changedValue);
            }
            else if (propertyName == "BaseXP")
            {
                dr["base_xp"] = int.Parse(changedValue);
            }
            else if (propertyName == "ExplorationRange")
            {
                dr["exploration_range"] = float.Parse(changedValue);

                float imageWidth = starbaseImage.Width;
                float imageHeight = starbaseImage.Height;
                float x = starbaseImage.X;
                float y = starbaseImage.Y;
                starbaseImage.GetChild(2).X = (x + (imageWidth / 2)) - (float.Parse(changedValue) / 100);
                starbaseImage.GetChild(2).Y = (y + (imageHeight / 2)) - (float.Parse(changedValue) / 100);
                starbaseImage.GetChild(2).Width = (float.Parse(changedValue) * 2) / 100;
                starbaseImage.GetChild(2).Height = (float.Parse(changedValue) * 2) / 100;
            }
            else if (propertyName == "BaseAssetID")
            {
                dr["base_asset_id"] = int.Parse(changedValue);
                foreach (DataGridViewRow row in _dgv.SelectedRows)
                {
                    _dgv.Rows[row.Index].Cells["base_asset_id"].Value = int.Parse(changedValue);
                    _dgv.Update();
                    _dgv.Refresh();
                }
            }
            else if (propertyName == "Color")
            {
                Color color = (Color)_changedValue;
                AdobeColors.HSL hsv = AdobeColors.RGB_to_HSL(color);

                dr["h"] = hsv.H;
                dr["s"] = hsv.S;
                dr["v"] = hsv.L;
            }
            else if (propertyName == "Type")
            {
                _layer.RemoveChild(starbaseImage);

                /*
                if (changedValue == "Mobs")
                {
                    dr["type"] = 0;
                    new MobSprite(_layer, dr, _pg);
                }
                else if (changedValue == "Planets")
                {
                    dr["type"] = 3;
                    new PlanetSprite(_layer, dr, _pg);
                }
                else if (changedValue == "Stargates")
                {
                    dr["type"] = 11;
                    new StargateSprite(_layer, dr, _pg);
                }
                else if (changedValue == "Decorations")
                {
                    dr["type"] = 37;
                    new DecorationSprite(_layer, dr, _pg);
                }
                else if (changedValue == "Harvestables")
                {
                    dr["type"] = 38;
                    new HarvestableSprite(_layer, dr, _pg);
                }*/

                _pg.SelectedObject = null;
            }
            else if (propertyName == "Scale")
            {
                dr["scale"] = float.Parse(changedValue);
            }
            else if (propertyName == "PositionX")
            {
                dr["position_x"] = float.Parse(changedValue);

                float dx = (float.Parse(changedValue) / 100) - starbaseImage.X;
                starbaseImage.TranslateBy(dx, 0);
            }
            else if (propertyName == "PositionY")
            {
                dr["position_y"] = float.Parse(changedValue);

                float dy = (float.Parse(changedValue) / 100) - starbaseImage.Y;
                starbaseImage.TranslateBy(0, dy);
            }
            else if (propertyName == "PositionZ")
            {
                dr["position_z"] = float.Parse(changedValue);
            }
            else if (propertyName == "Orientation_Yaw" || propertyName == "Orientation_Pitch" || propertyName == "Orientation_Roll")
            {
                QuaternionCalc qtmp = new QuaternionCalc();
                double[] q1 = qtmp.AngleToQuat(dp.Orientation_Yaw, dp.Orientation_Pitch, dp.Orientation_Roll);

                dr["orientation_z"] = q1[0];
                dr["orientation_u"] = q1[1];
                dr["orientation_v"] = q1[2];
                dr["orientation_w"] = q1[3];
            }
            else if (propertyName == "Name")
            {
                dr["name"] = changedValue;

                float x = starbaseImage.X;
                float y = starbaseImage.Y;
                PText name = (PText)starbaseImage.GetChild(3);
                name.Text = changedValue;
                name.TextAlignment = StringAlignment.Center;
                name.X = x - (name.Width / 2);
                name.Y = y - 20;

                foreach (DataGridViewRow row in _dgv.SelectedRows)
                {
                    _dgv.Rows[row.Index].Cells["name"].Value = changedValue;
                    _dgv.Update();
                    _dgv.Refresh();
                }
            }
            else if (propertyName == "AppearsInRadar")
            {
                dr["appears_in_radar"] = bool.Parse(changedValue);

                if (bool.Parse(changedValue) == true)
                {
                    changeImage(1);
                }
                else
                {
                    changeImage(0);
                }
            }
            else if (propertyName == "RadarRange")
            {
                dr["radar_range"] = float.Parse(changedValue);

                float imageWidth = starbaseImage.Width;
                float imageHeight = starbaseImage.Height;
                float x = starbaseImage.X;
                float y = starbaseImage.Y;
                starbaseImage.GetChild(1).X = (x + (imageWidth / 2)) - (float.Parse(changedValue) / 100);
                starbaseImage.GetChild(1).Y = (y + (imageHeight / 2)) - (float.Parse(changedValue) / 100);
                starbaseImage.GetChild(1).Width = (float.Parse(changedValue) * 2) / 100;
                starbaseImage.GetChild(1).Height = (float.Parse(changedValue) * 2) / 100;
            }
            else if (propertyName == "Destination")
            {
                dr["gate_to"] = int.Parse(changedValue);
            }
            else if (propertyName == "IsCapShip")
            {
                dr["capShip"] = bool.Parse(changedValue);
            }
            else if (propertyName == "IsDockable")
            {
                dr["dockable"] = bool.Parse(changedValue);
            }
            else if (propertyName == "SoundEffect")
            {
                dr["sound_effect_id"] = int.Parse(changedValue);
            }
            else if (propertyName == "SoundEffectRange")
            {
                dr["sound_effect_range"] = float.Parse(changedValue);
            }

            if (dr.RowState != DataRowState.Modified)
            {
                dr.SetModified();
            }
        }

        private void changeImage(int type)
        {
            //Setup our Image Path's based on our properties
            string dataDirectory = "Images";
            string filePath = Path.Combine("..", "..");
            string imageName = null;

            switch (type)
            {
                case 0:
                    imageName = "stations.gif";
                    appearsInRadar = false;
                    break;
                case 1:
                    imageName = "stations.gif";
                    appearsInRadar = true;
                    break;
            }

            if (File.Exists(Path.Combine(dataDirectory, imageName)))
            {
                filePath = "";
            }

            float x = starbaseImage.X;
            float y = starbaseImage.Y;

            //Load our icon.
            Image image = Image.FromFile(Path.Combine(filePath, Path.Combine(dataDirectory, imageName)));
            starbaseImage.Image = image;
            starbaseImage.X = x;
            starbaseImage.Y = y;
        }

        public DataRow getRow()
        {
            return dr;
        }

        public void setPropGrid()
        {
            _pg.SelectedObject = dp;
        }
    }
}
