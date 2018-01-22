using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using SharpDX;

namespace SharpDXPingPong.Components.WarpZone
{
    class BallSpeedUp : WarpComponent
    {
        public BallSpeedUp(Game game, string vertexShaderFilename, string pixelShaderFilename, Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename, camera)
        {
        }

        protected override Vector4 GetColor()
        {
            return new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        }
    }
}
