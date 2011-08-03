using System.Drawing;

namespace SharpMap.Rendering.Decoration
{
    public interface IMapDecoration
    {
        void Render(Graphics g, Map map);
    }
}