using System.Security.Cryptography;

namespace S1130.Site.Sessions;

/// <summary>
/// Gives every browser a random session id in an HttpOnly cookie. The id picks the visitor's private
/// machine and data folder, so it is only ever read from the cookie, never from the URL or body.
/// </summary>
public static class SessionCookie
{
	public const string Name = "s1130_session";
	private const string ItemKey = "S1130.SessionId";

	public static IApplicationBuilder UseSessionCookie(this IApplicationBuilder app)
	{
		return app.Use(async (context, next) =>
		{
			var id = context.Request.Cookies[Name];
			if (!IsValid(id))
			{
				id = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
			}
			context.Response.Cookies.Append(Name, id!, new CookieOptions
			{
				HttpOnly = true,
				SameSite = SameSiteMode.Strict,
				Secure = context.Request.IsHttps,
				IsEssential = true,
				MaxAge = TimeSpan.FromDays(30),
			});
			context.Items[ItemKey] = id;
			await next();
		});
	}

	public static string GetSessionId(this HttpContext context) => (string)context.Items[ItemKey]!;

	public static bool IsValid(string? id) =>
		id is { Length: 32 } && id.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
}
