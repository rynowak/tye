application app = {
  name: 'frontend-backend'

  instance frontend 'oam.dev/Container@v1alpha1' = {
    application: app.name
    name: 'frontend'
    properties: {
      build: {
        dotnet: {
          project: 'backend/backend.csproj'
        }
      }
      run: {
        container: {
          image: 'rynowak/frontend:0.5.0-dev'
        }
      }
      dependsOn: [
        {
          name: 'backend'
          kind: 'http'
          setEnv: {
            SERVICE__BACKEND__HOST: 'host'
            SERVICE__BACKEND__PORT: 'port'
          }
        }
      ]
      provides: [
        {
          name: 'frontend'
          kind: 'http'
          containerPort: 80
        }
      ]
    }
  }

  instance backend 'oam.dev/Container@v1alpha1' = {
    application: app.name
    name: 'backend'
    properties: {
      build: {
        dotnet: {
          project: 'backend/backend.csproj'
        }
      }
      run: {
        container: {
          image: 'rynowak/backend:0.5.0-dev'
        }
      }
      provides: [
        {
          name: 'backend'
          kind: 'http'
          containerPort: 80
        }
      ]
    }
  }
}