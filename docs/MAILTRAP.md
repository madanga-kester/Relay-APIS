# Development email

Set `Mail__Enabled=true`, `Mail__Host=sandbox.smtp.mailtrap.io`, `Mail__Port=2525`, `Mail__Username`, and `Mail__Password` only in development or staging secrets. The API uses STARTTLS-capable SMTP and never logs password-reset tokens or message contents.

Production email should use a managed SMTP provider or transactional email service with credentials outside source control. If reset-email volume becomes material, move delivery behind a durable background worker while keeping the `IEmailSender` contract unchanged.
