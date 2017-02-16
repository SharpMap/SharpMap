namespace SharpMap.Forms.Tools
{
    public class FeatureSelectTool : MapTool
    {
        public FeatureSelectTool()
            : base("FeatureEditorTool", "A tool to select features")
        {
        }
    }

    public class FeatureEditorTool : MapTool
    {
        public FeatureEditorTool() 
            : base("FeatureEditorTool", "A tool to edit features")
        {
        }
    }
}