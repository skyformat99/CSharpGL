﻿using CSharpGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StencilShadowVolume
{
    partial class ExtrudeVolumeNode : ModernNode, IRenderable
    {

        public static ExtrudeVolumeNode Create()
        {
            var model = new AdjacentTeapot();
            RenderMethodBuilder extrudVolumeBuilder, regularBuilder;
            {
                var vs = new VertexShader(extrudeVert);
                var gs = new GeometryShader(extrudeGeom);
                var fs = new FragmentShader(extrudeFrag);
                var array = new ShaderArray(vs, gs, fs);
                var map = new AttributeMap();
                map.Add("Position", AdjacentTeapot.strPosition);
                extrudVolumeBuilder = new RenderMethodBuilder(array, map, new PolygonModeState(PolygonMode.Line));
            }
            {
                var vs = new VertexShader(vertexCode);
                var fs = new FragmentShader(fragmentCode);
                var array = new ShaderArray(vs, fs);
                var map = new AttributeMap();
                map.Add("inPosition", AdjacentTeapot.strPosition);
                map.Add("inColor", AdjacentTeapot.strNormal);
                regularBuilder = new RenderMethodBuilder(array, map);
            }

            var node = new ExtrudeVolumeNode(model, extrudVolumeBuilder, regularBuilder);
            node.Initialize();
            node.ModelSize = model.GetModelSize();

            return node;
        }

        private ExtrudeVolumeNode(IBufferSource model, params RenderMethodBuilder[] builders)
            : base(model, builders)
        { }

        private bool renderBody = true;

        public bool RenderBody
        {
            get { return renderBody; }
            set { renderBody = value; }
        }

        private bool renderSilhouette = true;

        public bool RenderSilhouette
        {
            get { return renderSilhouette; }
            set { renderSilhouette = value; }
        }
        private vec3 lightPosition = new vec3(0, 1, 0) * 10;

        public vec3 LightPosition
        {
            get { return lightPosition; }
            set { lightPosition = value; }
        }

        private PolygonOffsetState fillOffsetState = new PolygonOffsetFillState(pullNear: false);

        private ThreeFlags enableRendering = ThreeFlags.BeforeChildren | ThreeFlags.Children | ThreeFlags.AfterChildren;
        /// <summary>
        /// Render before/after children? Render children? 
        /// RenderAction cares about this property. Other actions, maybe, maybe not, your choice.
        /// </summary>
        public ThreeFlags EnableRendering
        {
            get { return this.enableRendering; }
            set { this.enableRendering = value; }
        }

        public void RenderBeforeChildren(RenderEventArgs arg)
        {
            if (!this.IsInitialized) { this.Initialize(); }

            this.RotationAngle += 1f;

            ICamera camera = arg.CameraStack.Peek();
            mat4 projection = camera.GetProjectionMatrix();
            mat4 view = camera.GetViewMatrix();
            mat4 model = this.GetModelMatrix();

            if (this.RenderSilhouette)
            {
                var method = this.RenderUnit.Methods[0]; // the only render unit in this node.
                ShaderProgram program = method.Program;
                program.SetUniform("gWVP", projection * view * model);
                program.SetUniform("gWorld", model);
                program.SetUniform("gLightPos", this.lightPosition);

                method.Render(ControlMode.ByFrame);
            }

            if (this.RenderBody)
            {
                var method = this.RenderUnit.Methods[1]; // the only render unit in this node.
                ShaderProgram program = method.Program;
                program.SetUniform("mvpMat", projection * view * model);

                fillOffsetState.On();
                method.Render(ControlMode.Random);
                fillOffsetState.Off();
            }
        }

        public void RenderAfterChildren(RenderEventArgs arg)
        {
        }
    }
}