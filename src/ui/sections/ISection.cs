namespace HydraMenu.ui.sections
{
    internal abstract class ISection
    {
        public string sectionName = "";

        public virtual void Render() { }
    }
}