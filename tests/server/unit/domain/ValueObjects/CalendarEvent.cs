namespace SilverKinetics.w80.Domain.ValueObjects.UnitTests;

[TestFixture(TestOf = typeof(ValueObjects.Appointment))]
public class CalendarEvent
{
    [Test]
    public void IsOverlapping_dateSpanBeforeEvent_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(-4), endDateTime: now.AddHours(-3));
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhereEndIsExactlyStartOfEvent_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(-4), endDateTime: now);
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanPartiallyOverlappingEvent_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(-4), endDateTime: now.AddHours(2));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanCompletelyWithinTimespaceOfEvent_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(1), endDateTime: now.AddHours(2));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhereStartOverlapsWithEndOfEvent_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(2), endDateTime: now.AddHours(6));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhereStartIsExactlyAtEndOfEvent_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(4), endDateTime: now.AddHours(8));
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsOverlapping_dateSpanWhichIsAfterEvent_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now;
            evnt.EndDateTimeUTC = now.AddHours(4);

            var ret = evnt.IsOverlapping(startDateTime: now.AddHours(6), endDateTime: now.AddHours(8));
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_beforeThreshold_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now.AddHours(1);
            evnt.EndDateTimeUTC = now.AddHours(2);

            var ret = evnt.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_afterEvent_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now.AddHours(-2);
            evnt.EndDateTimeUTC = now.AddHours(-1);

            var ret = evnt.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.False);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_onStartBorderOfThreshold_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now.AddMinutes(30);
            evnt.EndDateTimeUTC = now.AddHours(2);

            var ret = evnt.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.True);
        }
    }

    [Test]
    public void IsNowWithinThresholdOfEventStart_onEndBorderOfThreshold_shouldReturnFalse()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now.AddHours(-2);
            evnt.EndDateTimeUTC = now.AddMinutes(30);

            var ret = evnt.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.False);
        }
    }
    [Test]
    public void IsNowWithinThresholdOfEventStart_withinThreshold_shouldReturnTrue()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var now = DateTime.UtcNow;
            var threshold = TimeSpan.FromMinutes(30);

            var evnt = new ValueObjects.Appointment();
            evnt.StartDateTimeUTC = now.AddMinutes(15);
            evnt.EndDateTimeUTC = now.AddHours(2);

            var ret = evnt.IsNowWithinThresholdOfEventStart(now, threshold);
            Assert.That(ret, Is.True);
        }
    }
}