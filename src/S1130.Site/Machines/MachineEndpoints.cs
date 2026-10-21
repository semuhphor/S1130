using Microsoft.Extensions.Options;
using S1130.Site.Sessions;

namespace S1130.Site.Machines;

/// <summary>HTTP endpoints for the session's machine under <c>/api/machine</c>.</summary>
public static class MachineEndpoints
{
	public static void MapMachineEndpoints(this IEndpointRouteBuilder app)
	{
		var api = app.MapGroup("/api/machine");

		api.MapGet("/config", (MachineManager machines) => new { printerName = machines.PrinterName });

		api.MapGet("", (HttpContext http, MachineManager machines) =>
			machines.Find(http.GetSessionId())?.Snapshot() ?? MachineSnapshot.Off);

		api.MapPost("/boot", (HttpContext http, MachineManager machines) =>
		{
			if (machines.MasterDiskPath == null)
				return Results.Problem("The master DMS disk is not installed on this server.", statusCode: 409);
			return WithMachine(http, machines, host =>
			{
				host.Boot(machines.MasterDiskPath);
				return Results.Ok(host.Snapshot());
			});
		});

		api.MapPost("/deck", async (HttpContext http, MachineManager machines, IOptions<SiteOptions> options) =>
		{
			var maxCards = options.Value.MaxDeckCards;
			var maxChars = maxCards * (TextDeck.Columns + 2);
			using var reader = new StreamReader(http.Request.Body);
			var buffer = new char[maxChars + 1];
			var length = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
			if (length > maxChars)
				return Results.Problem($"A deck may have at most {maxCards} cards of 80 columns.", statusCode: 413);

			List<SystemObjects.ICard> cards;
			try
			{
				cards = TextDeck.Parse(new string(buffer, 0, length), maxCards);
			}
			catch (DeckTooLargeException ex)
			{
				return Results.Problem(ex.Message, statusCode: 413);
			}
			return WithMachine(http, machines, host =>
				Results.Ok(new { loaded = cards.Count, hopper = host.LoadDeck(cards) }));
		});

		api.MapPost("/start", (HttpContext http, MachineManager machines) => Key(http, machines, h => h.Start()));
		api.MapPost("/stop", (HttpContext http, MachineManager machines) => Key(http, machines, h => h.Stop()));
		api.MapPost("/reset", (HttpContext http, MachineManager machines) => Key(http, machines, h => h.Reset()));

		api.MapGet("/printout", (HttpContext http, MachineManager machines, long since = 0) =>
			machines.Find(http.GetSessionId())?.ReadPrintout(since) ?? new PrintoutChunk(0, "", 0));

		api.MapDelete("/printout", (HttpContext http, MachineManager machines) =>
		{
			machines.Find(http.GetSessionId())?.ClearPrintout();
			return Results.NoContent();
		});
	}

	private static IResult Key(HttpContext http, MachineManager machines, Action<MachineHost> press)
	{
		var host = machines.Find(http.GetSessionId());
		if (host == null) return Results.Problem("The machine is not powered on. Boot it first.", statusCode: 409);
		press(host);
		return Results.Ok(host.Snapshot());
	}

	private static IResult WithMachine(HttpContext http, MachineManager machines, Func<MachineHost, IResult> action)
	{
		try
		{
			return action(machines.GetOrCreate(http.GetSessionId()));
		}
		catch (MachineLimitException ex)
		{
			return Results.Problem(ex.Message, statusCode: 503);
		}
	}
}
