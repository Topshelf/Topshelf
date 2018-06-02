using System.ComponentModel;

namespace System.Configuration.Install
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class ResDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = Res.GetString(base.Description);
                }
                return base.Description;
            }
        }

        public ResDescriptionAttribute(string description)
            : base(description)
        {
        }
    }
}
