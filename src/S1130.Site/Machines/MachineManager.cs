using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace S1130.Site.Machines;

/// <summary>Owns every session's machine: creates on demand, enforces the limit and stops idle ones.</summary>
public sealed class MachineManager : IDisposable
{
	private readonly ConcurrentDictionary<string, MachineHost> _machines = new();
	private readonly Lock _createLock = new();
	private readonly SiteOptions _options;
	private readonly string _dataRoot;
	private readonly ILogger<MachineManager> _logger;

	public MachineManager(IOptions<SiteOptions> options, IWebHostEnvironment environment, ILogger<MachineManager> logger)
	{
		_options = options.Value;
		_logger = logger;
		_dataRoot = Path.GetFullPath(_options.DataRoot, environment.ContentRootPath);
		MasterDiskPath = ResolveMasterDisk(_options.MasterDisk, environment.ContentRootPath);
	}

	/// <summary>Full path of the master DMS disk, or null if it cannot be found.</summary>
	public string? MasterDiskPath { get; }

	public int Count => _machines.Count;

	public MachineHost? Find(string sessionId)
	{
		if (!_machines.TryGetValue(sessionId, out var host)) return null;
		host.Touch();
		return host;
	}

	/// <exception cref="MachineLimitException">All machines are in use.</exception>
	public MachineHost GetOrCreate(string sessionId)
	{
		var host = Find(sessionId);
		if (host != null) return host;
		lock (_createLock)
		{
			if (_machines.TryGetValue(sessionId, out host)) return host;
			if (_machines.Count >= _options.MaxMachines) throw new MachineLimitException();
			host = new MachineHost(SessionDirectory(sessionId));
			_machines[sessionId] = host;
			_logger.LogInformation("Machine created for session {Session} ({Count} running)", Abbrev(sessionId), _machines.Count);
			return host;
		}
	}

	/// <summary>Stops machines not used for <paramref name="idle"/>; their disks are saved.</summary>
	public int StopIdle(TimeSpan idle)
	{
		var cutoff = DateTime.UtcNow - idle;
		var stopped = 0;
		foreach (var (id, host) in _machines)
		{
			if (host.LastUsedUtc >= cutoff || !_machines.TryRemove(id, out _)) continue;
			host.Dispose();
			stopped++;
			_logger.LogInformation("Idle machine stopped for session {Session}", Abbrev(id));
		}
		return stopped;
	}

	public void Dispose()
	{
		foreach (var id in _machines.Keys)
		{
			if (_machines.TryRemove(id, out var host)) host.Dispose();
		}
	}

	private string SessionDirectory(string sessionId) => Path.Combine(_dataRoot, "sessions", sessionId);

	private static string Abbrev(string sessionId) => sessionId[..8];

	private static string? ResolveMasterDisk(string configured, string contentRoot)
	{
		if (!string.IsNullOrWhiteSpace(configured))
		{
			var path = Path.GetFullPath(configured, contentRoot);
			return File.Exists(path) ? path : null;
		}
		for (var dir = new DirectoryInfo(contentRoot); dir != null; dir = dir.Parent)
		{
			var candidate = Path.Combine(dir.FullName, "ForReferenceOnly", "1130SW", "dms.dsk");
			if (File.Exists(candidate)) return candidate;
		}
		return null;
	}
}

public sealed class MachineLimitException() : Exception("All machines are busy. Try again later.");

/// <summary>Stops idle machines once a minute.</summary>
public sealed class IdleMachineReaper(MachineManager machines, IOptions<SiteOptions> options) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
		while (await timer.WaitForNextTickAsync(stoppingToken))
		{
			machines.StopIdle(TimeSpan.FromMinutes(options.Value.IdleMinutes));
		}
	}
}
