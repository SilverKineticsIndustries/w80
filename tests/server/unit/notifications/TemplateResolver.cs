namespace SilverKinetics.w80.Notifications.UnitTests;

[TestFixture(TestOf = typeof(Notifications.TemplateResolver))]
public class TemplateResolver
{
    [Test]
    public void ReplaceParameters_replaceSingleParam_paramShouldBeReplacesInOutput()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var resolver = new Notifications.TemplateResolver(config);

            var body = "Test {{one}}";
            var prms = new Dictionary<string,string>(){ {"one", "resolved"} };
            body = resolver.ReplaceParameters(body, prms);

            Assert.That(body, Is.EqualTo("Test resolved"));
        }
    }

    [Test]
    public void ReplaceParameters_omitReplacementParam_replacementParameterShouldBeLeftAsIs()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var resolver = new Notifications.TemplateResolver(config);

            var body = "Test {{one}}";
            var prms = new Dictionary<string,string>();
            body = resolver.ReplaceParameters(body, prms);

            Assert.That(body, Is.EqualTo("Test {{one}}"));
        }
    }

    [Test]
    public void ReplaceParameters_insertGlobalParam_globalParamShouldBeReplaced()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var resolver = new Notifications.TemplateResolver(config);

            var val = config[Keys.Appname];
            var body = "Test {{appname}}";
            var prms = new Dictionary<string,string>();
            body = resolver.ReplaceParameters(body, prms);

            Assert.That(body, Is.EqualTo("Test " + val));
        }
    }

    // TODO: This passes even when template is missing
    [Test]
    public void ResolveAsync_loadAllTemplatesForAllCulture_allTemplatesForAllSupportedCulturesShouldExist()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var config = ctx.Services.GetRequiredService<IConfiguration>();
            var resolver = new Notifications.TemplateResolver(config);

            Assert.DoesNotThrowAsync(async () => {
                foreach(var culture in SupportedCultures.Cultures)
                {
                    foreach(TemplateType template in Enum.GetValues(typeof(Domain.TemplateType)))
                    {
                        if (template == TemplateType.None)
                            continue;

                        var templateFQN = Templates.Meta[template].FullyQualifiedName;
                        await resolver.ResolveAsync(templateFQN, culture.Key,  new Dictionary<string,string>());
                    }
                }
            });
        }
    }
}