# 🚀 Deployment Guide

This guide covers deployment options for MyPhotoBooth application (v1.3.0 - CQRS Architecture with comprehensive testing).

## Table of Contents

- [Production Checklist](#production-checklist)
- [Docker Deployment](#docker-deployment)
- [Cloud Deployment](#cloud-deployment)
- [Environment Configuration](#environment-configuration)
- [Database Setup](#database-setup)
- [SSL/HTTPS Configuration](#sslhttps-configuration)
- [Monitoring](#monitoring)

## Production Checklist

Before deploying to production, ensure you:

- [ ] Generate a secure JWT secret key (min 32 characters)
- [ ] Use a strong database password
- [ ] Configure CORS for your production domain
- [ ] Set up HTTPS/SSL certificates
- [ ] Configure proper file storage (cloud storage recommended)
- [ ] Set up database backups
- [ ] Configure logging and monitoring
- [ ] Set appropriate file size limits
- [ ] Review and test all security settings
- [ ] Set up error tracking (Sentry, Application Insights, etc.)
- [ ] Configure CDN for static assets (optional)
- [ ] Configure production email service (SendGrid, AWS SES, etc.)

## Docker Deployment

### 1. Backend Dockerfile

Create `src/MyPhotoBooth.API/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["MyPhotoBooth.API/MyPhotoBooth.API.csproj", "MyPhotoBooth.API/"]
COPY ["MyPhotoBooth.Application/MyPhotoBooth.Application.csproj", "MyPhotoBooth.Application/"]
COPY ["MyPhotoBooth.Infrastructure/MyPhotoBooth.Infrastructure.csproj", "MyPhotoBooth.Infrastructure/"]
COPY ["MyPhotoBooth.Domain/MyPhotoBooth.Domain.csproj", "MyPhotoBooth.Domain/"]

# Restore dependencies
RUN dotnet restore "MyPhotoBooth.API/MyPhotoBooth.API.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/MyPhotoBooth.API"
RUN dotnet build "MyPhotoBooth.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyPhotoBooth.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyPhotoBooth.API.dll"]
```

### 2. Frontend Dockerfile

Create `src/client/Dockerfile`:

```dockerfile
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build
RUN npm run build

# Production image
FROM nginx:alpine

# Copy built files
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
```

Create `src/client/nginx.conf`:

```nginx
server {
    listen 80;
    server_name _;

    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # API proxy (optional)
    location /api {
        proxy_pass http://api:5149;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 3. Production Docker Compose

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: myphotobooth_postgres_prod
    environment:
      POSTGRES_DB: ${DB_NAME:-myphotobooth}
      POSTGRES_USER: ${DB_USER:-postgres}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups
    networks:
      - myphotobooth_network
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    build:
      context: ./src
      dockerfile: MyPhotoBooth.API/Dockerfile
    container_name: myphotobooth_api_prod
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:5149
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=${DB_NAME:-myphotobooth};Username=${DB_USER:-postgres};Password=${DB_PASSWORD}"
      JwtSettings__SecretKey: ${JWT_SECRET}
      JwtSettings__Issuer: ${JWT_ISSUER:-MyPhotoBooth.API}
      JwtSettings__Audience: ${JWT_AUDIENCE:-MyPhotoBooth.Client}
      JwtSettings__AccessTokenExpirationMinutes: ${JWT_ACCESS_EXPIRATION:-15}
      JwtSettings__RefreshTokenExpirationDays: ${JWT_REFRESH_EXPIRATION:-7}
      StorageSettings__PhotosBasePath: /app/storage/photos
      StorageSettings__MaxFileSizeMB: ${MAX_FILE_SIZE_MB:-50}
      EmailSettings__SmtpHost: ${SMTP_HOST}
      EmailSettings__SmtpPort: ${SMTP_PORT:-587}
      EmailSettings__SmtpUser: ${SMTP_USER}
      EmailSettings__SmtpPass: ${SMTP_PASS}
      EmailSettings__FromEmail: ${FROM_EMAIL:-noreply@myphotobooth.com}
      EmailSettings__FromName: ${FROM_NAME:-MyPhotoBooth}
      EmailSettings__EnableSsl: ${SMTP_SSL:-true}
    volumes:
      - api_storage:/app/storage
      - api_logs:/app/logs
    networks:
      - myphotobooth_network
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5149/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  client:
    build:
      context: ./src/client
      dockerfile: Dockerfile
    container_name: myphotobooth_client_prod
    environment:
      VITE_API_BASE_URL: ${API_BASE_URL:-http://localhost:5149/api}
    networks:
      - myphotobooth_network
    depends_on:
      - api
    restart: unless-stopped

  nginx:
    image: nginx:alpine
    container_name: myphotobooth_nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    networks:
      - myphotobooth_network
    depends_on:
      - client
      - api
    restart: unless-stopped

volumes:
  postgres_data:
    driver: local
  api_storage:
    driver: local
  api_logs:
    driver: local

networks:
  myphotobooth_network:
    driver: bridge
```

### 4. Environment Variables

Create `.env.production`:

```env
# Database
DB_NAME=myphotobooth
DB_USER=postgres
DB_PASSWORD=your-secure-database-password-here

# JWT
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters-long
JWT_ISSUER=MyPhotoBooth.API
JWT_AUDIENCE=MyPhotoBooth.Client
JWT_ACCESS_EXPIRATION=15
JWT_REFRESH_EXPIRATION=7

# Storage
MAX_FILE_SIZE_MB=50

# Email (Production)
SMTP_HOST=smtp.sendgrid.net
SMTP_PORT=587
SMTP_USER=apikey
SMTP_PASS=your-sendgrid-api-key
FROM_EMAIL=noreply@yourdomain.com
FROM_NAME=MyPhotoBooth
SMTP_SSL=true

# API
API_BASE_URL=https://yourdomain.com/api
```

### 5. Deploy

```bash
# Build and start services
docker-compose -f docker-compose.prod.yml --env-file .env.production up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop services
docker-compose -f docker-compose.prod.yml down
```

## Cloud Deployment

### AWS (Elastic Beanstalk)

1. **Install AWS CLI and EB CLI**
   ```bash
   pip install awsebcli
   ```

2. **Initialize EB Application**
   ```bash
   eb init -p docker myphotobooth
   ```

3. **Create Environment**
   ```bash
   eb create myphotobooth-prod
   ```

4. **Configure Environment Variables**
   ```bash
   eb setenv DB_PASSWORD=xxx JWT_SECRET=xxx
   ```

5. **Deploy**
   ```bash
   eb deploy
   ```

### Azure (App Service)

1. **Install Azure CLI**
   ```bash
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   ```

2. **Login**
   ```bash
   az login
   ```

3. **Create Resource Group**
   ```bash
   az group create --name MyPhotoBoothRG --location eastus
   ```

4. **Create PostgreSQL**
   ```bash
   az postgres flexible-server create \
     --resource-group MyPhotoBoothRG \
     --name myphotobooth-db \
     --location eastus \
     --admin-user myadmin \
     --admin-password <password> \
     --sku-name Standard_B1ms
   ```

5. **Create App Service Plan**
   ```bash
   az appservice plan create \
     --name MyPhotoBoothPlan \
     --resource-group MyPhotoBoothRG \
     --sku B1 \
     --is-linux
   ```

6. **Create Web App (API)**
   ```bash
   az webapp create \
     --resource-group MyPhotoBoothRG \
     --plan MyPhotoBoothPlan \
     --name myphotobooth-api \
     --runtime "DOTNET|10.0"
   ```

7. **Configure App Settings**
   ```bash
   az webapp config appsettings set \
     --resource-group MyPhotoBoothRG \
     --name myphotobooth-api \
     --settings ConnectionStrings__DefaultConnection="..." JWT_SECRET="..."
   ```

8. **Deploy**
   ```bash
   cd src/MyPhotoBooth.API
   az webapp deploy --resource-group MyPhotoBoothRG --name myphotobooth-api --src-path publish.zip
   ```

### Google Cloud (Cloud Run)

1. **Build Container**
   ```bash
   gcloud builds submit --tag gcr.io/PROJECT-ID/myphotobooth-api
   ```

2. **Deploy**
   ```bash
   gcloud run deploy myphotobooth-api \
     --image gcr.io/PROJECT-ID/myphotobooth-api \
     --platform managed \
     --region us-central1 \
     --allow-unauthenticated
   ```

## Database Setup

### Production Database

1. **Backup Strategy**
   ```bash
   # Create backup script
   cat > backup.sh << 'EOF'
   #!/bin/bash
   BACKUP_DIR="/backups"
   TIMESTAMP=$(date +%Y%m%d_%H%M%S)
   docker exec myphotobooth_postgres_prod pg_dump -U postgres myphotobooth > "$BACKUP_DIR/backup_$TIMESTAMP.sql"

   # Keep only last 7 days
   find $BACKUP_DIR -name "backup_*.sql" -mtime +7 -delete
   EOF

   chmod +x backup.sh
   ```

2. **Restore from Backup**
   ```bash
   docker exec -i myphotobooth_postgres_prod psql -U postgres myphotobooth < backup_file.sql
   ```

3. **Run Migrations**
   ```bash
   docker exec myphotobooth_api_prod dotnet ef database update
   ```

## SSL/HTTPS Configuration

### Using Let's Encrypt with Nginx

1. **Install Certbot**
   ```bash
   sudo apt-get install certbot python3-certbot-nginx
   ```

2. **Obtain Certificate**
   ```bash
   sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
   ```

3. **Auto-renewal**
   ```bash
   sudo certbot renew --dry-run
   ```

### Nginx SSL Configuration

Create `nginx/nginx.conf`:

```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    ssl_certificate /etc/nginx/ssl/fullchain.pem;
    ssl_certificate_key /etc/nginx/ssl/privkey.pem;

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    location / {
        proxy_pass http://client:80;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    location /api {
        proxy_pass http://api:5149;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Monitoring

### Health Checks

Add health check endpoint in `Program.cs`:

```csharp
app.MapHealthChecks("/health");
```

### Logging

Configure logging in `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Monitoring Tools

- **Application Insights** (Azure)
- **CloudWatch** (AWS)
- **Stackdriver** (GCP)
- **Sentry** (Error tracking)
- **Datadog** (APM)

## Performance Optimization

### Backend

1. Enable response caching
2. Use Redis for distributed caching
3. Enable gzip compression
4. Optimize database queries

### Frontend

1. Enable browser caching
2. Use CDN for static assets
3. Enable gzip/brotli compression
4. Lazy load images
5. Code splitting

## Security Hardening

1. **Rate Limiting** - Implement rate limiting for API endpoints
2. **CORS** - Configure proper CORS policies
3. **Security Headers** - Add security headers (HSTS, CSP, etc.)
4. **Input Validation** - Validate all inputs
5. **SQL Injection** - Use parameterized queries (EF Core handles this)
6. **XSS Protection** - Sanitize user inputs
7. **File Upload** - Validate file types and sizes

## Email Service Configuration

### Development (Mailpit)

For local development, Mailpit is already configured in `docker-compose.yml`:

```yaml
mailpit:
  image: axllent/mailpit:latest
  ports:
    - "8025:8025"
    - "1025:1025"
```

Access the Mailpit web UI at http://localhost:8025 to view captured emails.

### Production Email Services

Choose a production email service and configure the following settings in your environment:

#### SendGrid

```env
EmailSettings__SmtpHost=smtp.sendgrid.net
EmailSettings__SmtpPort=587
EmailSettings__SmtpUser=apikey
EmailSettings__SmtpPass=YOUR_SENDGRID_API_KEY
EmailSettings__FromEmail=noreply@yourdomain.com
EmailSettings__FromName=MyPhotoBooth
EmailSettings__EnableSsl=true
```

#### AWS SES

```env
EmailSettings__SmtpHost=email-smtp.us-east-1.amazonaws.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUser=YOUR_SES_SMTP_USERNAME
EmailSettings__SmtpPass=YOUR_SES_SMTP_PASSWORD
EmailSettings__FromEmail=noreply@yourdomain.com
EmailSettings__FromName=MyPhotoBooth
EmailSettings__EnableSsl=true
```

#### Mailgun

```env
EmailSettings__SmtpHost=smtp.mailgun.org
EmailSettings__SmtpPort=587
EmailSettings__SmtpUser=postmaster@yourdomain.com
EmailSettings__SmtpPass=YOUR_MAILGUN_PASSWORD
EmailSettings__FromEmail=noreply@yourdomain.com
EmailSettings__FromName=MyPhotoBooth
EmailSettings__EnableSsl=true
```

### Email Templates

The application sends the following email types:
- **Password Reset**: Contains a secure token link for password reset
- **Future**: Email verification, notifications, etc.

### Security Considerations

- **User Enumeration**: The forgot password endpoint returns success even if the email doesn't exist (prevents user enumeration)
- **Token Expiration**: Password reset tokens expire after a configurable period (default: 1 hour)
- **Rate Limiting**: Consider implementing rate limiting on password reset endpoints
- **Secure Tokens**: Uses ASP.NET Identity's cryptographic token generation

## Troubleshooting

### Container Issues

```bash
# View logs
docker-compose logs -f api
docker-compose logs -f client

# Restart service
docker-compose restart api

# Rebuild image
docker-compose build --no-cache api
```

### Database Connection Issues

```bash
# Test connection
docker exec -it myphotobooth_postgres_prod psql -U postgres -d myphotobooth

# Check network
docker network inspect myphotobooth_network
```

### Storage Issues

```bash
# Check storage permissions
docker exec myphotobooth_api_prod ls -la /app/storage

# Check disk space
docker exec myphotobooth_api_prod df -h
```

## Maintenance

### Update Application

```bash
# Pull latest code
git pull

# Rebuild and restart
docker-compose -f docker-compose.prod.yml build
docker-compose -f docker-compose.prod.yml up -d

# Clean up old images
docker image prune -a
```

### Database Maintenance

```bash
# Vacuum database
docker exec myphotobooth_postgres_prod psql -U postgres -d myphotobooth -c "VACUUM ANALYZE;"

# Check database size
docker exec myphotobooth_postgres_prod psql -U postgres -d myphotobooth -c "SELECT pg_size_pretty(pg_database_size('myphotobooth'));"
```

---

For more information, see [README.md](README.md)
