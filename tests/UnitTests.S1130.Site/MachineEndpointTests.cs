using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using S1130.Site.Machines;
using S1130.Site.Sessions;

namespace UnitTests.S1130.Site;

public class MachineEndpointTests : IClassFixture<MachineEndpointTests.SiteFactory>
{
	private readonly SiteFactory _factory;

	public MachineEndpointTests(SiteFactory factory) => _factory = factory;

	[Fact]
	public async Task HomePage_IsServed_AndSetsASessionCookie()
	{
		var client = _factory.CreateClient();
		var response = await client.GetAsync("/");

		response.EnsureSuccessStatusCode();
		Assert.Contains("Line Printer", await response.Content.ReadAsStringAsync());
		var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
		Assert.StartsWith(SessionCookie.Name + "=", cookie);
		Assert.Contains("httponly", cookie);
		Assert.Contains("samesite=strict", cookie);
	}

	[Fact]
	public async Task Configuration_ReportsThePrinterFromTheMasterDisk()
	{
		var config = await _factory.CreateClient().GetFromJsonAsync<MachineConfig>("/api/machine/config");
		Assert.Equal("1403 Printer", config!.PrinterName);
	}

	[Fact]
	public async Task Machine_BeforeBoot_IsPoweredOff()
	{
		var snapshot = await _factory.CreateClient().GetFromJsonAsync<MachineSnapshot>("/api/machine");
		Assert.False(snapshot!.PoweredOn);
	}

	[Fact]
	public async Task Keys_BeforeBoot_AreRejected()
	{
		var response = await _factory.CreateClient().PostAsync("/api/machine/start", null);
		Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
	}

	[Fact]
	public async Task Deck_LoadsCardsIntoTheHopper()
	{
		var response = await _factory.CreateClient().PostAsync("/api/machine/deck", new StringContent("// JOB\n// XEQ\n"));
		response.EnsureSuccessStatusCode();
		var body = await response.Content.ReadFromJsonAsync<DeckResult>();
		Assert.Equal(2, body!.Loaded);
		Assert.Equal(2, body.Hopper);
	}

	[Fact]
	public async Task Deck_TooLarge_IsRejected()
	{
		var deck = string.Concat(Enumerable.Repeat("X\n", 2001));
		var response = await _factory.CreateClient().PostAsync("/api/machine/deck", new StringContent(deck));
		Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
	}

	[Fact]
	public async Task Boot_Dms_ThePrintoutShowsTheJobHeader()
	{
		if (TestFiles.FindMasterDisk() == null) return;
		var client = _factory.CreateClient();
		(await client.PostAsync("/api/machine/deck", new StringContent("// JOB\n"))).EnsureSuccessStatusCode();
		(await client.PostAsync("/api/machine/boot", null)).EnsureSuccessStatusCode();

		var text = "";
		var deadline = DateTime.UtcNow.AddSeconds(10);
		while (DateTime.UtcNow < deadline && !text.Contains("V2 M12"))
		{
			var chunk = await client.GetFromJsonAsync<PrintoutChunk>($"/api/machine/printout?since={text.Length}");
			text += chunk!.Text;
			await Task.Delay(50);
		}
		Assert.Contains("V2 M12", text);
	}

	[Fact]
	public async Task Sessions_GetSeparateMachines()
	{
		var first = _factory.CreateClient();
		var second = _factory.CreateClient();
		(await first.PostAsync("/api/machine/deck", new StringContent("A\nB\nC\n"))).EnsureSuccessStatusCode();

		var other = await second.GetFromJsonAsync<MachineSnapshot>("/api/machine");
		Assert.False(other!.PoweredOn);
	}

	private sealed record DeckResult(int Loaded, int Hopper);
	private sealed record MachineConfig(string PrinterName);

	public sealed class SiteFactory : WebApplicationFactory<Program>
	{
		private readonly string _data = TestFiles.NewScratchDirectory();

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseSetting("Site:DataRoot", _data);
			builder.UseSetting("Site:MasterDisk", TestFiles.FindMasterDisk() ?? "");
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			try { Directory.Delete(_data, true); } catch (IOException) { }
		}
	}
}
