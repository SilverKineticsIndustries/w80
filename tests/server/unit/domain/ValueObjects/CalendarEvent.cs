namespace SilverKinetics.w80.Domain.UnitTests.ValueObjects;

[TestFixture(TestOf = typeof(Appointment))]
public class CalendarEvent
{
    [Test]
    public void IsOverlapping_dateSpanBeforeEvent_shouldReturnFalse()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(-4), endDateTime: now.AddHours(-3));
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhereEndIsExactlyStartOfEvent_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(-4), endDateTime: now);
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanPartiallyOverlappingEvent_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(-4), endDateTime: now.AddHours(2));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanCompletelyWithinTimespaceOfEvent_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(1), endDateTime: now.AddHours(2));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhereStartOverlapsWithEndOfEvent_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(2), endDateTime: now.AddHours(6));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhereStartIsExactlyAtEndOfEvent_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(4), endDateTime: now.AddHours(8));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhichIsAfterEvent_shouldReturnFalse()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var appoint = ctx.CreateAppointment(now, now.AddHours(4));
            var ret = appoint.IsOverlapping(startDateTime: now.AddHours(6), endDateTime: now.AddHours(8));
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_beforeThreshold_shouldReturnFalse()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);
            var appoint = ctx.CreateAppointment(now.AddHours(1), now.AddHours(2));
            var ret = appoint.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_afterEvent_shouldReturnFalse()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);
            var appoint = ctx.CreateAppointment(now.AddHours(-2), now.AddHours(-1));
            var ret = appoint.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_onStartBorderOfThreshold_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);
            var appoint = ctx.CreateAppointment(now.AddMinutes(30), now.AddHours(2));
            var ret = appoint.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_onEndBorderOfThreshold_shouldReturnFalse()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);
            var appoint = ctx.CreateAppointment(now.AddHours(-2), now.AddMinutes(30));
            var ret = appoint.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.False);
        }
    }
    [Test]
    public void IsNowWithinThresholdOfEventStart_withinThreshold_shouldReturnTrue()
    {
        using (var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);
            var appoint = ctx.CreateAppointment(now.AddMinutes(15), now.AddHours(2));
            var ret = appoint.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.True);
        }
    }
}