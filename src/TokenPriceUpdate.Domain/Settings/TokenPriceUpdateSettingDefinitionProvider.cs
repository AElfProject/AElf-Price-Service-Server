using Volo.Abp.Settings;

namespace TokenPriceUpdate.Settings
{
    public class TokenPriceUpdateSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //Define your own settings here. Example:
            //context.Add(new SettingDefinition(TokenPriceUpdateSettings.MySetting1));
        }
    }
}
