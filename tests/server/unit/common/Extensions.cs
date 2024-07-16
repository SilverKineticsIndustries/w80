namespace SilverKinetics.w80.Common.UnitTests;

[TestFixture(TestOf = typeof(Common.Extensions))]
public class Extensions
{
    [Test]
    public void WrapAsParameter_wrapParameterName_makeSureParameterIsWrapped()
    {
        var x = System.Guid.NewGuid();
        using(var ctx = TestContextFactory.Create())
        {
            string paramName = "companyName";
            string output = paramName.WrapAsParameter();

            Assert.Multiple(() => {
                Assert.That(output.StartsWith(Common.Extensions.WrapBeginToken), Is.True);
                Assert.That(output.EndsWith(Common.Extensions.WrapEndToken), Is.True);
            });
        }
    }

    [Test]
    public void WrapAsParameter_wrapEmptyName_argumentExceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            string paramName = " ";

            Assert.Throws(
                Is.TypeOf<ArgumentNullException>()
                .And.Message.EqualTo("Parameter name cannot be empty/blank/null. (Parameter 'parameter')"
            ),
            () => paramName.WrapAsParameter());
        }
    }

    [Test]
    public void WrapAsParameter_wrapParameterSecondTime_argumentExceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            string paramName = "companyName";
            string wrappedPameters = paramName.WrapAsParameter();

            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("Parameter name has already been wrapped. (Parameter 'parameter')"
            ),
            () => wrappedPameters.WrapAsParameter());
        }
    }

    [Test]
    public void WrapAsParameter_parameterContainsWrapToken_argumentExceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            string paramName = "companyName}}";

            Assert.Throws(
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("Parameter name has already been wrapped. (Parameter 'parameter')"
            ),
            () => paramName.WrapAsParameter());
        }
    }
}
