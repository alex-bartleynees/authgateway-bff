# AuthGateway BFF Helm Chart

This Helm chart deploys the AuthGateway BFF service with flexible configuration for different applications.

## Installation

### Installation with App-Specific Values

The chart uses a base `values.yaml` file (containing image tags and shared config) plus app-specific values files:

```bash
# Install for Product Feedback app
helm install product-feedback-bff ./authgateway-bff \
  -f values.yaml \
  -f values-product-feedback.yaml

# Install for another app
helm install ecommerce-bff ./authgateway-bff \
  -f values.yaml \
  -f values-ecommerce.yaml
```

**Note:** Always specify `-f values.yaml` first, then your app-specific file. Helm merges them with later files taking precedence.

## File Structure

- **values.yaml** - Base values file containing:
  - Image repository and tag (updated by CI/CD)
  - Shared infrastructure config (Redis, Keycloak, etc.)
  - Generic defaults

- **values-{app-name}.yaml** - App-specific overrides containing:
  - OpenID Connect ClientId
  - Reverse proxy backend cluster configuration
  - Any app-specific settings

This pattern allows:
- ✅ Single CI/CD pipeline that updates `values.yaml` with new image tags
- ✅ All apps automatically get the latest image version
- ✅ App-specific config stays separate and clean
- ✅ Easy to add new apps without modifying CI/CD

### Installation with Inline Overrides

```bash
helm install my-bff ./authgateway-bff \
  --set image.tag=v1.2.3 \
  --set config.reverseProxy.clusters.my-api.destinations.destination1.address=http://my-api:8080/
```

## Configuration

### Key Configuration Sections

#### Reverse Proxy Clusters

Configure which backend APIs to proxy to:

```yaml
config:
  reverseProxy:
    clusters:
      my-api:
        destinations:
          destination1:
            address: "http://my-api-svc.namespace.svc.cluster.local:5000/"
      another-api:
        destinations:
          destination1:
            address: "http://another-api-svc.namespace.svc.cluster.local:6000/"
```

#### Redis Configuration

```yaml
redis:
  secretName: "redis-auth"
  passwordKey: "REDIS_PASSWORD"
  useTls: true
  host: "redis-master.redis.svc.cluster.local:6379"
```

#### Keycloak/OIDC Configuration

```yaml
keycloak:
  secretName: "keycloak-auth"
  clientSecretKey: "CLIENT_SECRET"
  serverUrl: "https://identity.example.com"

config:
  openIdConnect:
    redirectUri: "/"
```

## Examples

### Example 1: Single Backend API

```yaml
# values-simple-app.yaml
image:
  tag: "1.0.0"

config:
  reverseProxy:
    clusters:
      backend-api:
        destinations:
          destination1:
            address: "http://backend-api:8080/"
```

### Example 2: Multiple Backend APIs

```yaml
# values-microservices-app.yaml
image:
  tag: "2.0.0"

config:
  reverseProxy:
    clusters:
      users-api:
        destinations:
          destination1:
            address: "http://users-api.microservices.svc.cluster.local:5000/"
      orders-api:
        destinations:
          destination1:
            address: "http://orders-api.microservices.svc.cluster.local:5001/"
      payments-api:
        destinations:
          destination1:
            address: "http://payments-api.microservices.svc.cluster.local:5002/"
```

### Example 3: Different Environment Settings

```yaml
# values-staging.yaml
image:
  tag: "staging-latest"

keycloak:
  serverUrl: "https://identity-staging.example.com"

redis:
  host: "redis-staging.redis.svc.cluster.local:6379"

config:
  reverseProxy:
    clusters:
      api:
        destinations:
          destination1:
            address: "http://api-staging.staging.svc.cluster.local:8080/"
```

## Usage with Multiple Apps

To use the same chart for different applications:

1. **Create app-specific values files**:
   ```bash
   # values-app1.yaml
   # values-app2.yaml
   # values-app3.yaml
   ```

2. **Install each application separately**:
   ```bash
   helm install app1-bff ./authgateway-bff -f values-app1.yaml -n app1-namespace
   helm install app2-bff ./authgateway-bff -f values-app2.yaml -n app2-namespace
   helm install app3-bff ./authgateway-bff -f values-app3.yaml -n app3-namespace
   ```

3. **Each installation gets its own ConfigMap** with app-specific settings based on the values file used.

## Upgrading

```bash
# Upgrade with new values
helm upgrade my-bff ./authgateway-bff -f my-values.yaml

# Upgrade just the image tag
helm upgrade my-bff ./authgateway-bff --reuse-values --set image.tag=v1.2.4
```

## Uninstalling

```bash
helm uninstall my-bff
```
