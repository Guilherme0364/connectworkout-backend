# ExerciseDB API Setup Guide

This guide explains how to obtain and configure the ExerciseDB API key for the ConnectWorkout application.

## Overview

ConnectWorkout uses the [ExerciseDB API](https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb) from RapidAPI to provide a comprehensive database of exercises with:
- 1300+ exercises
- Exercise demonstrations (GIF images)
- Muscle groups and equipment information
- Detailed exercise instructions

## Getting Your API Key

### 1. Create a RapidAPI Account

1. Go to [https://rapidapi.com](https://rapidapi.com)
2. Click **Sign Up** (top right)
3. Create an account using:
   - Email and password, OR
   - Sign up with Google/GitHub

### 2. Subscribe to ExerciseDB API

1. Visit the [ExerciseDB API page](https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb)
2. Click **Subscribe to Test** button
3. Choose a pricing plan:
   - **Basic (FREE)**:
     - 100 requests/day
     - Perfect for development and testing
     - **Recommended for getting started**
   - **Pro ($10/month)**:
     - 5000 requests/month
     - Better for production use
   - **Ultra ($25/month)**:
     - 10000 requests/month
     - For high-traffic applications

4. Click **Subscribe**
5. You'll be redirected to the API dashboard

### 3. Get Your API Key

1. On the ExerciseDB API page, look for the **Header Parameters** section in the center
2. You'll see `X-RapidAPI-Key` with a long string value
3. This is your API key - it looks something like:
   ```
   1df6d54d84msha1b6017ea1f4a35p1bc360jsn80dd5af9aa33
   ```
4. Copy this key

## Configuring ConnectWorkout

### 1. Update appsettings.json

Open `ConnectWorkout.API/appsettings.json` and find the `ExerciseDb` section:

```json
{
  "ExerciseDb": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "CacheExpiryMinutes": 1440
  }
}
```

Replace `YOUR_API_KEY_HERE` with your actual API key:

```json
{
  "ExerciseDb": {
    "ApiKey": "1df6d54d84msha1b6017ea1f4a35p1bc360jsn80dd5af9aa33",
    "CacheExpiryMinutes": 1440
  }
}
```

### 2. Cache Configuration (Optional)

The `CacheExpiryMinutes` setting controls how long exercise data is cached:
- **Default**: 1440 minutes (24 hours)
- Caching helps avoid hitting API rate limits
- With the free plan (100 requests/day), caching is **essential**

To adjust cache duration:
```json
"CacheExpiryMinutes": 720  // 12 hours
```

### 3. appsettings.Development.json (Optional)

For development, you can override settings in `appsettings.Development.json`:

```json
{
  "ExerciseDb": {
    "ApiKey": "your-development-api-key",
    "CacheExpiryMinutes": 60
  }
}
```

## Testing the Integration

### 1. Start the Application

```bash
cd ConnectWorkout.API
dotnet run
```

### 2. Test Endpoints

Visit Swagger UI at `http://localhost:7009/swagger` and try:

#### Get All Body Parts
```
GET /api/exercises/bodyparts
```

Expected response:
```json
[
  "back",
  "cardio",
  "chest",
  "lower arms",
  "lower legs",
  "neck",
  "shoulders",
  "upper arms",
  "upper legs",
  "waist"
]
```

#### Search Exercises
```
GET /api/exercises/search?name=bench
```

#### Get Exercises by Body Part
```
GET /api/exercises/bodypart/chest
```

### 3. Check Logs

The application logs API calls and cache hits:
```
Cache miss for exercises/bodyPartList, fetching from API
Cache hit for exercises/bodyPartList
```

## API Endpoints Available

The ExerciseDB service provides:

| Endpoint | Description |
|----------|-------------|
| `/api/exercises` | Get all exercises (1300+) |
| `/api/exercises/search?name={name}` | Search exercises by name |
| `/api/exercises/{id}` | Get specific exercise by ID |
| `/api/exercises/bodypart/{bodyPart}` | Get exercises by body part |
| `/api/exercises/target/{target}` | Get exercises by target muscle |
| `/api/exercises/equipment/{equipment}` | Get exercises by equipment |
| `/api/exercises/bodyparts` | List all body parts |
| `/api/exercises/targets` | List all target muscles |
| `/api/exercises/equipments` | List all equipment types |

## Monitoring Usage

### Check API Usage on RapidAPI

1. Go to [RapidAPI Dashboard](https://rapidapi.com/developer/dashboard)
2. Click on **My APIs** → **ExerciseDB**
3. View your usage statistics:
   - Requests today
   - Requests this month
   - Remaining quota

### Free Tier Limits

With the free Basic plan:
- **100 requests per day**
- Resets at midnight UTC
- Caching helps stay within limits:
  - Initial load: ~10 requests (body parts, targets, equipment lists)
  - Cached for 24 hours
  - Subsequent searches use cache

### Avoiding Rate Limits

1. **Enable caching** (already configured)
2. **Don't fetch all exercises repeatedly** - the service caches this
3. **Use specific endpoints** instead of searching all exercises
4. **Upgrade plan** if needed for production

## Troubleshooting

### Error: "API key for ExerciseDB is missing"

**Problem**: API key not configured
**Solution**: Add your API key to `appsettings.json`

### Error: 401 Unauthorized

**Problem**: Invalid API key
**Solution**:
1. Verify the API key in appsettings.json
2. Check if you're subscribed to ExerciseDB on RapidAPI
3. Ensure there are no extra spaces in the key

### Error: 429 Too Many Requests

**Problem**: Rate limit exceeded
**Solution**:
1. Wait for quota to reset (midnight UTC)
2. Increase cache duration
3. Upgrade to Pro plan

### Error: "Failed to fetch data from ExerciseDB API"

**Problem**: Network or API issue
**Solution**:
1. Check internet connection
2. Verify RapidAPI is online
3. Check application logs for details

## Environment Variables (Alternative)

Instead of `appsettings.json`, you can use environment variables:

### Windows (PowerShell)
```powershell
$env:ExerciseDb__ApiKey="your-api-key-here"
```

### Linux/Mac
```bash
export ExerciseDb__ApiKey="your-api-key-here"
```

### Docker
```yaml
environment:
  - ExerciseDb__ApiKey=your-api-key-here
```

## Security Best Practices

1. **Never commit API keys** to version control
2. **Add to .gitignore**:
   ```
   appsettings.json
   appsettings.*.json
   ```
3. **Use environment variables** in production
4. **Use separate keys** for dev/staging/production
5. **Rotate keys** periodically

## Additional Resources

- [ExerciseDB API Documentation](https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb)
- [RapidAPI Help Center](https://docs.rapidapi.com/)
- [ConnectWorkout Documentation](./README.md)

## Support

If you encounter issues:
1. Check the logs in the application console
2. Verify your API key is active on RapidAPI
3. Review this guide
4. Contact the development team

---

**Last Updated**: October 2025
**API Version**: ExerciseDB v1
