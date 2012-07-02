namespace SharpMap.Demo.Wms.Models
{
    using SharpMap.Styles;

    public class LayerData
    {
        public string LabelColumn { get; set; }
        public IStyle Style { get; set; }
    }
}