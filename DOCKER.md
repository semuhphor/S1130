# Running IBM 1130 Emulator with Docker

## Quick Start

### Prerequisites
- Docker Desktop installed
- Docker Compose installed

### Run with Docker Compose

1. **Build and start all services:**
   ```bash
   docker-compose up --build
   ```

2. **Access the application:**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

3. **Stop the services:**
   ```bash
   docker-compose down
   ```

### Run in detached mode (background)
```bash
docker-compose up -d
```

View logs:
```bash
docker-compose logs -f
```

## Individual Container Commands

### Backend API Only
```bash
cd src/S1130.WebApi
docker build -t s1130-backend .
docker run -p 5000:5000 s1130-backend
```

### Frontend Only
```bash
cd web-frontend
docker build -t s1130-frontend .
docker run -p 3000:80 s1130-frontend
```

## Production Deployment

For production deployment, update the `REACT_APP_API_URL` in `docker-compose.yml` to point to your actual backend URL:

```yaml
frontend:
  environment:
    - REACT_APP_API_URL=https://your-api-domain.com
```

## Troubleshooting

### Backend not responding
Check backend logs:
```bash
docker-compose logs backend
```

### Frontend can't connect to backend
1. Ensure backend is running: `docker-compose ps`
2. Check CORS settings in backend
3. Verify `REACT_APP_API_URL` environment variable

### Rebuild after code changes
```bash
docker-compose up --build
```

## Architecture

- **Backend**: ASP.NET Core 8 Web API running on port 5000
- **Frontend**: React 18 served by Nginx on port 80 (mapped to 3000)
- **Network**: Both containers on isolated bridge network for inter-service communication

## Container Details

### Backend Container
- Base: `mcr.microsoft.com/dotnet/aspnet:8.0`
- Multi-stage build for optimized size
- Health check endpoint available

### Frontend Container
- Base: `nginx:alpine`
- Optimized production build
- Gzip compression enabled
- React Router support configured
