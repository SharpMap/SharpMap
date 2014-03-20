using System.Text;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoParams
    {
        public static GetFeatureInfoParams Empty
        {
            get { return new GetFeatureInfoParams(1, null, Encoding.UTF8); }
        }

        private readonly int _pixelSensitivity;
        private readonly WmsServer.InterSectDelegate _intersectDelegate;
        private readonly Encoding _encoding;

        public GetFeatureInfoParams(int pixelSensitivity,
            WmsServer.InterSectDelegate intersectDelegate, 
            Encoding encoding)
        {
            _pixelSensitivity = pixelSensitivity;
            _intersectDelegate = intersectDelegate;
            _encoding = encoding;
        }

        public int PixelSensitivity
        {
            get { return _pixelSensitivity; }
        }

        public WmsServer.InterSectDelegate IntersectDelegate
        {
            get { return _intersectDelegate; }
        }

        public Encoding Encoding
        {
            get { return _encoding; }
        }
    }
}