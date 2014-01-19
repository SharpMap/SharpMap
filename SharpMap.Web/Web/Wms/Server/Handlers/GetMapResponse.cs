using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetMapResponse : IHandlerResponse
    {
        private readonly Image _image;
        private readonly ImageCodecInfo _codecInfo;

        public GetMapResponse(Image image, ImageCodecInfo codecInfo)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (codecInfo == null)
                throw new ArgumentNullException("codecInfo");
            _image = image;
            _codecInfo = codecInfo;
        }

        public void WriteToContextAndFlush(IContextResponse response)
        {
            //Png can't stream directy. Going through a memorystream instead
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                _image.Save(ms, _codecInfo, null);
                _image.Dispose();
                buffer = ms.ToArray();
            }
            response.Clear();
            response.ContentType = _codecInfo.MimeType;
            response.Write(buffer);
            response.End();
        }
    }
}