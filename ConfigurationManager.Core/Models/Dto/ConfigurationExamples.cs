using Swashbuckle.AspNetCore.Filters;
using ConfigurationManager.Core.Constants;

namespace ConfigurationManager.Core.Models.Dto;

public static class ConfigurationExampleHelper
{
    public static T CreateBaseExample<T>() where T : new()
    {
        var example = new T();
        var nameProp = typeof(T).GetProperty("Name");
        nameProp?.SetValue(example, "Имя конфигурации");

        var settingsProp = typeof(T).GetProperty("SettingsJson");
        settingsProp?.SetValue(example, DefaultSettings.GetDefaultSettingsJson());

        return example;
    }
}

public class CreateConfigurationRequestExample : IExamplesProvider<CreateConfigurationRequest>
{
    public CreateConfigurationRequest GetExamples() =>
        ConfigurationExampleHelper.CreateBaseExample<CreateConfigurationRequest>();
}

public class UpdateConfigurationRequestExample : IExamplesProvider<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequest GetExamples() =>
        ConfigurationExampleHelper.CreateBaseExample<UpdateConfigurationRequest>();
}