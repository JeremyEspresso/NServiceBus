namespace NServiceBus.Core.Tests.OpenTelemetry
{
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;
    using Particular.Approvals;

    [TestFixture]
    public class ActivityTagsTests
    {
        [Test]
        public void Verify_ActivityTags()
        {
            var activityTags = typeof(ActivityTags)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(x => $"{x.Name} => {x.GetRawConstantValue()}")
                .ToList();

            Approver.Verify(new
            {
                Note = "Changes to activity tags should result in ActivitySource version updates",
                ActivitySourceVersion = ActivitySources.Main.Version,
                Tags = activityTags
            });
        }
    }
}