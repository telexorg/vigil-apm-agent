# VigilAgent SDK for .NET

## 🚀 What is VigilAgent?

VigilAgent is a combination of a C# SDK and a hosted telemetry API. Together, they work to monitor and analyze the behavior of your backend services in real time.

* The SDK is installed in your C# application—it's responsible for collecting runtime metrics, trace events, and diagnostic signals.
* The API lives in the cloud—it receives telemetry data, processes it, and makes insights available through connected tools like Telex.

VigilAgent is designed to help developers observe, diagnose, and understand what their backend services are doing—without requiring complex setup.

### Key features:

* Runtime performance metrics (CPU, memory, garbage collection)
* Project-aware telemetry submission
* Background batching and retry handling
* Supports request tracing and instrumentation
* Secure communication using API key authentication

✅ The only required configuration to start is your API key. All other settings are optional and defaulted for convenience.

---

## ⚙️ 2. Installation & Setup

To add VigilAgent to your C# backend project, follow these steps:

📦 **Step 1: Install the NuGet Package**
Run this from your terminal or package manager console:

```bash
dotnet add package Vigil.Apm.Agent
```

🧩 **Step 2: Register Services in Program.cs**
Choose one of three configuration styles based on your project needs.

✅ **Option 1: Configure via Code**
Use this if you want to set the API key directly in code:

```csharp
builder.Services.AddTelemetryExporter(options =>
{
    options.ApiKey = "YOUR_API_KEY";
});
```

✅ **Option 2: Load from appsettings.json**
Define a configuration section like this:

```json
"Vigil": {
  "ApiKey": "YOUR_API_KEY"
}
```

Then bind it:

```csharp
builder.Services.AddTelemetryExporter(configuration);
```

✅ **Option 3: Combine Config + Code Override**
Use configuration as a base, then override specific values (like API key) in code:

```csharp
builder.Services.AddTelemetryExporter(configuration, options =>
{
    options.ApiKey = "YOUR_API_KEY";
});
```

This pattern is useful for projects that fetch the API key dynamically (e.g. from secrets or env vars).

🔧 **Default Behavior**
Unless customized, VigilAgent will:

* Flush telemetry every 5 seconds
* Send up to 10 events per batch
* Collect runtime metrics (CPU, memory, etc.)
* Retry failed sends up to 3 times
* Use a default project name and environment label

✅ Only `ApiKey` is required to start. All other settings are optional and have sensible defaults.

---

## 🛠️ 3. Middleware Activation

For telemetry collection and export to work, you must add the VigilAgent middleware to your application’s pipeline. Without it, no request tracing, error logging, or runtime metrics will be captured.

✅ **Add This to Program.cs**

```csharp
app.UseRouting();

app.UseVigilTelemetryCollector(); // Required for telemetry to activate

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
```

🟡 This middleware connects VigilAgent to your request lifecycle. It enables the SDK to detect incoming traffic, track performance, and prepare telemetry for export.

⚙️ **What It Enables**
Once added, the middleware activates features based on your `TelemetryOptions`:

* `EnableRuntimeMetrics` → collects system metrics like CPU and memory
* `EnableExceptionLogging` → tracks unhandled exceptions during requests
* `EnableDebugLogs` → adds extra logging for development/debug environments

These options are customizable, but the middleware itself is the gateway that makes telemetry processing possible.

---

## 🔐 4. API Key Authentication

To securely transmit and associate telemetry data with your project, VigilAgent requires an API key.

The key is used by both:

* The SDK, which attaches the API key to outgoing telemetry requests.
* The backend API, which receives the data and verifies the key against registered projects before processing it.

🧭 **Where to Get Your API Key**

At the moment, API keys are issued through an http client like Postman using this URL

```
https://vigil-apm-agent.onrender.com/api/v1/auth/register-project
```

To get your API key, make a post request to the above url with your Telex Organization ID and your Project Name. As shown below

```json
{
    orgId: "127857",
    projectName: "traidr"
}
```
* Each key is linked to a specific organization and project.
* Use a separate key for each service if you want isolated tracking.

🔐 **How the Key is Validated**
When a telemetry event or metric is sent:

* The Vigil API receives the request.
* The header `X-VIGIL-API-KEY` is read.
* The key is decrypted and matched to its associated project.
* If valid, telemetry is accepted and tagged with project metadata.
* If invalid, the request is rejected with a 401 Unauthorized response.

💡 **Developer Behavior**
The SDK handles everything under the hood:

* It attaches the API key to every outgoing batch.
* It does not proceed with sending telemetry if the key is missing.
* Only the key needs to be provided—other project info is resolved automatically.

Always make sure your API key is set correctly in configuration. If no key is present, VigilAgent will disable telemetry submission.

---

## 🏁 5. What Happens Next

Once VigilAgent is installed in your C# backend and you've added the middleware, telemetry will begin flowing to the Vigil API in the background.

💬 **Connect with Telex**

* Go to Telex and create your organization.
* Add VigilAgent to your organization using your issued API key.
* If the agent isn’t automatically discovered, you can manually add it using:

```
https://vigil-apm-agent.onrender.com/api/v1/.well-known/agent.json
```

🧠 **Interact with Your Agent**
After activation, you don’t browse dashboards or inspect raw data—you chat directly with your agent in Telex.

Ask questions like:

* “Have there been any exceptions lately?”
* “How is the system performing?”
* “Did anything change in the last hour?”

Your agent responds with insights based on the telemetry your service is sending in real time.
VigilAgent turns backend behavior into a conversation—simple, direct, and context-aware.
