namespace SilverKinetics.w80.Common.Configuration;

public static class Keys
{
    public const string Domain = "Domain";
    public const string Appname = "Appname";
    public const string JwtIssuer = "Jwt:Issuer";
    public const string DatabaseName = "Database:Name";
    public const string StatisticsRunPeriodInSeconds = "StatisticsRunPeriodInSeconds";
    public const string EmailNotificationsRunPeriodInSeconds= "EmailNotificationsRunPeriodInSeconds";
    public const string JwtRefreshLifetimeInDays = "Jwt:RefreshLifetimeInDays";
    public const string JwtAccessLifetimeInMinutes = "Jwt:AccessLifetimeInMinutes";
    public const string InvitationCodeLifetimeInHours = "InvitationCodeLifetimeInHours";
    public const string EmailConfirmationLifetimeInHours = "EmailConfirmationLifetimeInHours";
    public const string NotificationsEmailBaseAPI = "Notifications:EmailBaseApi";
    public const string NotificationsFromEmailAddress = "Notifications:EmailFromAddress";
    public const string NotificationsFromEmailName = "Notifications:EmailFromName";
    public const string PasswordMinimumLength = "PasswordMinimumLength";
    public const string EmailAlertThresholdInMinutes = "EmailAlertThresholdInMinutes";
    public const string ReCaptchaEnabled = "ReCaptcha:Enabled";
    public const string ReCaptchaValidationEndpointURL = "ReCaptcha:ValidationEndpointURL";
    public const string RefreshCookiePath = "RefreshCookiePath";

    public static class Secrets
    {
        public const string JwtKey = "Jwt:Key";
        public const string CaptchaKey = "CaptchaKey";
        public const string InvitationKey = "InvitationKey";
        public const string EmailSenderKey = "Notifications:EmailSenderKey";
        public const string EmailConfirmationKey = "EmailConfirmationKey";
        public const string DatabaseConnectionString = "Database:ConnectionString";
    }
}