namespace HydraMenu.ui.sections
{
    internal abstract class ISection
    {
        public string name = "";

        public virtual void Render() { }
    }
}