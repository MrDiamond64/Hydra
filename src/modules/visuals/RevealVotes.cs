namespace LunarMenu.modules.visuals
{
    internal class RevealVotes : Module
    {
        public RevealVotes() : base("RevealVotes") { }

        public bool RevealAnonymousVotes { get; set; } = false;
    }
}
