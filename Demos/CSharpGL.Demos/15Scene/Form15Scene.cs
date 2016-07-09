﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace CSharpGL.Demos
{
    public partial class Form15Scene : Form
    {

        private SatelliteManipulater rotator;

        public Form15Scene()
        {
            InitializeComponent();

            this.glCanvas1.OpenGLDraw += glCanvas1_OpenGLDraw;
            //this.glCanvas1.MouseDown += glCanvas1_MouseDown;
            //this.glCanvas1.MouseMove += glCanvas1_MouseMove;
            //this.glCanvas1.MouseUp += glCanvas1_MouseUp;
            //this.glCanvas1.MouseWheel += glCanvas1_MouseWheel;
            //this.glCanvas1.Resize += glCanvas1_Resize;

            Application.Idle += Application_Idle;
            OpenGL.ClearColor(0x87 / 255.0f, 0xce / 255.0f, 0xeb / 255.0f, 0xff / 255.0f);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            this.Text = string.Format("{0} - FPS: {1}", this.GetType().Name, this.glCanvas1.FPS.ToShortString());
        }

        private void glCanvas1_OpenGLDraw(object sender, PaintEventArgs e)
        {
            OpenGL.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            this.scene.Render(RenderModes.Render, this.glCanvas1.ClientRectangle);

            // Cross cursor shows where the mouse is.
            Point mousePosition = this.glCanvas1.PointToClient(Control.MousePosition);
            OpenGL.DrawText(mousePosition.X - offset.X,
                this.glCanvas1.Height - (mousePosition.Y + offset.Y) - 1,
                Color.Red, "Courier New", crossCursorSize, "o");

        }

        private const float crossCursorSize = 40.0f;

        private Point offset = new Point(13, 11);

        private void glCanvas1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'b')
            {
                BlendingSourceFactor source;
                BlendingDestinationFactor dest;
                this.blendFactorHelper.GetNext(out source, out dest);
                this.glText.BlendSwitch.SourceFactor = source;
                this.glText.BlendSwitch.DestFactor = dest;
                this.UpdateLabel();
            }
            else if (e.KeyChar == 'o')
            {
                if (this.openTextureDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string ttfFilename = this.openTextureDlg.FileName;
                    this.scene.UIRoot.Children.Remove(this.glText);
                    this.glText.Dispose();
                    FontResource fontResouce = FontResource.Load(ttfFilename, ' ', (char)126);
                    var glText = new UIText(AnchorStyles.Left | AnchorStyles.Top,
                        new Padding(3, 3, 3, 3), new Size(850, 50), -100, 100, fontResouce);
                    glText.Initialize();
                    glText.SwitchList.Add(new ClearColorSwitch());// show black back color to indicate glText's area.
                    glText.Text = "The quick brown fox jumps over the lazy dog!";
                    this.glText = glText;

                    this.scene.UIRoot.Children.Add(glText);

                    this.formPropertyGrid.DisplayObject(glText);
                }
            }
        }

    }

}
